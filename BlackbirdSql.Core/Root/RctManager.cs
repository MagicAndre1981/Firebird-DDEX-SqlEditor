﻿
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Config;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;

using Cmd = BlackbirdSql.Sys.Cmd;



namespace BlackbirdSql;


// =========================================================================================================
//											RctManager Class
//
/// <summary>
/// Manages the RunningConnectionTable.
/// </summary>
/// <remarks>
/// Clients should always interface with the Rct through this agent.
/// This class's singleton instance should remain active throughout the ide session.
/// </remarks>
// =========================================================================================================
public sealed class RctManager : IDisposable
{


	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractRunningConnectionTable
	// ---------------------------------------------------------------------------------


	/// <summary>
	/// Singleton .ctor.
	/// </summary>
	private RctManager()
	{
		if (_Instance != null)
		{
			TypeAccessException ex = new(Resources.ExceptionDuplicateSingletonInstances.FmtRes(GetType().FullName));
			Diag.Dug(ex);
			throw ex;
		}

		_Instance = this;
	}

	/// <summary>
	/// Gets the instance of the RctManager for this session.
	/// We do not auto-create to avoid instantiation confusion.
	/// Use CreateInstance() to instantiate.
	/// </summary>
	public static RctManager Instance => _Instance;


	/// <summary>
	/// Creates the singleton instance of the RctManager for this session.
	/// Instantiation must always occur here and not by the Instance accessor to avoid
	/// confusion.
	/// </summary>
	public static RctManager CreateInstance() => new RctManager();



	/// <summary>
	/// Disposal of Rct at the end of an IDE session.
	/// </summary>
	public void Dispose()
	{
		Delete();
	}


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields and Constants - RctManager
	// =========================================================================================================


	private static RctManager _Instance;
	private RunningConnectionTable _Rct;
	private static string _UnadvisedConnectionString = null;
	private static bool _AdvisingExplorerEvents = false;


	#endregion Fields and Constants




	// =========================================================================================================
	#region Property accessors - RctManager
	// =========================================================================================================


	public static bool Available => _Instance != null && _Instance._Rct != null
		&& !_Instance._Rct.ShutdownState && !_Instance._Rct.Loading;

