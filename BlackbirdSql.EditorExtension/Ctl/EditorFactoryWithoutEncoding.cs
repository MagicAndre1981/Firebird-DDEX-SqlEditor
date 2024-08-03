﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorFactoryWithoutEncoding

using System.Runtime.InteropServices;
using BlackbirdSql.Core;
using Microsoft.VisualStudio.Shell;



namespace BlackbirdSql.EditorExtension.Ctl;

[Guid(SystemData.C_EditorFactoryGuid)]
[ProvideMenuResource("Menus.ctmenu", 1)]


public sealed class EditorFactoryWithoutEncoding : AbstractEditorFactory
{

	public EditorFactoryWithoutEncoding() : base(withEncoding: false)
	{
	}

}
