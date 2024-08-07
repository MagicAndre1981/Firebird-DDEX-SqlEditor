﻿#region Assembly Microsoft.SqlServer.DataStorage, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.DataStorage.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Shared.Interfaces;

public interface IBsStorageView : IDisposable
{
	long EnsureRowsInBuf(long startRow, long totalRowCount);

	object GetCellData(long i64Row, int iCol);

	string GetCellDataAsString(long iRow, int iCol);

	void DeleteRow(long iRow);

	long RowCount { get; }

	IBsColumnInfo GetColumnInfo(int iCol);

	int ColumnCount { get; }

	bool IsStorageClosed();
}
