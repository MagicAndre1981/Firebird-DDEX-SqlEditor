// Microsoft.VisualStudio.Data.Tools.Design, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.ISqlServerObjectExplorerService

using System.Runtime.InteropServices;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Core.Interfaces;

[ComImport]
[Guid(SystemData.C_DesignerExplorerServicesGuid)]
public interface IBsDesignerExplorerServices
{
	void CloneQuery(string datasetKey, string baseName, string initialScript);
	void NewQuery(string datasetKey, string baseName, string initialScript);

	void ViewCode(IVsDataExplorerNode node, EnModelTargetType targetType);

	/*
	void Add(SqlConnectionStringBuilder connection, SqlServerObjectType objectType);

	void Browse(SqlConnectionStringBuilder connection);

	void Delete(SqlConnectionStringBuilder connection, SqlServerObjectType objectType, params string[] name);

	void Execute(SqlConnectionStringBuilder connection, params string[] name);

	void OpenQuery(SqlConnectionStringBuilder connection);

	void ViewData(SqlConnectionStringBuilder connection, SqlServerObjectType objectType, params string[] name);

	void ViewDesigner(SqlConnectionStringBuilder connection, SqlServerObjectType objectType, params string[] name);
	*/
}
