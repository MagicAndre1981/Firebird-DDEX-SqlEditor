﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalizedDisplayNameAttribute

using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Core.Ctl.ComponentModel;

namespace BlackbirdSql.Shared.Ctl.ComponentModel;

public sealed class GlobalizedCategoryAttribute : AbstractGlobalizedCategoryAttribute
{
	public override System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;


	public GlobalizedCategoryAttribute(string resourceName) : base(resourceName)
	{
	}
}
