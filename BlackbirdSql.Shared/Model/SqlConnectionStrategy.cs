﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.SqlConnectionStrategy

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Controls.PropertiesWindow;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Model;
using Microsoft.VisualStudio.Data.Services;



namespace BlackbirdSql.Shared.Model;


public class SqlConnectionStrategy : AbstractConnectionStrategy
{
	private class SqlBatchExecutionHandler : IBBatchExecutionHandler
	{
		public void Register(IDbConnection conn, IBsNativeDbStatementWrapper sqlStatement, QESQLBatch batch)
		{
			if (NativeDb.IsSupportedConnection(conn))
			{
				new NativeDbConnectionWrapperProxy(conn).InfoMessageEvent += batch.OnSqlInfoMessage;
				if (DbCommandWrapper.IsSupportedCommandType(sqlStatement))
				{
					new DbCommandWrapper(sqlStatement).StatementCompletedEvent += batch.OnSqlStatementCompleted;
				}
			}
		}

		public void UnRegister(IDbConnection conn, IBsNativeDbStatementWrapper sqlStatement, QESQLBatch batch)
		{
			if (NativeDb.IsSupportedConnection(conn))
			{
				new NativeDbConnectionWrapperProxy(conn).InfoMessageEvent -= batch.OnSqlInfoMessage;
				if (DbCommandWrapper.IsSupportedCommandType(sqlStatement))
				{
					new DbCommandWrapper(sqlStatement).StatementCompletedEvent -= batch.OnSqlStatementCompleted;
				}
				else
				{
					Diag.Dug(new DataException("Supported connection, but not supported command?"));
				}
			}
		}
	}

	private bool? _IsCloudConnection;

	protected bool _IsDwConnection = false;

	private int? _Spid;

	private string _TracingId;

	private string _ProductLevel;

	public static readonly Csb BuilderWithDefaultApplicationName = new("server=localhost;", false);


	private ConnectedPropertiesWindow _propertiesWindowObject;




	public virtual bool IsCloudConnection
	{
		get
		{
			lock (_LockObject)
			{
				bool result = true;
				if (base.Connection != null /* && base.Connection.State == ConnectionState.Open */)
				{
					if (!_IsCloudConnection.HasValue)
					{
						// FbConnection sqlConnection = (FbConnection)Connection;
						try
						{
							Csb csa = new(Connection, false);
							_IsCloudConnection = result = csa.IsServerConnection;
						}
						catch (Exception e)
						{
							Diag.Dug(e);
						}
						/*
						finally
						{

							if (sqlConnection != null)
							{
								sqlConnection.Close();
								sqlConnection.Dispose();
							}
						}
						*/
					}

					// result = _IsCloudConnection.HasValue && _IsCloudConnection.Value;
				}
				else
				{
					if (_IsCloudConnection.HasValue)
					{
						Diag.Dug(new DataException("connection is not open, but _IsCloudConnection has a value.  This value should be cleared when we disconnect"));
					}
				}

				return result;
			}
		}
	}


	public virtual bool IsDwConnection
	{
		get
		{
			return _IsDwConnection;
			/*
			lock (_LockObject)
			{
				bool result = false;
				if (base.Connection != null && base.Connection.State == ConnectionState.Open)
				{
					if (!_IsDwConnection.HasValue)
					{
						FbConnection sqlConnection = null;
						try
						{
							sqlConnection = ReliableConnectionHelper.CloneAndOpenConnection(base.Connection);
							_IsDwConnection = SqlVersionUtils.IsSqlDwAzure(base.Connection);
						}
						catch (Exception e)
						{
							Tracer.LogExCatch(typeof(SqlConnectionStrategy), e);
						}
						finally
						{
							if (sqlConnection != null)
							{
								sqlConnection.Close();
								sqlConnection.Dispose();
							}
						}
					}

					result = _IsDwConnection.HasValue && _IsDwConnection.Value;
				}
				else
				{
					SqlTracer.AssertTraceEvent(!_IsDwConnection.HasValue, TraceEventType.Error, EnSqlTraceId.VSShell, "connection is not open, but _IsDwConnection has a value.  This value should be cleared when we disconnect");
				}

				return result;
			}
			*/
		}
	}

