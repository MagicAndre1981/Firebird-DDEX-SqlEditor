﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Runtime.InteropServices;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Ctl.Commands;

public class SqlEditorSqlDatabaseCommand : AbstractSqlEditorCommand
{
	/// <summary>
	/// Records the last moniker created so that we can do a fast equivalency comparison
	/// on the connection and use this static for the DatasetKey if they are equivalent.
	/// This avoids repeatedly creating a new MonikerAgent and going through the
	/// registration process each time.
	/// </summary>
	private static CsbAgent _Csa = null;

	public SqlEditorSqlDatabaseCommand()
	{
		// Diag.Trace();
	}

	public SqlEditorSqlDatabaseCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
		// Diag.Trace();
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		if (!IsEditorExecutingOrDebugging())
		{
			// Diag.Trace("SqlEditorSqlDatabaseCommand:HandleQueryStatus enabled");
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
		if (auxiliaryDocDataForEditor == null)
		{
			Exception ex = new("AuxiliaryDocData NOT FOUND");
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}

		QueryManager qryMgr = auxiliaryDocDataForEditor.QryMgr;

		if (qryMgr == null)
		{
			ArgumentNullException ex = new("QryMgr is null");
			Diag.Dug(ex);
			return VSConstants.S_OK;
		}

		if (pvaIn != IntPtr.Zero)
		{
			string selectedDatasetKey = (string)Marshal.GetObjectForNativeVariant(pvaIn);
			// Tracer.Trace(GetType(), "HandleExec()", "pvaIn selectedDatasetKey: {0}", selectedDatasetKey);

			try
			{
				SetDatasetKeyDisplayMember(auxiliaryDocDataForEditor, selectedDatasetKey);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				return VSConstants.S_OK;
			}
		}
		else if (pvaOut != IntPtr.Zero)
		{
			IDbConnection connection = qryMgr.ConnectionStrategy.Connection;
			object objDatasetKey;

			if (connection == null /* || connection.State != ConnectionState.Open */ || string.IsNullOrEmpty(connection.Database))
			{
				// Tracer.Trace(GetType(), "HandleExec()", "pvaOut Current selection is empty.");
				objDatasetKey = string.Empty;
			}
			else
			{
				if (_Csa == null || !_Csa.Equals(connection))
				{
					_Csa = new(connection);
					_Csa.RegisterDataset();
				}

				objDatasetKey = _Csa.DatasetKey;
				// Tracer.Trace(GetType(), "HandleExec()", "pvaOut Current selection objDatasetKey: {0}.", objDatasetKey);
			}
			Marshal.GetNativeVariantForObject(objDatasetKey, pvaOut);
		}


		return VSConstants.S_OK;
	}

	private void SetDatasetKeyDisplayMember(AuxiliaryDocData docData, string selectedDatasetKey)
	{
		IVsUserData userData = docData.GetIVsUserData();

		if (userData == null)
		{
			ArgumentNullException ex = new("IVsUserData is null");
			Diag.Dug(ex);
			throw ex;
		}

		CsbAgent csa = (CsbAgent)docData.GetUserDataCsb();

		if (csa != null && csa.DatasetKey == null)
			csa.RegisterDataset();

		string connectionString;

		if (csa == null || csa.DatasetKey != selectedDatasetKey)
		{
			try
			{
				connectionString = CsbAgent.GetDatasetConnectionString(selectedDatasetKey);
				if (connectionString == null)
					throw new ArgumentException($"DatasetKey {selectedDatasetKey} is not registered.");

				csa = new(connectionString);
				csa.RegisterDataset();
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

		connectionString = csa.ConnectionString;

		if (string.IsNullOrWhiteSpace(connectionString))
		{
			ArgumentNullException ex = new("ConnectionString is null");
			Diag.Dug(ex);
			throw ex;
		}


		AbstractConnectionStrategy connectionStrategy = docData.QryMgr.ConnectionStrategy;

		connectionStrategy.SetDatasetKeyOnConnection(selectedDatasetKey, csa);
		// IDbConnection connection = connectionStrategy.Connection;

		// if (connection != null && connection.State == ConnectionState.Open)
		// {

		Guid clsid = LibraryData.CLSID_PropertyDatabaseConnectionChanged;
		Core.Native.ThrowOnFailure(userData.SetData(ref clsid, connectionString), (string)null);

		Guid clsid2 = new(LibraryData.SqlEditorConnectionStringGuid);
		Core.Native.ThrowOnFailure(userData.SetData(ref clsid2, (object)csa), (string)null);

		_Csa = csa;
		// }
	}
}