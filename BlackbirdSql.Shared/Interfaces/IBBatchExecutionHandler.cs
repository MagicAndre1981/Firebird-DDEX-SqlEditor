﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


using System.Data;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Sys.Interfaces;

namespace BlackbirdSql.Shared.Interfaces;


public interface IBBatchExecutionHandler
{
	void Register(IDbConnection conn, IBsNativeDbStatementWrapper sqlStatement, QESQLBatch batch);

	void UnRegister(IDbConnection conn, IBsNativeDbStatementWrapper sqlStatement, QESQLBatch batch);
}
