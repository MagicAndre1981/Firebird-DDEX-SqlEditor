// Microsoft.SqlServer.DlgGrid, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.IDlgGridControl

using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Events;

// using Microsoft.SqlServer.Management.UI.Grid;




namespace BlackbirdSql.Shared.Interfaces;


// [CLSCompliant(false)]
public interface IBsDlgGridControl : IBsGridControl
{
	int SelectedRow { get; set; }

	int[] SelectedRows { get; set; }

	int RowCount { get; }

	IBsDlgStorage DlgStorage { get; set; }

	event FillControlWithDataEventHandler FillControlWithDataEvent;

	event SetCellDataFromControlEventHandler SetCellDataFromControlEvent;

	void AddRow(GridCellCollection row);

	void InsertRow(int nRowIndex, GridCellCollection row);

	void DeleteRow(int nRowIndex);

	GridCellCollection GetRowInfo(int nRowNum);

	void SetRowInfo(int nRowNum, GridCellCollection row);

	GridCell GetCellInfo(int nRowNum, int nColNum);

	void SetCellInfo(int nRowNum, int nColNum, GridCell cell);

	void GetSelectedCell(out int nRowIndex, out int nColIndex);

	void SetSelectedCell(int nRowIndex, int nColIndex);

	bool IsCellDirty(int nRowIndex, int nColIndex);

	void SetCellDirtyState(int nRowIndex, int nColIndex, bool bDirty);

	bool IsRowDirty(int nRowIndex);
}
