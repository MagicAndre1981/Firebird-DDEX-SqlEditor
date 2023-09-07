﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using BlackbirdSql.Common.Controls.Enums;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.Grid;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Core.Enums;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

using Constants = Microsoft.VisualStudio.OLE.Interop.Constants;


namespace BlackbirdSql.Common.Controls.ResultsPane
{
	public class GridResultsPanel : AbstractGridResultsPanel, IOleCommandTarget
	{
		private ResultSetAndGridContainerCollection m_gridContainers = new ResultSetAndGridContainerCollection();

		private bool m_includeColumnHeaders;

		private bool m_quoteStringsContainingCommas;

		private SolidBrush m_brushNullObjects;

		public ResultSetAndGridContainerCollection GridContainers => m_gridContainers;

		public int NumberOfGrids
		{
			get
			{
				if (m_gridContainers != null)
				{
					return m_gridContainers.Count;
				}

				return 0;
			}
		}

		private bool IsCurrentControlSaveable
		{
			get
			{
				GridControl focusedGrid = FocusedGrid;
				if (focusedGrid == null)
				{
					return false;
				}

				QEResultSet gridResultSet = GetGridResultSet(focusedGrid);
				if (gridResultSet != null && gridResultSet.RowCount > 0)
				{
					return true;
				}

				return false;
			}
		}

		public GridResultsPanel(string defaultResultsDirectory)
			: base(defaultResultsDirectory)
		{
			Guid clsid = LibraryData.CLSID_CommandSet;
			MenuCommand menuCommand = new MenuCommand(OnSelectAll,
				new CommandID(VSConstants.CMDSETID.StandardCommandSet97_guid,
				(int)VSConstants.VSStd97CmdID.SelectAll));
			MenuCommand menuCommand2 = new MenuCommand(OnCopy,
				new CommandID(VSConstants.CMDSETID.StandardCommandSet97_guid,
				(int)VSConstants.VSStd97CmdID.Copy));
			MenuCommand menuCommand3 = new MenuCommand(OnSaveAs,
				new CommandID(clsid, (int)EnCommandSet.CmdIdSaveResultsAs));
			MenuCommand menuCommand4 = new MenuCommand(OnPrint,
				new CommandID(VSConstants.CMDSETID.StandardCommandSet97_guid,
				(int)VSConstants.VSStd97CmdID.Print));
			MenuCommand menuCommand5 = new MenuCommand(OnPrintPageSetup,
				new CommandID(VSConstants.CMDSETID.StandardCommandSet97_guid,
				(int)VSConstants.VSStd97CmdID.PageSetup));
			MenuCommand menuCommand6 = new MenuCommand(OnCopyWithHeaders,
				new CommandID(clsid, (int)EnCommandSet.CmdIdCopyWithHeaders));
			MenuService.AddRange(new MenuCommand[6] { menuCommand, menuCommand2, menuCommand3,
				menuCommand4, menuCommand5, menuCommand6 });
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 123)
			{
				if (FocusedGrid != null && CommonUtils.GetCoordinatesForPopupMenuFromWM_Context(ref m, out var x, out var y, (Control)(object)FocusedGrid))
				{
					CommonUtils.ShowContextMenu((int)EnCommandSet.ContextIdResultsWindow, x, y, this);
				}
			}
			else
			{
				base.WndProc(ref m);
			}
		}

		protected override void Dispose(bool bDisposing)
		{
			Tracer.Trace(GetType(), "GridResultsTabPanel.Dispose", "", null);
			if (bDisposing)
			{
				if (m_brushNullObjects != null)
				{
					m_brushNullObjects.Dispose();
					m_brushNullObjects = null;
				}

				Clear();
				m_gridContainers = null;
			}

			base.Dispose(bDisposing);
		}

