﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core.Ctl.Enums;


namespace BlackbirdSql.Common.Ctl.Commands;

public class SqlEditorResultsAsTextCommand : AbstractSqlEditorCommand
{
	public SqlEditorResultsAsTextCommand()
	{
	}

	public SqlEditorResultsAsTextCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
		if (auxiliaryDocDataForEditor != null)
		{
			if (!IsEditorExecutingOrDebugging())
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
			}

			if (auxiliaryDocDataForEditor.SqlOutputMode == EnSqlOutputMode.ToText)
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;
			}
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AuxiliaryDocData auxDocData = GetAuxiliaryDocDataForEditor();
		if (auxDocData != null)
		{
			auxDocData.SqlOutputMode = EnSqlOutputMode.ToText;
		}

		return VSConstants.S_OK;
	}
}
