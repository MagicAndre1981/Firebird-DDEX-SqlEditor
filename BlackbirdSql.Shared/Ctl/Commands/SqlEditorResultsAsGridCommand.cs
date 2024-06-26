﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Interfaces;


namespace BlackbirdSql.Shared.Ctl.Commands;

public class SqlEditorResultsAsGridCommand : AbstractSqlEditorCommand
{
	public SqlEditorResultsAsGridCommand()
	{
	}

	public SqlEditorResultsAsGridCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (AuxDocData == null)
			return VSConstants.S_OK;

		if (!StoredIsExecuting)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		if (StoredAuxDocData.SqlOutputMode == EnSqlOutputMode.ToGrid)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (AuxDocData != null)
			StoredAuxDocData.SqlOutputMode = EnSqlOutputMode.ToGrid;

		return VSConstants.S_OK;
	}
}
