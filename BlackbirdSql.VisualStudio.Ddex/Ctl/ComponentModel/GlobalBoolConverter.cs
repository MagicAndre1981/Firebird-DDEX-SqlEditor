// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalBoolConverter

using System.Security.Permissions;
using BlackbirdSql.Core.Ctl.ComponentModel;


namespace BlackbirdSql.VisualStudio.Ddex.Ctl.ComponentModel;

/// <summary>
/// Localized dll globalized True/False bool type convertor.
/// Resources are in AbstractBoolConverter AttributeResources.resx.
/// </summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class GlobalBoolConverter : AbstractBoolConverter
{
	public override string LiteralFalseResource => "GlobalizedLiteralFalse";
	public override string LiteralTrueResource => "GlobalizedLiteralTrue";

}