	public static bool AdvisingExplorerEvents
	{
		get { return _AdvisingExplorerEvents; }
		set { _AdvisingExplorerEvents = value; }
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The table of all registered databases of the current data provider located in
	/// Server Explorer, FlameRobin and the Solution's Project settings, and any
	/// volatile unique connections defined in SqlEditor.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static DataTable Databases => _Instance == null ? null : Rct?.Databases;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The sorted and filtered view of the Databases table containing only
	/// DataSources/Servers.
	/// </summary>
	/// <returns>
	/// The populated <see cref="DataTable"/> that can be used together with
	/// <see cref="Databases"/> in a 1-n scenario. <see cref="ErmBindingSource"/> for
	/// an example.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static DataTable DataSources => _Instance == null ? null : Rct?.DataSources;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The glyph used to identify connections derived from Project EDM connections.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static char EdmGlyph => SystemData.C_EdmGlyph;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if a connection dialog exists and has been activated through a
	/// UIHierarchy marshaler or the DataSources Explorer else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsEdmConnectionSource =>
		GetConnectionSource() == EnConnectionSource.EntityDataModel;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns true if the Rct is physically in a loading state and both sync and
	/// async tasks are not in Inactive or Shutdown states.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool Loading => _Instance != null && _Instance._Rct != null && _Instance._Rct.Loading;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The glyph used to identify connections derived from Project connections
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static char ProjectDatasetGlyph => SystemData.C_ProjectDatasetGlyph;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the Rct else null if the rct is in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static RunningConnectionTable Rct
	{
		get
		{
			if (_Instance == null)
				return null;

			if (_Instance._Rct == null || Loading)
				EnsureLoaded();

			return _Instance._Rct;
		}
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns an IEnumerable of the registered datasets else null if the rct is in a
	/// shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IEnumerable<string> RegisteredDatasets => Rct?.RegisteredDatasets;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the sequential stamp of the last attempt to modify the
	/// <see cref="RunningConnectionTable"/> and is used to perform drift detection
	/// to ensure uniformity of connections and <see cref="Csb"/>s globally across
	/// the extension.
	/// </summary>
	/// <remarks>
	/// Objects that use connections or <see cref="Csb"/>s record the stamp at the
	/// time their connection or Csa is created.
	/// Whenever the connection or Csa is accessed it must perform drift detection and
	/// compare it's saved stamp value against the Rct's stamp. If they differ it must
	/// check if it's connection requires updating or Csa requires renewing and also
	/// always update it's stamp to the most recent value irrelevant of any updates.
	/// If a Csa requires drift detection the built-in stamp drift detection should be
	/// enabled and used.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static long Stamp => RunningConnectionTable.Stamp;



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns the global shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool ShutdownState => _Instance != null && _Instance._Rct != null && _Instance._Rct.ShutdownState;


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The glyph used to identify connections derived from the External utility
	/// (FlameRobin for the Firebird port).
	/// connections.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static char UtilityDatasetGlyph => SystemData.C_UtilityDatasetGlyph;


	#endregion Property accessors




	// =========================================================================================================
	#region Methods - RctManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns a Csb instance from a registered connection using a
	/// ConnectionInfo object else null if the rct is in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneRegistered(IBPropertyAgent connectionInfo)
	{
		if (connectionInfo == null)
		{
			ArgumentNullException ex = new(nameof(connectionInfo));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;

		Csb csa = new(connectionInfo, false);
		string connectionUrl = csa.SafeDatasetMoniker;


		if (!Rct.TryGetHybridRowValue(connectionUrl, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for key: {connectionUrl}");
			Diag.Dug(ex);
			throw ex;
		}


		return new Csb((string)row[SysConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns instance from a registered connection using an IDbConnection
	/// else null if the rct is in a shutdown state..
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneRegistered(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connection.ConnectionString, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for ConnectionString: {connection.ConnectionString}");
			Diag.Dug(ex);
			throw ex;
		}


		return new Csb((string)row[SysConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns an instance from a registered connection using a Server
	/// Explorer node else null if the rct is in a shutdown state.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneRegistered(IVsDataExplorerNode node)
	{
		if (node == null)
		{
			ArgumentNullException ex = new(nameof(node));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;

		Csb csa = new(node, false);
		string connectionUrl = csa.SafeDatasetMoniker;


		if (!Rct.TryGetHybridRowValue(connectionUrl, out DataRow row))
		{
			KeyNotFoundException ex = new KeyNotFoundException($"Connection url not found in RunningconnectionTable for key: {connectionUrl}");
			Diag.Dug(ex);
			throw ex;
		}


		return new Csb((string)row[SysConstants.C_KeyExConnectionString]);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones and returns an instance from a registered connection using either a
	/// DatasetKey or ConnectionString else null if the rct is in a shutdown state.
	/// </summary>
	/// <param name="connectionKey">
	/// The DatasetKey or ConnectionString.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static Csb CloneRegistered(string hybridKey)
	{
		if (hybridKey == null)
		{
			ArgumentNullException ex = new(nameof(hybridKey));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(hybridKey, out DataRow row))
			return null;

		return new Csb((string)row[SysConstants.C_KeyExConnectionString]);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Clones a Csb instance from a registered connection using an
	/// IDbConnection , else null if the rct is in a shutdown state.
	/// Finally registers the csa for validity state checks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb CloneVolatile(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;


		Csb csa = CloneRegistered(connection);
		csa.RegisterValidationState(connection.ConnectionString);

		return csa;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Resets the underlying Running Connection Table in preparation for and ide
	/// shutdown or solution unload.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Delete()
	{
		if (_Instance != null && _Instance._Rct != null)
		{
			_Instance._Rct.Dispose();
			_Instance._Rct = null;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the SE connection key given the ConnectionUrl or DatasetKey
	/// or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetStoredConnectionKey(string connectionValue)
	{
		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connectionValue, out DataRow row))
			return null;

		object @object = row[SysConstants.C_KeyExConnectionKey];

		return @object == DBNull.Value  ? null : @object?.ToString();
	}


	public static void DisableExternalEvents() => Rct?.DisableExternalEvents();

	public static void EnableExternalEvents() => Rct?.EnableExternalEvents();

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the current connection dialog that is active else EnConnectionSource.None.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static EnConnectionSource GetConnectionSource()
	{
		// Definitely None
		if (ApcManager.IdeShutdownState)
			return EnConnectionSource.None;

		// Definitely ServerExplorer
		if (RctManager.AdvisingExplorerEvents)
			return EnConnectionSource.ServerExplorer;

		/*
		 * We're just peeking.
		 * 
		if (!ThreadHelper.CheckAccess())
		{
			// Fire and wait.

			EnConnectionSource result = ThreadHelper.JoinableTaskFactory.Run(async delegate
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				return GetConnectionSourceImpl(caller);
			});

			return result;
		}
		*/

		return GetConnectionSourceImpl();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets (on the ui thread) the current connection dialog that is active else
	/// EnConnectionSource.None.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static EnConnectionSource GetConnectionSourceImpl()
	{
		// We're just peeking.
		// Diag.ThrowIfNotOnUIThread();

		string objectKind = ApcManager.ActiveWindowObjectKind;

		// Probably nothing
		if (objectKind == null)
		{
			// Tracer.Trace(typeof(UnsafeCmd), $"GetConnectionSource({caller})", "\nProbably EnConnectionSource.None: ActiveWindowObjectKind is null.");
			return EnConnectionSource.None;
		}

		string seGuid = VSConstants.StandardToolWindows.ServerExplorer.ToString("B", CultureInfo.InvariantCulture);

		// Definitely ServerExplorer
		if (objectKind != null && objectKind.Equals(seGuid, StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nDefinitely EnConnectionSource.ServerExplorer: ActiveWindowObjectKind is ServerExplorer ToolWindow.");
			return EnConnectionSource.ServerExplorer;
		}

		string datasourceGuid = VSConstants.StandardToolWindows.DataSource.ToString("B", CultureInfo.InvariantCulture);

		// Definitely EntityDataModel 
		if (objectKind != null && objectKind.Equals(datasourceGuid, StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nDefinitely EnConnectionSource.EntityDataModel: ActiveWindowObjectKind is DataSource ToolWindow.");
			return EnConnectionSource.EntityDataModel;
		}


		string objectType = ApcManager.ActiveWindowObjectType;

		// Probably nothing
		if (objectType == null)
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nProbably EnConnectionSource.None: ActiveWindowObjectType is null, ObjectKind: {0}.", objectKind);
			return EnConnectionSource.None;
		}

		// Definitely Session
		if (objectType.StartsWith("BlackbirdSql.", StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nDefinitely EnConnectionSource.Session: ActiveWindowObjectType StartsWith 'BlackbirdSql'.");
			return EnConnectionSource.Session;
		}

		// Most likely Application.
		if (objectType.Equals("System.ComponentModel.Design.DesignerHost", StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nLikely EnConnectionSource.Application: ActiveWindowObjectType is ComponentModel.Design.DesignerHost.");
			return EnConnectionSource.Application;
		}

		// Most likely EntityDataModel or some other design model document initialized from
		// Solution Explorer that opens the connection dialog.
		// (Removed solution explorer as the kind to include other possible hierarchy launch locations.)
		if (objectType.Equals("Microsoft.VisualStudio.PlatformUI.UIHierarchyMarshaler", StringComparison.InvariantCultureIgnoreCase))
		{
			// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "\nLikely EnConnectionSource.EntityDataModel: ActiveWindowObjectType is Microsoft.VisualStudio.PlatformUI.UIHierarchyMarshaler.");
			return EnConnectionSource.EntityDataModel;
		}


		// No known connection source
		// Tracer.Trace(typeof(UnsafeCmd), "GetConnectionSource()", "No known ConnectionSource. ObjectType: {0}, ObjectKind: {1}.", objectType, objectKind);

		return EnConnectionSource.None;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a connection string given the ConnectionUrl or DatasetKey
	/// or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetConnectionString(string connectionValue)
	{
		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connectionValue, out DataRow row))
			return null;

		object @object = row[SysConstants.C_KeyExConnectionString];

		return @object == DBNull.Value ? null : @object?.ToString();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a connection DatasetKey given the ConnectionUrl or DatasetKey
	/// or DatsetKey synonym.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string GetDatasetKey(string connectionValue)
	{
		if (Rct == null)
			return null;

		if (!Rct.TryGetHybridRowValue(connectionValue, out DataRow row))
			return null;

		object @object = row[SysConstants.C_KeyExDatasetKey];

		return @object == DBNull.Value ? null : @object?.ToString();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and returns an instance from a registered connection using a
	/// ConnectionString else registers a new connection if none exists else null if the
	/// rct is in a shutdown state..
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static Csb EnsureInstance(string connectionString, EnConnectionSource source)
	{
		if (connectionString == null)
		{
			ArgumentNullException ex = new(nameof(connectionString));
			Diag.Dug(ex);
			throw ex;
		}

		if (ShutdownState)
			return null;


		if (Rct.TryGetHybridRowValue(connectionString, out DataRow row))
			return new Csb((string)row[SysConstants.C_KeyExConnectionString]);

		// Calls to this method expect a registered connection. If it doesn't exist it means
		// we're creating a new configured connection.

		Csb csa = new(connectionString);
		string datasetId = csa.DatasetId;

		if (string.IsNullOrWhiteSpace(datasetId))
			datasetId = csa.Dataset;

		// If the proposed key matches the proposed generated one, drop it.

		if (csa.ContainsKey(SysConstants.C_KeyExConnectionName)
			&& !string.IsNullOrWhiteSpace(csa.ConnectionName)
			&& (SysConstants.DatasetKeyFormat.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName
			|| SysConstants.DatasetKeyAlternateFormat.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName))
		{
			csa.Remove(SysConstants.C_KeyExConnectionName);
		}

		Rct.RegisterUniqueConnection(csa.ConnectionName, datasetId, source, ref csa);

		return csa;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and returns an instance from a registered connection using a
	/// ConnectionString else registers a new node connection if none exists else null
	/// if the rct is in a shutdown state.
	/// Finally registers the csa for validity state checks.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb EnsureVolatileInstance(IDbConnection connection, EnConnectionSource source)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		if (Rct == null)
			return null;


		Csb csa;


		if (Rct.TryGetHybridRowValue(connection.ConnectionString, out DataRow row))
		{
			csa = new((string)row[SysConstants.C_KeyExConnectionString]);
			csa.RegisterValidationState(connection.ConnectionString);

			// Tracer.Trace(typeof(Csb), "EnsureVolatileInstance()", "Found registered DataRow for dbConnectionString: {0}\nrow ConnectionString: {1}\ncsa.ConnectionString: {2}.", connection.ConnectionString, (string)row[DbConstants.C_KeyExConnectionString], csa.ConnectionString);

			return csa;
		}

		// New registration.

		// Tracer.Trace(typeof(Csb), "EnsureVolatileInstance()", "Could NOT find registered DataRow for dbConnectionString: {0}.", connection.ConnectionString);

		csa = new(connection);
		string datasetId = csa.DatasetId;

		if (string.IsNullOrWhiteSpace(datasetId))
			datasetId = csa.Dataset;

		if (!string.IsNullOrWhiteSpace(csa.ConnectionName)
			&& (SysConstants.DatasetKeyFormat.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName
			|| SysConstants.DatasetKeyAlternateFormat.FmtRes(csa.DataSource, datasetId) == csa.ConnectionName))
		{
			csa.ConnectionName = SysConstants.C_DefaultExConnectionName;
		}

		if (Rct.RegisterUniqueConnection(csa.ConnectionName, datasetId, source, ref csa))
		{
			csa.RegisterValidationState(connection.ConnectionString);
		}


		return csa;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invalidates the Rct for active static CsbAgents so that they can preform
	/// validation checkS.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void Invalidate()
	{
		if (Rct == null)
			return;

		Rct.Invalidate();

		// Tracer.Trace(typeof(RctManager), "Invalidate()", "New stamp: {0}", Stamp);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initiates loading of the RunningConnectionTable.
	/// Loads ServerExplorer, FlameRobin and Application connection configurations from
	/// OnSolutionLoad, package initialization, ServerExplorer or any other applicable
	/// activation event.
	/// </summary>
	/// <returns>
	/// True if the load succeeded or connections were already loaded else false if the
	/// rct is in a shutdown state.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool LoadConfiguredConnections()
	{
		return ResolveDeadlocks(true);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Ensures the rct is loaded, waiting or synchronoulsy loading if required.
	/// </summary>
	/// <returns>
	/// True if the load succeeded or connections were already loaded else false if the
	/// rct is in a shutdown state.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool EnsureLoaded()
	{
		return ResolveDeadlocks(false);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Resolves red zone deadlocks then loads the Rct if neccesary.
	/// </summary>
	/// <param name="asynchronous" >
	/// True is the caller is making an async request to ensure the rct is loaded or in
	/// a loaing state, else False if the loaded rct is required.
	/// </param>
	/// <returns>
	/// True if the load succeeded or connections were already loaded else false if the
	/// rct is in a shutdown state.
	/// </returns>
	/// <remarks>
	/// This is the entry point for loading configured connections and is a deadlock
	/// Bermuda Triangle :), because there are 3 basic events that can activate it.
	/// 1. ServerExplorer before the extension has fully completed async initialization.
	/// 2. UICONTEXT.SolutionExists also before async initialization.
	/// 3. Extension async initialization.
	/// 4. Any of 1, 2 or 3 after LoadConfiguredConnections() is already activated.
	/// The only way to control this is to create a managed awaitable or sync process to
	/// take over loading because switching between the ui and threadpool occurs
	/// several times during the load.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool ResolveDeadlocks(bool asynchronous)
	{
		// Tracer.Trace(typeof(RctManager), "LoadConfiguredConnections()", "Instance._Rct == null: {0}", Instance._Rct == null);

		// Shutdown state.
		if (_Instance == null)
			return false;

		// Rct has not been initialized.
		if (!Loading && Instance._Rct == null)
		{
			Instance._Rct = RunningConnectionTable.CreateInstance();
			Instance._Rct.LoadConfiguredConnections();

			if (asynchronous)
				return true;

			if (PersistentSettings.IncludeAppConnections && !ThreadHelper.CheckAccess())
				Instance._Rct.WaitForAsyncLoad();

			return true;
		}
		// Aynchronous request. Rct has been initialized or is loading, so just exit.
		else if (asynchronous)
		{
			return false;
		}
		// Synchronous request and loading - deadlock red zone.
		else if (Loading)
		{
			// If sync loads are still active we're safe and can wait because no switching
			// of threads takes place here.
			Instance._Rct.WaitForSyncLoad();

			// If we're not on the main thread or the async payload is not in a
			// pending state we can safely wait.
			if (!ThreadHelper.CheckAccess() || !Instance._Rct.AsyncPending)
			{
				Instance._Rct.WaitForAsyncLoad();
			}
			else
			{
				// Tracer.Trace(typeof(RctManager), "LoadConfiguredConnections()", "Cancelling Async and switching to sync");

				// We're on the main thread and async is pending. This is a deadlock red zone.
				// Send the async payload a cancel notification and execute the cancelled
				// async task on the main thread.
				Instance._Rct.LoadUnsafeConfiguredConnectionsSync();
			}
		}


		return Instance._Rct != null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Loads a project's App.Config Settings and EDM connections.
	/// </summary>
	/// <param name="probject">The <see cref="EnvDTE.Project"/>.</param>
	/// <remarks>
	/// This method only applies to projects late loaded or added after a solution has
	/// completed loading.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool LoadProjectConnections(object probject)
	{
		// Tracer.Trace(typeof(RctManager), "LoadProjectConnections()");

		if (!PersistentSettings.IncludeAppConnections || _Instance == null || Instance._Rct == null || ShutdownState)
			return false;

		return Instance._Rct.AsyncLoadConfiguredConnections(probject);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Modifies the name and connection information of Server Explorer's internal
	/// IVsDataExplorerConnectionManager connection entry.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private static bool ModifyServerExplorerConnection(IVsDataExplorerConnection explorerConnection,
		ref Csb csa, bool modifyExplorerConnection)
	{
		// Tracer.Trace(typeof(RctManager), "ModifyServerExplorerConnection()",
		//	"csa.ConnectionString: {0}, se.DecryptedConnectionString: {1}, modifyExplorerConnection: {2}.",
		//	csa.ConnectionString, explorerConnection.Connection.DecryptedConnectionString(), modifyExplorerConnection);

		Rct?.DisableEvents();

		// finally is unreliable.
		try
		{

			Csb seCsa = new(explorerConnection.Connection.DecryptedConnectionString(), false);
			string connectionKey = explorerConnection.GetConnectionKey(true);

			if (string.IsNullOrWhiteSpace(csa.ConnectionKey) || csa.ConnectionKey != connectionKey)
			{
				csa.ConnectionKey = connectionKey;
			}


			// Sanity check. Should already be done.
			// Perform a deep validation of the updated csa to ensure an update
			// is in fact required.
			bool updateRequired = !AbstractCsb.AreEquivalent(csa, seCsa, Csb.DescriberKeys, true);

			if (explorerConnection.DisplayName.Equals(csa.DatasetKey))
			{
				if (!updateRequired)
				{
					explorerConnection.ConnectionNode.Select();
					Rct?.EnableEvents();
					return false;
				}
			}
			else
			{
				explorerConnection.DisplayName = csa.DatasetKey;

				if (!updateRequired)
				{
					Rct?.EnableEvents();
					return true;
				}
			}

			if (!modifyExplorerConnection)
			{
				Rct?.EnableEvents();
				return false;
			}

			// An update is required...


			explorerConnection.Connection.SetConnectionString(csa.ConnectionString);
			explorerConnection.ConnectionNode.Select();

			Rct?.EnableEvents();

			return true;
		}
		catch
		{
			Rct?.EnableEvents();
			return false;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Prevents name mangling of the DataSource name. Returns a uniformly cased
	/// Server/DataSource name of the provided name.
	/// To prevent name mangling of server names, the case of the first connection
	/// discovered for a server name is the case that will be used for all future
	/// connections for that server.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string RegisterServer(string serverName, int port)
	{
		if (ShutdownState || _Instance == null || _Instance._Rct == null)
			return serverName;

		return _Instance._Rct.RegisterServer(serverName, port);
	}


	// ---------------------------------------------------------------------------------
	// Perform a single pass check to ensure an SE connection has had it's events
	// advised. This will occur if the SE was adding a new conection and we
	// registered it with the Rct before it was registered with Server Explorer.
	// ---------------------------------------------------------------------------------
	public static void RegisterUnadvisedConnection(IVsDataExplorerConnection explorerConnection)
	{
		if (_UnadvisedConnectionString == null)
			return;

		try
		{
			if (_UnadvisedConnectionString == explorerConnection.DecryptedConnectionString())
			{
				Rct.AdviseEvents(explorerConnection);
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			_UnadvisedConnectionString = null;
			throw;
		}

		_UnadvisedConnectionString = null;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Stores the ConnectionString of a connection added by the SE. The connection
	/// events cannot be linked until the SE has registered it in it's own tables.
	/// The stored value wil be retrieved as a single pass in IVsDataViewSupport
	/// and linked using <see cref="RegisterUnadvisedConnection"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void StoreUnadvisedConnection(string connectionString)
	{
		_UnadvisedConnectionString = connectionString;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Lists all entries in the RunningConnectionTable to debug out. The command will
	/// appear on any Sited node of the SE on debug builds.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void TraceRct()
	{
		try
		{
			string str = "\nRegistered Datasets: ";
			object datasetKey;

			foreach (DataRow row in Rct.InternalConnectionsTable.Rows)
			{
				datasetKey = row[SysConstants.C_KeyExDatasetKey];

				if (datasetKey == DBNull.Value || string.IsNullOrWhiteSpace((string)datasetKey))
					continue;
				str += "\n--------------------------------------------------------------------------------------";
				str += $"\nDATASETKEY: {(string)row[SysConstants.C_KeyExDatasetKey]}, ConnectionUrl: {(string)row[SysConstants.C_KeyExConnectionUrl]}";
				str += "\n\t------------------------------------------";
				str += "\n\t";

				foreach (DataColumn col in Rct.Databases.Columns)
				{
					if (col.ColumnName == SysConstants.C_KeyExDatasetKey || col.ColumnName == SysConstants.C_KeyExConnectionUrl || col.ColumnName == SysConstants.C_KeyExConnectionString)
					{
						continue;
					}
					str += $"{col.ColumnName}: {(row[col.ColumnName] == null ? "null" : row[col.ColumnName] == DBNull.Value ? "DBNull" : row[col.ColumnName].ToString())}, ";
				}
				str += "\n\t------------------------------------------";
				str += $"\n\tConnectionString: {(string)row[SysConstants.C_KeyExConnectionString]}";
			}

			str += "\n--------------------------------------------------------------------------------------";
			str += "\n--------------------------------------------------------------------------------------";
			str += $"\nDATASETKEY INDEXES:";

			foreach (KeyValuePair<string, int> pair in Rct)
			{
				str += $"\n\t{pair.Key}: {pair.Value}";
			}

			Tracer.Information(typeof(RctManager), "TraceRct()", "{0}", str);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates a connection string with the registration properties of it's unique
	/// registered connection, if it exists.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string UpdateConnectionFromRegistration(IDbConnection connection)
	{
		if (connection == null)
		{
			ArgumentNullException ex = new(nameof(connection));
			Diag.Dug(ex);
			throw ex;
		}

		string connectionString = connection.ConnectionString;

		if (ShutdownState)
			return connectionString;


		Csb csa = new(connectionString);


		if (!Rct.TryGetHybridRowValue(csa.SafeDatasetMoniker, out DataRow row))
			return connectionString;

		object colObject = row[SysConstants.C_KeyExDatasetKey];
		if (!Cmd.IsNullValue(colObject))
			csa.DatasetKey = (string)colObject;

		colObject = row[SysConstants.C_KeyExConnectionKey];
		if (!Cmd.IsNullValue(colObject))
			csa.ConnectionKey = (string)colObject;

		colObject = row[SysConstants.C_KeyExConnectionName];
		if (!Cmd.IsNullValue(colObject))
			csa.ConnectionName = (string)colObject;

		if (!string.IsNullOrEmpty(csa.ConnectionName))
			colObject = null;
		else
			colObject = row[SysConstants.C_KeyExDatasetId];
		if (!Cmd.IsNullValue(colObject))
			csa.DatasetId = (string)colObject;

		colObject = row[SysConstants.C_KeyExConnectionSource];
		if (!Cmd.IsNullValue(colObject))
			csa.ConnectionSource = (EnConnectionSource)(int)colObject;

		return csa.ConnectionString;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates an existing registered connection using the provided connection string.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static Csb UpdateOrRegisterConnection(string connectionString,
		EnConnectionSource source, bool addExplorerConnection, bool modifyExplorerConnection)
	{
		if (Rct == null)
			return null;


		Csb csa = Rct.UpdateRegisteredConnection(connectionString, source, false);


		// If it's null force a creation.
		csa ??= EnsureInstance(connectionString, source);

		// Update the SE.
		UpdateServerExplorer(ref csa, addExplorerConnection, modifyExplorerConnection);

		return csa;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the Server Explorer tables when a connection has been added or modified.
	/// </summary>
	/// <remarks>
	/// There is a caveat to a requested addExplorerConnection. If the SE is adding a
	/// connection, we will be unable to update it here because it will not yet exist
	/// in the SE connection table.
	/// In that case the <see cref="RctManager"/> will tag it for updating using
	/// <see cref="StoreUnadvisedConnection"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	private static bool UpdateServerExplorer(ref Csb csa,
		bool addExplorerConnection, bool modifyExplorerConnection)
	{
		// Tracer.Trace(typeof(RctManager), "UpdateServerExplorer()", "csa.ConnectionString: {0}, addServerExplorerConnection: {1}, modifyExplorerConnection: {2}.", csa.ConnectionString, addExplorerConnection, modifyExplorerConnection);

		csa.ConnectionSource = EnConnectionSource.ServerExplorer;

		IVsDataExplorerConnectionManager manager = ApcManager.ExplorerConnectionManager;

		(_, IVsDataExplorerConnection explorerConnection) = manager.SearchExplorerConnectionEntry(csa.ConnectionString, false);

		if (explorerConnection != null)
			return ModifyServerExplorerConnection(explorerConnection, ref csa, modifyExplorerConnection);

		if (!addExplorerConnection)
			return false;


		csa.ConnectionKey = csa.DatasetKey;

		Rct.DisableEvents();

		explorerConnection = manager.AddConnection(csa.DatasetKey, new(SystemData.ProviderGuid), csa.ConnectionString, false);

		Rct.AdviseEvents(explorerConnection);

		explorerConnection.ConnectionNode.Select();

		try
		{
			explorerConnection.Connection.EnsureConnected();
			explorerConnection.ConnectionNode.Refresh();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		Rct.EnableEvents();

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Validates IVsDataExplorerConnection to establish if it's stored DatasetKey
	/// matches the DisplayName (proposed ConnectionName) and updates the Server
	/// Explorer internal table if it doesn't. Also checks if a wizard spawned from a
	/// UIHierarchyMarshaler has corrupted the root node.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void ValidateAndUpdateExplorerConnectionRename(IVsDataExplorerConnection explorerConnection,
		string proposedConnectionName, Csb csa)
	{
		if (Rct == null)
			return;


		// Wizards spawned from a UIHierarchyMarshaler will likely misbehave and corrupt
		// our connection node if it uses a 'New Connection' dialog.
		// We tag the connection string in our IVsDataConnectionProperties and
		// IVsDataConnectionUIProperties implementations with "edmx" and "edmu"
		// respectively, to identify it as a connection created or selected in a dialog
		// spawned from UIHierarchyMarshaler or DataSources ToolWindow.
		// If the connection doesn't exist in the SE we will have created it on the fly
		// to prevent the wizard throwing an exception.
		// Whether created or selected, the wizard will misbehave and rename the
		// connection, and we lose the DatasetId if it existed.
		// A node changed event is raised which we pick up here, and we perform a
		// reverse repair.

		if (csa.ContainsKey(SysConstants.C_KeyExEdmx) || csa.ContainsKey(SysConstants.C_KeyExEdmu))
		{
			if (csa.ContainsKey(SysConstants.C_KeyExEdmu))
			{
				// Clear out any UIHierarchyMarshaler or DataSources ToolWindow ConnectionString identifiers.
				csa.Remove(SysConstants.C_KeyExEdmu);
				csa.Remove(SysConstants.C_KeyExEdmx);

				// Tracer.Trace(typeof(RctManager), "ValidateAndUpdateExplorerConnectionRename()", "\nEDMU repairString: {0}.", csa.ConnectionString);

				UpdateOrRegisterConnection(csa.ConnectionString, EnConnectionSource.ServerExplorer, false, true);
			}
			else
			{
				string storedConnectionString = GetConnectionString(csa.SafeDatasetMoniker);

				if (storedConnectionString == null)
				{
					ApplicationException ex = new($"ExplorerConnection rename failed. Possibly corrupted by the EDMX wizard. Proposed connection name: {proposedConnectionName}, Explorer connection string: {csa.ConnectionString}.");
					Diag.Dug(ex);
					throw ex;
				}

				// Tracer.Trace(typeof(RctManager), "ValidateAndUpdateExplorerConnectionRename()", "EDMX repairString: {0}.", storedConnectionString);

				UpdateOrRegisterConnection(storedConnectionString, EnConnectionSource.ServerExplorer, false, true);
			}

			return;

		}

		if (proposedConnectionName == csa.DatasetKey)
			return;


		// Tracer.Trace(typeof(RctManager), "ValidateAndUpdateExplorerConnectionRename()", "proposedConnectionName: {0}, connectionString: {1}.", proposedConnectionName, csa.ConnectionString);

		string msg;
		string caption;

		// Sanity check.
		if (proposedConnectionName.StartsWith(NativeDb.Scheme))
		{
			if (csa.ContainsKey(SysConstants.C_DefaultExDatasetKey))
			{
				Rct.DisableEvents();
				explorerConnection.DisplayName = csa.DatasetKey;
				Rct.EnableEvents();

				caption = ControlsResources.RctManager_CaptionInvalidConnectionName;
				msg = ControlsResources.RctManager_TextInvalidConnectionName.FmtRes(NativeDb.Scheme, proposedConnectionName);
				MessageCtl.ShowEx(msg, caption, MessageBoxButtons.OK);

				return;
			}

			proposedConnectionName = null;
		}


		string connectionUrl = csa.SafeDatasetMoniker;
		string proposedDatasetId = csa.DatasetId;
		string dataSource = csa.DataSource;
		string dataset = csa.Dataset;


		// Check whether the connection name will change.
		Rct.GenerateUniqueDatasetKey(EnConnectionSource.ServerExplorer, proposedConnectionName, proposedDatasetId, dataSource, dataset,
			connectionUrl, connectionUrl, out _, out _, out string uniqueDatasetKey,
			out string uniqueConnectionName, out string uniqueDatasetId);

		// Tracer.Trace(typeof(RctManager), "ValidateAndUpdateExplorerConnectionRename()", "GenerateUniqueDatasetKey results: proposedConnectionName: {0}, proposedDatasetId: {1}, dataSource: {2}, dataset: {3}, uniqueDatasetKey: {4}, uniqueConnectionName: {5}, uniqueDatasetId: {6}.",
		//	proposedConnectionName, proposedDatasetId, dataSource, dataset, uniqueDatasetKey ?? "Null", uniqueConnectionName ?? "Null", uniqueDatasetId ?? "Null");

		if (!string.IsNullOrEmpty(uniqueConnectionName))
		{
			caption = ControlsResources.RctManager_CaptionConnectionNameConflict;
			msg = ControlsResources.RctManager_TextConnectionNameConflictLong.FmtRes(proposedConnectionName, uniqueConnectionName);

			if (MessageCtl.ShowEx(msg, caption, MessageBoxButtons.YesNo) == DialogResult.No)
			{
				Rct.DisableEvents();
				explorerConnection.DisplayName = GetDatasetKey(connectionUrl);
				Rct.EnableEvents();

				return;
			}
		}


		// At this point we're good to go.
		csa.DatasetKey = uniqueDatasetKey;

		if (uniqueConnectionName == null && proposedConnectionName == null)
			csa.Remove(SysConstants.C_KeyExConnectionName);
		else if (!string.IsNullOrEmpty(uniqueConnectionName))
			csa.ConnectionName = uniqueConnectionName;
		else
			csa.ConnectionName = proposedConnectionName;

		if (uniqueDatasetId == null && proposedDatasetId == null)
			csa.Remove(SysConstants.C_KeyExDatasetId);
		else if (!string.IsNullOrEmpty(uniqueDatasetId))
			csa.DatasetId = uniqueDatasetId;
		else
			csa.DatasetId = proposedDatasetId;


		UpdateOrRegisterConnection(csa.ConnectionString, EnConnectionSource.ServerExplorer, false, true);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Validates an IVsDataConnectionProperties Site to ensure it contains no
	/// unauthorised, invalid or redundant registration keys.
	/// This is to ensure redundant properties do not appear in future connection
	/// dialogs and that settings are valid for both the SE and BlackbirdSql connection
	/// tables.
	/// </summary>
	/// <returns>
	/// A tuple where... 
	/// Item1 (rSuccess): true if validation was successful else false if add or modify
	/// should be cancelled.
	/// Item2 (rAddInternally): true if connectionUrl (connection) has
	/// changed and requires adding a new connection internally.
	/// Item3 (rModifyInternally): true if connectionUrl changed and will now
	/// require internally modifying
	/// another connection.
	/// </returns>
	/// <param name="site">
	/// The IVsDataConnectionProperties Site (usually the Site of an
	/// IVsDataConnectionUIControl control.
	/// </param>
	/// <param name="source">
	/// The source requesting the validation.
	/// </param>
	/// <param name="serverExplorerInsertMode">
	/// Boolean indicating wehther or not a connection is being added or modified.
	/// </param>
	/// <param name="storedConnectionString">
	/// The original ConnectionString before any editing of the Site took place else
	/// null on a new connection.
	/// </param>
	// ---------------------------------------------------------------------------------
	public static (bool, bool, bool) ValidateSiteProperties(IVsDataConnectionProperties site, EnConnectionSource connectionSource,
		bool serverExplorerInsertMode, string storedConnectionString)
	{
		bool rSuccess = true;
		bool rAddInternally = false;
		bool rModifyInternally = false;

		if (Rct == null)
			return (rSuccess, rAddInternally, rModifyInternally);

		string storedConnectionUrl = null;

		if (storedConnectionString != null)
			storedConnectionUrl = Csb.CreateConnectionUrl(storedConnectionString);


		string proposedConnectionName = site.ContainsKey(SysConstants.C_KeyExConnectionName)
			? (string)site[SysConstants.C_KeyExConnectionName] : null;
		if (Cmd.IsNullValueOrEmpty(proposedConnectionName))
			proposedConnectionName = null;

		string msg = null;
		string caption = null;

		if (!string.IsNullOrWhiteSpace(proposedConnectionName) && proposedConnectionName.StartsWith(NativeDb.Scheme))
		{
			caption = ControlsResources.RctManager_CaptionInvalidConnectionName;
			msg = ControlsResources.RctManager_TextInvalidConnectionName.FmtRes(NativeDb.Scheme, proposedConnectionName);

			MessageCtl.ShowEx(msg, caption, MessageBoxButtons.OK);

			rSuccess = false;
			return (rSuccess, rAddInternally, rModifyInternally);
		}

		string proposedDatasetId = site.ContainsKey(SysConstants.C_KeyExDatasetId)
			? (string)site[SysConstants.C_KeyExDatasetId] : null;
		if (Cmd.IsNullValueOrEmpty(proposedDatasetId))
			proposedDatasetId = null;

		string dataSource = (string)site[SysConstants.C_KeyDataSource];
		string database = (string)site[SysConstants.C_KeyDatabase];
		string dataset = (string.IsNullOrWhiteSpace(database) ? "" : Path.GetFileNameWithoutExtension(database));

		string connectionUrl = (site as IBDataConnectionProperties).Csa.DatasetMoniker;

		// Tracer.Trace(typeof(RctManager), "ValidateSiteProperties()", "proposedConnectionName: {0}, proposedDatasetId: {1}, dataSource: {2}, dataset: {3}.",
		//	proposedConnectionName, proposedDatasetId, dataSource, dataset);

		// Validate the proposed names.
		bool createNew = Rct.GenerateUniqueDatasetKey(connectionSource, proposedConnectionName, proposedDatasetId,
			dataSource, dataset, connectionUrl, storedConnectionUrl, out EnConnectionSource storedConnectionSource,
			out string changedTargetDatasetKey, out string uniqueDatasetKey, out string uniqueConnectionName,
			out string uniqueDatasetId);

		// Tracer.Trace(typeof(RctManager), "ValidateSiteProperties()", "GenerateUniqueDatasetKey results: proposedConnectionName: {0}, proposedDatasetId: {1}, dataSource: {2}, dataset: {3}, createnew: {4}, storedConnectionSource: {5}, changedTargetDatasetKey: {6}, uniqueDatasetKey : {7}, uniqueConnectionName: {8}, uniqueDatasetId: {9}.",
		//	proposedConnectionName, proposedDatasetId, dataSource, dataset, createNew, storedConnectionSource,
		//	changedTargetDatasetKey ?? "Null", uniqueDatasetKey ?? "Null",
		//	uniqueConnectionName == null ? "Null" : (uniqueConnectionName == string.Empty ? "string.Empty" : uniqueConnectionName),
		//	uniqueDatasetId == null ? "Null" : (uniqueDatasetId == string.Empty ? "string.Empty" : uniqueDatasetId));

		// If we're in the EDM and the stored connection source is not ServerExplorer we have to create it in the SE to get past the EDM bug.
		// Also, if we're in the SE and the stored connection source is not ServerExplorer we have to create it in the SE.
		if (!createNew && storedConnectionSource != EnConnectionSource.ServerExplorer
			&& (connectionSource == EnConnectionSource.ServerExplorer || connectionSource == EnConnectionSource.EntityDataModel))
		{
			createNew = true;
		}



		#region ---------------- User Prompt Section -----------------



		if (!string.IsNullOrEmpty(uniqueConnectionName) && !string.IsNullOrEmpty(proposedConnectionName))
		{
			// Handle all cases where there's a connection name conflict.

			// The settings provided will create a new SE connection with a connection name conflict.
			if (createNew && !serverExplorerInsertMode && (connectionSource == EnConnectionSource.ServerExplorer
				|| connectionSource == EnConnectionSource.EntityDataModel))
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionNameConflict;
				msg = ControlsResources.RctManager_TextNewSEConnectionNameConflict.FmtRes(proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will create a new Session connection as well as a new SE connection with a connection name conflict.
			else if (createNew && !serverExplorerInsertMode)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionNameConflict;
				msg = ControlsResources.RctManager_TextNewConnectionNameConflict.FmtRes(proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will switch connections with a connection name conflict.
			else if (changedTargetDatasetKey != null)
			{
				caption = ControlsResources.RctManager_CaptionConnectionChangeNameConflict;
				msg = ControlsResources.RctManager_TextConnectionChangeNameConflict.FmtRes(changedTargetDatasetKey, proposedConnectionName, uniqueConnectionName);
			}
			// The settings provided will cause a connection name conflict.
			else
			{
				caption = ControlsResources.RctManager_CaptionConnectionNameConflict;
				msg = ControlsResources.RctManager_TextConnectionNameConflict.FmtRes(proposedConnectionName, uniqueConnectionName);
			}
		}
		else if (!string.IsNullOrEmpty(uniqueDatasetId) && !string.IsNullOrEmpty(proposedDatasetId))
		{
			// Handle all cases where there's a DatasetId conflict.

			// The settings provided will create a new SE connection with a DatasetId conflict.
			if (createNew && !serverExplorerInsertMode && (connectionSource == EnConnectionSource.ServerExplorer
				|| connectionSource == EnConnectionSource.EntityDataModel))
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextNewSEConnectionDatabaseNameConflict.FmtRes(proposedDatasetId, uniqueDatasetId);
			}
			// The settings provided will create a new Session connection as well as a new SE connection with a DatasetId conflict.
			else if (createNew && !serverExplorerInsertMode)
			{
				caption = ControlsResources.RctManager_CaptionNewConnectionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextNewConnectionDatabaseNameConflict.FmtRes(proposedDatasetId, uniqueDatasetId);
			}
			// The settings provided will switch connections with a DatasetId conflict.
			else if (changedTargetDatasetKey != null)
			{
				caption = ControlsResources.RctManager_CaptionConnectionChangeDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextConnectionChangeDatabaseNameConflict.FmtRes(changedTargetDatasetKey, proposedDatasetId, uniqueDatasetId);
			}
			// The settings provided will cause a DatasetId conflict.
			else
			{
				caption = ControlsResources.RctManager_CaptionDatabaseNameConflict;
				msg = ControlsResources.RctManager_TextDatabaseNameConflict.FmtRes(proposedDatasetId, uniqueDatasetId);
			}
		}
		// Handle all cases where there is no conflict.
		// The settings provided will create a new SE connection.
		else if (createNew && !serverExplorerInsertMode &&
			(connectionSource == EnConnectionSource.ServerExplorer || connectionSource == EnConnectionSource.EntityDataModel))
		{
			caption = ControlsResources.RctManager_CaptionNewConnection;
			msg = ControlsResources.RctManager_TextNewSEConnection;
		}
		// The settings provided will create a new Session connection as well as a new SE connection.
		else if (createNew && !serverExplorerInsertMode)
		{
			caption = ControlsResources.RctManager_CaptionNewConnection;
			msg = ControlsResources.RctManager_TextNewConnection;
		}
		// The settings provided will switch connections.
		else if (changedTargetDatasetKey != null)
		{
			// The target connection will change.
			caption = ControlsResources.RctManager_CaptionConnectionChanged;
			msg = ControlsResources.RctManager_TextConnectionChanged.FmtRes(changedTargetDatasetKey);
		}
		// If it's an SE connection and it's not the SE modifying warn if the connection name is being modified.
		else if (connectionSource != EnConnectionSource.ServerExplorer
			&& storedConnectionSource == EnConnectionSource.ServerExplorer &&
			(uniqueConnectionName == string.Empty || uniqueDatasetId == string.Empty))
		{
			// The target connection name will change.
			caption = ControlsResources.RctManager_CaptionConnectionNameChange;
			msg = ControlsResources.RctManager_TextSEConnectionNameChange;
		}


		if (msg != null && MessageCtl.ShowEx(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
			return (false, false, false);



		#endregion ---------------- User Prompt Section -----------------


		if (uniqueConnectionName == null )

		// At this point we're good to go.

		// Clean up the site properties.
		if (!site.ContainsKey(SysConstants.C_KeyExDatasetKey)
			|| (string)site[SysConstants.C_KeyExDatasetKey] != uniqueDatasetKey)
		{
			site[SysConstants.C_KeyExDatasetKey] = uniqueDatasetKey;
		}

		if (uniqueConnectionName == null && proposedConnectionName == null)
			site.Remove(SysConstants.C_KeyExConnectionName);
		else if (!string.IsNullOrEmpty(uniqueConnectionName))
			site[SysConstants.C_KeyExConnectionName] = uniqueConnectionName;
		else
			site[SysConstants.C_KeyExConnectionName] = proposedConnectionName;

		if (uniqueDatasetId == null && proposedDatasetId == null)
			site.Remove(SysConstants.C_KeyExDatasetId);
		else if (!string.IsNullOrEmpty(uniqueDatasetId))
			site[SysConstants.C_KeyExDatasetId] = uniqueDatasetId;
		else
			site[SysConstants.C_KeyExDatasetId] = proposedDatasetId;


		// Establish the connection owner.
		// if the explorer connection exists or if it's the source (or EntityDataModel) it automatically is the owner.
		string connectionKey = site.FindConnectionKey();

		if (connectionKey == null && (connectionSource == EnConnectionSource.ServerExplorer
			|| connectionSource == EnConnectionSource.EntityDataModel))
		{
			connectionKey = uniqueDatasetKey;
		}

		// Tracer.Trace(typeof(RctManager), "ValidateSiteProperties()", "Retrieved ConnectionKey: {0}.", connectionKey ?? "Null");

		if (connectionKey != null)
		{
			// If the SE connection exists then it is the owner and we have to set the SE ConnectionKey
			// and make the SE the owner.
			string strValue = ((string)site[SysConstants.C_KeyExConnectionKey]).Trim();

			if (strValue != connectionKey)
				site[SysConstants.C_KeyExConnectionKey] = connectionKey;

			EnConnectionSource siteConnectionSource = (EnConnectionSource)site[SysConstants.C_KeyExConnectionSource];

			if (siteConnectionSource != EnConnectionSource.ServerExplorer)
				site[SysConstants.C_KeyExConnectionSource] = EnConnectionSource.ServerExplorer;
		}

		if (!serverExplorerInsertMode && createNew)
			rAddInternally = true;

		rModifyInternally = changedTargetDatasetKey != null || (!rAddInternally
			&& connectionSource != EnConnectionSource.ServerExplorer && connectionSource != EnConnectionSource.EntityDataModel);

		// Tag the site as being updated by the edmx wizard if it's not being done internally, which will
		// use IVsDataConnectionUIProperties.Parse().
		// We do this because the wizard will attempt to rename the connection and we'll pick it up in
		// the rct on an IVsDataExplorerConnection.NodeChanged event, and reverse the rename and drop the tag.
		if (connectionSource == EnConnectionSource.EntityDataModel && !rAddInternally && !rModifyInternally)
			site["edmu"] = true;


		rSuccess = true;

		return (rSuccess, rAddInternally, rModifyInternally);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Verifies wether or not a source has update rights over a peristent and/or
	/// volatile stored connection, given the source and the owning source of the
	/// connecton.
	/// <summary>
	// ---------------------------------------------------------------------------------
	public static bool VerifyUpdateRights(EnConnectionSource updater,
		EnConnectionSource owner)
	{
		return AbstractRunningConnectionTable.VerifyUpdateRights(updater, owner);
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handling - RctManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Insurance for removing an SE connection whose OnExplorerConnectionNodeRemoving event may not be fired.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void OnExplorerConnectionClose(object sender, IVsDataExplorerConnection explorerConnection)
	{
		// Tracer.Trace(GetType(), "OnNodeRemoving()", "Node: {0}.", e.Node != null ? e.Node.ToString() : "null");
		if (ShutdownState || _Instance == null || _Instance._Rct == null
			|| _Instance._Rct.Loading || explorerConnection.ConnectionNode == null)
		{
			return;
		}

		DataExplorerNodeEventArgs eventArgs = new(explorerConnection.ConnectionNode);
		Rct.OnExplorerConnectionNodeRemoving(sender, eventArgs);

		IVsDataConnection site = explorerConnection.Connection;

		site?.DisposeLinkageParser();
	}


	#endregion Event handling

}