	public string TracingID
	{
		get
		{
			lock (_LockObject)
			{
				if (Connection == null || Connection.State != ConnectionState.Open)
				{
					return string.Empty;
				}

				return _TracingId ?? string.Empty;
			}
		}
	}

	public string Spid
	{
		get
		{
			lock (_LockObject)
			{
				if (Connection == null || Connection.State != ConnectionState.Open)
				{
					return string.Empty;
				}

				return _Spid.HasValue ? _Spid.ToString() : string.Empty;
			}
		}
	}

	public override string DisplayUserName
	{
		get
		{
			string text = base.DisplayUserName;
			if (!string.IsNullOrEmpty(Spid))
			{
				text = string.Format(CultureInfo.CurrentCulture, ControlsResources.DisplayUserNameAndSPID, text, Spid);
			}

			return text;
		}
	}

	public SqlConnectionStrategy()
	{
		ConnectionChangedPriority += HandleConnectionChanged;
	}


	protected override IDbConnection CreateDbConnectionFromConnectionInfo(ConnectionPropertyAgent ci, bool openConnection)
	{
		if (ci != null)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "CreateDbConnectionFromConnectionInfo", "ConnectionPropertyAgent is not null");
			if (openConnection)
			{
				CreateAndOpenDbConnectionFromConnectionInfo(ci, out IDbConnection connection);
				return connection;
			}

