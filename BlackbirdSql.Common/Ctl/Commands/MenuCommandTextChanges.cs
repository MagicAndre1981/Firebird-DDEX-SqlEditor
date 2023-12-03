﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

using BlackbirdSql.Common.Ctl.Interfaces;


namespace BlackbirdSql.Common.Ctl.Commands;

[ComVisible(false)]
public class MenuCommandTextChanges(EventHandler handler, CommandID command)
	: MenuCommand(handler, command), IBMenuCommandTextChanges
{
	private string text;

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}
}