		public static void SelectAllCellInGrid(GridControl grid)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			Tracer.Trace(typeof(GridResultsPanel), "GridResultsTabPanel.SelectAllCellInGrid", "", null);
			BlockOfCellsCollection val = new BlockOfCellsCollection();
			BlockOfCells val2 = new BlockOfCells(0L, 1);
			QEResultSet qEResultSet = (QEResultSet)(object)grid.GridStorage;
			if (qEResultSet.StoredAllData)
			{
				val2.Width = qEResultSet.TotalNumberOfColumns - 1;
				val2.Height = qEResultSet.RowCount;
				if (val2.Height > 0)
				{
					val.Add(val2);
					grid.SelectedCells = val;
				}
			}
		}

		public override void Initialize(object rawServiceProvider)
		{
			Tracer.Trace(GetType(), "GridResultsTabPanel.Initialize", "", null);
			SuspendLayout();
			base.Initialize(rawServiceProvider);
			_firstGridPanel.Dock = DockStyle.Fill;
			_firstGridPanel.Height = ClientRectangle.Height;
			_firstGridPanel.Tag = -1;
			Controls.Add(_firstGridPanel);
			if (m_brushNullObjects == null)
			{
				Color color = Color.FromKnownColor(KnownColor.Info);
				m_brushNullObjects = new SolidBrush(color);
			}

			ResumeLayout();
		}

		public override void Clear()
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Expected O, but got Unknown
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Expected O, but got Unknown
			Tracer.Trace(GetType(), "GridResultsTabPanel.Clear", "", null);
			base.Clear();
			Tracer.Trace(GetType(), "GridResultsTabPanel.Clear: disposing grid containers", "", null);
			if (_firstGridPanel != null)
			{
				for (int i = 0; i < _firstGridPanel.HostedControlsCount; i++)
				{
					if (_firstGridPanel.GetHostedControl(i) is GridResultsGrid gridResultsGrid)
					{
						gridResultsGrid.AdjustSelectionForButtonClick -= OnAdjustSelectionForButtonClick;
						gridResultsGrid.GridSpecialEvent -= new GridSpecialEventHandler(OnSpecialGridEvent);
						gridResultsGrid.HeaderButtonClicked -= new HeaderButtonClickedEventHandler(OnHeaderButtonClicked);
						gridResultsGrid.KeyPressedOnCell -= new KeyPressedOnCellEventHandler(OnKeyPressedOnCell);
						((Control)(object)gridResultsGrid).GotFocus -= OnGridGotFocus;
					}
				}
			}

			if (m_gridContainers != null)
			{
				foreach (ResultSetAndGridContainer gridContainer in m_gridContainers)
				{
					gridContainer.Dispose();
				}

				m_gridContainers.Clear();
			}

			Tracer.Trace(GetType(), "GridResultsTabPanel.Clear: returning", "", null);
		}

		public void SaveGrid(GridControl grid, EnGridSaveFormats saveFormat, TextWriter writer)
		{
			Tracer.Trace(GetType(), "GridResultsTabPanel.SaveGrid", "", null);
			if (writer == null)
			{
				Exception ex = new ArgumentNullException("writer");
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}

			QEResultSet gridResultSet = GetGridResultSet(grid);
			if (gridResultSet == null)
			{
				Exception ex2 = new ArgumentException("", "grid");
				Tracer.LogExThrow(GetType(), ex2);
				throw ex2;
			}

			int numberOfDataColumns = gridResultSet.NumberOfDataColumns;
			int num = (int)gridResultSet.TotalNumberOfRows;
			if (num == 0 || numberOfDataColumns <= 0)
			{
				return;
			}

			StringBuilder stringBuilder = new StringBuilder(256);
			string value = saveFormat == EnGridSaveFormats.CommaSeparated ? CultureInfo.CurrentCulture.TextInfo.ListSeparator : "\t";
			if (m_includeColumnHeaders)
			{
				grid.GetHeaderInfo(1, out string text, out Bitmap _);
				stringBuilder.Append(text ?? string.Empty);
				for (int i = 2; i <= numberOfDataColumns; i++)
				{
					grid.GetHeaderInfo(i, out text, out Bitmap _);
					stringBuilder.Append(value);
					stringBuilder.Append(text ?? string.Empty);
				}

				writer.WriteLine(stringBuilder.ToString());
			}

			for (int j = 0; j < num; j++)
			{
				stringBuilder.Length = 0;
				for (int k = 1; k <= numberOfDataColumns; k++)
				{
					string cellDataAsString = gridResultSet.GetCellDataAsString(j, k);
					bool num2 = saveFormat == EnGridSaveFormats.CommaSeparated && m_quoteStringsContainingCommas && cellDataAsString.Contains(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
					if (k != 1)
					{
						stringBuilder.Append(value);
					}

					if (num2)
					{
						stringBuilder.Append("\"");
						stringBuilder.Append(cellDataAsString.Replace("\"", "\"\""));
						stringBuilder.Append("\"");
					}
					else
					{
						stringBuilder.Append(cellDataAsString);
					}
				}

				writer.WriteLine(stringBuilder.ToString());
			}

			writer.Flush();
		}

		public GridResultsGrid AddGridContainer(ResultSetAndGridContainer cont, Font curGridFont, Color curBkColor, Color curFkColor, Color selectedCellColor, Color inactiveSelectedCellColor)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Expected O, but got Unknown
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Expected O, but got Unknown
			m_gridContainers.Add(cont);
			GridResultsGrid gridResultsGrid = new GridResultsGrid();
			((Control)(object)gridResultsGrid).Visible = false;
			((Control)(object)gridResultsGrid).Dock = DockStyle.Fill;
			((Control)(object)gridResultsGrid).Tag = m_gridContainers.Count - 1;
			gridResultsGrid.AdjustSelectionForButtonClick += OnAdjustSelectionForButtonClick;
			gridResultsGrid.GridSpecialEvent += new GridSpecialEventHandler(OnSpecialGridEvent);
			gridResultsGrid.HeaderButtonClicked += new HeaderButtonClickedEventHandler(OnHeaderButtonClicked);
			gridResultsGrid.KeyPressedOnCell += new KeyPressedOnCellEventHandler(OnKeyPressedOnCell);
			gridResultsGrid.CustomizeCellGDIObjects += new CustomizeCellGDIObjectsEventHandler(OnCustomizeCellGDIObjects);
			((Control)(object)gridResultsGrid).GotFocus += OnGridGotFocus;
			((Control)(object)gridResultsGrid).Font = curGridFont;
			gridResultsGrid.HeaderFont = curGridFont;
			gridResultsGrid.SetIncludeHeadersOnDragAndDrop(m_includeColumnHeaders);
			if (_firstGridPanel.HostedControlsCount == 0)
			{
				_firstGridPanel.HostedControlsMinInitialSize = CommonUtils.DefaultInitialMinNumberOfVisibleRows * (gridResultsGrid.RowHeight + 1) + gridResultsGrid.HeaderHeight + 1 + CommonUtils.GetExtraSizeForBorderStyle(gridResultsGrid.BorderStyle);
				_firstGridPanel.HostedControlsMinSize = ((Control)(object)gridResultsGrid).GetPreferredSize(((Control)(object)gridResultsGrid).Size).Height;
			}

			_firstGridPanel.AddControl((Control)(object)gridResultsGrid, limitMaxControlHeightToClientArea: false);
			((Control)(object)gridResultsGrid).BackColor = curBkColor;
			cont.Initialize(gridResultsGrid);
			cont.GridCtl.SetBkAndForeColors(curBkColor, curFkColor);
			cont.GridCtl.SetSelectedCellColor(selectedCellColor);
			cont.GridCtl.SetInactiveSelectedCellColor(inactiveSelectedCellColor);
			return gridResultsGrid;
		}

		public void ApplyCurrentGridFont(Font font)
		{
			if (m_gridContainers == null)
			{
				return;
			}

			foreach (ResultSetAndGridContainer gridContainer in m_gridContainers)
			{
				((Control)gridContainer.GridCtl).Font = font;
				gridContainer.GridCtl.HeaderFont = font;
			}
		}

		public void ApplyCurrentGridColor(Color bkColor, Color fkColor)
		{
			if (m_gridContainers == null)
			{
				return;
			}

			foreach (ResultSetAndGridContainer gridContainer in m_gridContainers)
			{
				gridContainer.GridCtl.SetBkAndForeColors(bkColor, fkColor);
			}
		}

		public void ApplySelectedCellColor(Color selectedCellColor)
		{
			if (m_gridContainers == null)
			{
				return;
			}

			foreach (ResultSetAndGridContainer gridContainer in m_gridContainers)
			{
				gridContainer.GridCtl.SetSelectedCellColor(selectedCellColor);
			}
		}

		public void ApplyInactiveSelectedCellColor(Color inactiveCellColor)
		{
			if (m_gridContainers == null)
			{
				return;
			}

			foreach (ResultSetAndGridContainer gridContainer in m_gridContainers)
			{
				gridContainer.GridCtl.SetInactiveSelectedCellColor(inactiveCellColor);
			}
		}

		public void ApplyHighlightedCellColor(Color highlightedCellColor)
		{
			m_brushNullObjects?.Dispose();

			m_brushNullObjects = new SolidBrush(highlightedCellColor);
			InvalidateGridControls();
		}

		private void InvalidateGridControls()
		{
			if (m_gridContainers == null)
			{
				return;
			}

			foreach (ResultSetAndGridContainer gridContainer in m_gridContainers)
			{
				if (gridContainer.GridCtl is GridResultsGrid gridResultsGrid && ((Control)(object)gridResultsGrid).IsHandleCreated)
				{
					((Control)(object)gridResultsGrid).Invalidate();
				}
			}
		}

		public void SetGridTabOptions(bool shouldInclude, bool shouldQuote)
		{
			m_includeColumnHeaders = shouldInclude;
			m_quoteStringsContainingCommas = shouldQuote;
			if (m_gridContainers == null)
			{
				return;
			}

			foreach (ResultSetAndGridContainer gridContainer in m_gridContainers)
			{
				gridContainer.GridCtl.SetIncludeHeadersOnDragAndDrop(shouldInclude);
			}
		}

		private void OnGridGotFocus(object sender, EventArgs e)
		{
			GridControl val = (GridControl)(sender is GridControl ? sender : null);
			if (val != null)
			{
				_lastFocusedControl = (Control)(object)val;
			}
		}

		private void OnKeyPressedOnCell(object sender, KeyPressedOnCellEventArgs a)
		{
			GridControl val = (GridControl)(sender is GridControl ? sender : null);
			if (a.Modifiers == Keys.Control && a.Key == Keys.Space)
			{
				val.GetCurrentCell(out _, out int num2);
				if (num2 >= 0)
				{
					SelectWholeRowOrColumn(val, num2, bRow: false);
				}
			}
			else if (a.Modifiers == Keys.Shift && a.Key == Keys.Space)
			{
				val.GetCurrentCell(out long num, out _);
				if (num >= 0)
				{
					SelectWholeRowOrColumn(val, num, bRow: true);
				}
			}
		}

		private void OnAdjustSelectionForButtonClick(object sender, AdjustSelectionForButtonClickEventArgs e)
		{
			GridResultsGrid grid = (GridResultsGrid)sender;
			if (e.ColumnIndex == 0)
			{
				if ((ModifierKeys & Keys.Shift) != Keys.Shift)
				{
					SelectWholeRowOrColumn((GridControl)(object)grid, e.RowIndex, bRow: true);
				}
				else
				{
					SelectBlockOfCellsAsRowSelection(grid, e.RowIndex);
				}
			}
		}

		private void OnSpecialGridEvent(object sender, GridSpecialEventArgs e)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			if (e.EventType == 0)
			{
				GridControl grid = (GridControl)sender;
				QEResultSet gridResultSet = GetGridResultSet(grid);
				if (gridResultSet.StoredAllData)
				{
					HandleXMLCellClick(gridResultSet, e.RowIndex, e.ColumnIndex);
				}
			}
		}

		public bool HandleXMLCellClick(QEResultSet rs, long nRowIndex, int nColIndex)
		{
			Tracer.Trace(GetType(), "GridResultsTabPanel.HandleXMLCellClick", "", null);
			Cursor current = Cursor.Current;
			string text = null;
			XmlWriter xmlWriter = null;
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				if (!rs.IsSyntheticXmlColumn(nColIndex - 1))
				{
					string cellDataAsString = rs.GetCellDataAsString(nRowIndex, nColIndex);
					if (cellDataAsString != null && cellDataAsString.ToUpperInvariant() == "NULL")
					{
						return false;
					}
				}

				text = AbstractQESQLBatchConsumer.GetTempXMLFileName();
				XmlWriterSettings xmlWriterSettings = new()
				{
					ConformanceLevel = ConformanceLevel.Fragment,
					CheckCharacters = false,
					Indent = true,
					IndentChars = "  "
				};
				XmlReaderSettings xmlReaderSettings = new()
				{
					CheckCharacters = false,
					ConformanceLevel = ConformanceLevel.Fragment,
					ValidationType = ValidationType.None,
					IgnoreWhitespace = false
				};
				string text2 = null;
				if (rs.IsSyntheticXmlColumn(nColIndex - 1))
				{
					long totalNumberOfRows = rs.TotalNumberOfRows;
					StringBuilder stringBuilder = new(8000);
					for (long num = 0L; num < totalNumberOfRows; num++)
					{
						stringBuilder.Append(rs.GetCellDataAsString(num, 1));
					}

					text2 = stringBuilder.ToString();
					stringBuilder.Length = 0;
					stringBuilder = null;
				}
				else
				{
					text2 = rs.GetCellDataAsString(nRowIndex, nColIndex);
				}

				xmlWriter = XmlWriter.Create(text, xmlWriterSettings);
				if (text2 != null)
				{
					using XmlReader xmlReader = XmlReader.Create(new StringReader(text2), xmlReaderSettings);
					while (!xmlReader.EOF)
					{
						xmlWriter.WriteNode(xmlReader, defattr: false);
					}
				}

				text2 = null;
				xmlWriter.Flush();
				xmlWriter.Close();
				xmlWriter = null;
				string text3 = rs.ColumnNames[nColIndex - 1];
				if (text3 != null)
				{
					_ = text3.Length;
					_ = 0;
				}

				IVsProject3 miscellaneousProject = Cmd.GetMiscellaneousProject(_serviceProvider);
				VSADDRESULT[] pResult = new VSADDRESULT[1];
				VSADDITEMOPERATION dwAddItemOperation = VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE;
				Native.WrapComCall(miscellaneousProject.AddItem(uint.MaxValue, dwAddItemOperation, text3, 1u, new string[1] { text }, IntPtr.Zero, pResult), Array.Empty<int>());
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (xmlWriter != null)
				{
					xmlWriter.Flush();
					xmlWriter.Close();
				}

				if (File.Exists(text))
				{
					File.Delete(text);
				}

				Cursor.Current = current;
			}

			return true;
		}

		private void OnHeaderButtonClicked(object sender, HeaderButtonClickedEventArgs a)
		{
			if (a.Button == MouseButtons.Left)
			{
				GridResultsGrid grid = (GridResultsGrid)sender;
				if (a.ColumnIndex == 0)
				{
					SelectAllCellInGrid((GridControl)(object)grid);
				}
				else if ((ModifierKeys & Keys.Shift) != Keys.Shift)
				{
					SelectWholeRowOrColumn((GridControl)(object)grid, a.ColumnIndex, bRow: false);
				}
				else
				{
					SelectBlockOfCellsAsColumnSelection(grid, a.ColumnIndex);
				}
			}
		}

		private void OnCustomizeCellGDIObjects(object sender, CustomizeCellGDIObjectsEventArgs args)
		{
			GridResultsGrid gridResultsGrid = sender as GridResultsGrid;
			long rowIndex = args.RowIndex;
			int columnIndex = args.ColumnIndex;
			if (columnIndex > 0 && gridResultsGrid.GridStorage is QEResultSet qEResultSet && rowIndex < qEResultSet.TotalNumberOfRows && columnIndex < qEResultSet.TotalNumberOfColumns)
			{
				if (qEResultSet.IsCellDataNull(rowIndex, columnIndex))
				{
					args.BKBrush = m_brushNullObjects;
				}
				else
				{
					args.BKBrush = gridResultsGrid.BackGroundBrush;
				}
			}
		}

		private void SelectBlockOfCellsAsColumnSelection(GridResultsGrid grid, int clickedColumnStorageIndex)
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			QEResultSet qEResultSet = (QEResultSet)(object)grid.GridStorage;
			if (!qEResultSet.StoredAllData)
			{
				return;
			}

			BlockOfCells currentSelectedBlock = grid.CurrentSelectedBlock;
			long rowCount = qEResultSet.RowCount;
			if (currentSelectedBlock.IsEmpty || rowCount <= 0)
			{
				SelectWholeRowOrColumn((GridControl)(object)grid, clickedColumnStorageIndex, bRow: false);
				return;
			}

			int uIColumnIndexByStorageIndex = grid.GetUIColumnIndexByStorageIndex(clickedColumnStorageIndex);
			int num2;
			int num3;
			if (currentSelectedBlock.OriginalX >= uIColumnIndexByStorageIndex)
			{
				num2 = uIColumnIndexByStorageIndex;
				num3 = currentSelectedBlock.OriginalX;
			}
			else
			{
				num2 = currentSelectedBlock.OriginalX;
				num3 = uIColumnIndexByStorageIndex;
			}

			BlockOfCells val = new(0L, num2)
			{
				Width = num3 - num2 + 1,
				Height = rowCount
			};
			BlockOfCellsCollection val2 = new BlockOfCellsCollection
			{
				val
			};
			grid.SetSelectedCellsAndCurrentCell(val2, 0L, currentSelectedBlock.OriginalX);
		}

		private void SelectBlockOfCellsAsRowSelection(GridResultsGrid grid, long clickedRowIndex)
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Expected O, but got Unknown
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected O, but got Unknown
			BlockOfCells currentSelectedBlock = grid.CurrentSelectedBlock;
			if (currentSelectedBlock.IsEmpty)
			{
				SelectWholeRowOrColumn((GridControl)(object)grid, clickedRowIndex, bRow: true);
				return;
			}

			long num;
			long num2;
			if (currentSelectedBlock.OriginalY >= clickedRowIndex)
			{
				num = clickedRowIndex;
				num2 = currentSelectedBlock.OriginalY;
			}
			else
			{
				num = currentSelectedBlock.OriginalY;
				num2 = clickedRowIndex;
			}

			QEResultSet qEResultSet = (QEResultSet)(object)grid.GridStorage;
			if (qEResultSet.StoredAllData)
			{
				BlockOfCells val = new(num, 1)
				{
					Width = qEResultSet.TotalNumberOfColumns - 1,
					Height = num2 - num + 1
				};
				BlockOfCellsCollection val2 = new()
				{
					val
				};
				grid.SetSelectedCellsAndCurrentCell(val2, currentSelectedBlock.OriginalY, 1);
			}
		}

		private void OnSelectAll(object sender, EventArgs a)
		{
			Tracer.Trace(GetType(), "GridResultsTabPanel.OnSelectAll", "", null);
			GridControl focusedGrid = FocusedGrid;
			if (focusedGrid != null)
			{
				SelectAllCellInGrid(focusedGrid);
			}
		}

		private void OnSaveAs(object sender, EventArgs a)
		{
			Tracer.Trace(GetType(), "GridResultsTabPanel.OnSaveAs", "", null);
			GridControl focusedGrid = FocusedGrid;
			if (focusedGrid == null)
			{
				return;
			}

			Cursor current = Cursor.Current;
			try
			{
				TextWriter textWriterForSaveResultsFromGrid = GetTextWriterForSaveResultsFromGrid(out EnGridSaveFormats saveFormat);
				if (textWriterForSaveResultsFromGrid == null)
				{
					return;
				}

				try
				{
					Cursor.Current = Cursors.WaitCursor;
					SaveGrid(focusedGrid, saveFormat, textWriterForSaveResultsFromGrid);
					textWriterForSaveResultsFromGrid.Flush();
				}
				catch (Exception e)
				{
					Tracer.LogExCatch(GetType(), e);
					Cmd.ShowExceptionInDialog(SharedResx.ErrWhileSavingResults, e);
				}
				finally
				{
					textWriterForSaveResultsFromGrid.Close();
				}
			}
			finally
			{
				Cursor.Current = current;
			}
		}

		private void SelectWholeRowOrColumn(GridControl grid, long nIndex, bool bRow)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Expected O, but got Unknown
			Tracer.Trace(GetType(), "GridResultsTabPanel.SelectWholeRowOrColumn", "nIndex = {0}, bRow = {1}", nIndex, bRow);
			QEResultSet qEResultSet = (QEResultSet)(object)grid.GridStorage;
			if (!qEResultSet.StoredAllData)
			{
				return;
			}

			BlockOfCellsCollection val = new BlockOfCellsCollection();
			BlockOfCells val2;
			if (bRow)
			{
				val2 = new(nIndex, 1)
				{
					Height = 1L,
					Width = qEResultSet.TotalNumberOfColumns - 1
				};
			}
			else
			{
				if (qEResultSet.RowCount <= 0)
				{
					return;
				}

				int uIColumnIndexByStorageIndex = grid.GetUIColumnIndexByStorageIndex((int)nIndex);
				val2 = new(0L, uIColumnIndexByStorageIndex)
				{
					Width = 1,
					Height = qEResultSet.RowCount
				};
			}

			val.Add(val2);
			grid.SelectedCells = val;
		}

		private QEResultSet GetGridResultSet(GridControl grid)
		{
			Tracer.Trace(GetType(), "GridResultsTabPanel.GetGridResultSet", "", null);
			int index = (int)((Control)(object)grid).Tag;
			return m_gridContainers[index].QEResultSet;
		}

		private TextWriter GetTextWriterForSaveResultsFromGrid(out EnGridSaveFormats saveFormat)
		{
			Tracer.Trace(GetType(), "DisplaySQLResultsControl.GetTextWriterForSaveResultsFromGrid", "", null);
			saveFormat = EnGridSaveFormats.CommaSeparated;
			FileEncodingDialog fileEncodingDialog = new FileEncodingDialog();
			SaveFormats saveFormats = new SaveFormats();
			string fileNameUsingSaveDialog = CommonUtils.GetFileNameUsingSaveDialog(CommonUtils.MakeVsFilterString(saveFormats.FilterString), SharedResx.SaveGridResults, DefaultResultsDirectory, fileEncodingDialog, out int filterIndex);
			if (fileNameUsingSaveDialog != null)
			{
				DefaultResultsDirectory = Path.GetDirectoryName(fileNameUsingSaveDialog);
				StreamWriter result = new StreamWriter(fileNameUsingSaveDialog, append: false, fileEncodingDialog.Encoding)
				{
					AutoFlush = false
				};
				saveFormat = saveFormats[filterIndex];
				return result;
			}

			return null;
		}

		public int QueryStatus(ref Guid guidGroup, uint nCmdId, OLECMD[] oleCmd, IntPtr oleText)
		{
			CommandID commandID = new CommandID(guidGroup, (int)oleCmd[0].cmdID);
			MenuCommand menuCommand = MenuService.FindCommand(commandID);

			if (menuCommand == null)
				return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;

			if (guidGroup.Equals(VSConstants.CMDSETID.StandardCommandSet97_guid))
			{
				switch ((VSConstants.VSStd97CmdID)commandID.ID)
				{
					case VSConstants.VSStd97CmdID.Copy:
					case VSConstants.VSStd97CmdID.Print:
					case VSConstants.VSStd97CmdID.SelectAll:
					case VSConstants.VSStd97CmdID.PageSetup:
						{
							if (FocusedGrid == null)
							{
								break;
							}

							bool visible = menuCommand.Supported = true;
							menuCommand.Visible = visible;
							if (commandID.ID == (int)VSConstants.VSStd97CmdID.Copy
								|| commandID.ID == (int)VSConstants.VSStd97CmdID.SelectAll)
							{
								menuCommand.Enabled = false;
								GridControl focusedGrid = FocusedGrid;
								if (focusedGrid != null)
								{
									BlockOfCellsCollection selectedCells = focusedGrid.SelectedCells;
									if (selectedCells != null && ((CollectionBase)(object)selectedCells).Count > 0)
									{
										menuCommand.Enabled = true;
									}
								}
							}

							oleCmd[0].cmdf = (uint)menuCommand.OleStatus;
							return VSConstants.S_OK;
						}
				}
			}
			else if (guidGroup.Equals(LibraryData.CLSID_CommandSet))
			{
				bool visible = menuCommand.Supported = true;
				menuCommand.Visible = visible;
				switch ((EnCommandSet)commandID.ID)
				{
					case EnCommandSet.CmdIdSaveResultsAs:
						visible = menuCommand.Visible = IsCurrentControlSaveable;
						menuCommand.Enabled = visible;
						break;
					case EnCommandSet.CmdIdCopyWithHeaders:
						menuCommand.Enabled = IsCurrentControlSaveable;
						break;
				}

				oleCmd[0].cmdf = (uint)menuCommand.OleStatus;
				return VSConstants.S_OK;
			}

			return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
		}

		public int Exec(ref Guid guidGroup, uint cmdId, uint cmdExcept, IntPtr variantIn, IntPtr variantOut)
		{
			MenuCommand menuCommand = MenuService.FindCommand(new CommandID(guidGroup, (int)cmdId));
			if (menuCommand != null)
			{
				if (guidGroup.Equals(VSConstants.CMDSETID.StandardCommandSet97_guid))
				{
					if (cmdId == 15 || cmdId == 31 || cmdId == 27 || cmdId == 227)
					{
						menuCommand.Invoke();
						return VSConstants.S_OK;
					}
				}
				else if (guidGroup.Equals(LibraryData.CLSID_CommandSet))
				{
					menuCommand.Invoke();
					return VSConstants.S_OK;
				}
			}

			return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
		}
	}
}