			Csb csa = [];
			PopulateConnectionStringBuilder(csa, ci);
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "CreateDbConnectionFromConnectionInfo", "Csb connectionString: {0}", csa.ConnectionString);

			return NativeDb.CreateDbConnection(csa.ConnectionString);
		}

		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "CreateDbConnectionFromConnectionInfo", "ERROR ConnectionInfo is null.");

		return null;
	}

	protected virtual void CreateAndOpenDbConnectionFromConnectionInfo(ConnectionPropertyAgent ci, out IDbConnection connection)
	{
		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "CreateAndOpenDbConnectionFromConnectionInfo", "Enter");

		Csb csa = [];
		PopulateConnectionStringBuilder(csa, ci);

		connection = NativeDb.CreateDbConnection(csa.ToString());

		try
		{
			connection.Open();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			MessageCtl.ShowEx(ex, ControlsResources.ErrDatabaseConnection.FmtRes(connection.ConnectionString, ex.Message), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}

		if (connection.State != ConnectionState.Open)
		{
			DataException ex = new("Failed to open connection");
			Diag.Dug(ex);
			MessageCtl.ShowEx(ex, ControlsResources.ErrDatabaseConnection.FmtRes(connection.ConnectionString, ex.Message), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		/*
		if (ReliableConnectionHelper.OpenConnection(sqlConnectionStringBuilder, useRetry: true) is ReliableSqlConnection reliableSqlConnection && !string.IsNullOrEmpty(reliableSqlConnection.Database) && reliableSqlConnection.State == ConnectionState.Open)
		{
			connection = reliableSqlConnection.GetUnderlyingConnection();
		}
		*/
	}

	private void HandleConnectionChanged(object sender, ConnectionChangedEventArgs args)
	{
		IDbConnection previousConnection = args.PreviousConnection;
		if (previousConnection != null)
		{
			if (previousConnection is DbConnection dbConnection)
			{
				dbConnection.StateChange -= HandleConnectionStateChanged;
			}
		}

		lock (_LockObject)
		{
			_Spid = null;
			_ProductLevel = null;
			_TracingId = null;
			// _IsCloudConnection = null;
			// _IsDwConnection = null;
			if (Connection != null)
			{
				(Connection as DbConnection).StateChange += HandleConnectionStateChanged;
				UpdateStateForCurrentConnection(Connection.State, ConnectionState.Closed);
			}
		}
	}

	private void HandleConnectionStateChanged(object sender, StateChangeEventArgs args)
	{
		UpdateStateForCurrentConnection(args.CurrentState, args.OriginalState);
	}

	private void UpdateStateForCurrentConnection(ConnectionState currentState, ConnectionState previousState)
	{
		/*
		if (currentState == ConnectionState.Broken || currentState == ConnectionState.Closed)
		{
			lock (_LockObject)
			{
				_Spid = null;
				_TracingId = null;
				_ProductLevel = null;
				_IsCloudConnection = null;
				_IsDwConnection = null;
			}
		}

		if (currentState == ConnectionState.Open)
		{
			QueryServerSideProperties();
		}

		if (currentState != ConnectionState.Open || previousState != 0 || !string.IsNullOrEmpty(SqlServerConnectionService.GetDatabaseName(ConnectionInfo)) || Connection == null)
		{
			return;
		}

		try
		{
			FbConnection asSqlConnection = ReliableConnectionHelper.GetAsSqlConnection(Connection);
			string text = null;
			try
			{
				text = GetDefaultDatabaseForLogin(asSqlConnection, useExistingDbOnError: false);
			}
			catch (Exception ex)
			{
				if (ex.menamy != "EnumeratorException")
					throw ex;
			}

			if (string.IsNullOrEmpty(text))
			{
				text = Master;
			}

			SqlServerConnectionService.SetDatabaseName(ConnectionInfo, text);
		}
		catch (Exception e)
		{
			Diag.Dug(e);
		}
		*/
	}



	protected override void AcquireConnectionInfo(bool tryOpenConnection, out ConnectionPropertyAgent ci, out IDbConnection connection)
	{
		if (ConnectionInfo != null)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "AcquireConnectionInfo", "ConnectionInfo is not null");
			ci = ConnectionInfo;
			connection = CreateDbConnectionFromConnectionInfo(ci, tryOpenConnection);
		}
		else
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "AcquireConnectionInfo", "ConnectionInfo is null. Prompting");
			ci = PromptForConnectionForEditor(out connection);
			if (tryOpenConnection && connection != null && connection.State != ConnectionState.Open)
				connection.Open();
		}
	}

	protected override void ChangeConnectionInfo(bool tryOpenConnection, out ConnectionPropertyAgent ci, out IDbConnection connection)
	{
		ci = PromptForConnectionForEditor(out connection);
		if (tryOpenConnection && connection != null && connection.State != ConnectionState.Open)
			connection.Open();
	}



	private ConnectionPropertyAgent PromptForConnectionForEditor(out IDbConnection connection)
	{
		if (Connection != null)
			connection = NativeDb.CreateDbConnection(Connection.ConnectionString);
		else
			connection = null;

		return PromptForConnection(ref connection, ValidateConnectionForEditor);
	}

	public static ConnectionPropertyAgent PromptForConnection(ref IDbConnection connection)
	{
		return PromptForConnection(ref connection, ValidateConnection);
	}

	private static ConnectionPropertyAgent PromptForConnection(ref IDbConnection connection, VerifyConnectionDelegate validateConnectionDelegate)
	{
		Diag.ThrowIfNotOnUIThread();

		// Tracer.Trace(typeof(SqlConnectionStrategy), "PromptForConnection()");

		IBDataConnectionDlgHandler connectionDialogHandler = ApcManager.EnsureService<IBDataConnectionDlgHandler>();

		using (connectionDialogHandler)
		{
			try
			{
				connectionDialogHandler.AddSources(new Guid(VS.AdoDotNetTechnologyGuid));

				if (connection != null)
				{
					if (RctManager.ShutdownState)
						return null;

					string connectionString = RctManager.UpdateConnectionFromRegistration(connection);

					connectionDialogHandler.Title = "Modify SqlEditor Connection";
					connectionDialogHandler.EncryptedConnectionString = DataProtection.EncryptString(connectionString);
				}
				else
				{
					connectionDialogHandler.Title = "Configure SqlEditor Connection";
				}

				if (connectionDialogHandler.ShowDialog())
				{
					string connectionString = DataProtection.DecryptString(connectionDialogHandler.EncryptedConnectionString);

					if (RctManager.ShutdownState)
						return null;

					// Tracer.Trace(typeof(SqlConnectionStrategy), "PromptForConnection()", "ConnectionString reult: {0}.", connectionString);

					ConnectionPropertyAgent connectionInfo = new ();
					connectionInfo.Parse(connectionString);
					connection = NativeDb.CreateDbConnection(connectionString);

					return connectionInfo;
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}
		}

		/*

		using (ConnectionDialogWrapper connectionDialogWrapper = new ConnectionDialogWrapper())
		{
			ConnectionInfo connectionInfo = new ConnectionPropertyAgent();
			connectionDialogWrapper.ConnectionVerifier = validateConnectionDelegate;

			IVsUIShell uiShell = Package.GetGlobalService(typeof(IVsUIShell)) as IVsUIShell
				?? throw Diag.ExceptionService(typeof(IVsUIShell));

			try
			{
				___(uiShell.GetDialogOwnerHwnd(out var phwnd));

				if (connectionDialogWrapper.ShowDialogValidateConnection(phwnd, connectionInfo, out connection) == true)
					return connectionInfo;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				throw;
			}

		}
		*/

		connection = null;

		return null;
	}

	protected virtual IDbConnection ValidateConnectionForEditor(ConnectionPropertyAgent ci /*, IServerConnectionProvider server*/)
	{
		IDbConnection dbConnection = ValidateConnection(ci /*, server*/);

		/*
		if (ci.ServerType == SqlServerConnectionProvider.ServerType && dbConnection != null && SqlVersionUtils.IsSqlDwAps(dbConnection))
		{
			ConnectionDialogException ex = new(ControlsResources.ErrQEPdwNotSupported);
			Diag.Dug(ex);
			throw ex;
		}
		*/

		return dbConnection;
	}

	public static IDbConnection ValidateConnection(ConnectionPropertyAgent ci /* , IServerConnectionProvider server */)
	{
		ci.DataSource = ci.DataSource.Trim();
		/*
		if (string.IsNullOrWhiteSpace(ci.ApplicationName) || string.Equals(BuilderWithDefaultApplicationName.ApplicationName, ci.ApplicationName, StringComparison.Ordinal))
		{
			ci.ApplicationName = C_ApplicationName;
		}
		*/

		Csb csa = [];

		PopulateConnectionStringBuilder(csa, ci);

		IDbConnection dbConnection = NativeDb.CreateDbConnection(csa.ConnectionString);

		dbConnection.Open();
		using IDbCommand dbCommand = dbConnection.CreateCommand();
		dbCommand.CommandText = "Select 1";
		dbCommand.Connection = dbConnection;
		dbCommand.ExecuteNonQuery();

		return dbConnection;
	}

	public override List<string> GetAvailableDatabases()
	{
		List<string> list = [];

		if (RctManager.ShutdownState)
			return list;

		foreach (string dataset in RctManager.RegisteredDatasets)
		{
			list.Add(dataset);
		}

		return list;

		/*
		IDbCommand dbCommand = null;
		IDataReader dataReader = null;
		IDbConnection dbConnection = null;
		Exception ex = null;
		lock (_LockObject)
		{
			try
			{
				if (Connection != null && Connection.State == ConnectionState.Open)
				{
					if (IsCloudConnection && !string.Equals("master", Connection.Database, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(Connection.Database);
						return list;
					}

					dbConnection = ReliableConnectionHelper.CloneAndOpenConnection(Connection);
					dbCommand = dbConnection.CreateCommand();
					Version version = new Version(ReliableConnectionHelper.ReadServerVersion(Connection));
					if (version.Major == 8)
					{
						dbCommand.CommandText = "SELECT dtb.name AS [Name], dtb.status AS [Status] FROM master.dbo.sysdatabases dtb";
					}
					else
					{
						dbCommand.CommandText = "SELECT dtb.name AS [Name], dtb.state AS [State] FROM master.sys.databases dtb";
					}

					Dictionary<string, int> dictionary = new Dictionary<string, int>(1024);
					dataReader = dbCommand.ExecuteReader();
					bool flag = dataReader.Read();
					while (flag)
					{
						dataReader.GetName(0);
						string @string = dataReader.GetString(0);
						bool flag2 = true;
						if (version.Major == 8)
						{
							flag2 = (dataReader.GetInt32(1) & 0x3E0) == 0;
						}
						else
						{
							int num = Convert.ToInt32(dataReader.GetValue(1), CultureInfo.InvariantCulture);
							if (num == 1 || num == 2 || num == 3 || num == 4 || num == 6)
							{
								flag2 = false;
							}
						}

						if (flag2)
						{
							if (dictionary.ContainsKey(@string))
							{
								dictionary[@string]++;
							}
							else
							{
								dictionary[@string] = 1;
							}
						}

						flag = dataReader.Read();
					}

					if (dataReader != null)
					{
						dataReader.Dispose();
						dataReader = null;
					}

					foreach (string key in dictionary.Keys)
					{
						if (dictionary[key] == 1)
						{
							list.Add(key);
						}
					}

					list.Sort();
				}
			}
			catch (FbException ex2)
			{
				Tracer.LogExCatch(GetType(), ex2);
				ex = ex2;
			}
			catch (Exception ex3)
			{
				Tracer.LogExCatch(GetType(), ex3);
				ex = ex3;
			}
			finally
			{
				if (dataReader != null)
				{
					dataReader.Dispose();
					dataReader = null;
				}

				if (dbCommand != null)
				{
					dbCommand.Dispose();
					dbCommand = null;
				}

				if (dbConnection != null)
				{
					if (dbConnection.State != 0)
					{
						dbConnection.Close();
					}

					dbConnection.Dispose();
				}
			}

			if (ex != null)
			{
				Cmd.ShowExceptionInDialog(ControlsResources.SqlEditorNoAvailableDatabase, ex);
				return list;
			}

			return list;
		}
		*/
	}

	public override void ResetAndEnableConnectionStatistics()
	{
		lock (_LockObject)
		{
			try
			{
				// FbCommand cmd = new("ALTER SESSION RESET", (FbConnection)Connection);
				// cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			// FbConnection asSqlConnection = ReliableConnectionHelper.GetAsSqlConnection(Connection);
			// asSqlConnection.ResetStatistics();
			// asSqlConnection.StatisticsEnabled = true;
		}
	}

	public override void SetDatasetKeyOnConnection(string selectedDatasetKey, DbConnectionStringBuilder csb)
	{
		// Tracer.Trace(GetType(), "SetDatasetKeyOnConnection()", "selectedDatasetKey: {0}, ConnectionString: {1}.", selectedDatasetKey, csb.ConnectionString);

		lock (_LockObject)
		{
			if (Connection != null /* && Connection.State == ConnectionState.Open */ && IsCloudConnection)
			{
				try
				{
					_Csa = (Csb)csb;
					_Stamp = RctManager.Stamp;

					if (csb == null || _Csa.DatasetKey != selectedDatasetKey)
					{
						_Csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(selectedDatasetKey);
						_Stamp = RctManager.Stamp;
					}

					if (_Csa != null)
					{
						if (Connection.State == ConnectionState.Open)
							Connection.Close();
						Connection.ConnectionString = _Csa.ConnectionString;
						if (ConnectionInfo == null)
							ConnectionInfo = new();
						ConnectionInfo.Parse(_Csa);
						SetConnectionInfo(ConnectionInfo);
						Connection.Open();
					}
				}
				catch (DbException ex)
				{
					Diag.Dug(ex);
					MessageCtl.ShowEx(ex, string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrDatabaseNotAccessible, selectedDatasetKey), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			else
			{
				base.SetDatasetKeyOnConnection(selectedDatasetKey, csb);
			}
		}
	}

	public override object GetPropertiesWindowDisplayObject()
	{
		// return null;

		if (ConnectionInfo == null)
		{
			return DisconnectedPropertiesWindow.Instance;
		}

		_propertiesWindowObject ??= new ConnectedPropertiesWindow(this);

		return _propertiesWindowObject;

	}

	protected virtual void QueryServerSideProperties()
	{
		lock (_LockObject)
		{
			if (Connection == null || Connection.State != ConnectionState.Open)
			{
				return;
			}

			StringBuilder stringBuilder = new StringBuilder(256);
			stringBuilder.AppendLine("select @@spid;");
			stringBuilder.AppendLine("select SERVERPROPERTY('ProductLevel');");
			if (IsCloudConnection && !IsDwConnection)
			{
				stringBuilder.AppendLine("SELECT CONVERT(NVARCHAR(36), CONTEXT_INFO());");
			}

			IDbCommand dbCommand = Connection.CreateCommand();
			dbCommand.CommandText = stringBuilder.ToString();
			IDataReader dataReader = null;
			try
			{
				dataReader = dbCommand.ExecuteReader();
				if (dataReader.Read())
				{
					_Spid = int.Parse(dataReader.GetValue(0).ToString(), CultureInfo.InvariantCulture);
					dataReader.NextResult();
				}

				if (dataReader.Read())
				{
					_ProductLevel = dataReader.GetValue(0).ToString();
					dataReader.NextResult();
				}

				if (IsCloudConnection && !IsDwConnection && dataReader.Read())
				{
					_TracingId = dataReader.GetValue(0).ToString();
				}
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				_ = Connection.State;
				_ = 1;
			}
			finally
			{
				if (dataReader != null)
				{
					dataReader.Close();
					dataReader = null;
				}

				if (dbCommand != null)
				{
					dbCommand.Dispose();
					dbCommand = null;
				}
			}
		}
	}

	/*
	public static string GetDefaultDatabaseForLogin(string connectionString, bool useExistingDbOnError)
	{
		using FbConnection conn = ConnectionHelperUtils.OpenSqlConnection(new Csb(connectionString));
		return GetDefaultDatabaseForLogin(conn, useExistingDbOnError);
	}
	*/


	public static string GetDefaultDatabaseForLogin(DbConnection conn, bool useExistingDbOnError)
	{
		return conn.Database;
		/*
		ServerConnection serverConnection = new ServerConnection(conn);
		string result = string.Empty;
		Enumerator enumerator = new Enumerator();
		Request request = new Request
		{
			Urn = "Server/Login[@Name='" + Urn.EscapeString(serverConnection.TrueLogin) + "']",
			Fields = new string[1]
		};
		request.Fields[0] = "DefaultDatabase";
		try
		{
			DataSet dataSet = enumerator.Process(conn, request);
			if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
			{
				result = Convert.ToString(dataSet.Tables[0].Rows[0]["DefaultDatabase"], CultureInfo.InvariantCulture);
			}

			return result;
		}
		catch (EnumeratorException ex)
		{
			if (useExistingDbOnError)
			{
				SqlTracer.TraceEvent(TraceEventType.Information, EnSqlTraceId.TSqlEditorAndLanguageServices, "An error occurred when querying the default database name. Current database name or master will be used instead of throwing. The error is '{0}'", ex.ToString());
				string database = conn.Database;
				return !string.IsNullOrEmpty(database) ? database : Master;
			}

			throw;
		}
		*/
	}

	public override Version GetServerVersion()
	{
		Version version = new Version();
		if (Connection != null && Connection.State == ConnectionState.Open)
		{
			try
			{
				version = Connection.GetVersion();
			}
			catch (InvalidOperationException)
			{
			}

			if (version.Equals(new Version()))
			{
				Diag.Dug(new DataException("GetServerVersion is returning version (0,0).  Something is wrong!!"));
			}
		}

		return version;
	}

	public override string GetProductLevel()
	{
		lock (_LockObject)
		{
			return _ProductLevel ?? string.Empty;
		}
	}

	public override bool HasTransactions
	{
		get
		{
			if (!TtsActive)
				return false;

			return Transaction.HasTransactions();
		}
	}

	public override bool TtsActive => Transaction != null;


	public override bool CommitTransactions()
	{
		if (!HasTransactions)
			return false;

		try
		{
			Transaction.Commit();
			return true;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			Transaction?.Dispose();
			Transaction = null;
		}
	}

	public override void RollbackTransactions()
	{
		if (!HasTransactions)
			return;

		try
		{
			Transaction.Rollback();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
		finally
		{
			Transaction?.Dispose();
			Transaction = null;
		}
	}


	public override IBBatchExecutionHandler CreateBatchExecutionHandler()
	{
		return new SqlBatchExecutionHandler();
	}


}
