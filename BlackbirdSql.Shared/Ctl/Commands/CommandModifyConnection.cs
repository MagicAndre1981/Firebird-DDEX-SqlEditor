﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorChangeConnectionCommand

using System;
using System.Data;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandModifyConnection : AbstractCommand
{

	public CommandModifyConnection()

	{
	}

	public CommandModifyConnection(IBsTabbedEditorPane tabbedEditor) : base(tabbedEditor)
	{
	}



	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked && CachedStrategy != null)
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked)
			return VSConstants.S_OK;

		if (!RequestDeactivateQuery(Resources.MsgQueryAbort_UncommittedTransactions))
			return VSConstants.S_OK;

		CachedQryMgr.ModifyConnection();

		return VSConstants.S_OK;
	}

}
