﻿// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.SplitViewContainer
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Shared.Controls.Widgets;


[DesignerCategory("code")]
public class SplitViewContainer : Control, IServiceProvider
{
	public class SplitterEx : Splitter
	{
		private long _LastClickTime;

		private readonly VsFontColorPreferences _VsFontColorPreferences;

		public static long DoubleClickTicks => SystemInformation.DoubleClickTime * 10000;

		public SplitterEx(IServiceProvider serviceProvider)
		{
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, value: false);
			SetBackColor();
			_VsFontColorPreferences = new VsFontColorPreferences();
			_VsFontColorPreferences.PreferencesChangedEvent += VsFontColorPreferences_PreferencesChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_VsFontColorPreferences.PreferencesChangedEvent -= VsFontColorPreferences_PreferencesChanged;
				_VsFontColorPreferences.Dispose();
			}
			base.Dispose(disposing);
		}

		private void VsFontColorPreferences_PreferencesChanged(object sender, EventArgs args)
		{
			SetBackColor();
		}

		private void SetBackColor()
		{
			BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOW);
		}

		public void SendMouseDown(Control sourceControl, MouseEventArgs e)
		{
			Point point = PointToClient(sourceControl.PointToScreen(e.Location));
			MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
			OnMouseDown(e2);
		}

		public void SendMouseMove(Control sourceControl, MouseEventArgs e)
		{
			Point point = PointToClient(sourceControl.PointToScreen(e.Location));
			MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
			OnMouseMove(e2);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			long ticks = DateTime.Now.Ticks;
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left)
			{
				long num = ticks - _LastClickTime;
				_LastClickTime = ticks;
				if (num >= 0 && num < DoubleClickTicks)
				{
					OnDoubleClick(EventArgs.Empty);
				}
			}
		}
	}

	public const string OPTION_ORIENTATION = "SplitView.Orientation";

	public const string OPTION_PERCENTAGEX = "SplitView.Percentage.X";

	public const string OPTION_PERCENTAGEY = "SplitView.Percentage.Y";

	public const string OPTION_SHOWSPLITTER = "SplitView.ShowSplitter";

	public const string OPTION_PRIMARYVIEW = "SplitView.PrimaryTab";

	private SplitViewSplitterStrip _splitViewStrip;

	private readonly Panel _panelDesign;

	private Panel _panelTop;

	private Panel _panelBottom;

	private Panel _SplitContainer;

	private bool _currentlySettingBounds;

	private SplitterEx _splitter;

	private float _horizontalPrimaryPercentage = 0.66f;

	private float _verticalPrimaryPercentage = 0.5f;

	private bool _mouseDownOnSplitter;

	private Point _startDragLocation;

	private Orientation _orientation;

	private ToolStrip _pathStrip;

	private bool _pathStripVisibleInSplitMode = true;

	public bool UseCustomTabActivation { get; set; }

	public bool IsSplitterVisible
	{
		get
		{
			return SplitterBar.ShowSplitter;
		}
		set
		{
			bool showSplitter = SplitterBar.ShowSplitter;
			SplitterBar.ShowSplitter = value;

			if (showSplitter == SplitterBar.ShowSplitter)
				return;

			_SplitContainer.Layout -= SplitContainer_Layout;

			_SplitContainer.SuspendLayout();
			SplitterBar.SuspendLayout();
			_panelTop.SuspendLayout();
			_panelBottom.SuspendLayout();

			try
			{
				if (ShouldSwapButtons())
				{
					if (SplitterBar.ShowSplitter)
					{
						SwapButtonsNoSuspendResume();
					}
					else
					{
						Guid guid = (Guid)SplitterBar.PrimaryPaneFirstButton.Tag;
						UpdateActiveTab(guid);
						TabActivationRequestEvent?.Invoke(this, new TabActivationEventArgs(guid, true));
					}
				}
				if (SplitterBar.ShowSplitter)
				{
					ScaleView(PercentSplit);
				}
				else
				{
					ExpandPanel();
				}
				LoadPathItems();
			}
			finally
			{
				_panelBottom.ResumeLayout();
				_panelTop.ResumeLayout();
				SplitterBar.ResumeLayout();
				_SplitContainer.ResumeLayout();
			}

			_SplitContainer.Layout += SplitContainer_Layout;
		}
	}

	public Orientation Orientation
	{
		get
		{
			return _orientation;
		}
		set
		{
			if (_orientation != value)
			{
				_orientation = value;
				SplitterBar.VSplitButton.Checked = Orientation == Orientation.Vertical && SplitterBar.ShowSplitter;
				SplitterBar.HSplitButton.Checked = Orientation == Orientation.Horizontal && SplitterBar.ShowSplitter;
				UpdateLayout();
				OrientationChangedEvent?.Invoke(this, new EventArgs());
			}
		}
	}

	private float PercentSplit
	{
		get
		{
			if (Orientation != 0)
			{
				return _verticalPrimaryPercentage;
			}
			return _horizontalPrimaryPercentage;
		}
		set
		{
			if (Orientation == Orientation.Horizontal)
			{
				_horizontalPrimaryPercentage = value;
			}
			else
			{
				_verticalPrimaryPercentage = value;
			}
		}
	}

	public Panel PanelDesign => _panelDesign;

	public Panel PanelTop
	{
		get
		{
			return _panelTop;
		}
		set
		{
			if (value != null && _panelTop != value)
			{
				value.Size = _panelTop.Size;
				value.Dock = _panelTop.Dock;

				_SplitContainer.SuspendLayout();

				try
				{
					int num = _SplitContainer.Controls.IndexOf(_panelTop);
					_SplitContainer.Controls.RemoveAt(num);
					_SplitContainer.Controls.Add(value);
					_SplitContainer.Controls.SetChildIndex(value, num);
				}
				finally
				{
					_SplitContainer.ResumeLayout();
				}

				_SplitContainer.Update();
				_panelTop = value;
			}
		}
	}

	public Panel PanelBottom
	{
		get
		{
			return _panelBottom;
		}
		set
		{
			if (value != null && _panelBottom != value)
			{
				_SplitContainer.SuspendLayout();
				value.SuspendLayout();

				try
				{
					value.Size = _panelBottom.Size;
					value.Dock = _panelBottom.Dock;
					int num = _SplitContainer.Controls.IndexOf(_panelBottom);
					_SplitContainer.Controls.RemoveAt(num);
					value.Controls.Add(SplitterBar);
					_SplitContainer.Controls.Add(value);
					_SplitContainer.Controls.SetChildIndex(value, num);
				}
				finally
				{
					value.ResumeLayout();
					_SplitContainer.ResumeLayout();
				}

				_SplitContainer.Update();
				_panelBottom = value;
			}
		}
	}

	public bool PathStripVisibleInSplitMode
	{
		get
		{
			return _pathStripVisibleInSplitMode;
		}
		set
		{
			_pathStripVisibleInSplitMode = value;
		}
	}

	public ToolStrip PathStrip => _pathStrip;

	public bool SplittersVisible
	{
		set
		{
			SplitterEx splitter = Splitter;
			bool visible = SplitterBar.Visible = value;
			splitter.Visible = visible;
		}
	}

	public SplitterEx Splitter => _splitter;

	public SplitViewSplitterStrip SplitterBar => _splitViewStrip;

	public event EventHandler IsSplitterVisibleChangedEvent;

	public event EventHandler OrientationChangedEvent;

	public event EventHandler PanelSwappedEvent;

	public event EventHandler<TabActivationEventArgs> TabActivationRequestEvent;

	public SplitViewContainer()
	{
		InitializeComponent();
		BackColor = VsColorUtilities.GetShellColor(__VSSYSCOLOREX3.VSCOLOR_WINDOW);
		_panelDesign = _panelTop;
		UseCustomTabActivation = false;
		Text = "SplitViewContainer";
		SplitterBar.ChevronButton.Click += SplitterBar_SplitButtonClicked;
		SplitterBar.HSplitButton.Click += SplitterBar_SplitButtonClicked;
		SplitterBar.VSplitButton.Click += SplitterBar_SplitButtonClicked;
		SplitterBar.SwapButton.Click += SwapButton_Click;
		SplitterBar.DesignerXamlClickEvent += DesignerXamlButton_Click;
		SplitterBar.DesignerXamlDoubleClickEvent += DesignerXamlButton_DoubleClick;
		SplitterBar.ShowSplitterChangedEvent += SplitterBar_ShowSplitterChanged;
		Splitter.DoubleClick += Splitter_DoubleClick;
		Splitter.SplitterMovedEvent += Splitter_SplitterMoved;
		LoadPathItems();
		UpdateSplitter();
		_SplitContainer.Layout += SplitContainer_Layout;
		SplitterBar.Layout += SizePath;
		_pathStrip.Layout += SizePath;
	}

	private void UpdateLayout()
	{
		_SplitContainer.SuspendLayout();
		PanelTop.SuspendLayout();
		PanelBottom.SuspendLayout();
		SplitterBar.SuspendLayout();
		Splitter.SuspendLayout();

		try
		{
			UpdateSplitter();
			if (SplitterBar.ShowSplitter)
			{
				ScaleView(PercentSplit);
			}
			else
			{
				ExpandPanel();
			}
			LoadPathItems();
		}
		finally
		{
			Splitter.ResumeLayout();
			SplitterBar.ResumeLayout();
			PanelBottom.ResumeLayout();
			PanelTop.ResumeLayout();
			_SplitContainer.ResumeLayout();
		}
	}

	public void CustomizeSplitterBarButton(Guid buttonTagGuid, EnSplitterBarButtonDisplayStyle displayStyle, string buttonText, Image buttonImage, string toolTipText = null)
	{
		ToolStripButton toolStripButton = null;
		foreach (ToolStripButton item in SplitterBar.EnumerateAllButtons())
		{
			if ((Guid)item.Tag == buttonTagGuid)
			{
				toolStripButton = item;
				break;
			}
		}
		if (toolStripButton != null)
		{
			ToolStripItemDisplayStyle displayStyle2 = ToolStripItemDisplayStyle.None;
			switch (displayStyle)
			{
				case EnSplitterBarButtonDisplayStyle.Text:
					displayStyle2 = ToolStripItemDisplayStyle.Text;
					break;
				case EnSplitterBarButtonDisplayStyle.Image:
					displayStyle2 = ToolStripItemDisplayStyle.Image;
					break;
				case EnSplitterBarButtonDisplayStyle.ImageAndText:
					displayStyle2 = ToolStripItemDisplayStyle.ImageAndText;
					break;
			}
			toolStripButton.DisplayStyle = displayStyle2;
			SizeF scaleFactor = ControlUtils.GetScaleFactor(this);
			toolStripButton.Image = buttonImage == null ? null : ControlUtils.ScaleImage(buttonImage, scaleFactor);
			toolStripButton.Text = buttonText;
			toolStripButton.ToolTipText = toolTipText ?? buttonText;
		}
	}

	public void LoadWindowState(string section)
	{
	}

	public void SaveWindowState(string section)
	{
	}

	public void ClearActiveTab()
	{
		SplitterBar.Button1.Checked = false;
		SplitterBar.Button2.Checked = false;
		foreach (ToolStripButton primaryPaneButton in SplitterBar.PrimaryPaneButtons)
		{
			primaryPaneButton.Checked = false;
		}
		foreach (ToolStripButton secondaryPaneButton in SplitterBar.SecondaryPaneButtons)
		{
			secondaryPaneButton.Checked = false;
		}
	}

	private void DesignerXamlButton_Click(object sender, EventArgs e)
	{
		if (sender is not ToolStripButton toolStripButton)
			return;

		if (!UseCustomTabActivation && !toolStripButton.Checked)
		{
			UpdateActiveTab((Guid)toolStripButton.Tag);

			if (!IsSplitterVisible)
				SwapPanels();
		}

		// Tracer.Trace(GetType(), "DesignerXamlButton_Click()", "tag: {0}.", toolStripButton.Tag);

		if (!IsSplitterVisible)
			TabActivationRequestEvent?.Invoke(sender, new TabActivationEventArgs((Guid)toolStripButton.Tag, true));
		else
			TabActivationRequestEvent?.Invoke(sender, new TabActivationEventArgs((Guid)toolStripButton.Tag));
	}

	public void CycleToNextButton()
	{
		SplitterBar.GetButtonInCyclicOrderFromChecked(forward: true)?.PerformClick();
	}

	public void CycleToPreviousButton()
	{
		SplitterBar.GetButtonInCyclicOrderFromChecked(forward: false)?.PerformClick();
	}

	private void ExpandPanel()
	{
		ScaleView(1f);
		PanelBottom.Size = Size.Empty;
	}

	private void DesignerXamlButton_DoubleClick(object sender, EventArgs e)
	{
		ShowView(sender as ToolStripButton);
	}

	public Rectangle GetAvailableBounds(bool isTopOrLeft)
	{
		Rectangle empty;
		if (isTopOrLeft)
		{
			empty = PanelTop.ClientRectangle;
		}
		else if (Orientation == Orientation.Horizontal)
		{
			empty = PanelBottom.ClientRectangle;
			int num2 = empty.Y = SplitterBar.Height - Splitter.Height;
			empty.Height = Math.Max(0, empty.Height - num2);
		}
		else
		{
			empty = PanelBottom.ClientRectangle;
			int num4 = empty.X = SplitterBar.Width - Splitter.Width;
			empty.Width = Math.Max(0, empty.Width - num4);
		}
		return empty;
	}

	protected override object GetService(Type service)
	{
		if (Parent is IServiceProvider serviceProvider)
		{
			return serviceProvider.GetService(service);
		}
		return base.GetService(service);
	}

	private void InitializeComponent()
	{
		SuspendLayout();

		try
		{
			_panelTop = new Panel
			{
				Dock = DockStyle.Top,
				Name = "PanelTop"
			};
			_SplitContainer = new Panel();

			_SplitContainer.SuspendLayout();

			try
			{


				_SplitContainer.Dock = DockStyle.Fill;
				_SplitContainer.Name = "SplitContainer";
				_splitViewStrip = new SplitViewSplitterStrip(this);
				_splitViewStrip.MouseDown += new MouseEventHandler(SplitterBar_MouseDown);
				_splitViewStrip.MouseUp += new MouseEventHandler(SplitterBar_MouseUp);
				_splitViewStrip.MouseMove += new MouseEventHandler(SplitterBar_MouseMove);
				_splitViewStrip.MouseDoubleClick += new MouseEventHandler(SplitterBar_MouseDoubleClick);
				_pathStrip = new ToolStrip
				{
					Text = "PathControl",
					CanOverflow = false,
					Dock = DockStyle.Bottom,
					GripStyle = ToolStripGripStyle.Hidden,
					AutoSize = false,
					Height = _splitViewStrip.Height,
					BackColor = SystemColors.Control
				};
				_panelBottom = new Panel();

				_panelBottom.SuspendLayout();

				try
				{
					_panelBottom.Dock = DockStyle.Fill;
					_panelBottom.Controls.Add(_splitViewStrip);
					_panelBottom.Name = "PanelBottom";
					_splitter = new SplitterEx(this)
					{
						Dock = DockStyle.Top,
						MinExtra = _splitViewStrip.Height,
						Size = new Size(1, 1)
					};
					BackColor = SystemColors.Window;
					_SplitContainer.Controls.Add(_panelBottom);
					_SplitContainer.Controls.Add(_splitter);
					_SplitContainer.Controls.Add(_panelTop);
					Controls.Add(_SplitContainer);
					Controls.Add(_pathStrip);
				}
				finally
				{
					_panelBottom.ResumeLayout(false);
				}
			}
			finally
			{
				_SplitContainer.ResumeLayout(false);
			}
		}
		finally
		{
			ResumeLayout(false);
		}

		PerformLayout();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	private void LoadPathItems()
	{
		bool flag = Orientation == Orientation.Horizontal && !IsSplitterVisible;
		ToolStrip obj = flag ? SplitterBar : _pathStrip;
		obj.SuspendLayout();
		obj.ResumeLayout();
		_currentlySettingBounds = true;
		try
		{
			if (PathStripVisibleInSplitMode && _pathStrip.Visible == flag)
			{
				_pathStrip.Visible = !flag;
				if (flag && _mouseDownOnSplitter)
				{
					ExpandPanel();
				}
				else if (_pathStrip.Visible && Orientation == Orientation.Horizontal)
				{
					PanelTop.Height = Math.Min(PanelTop.Height, Height - _pathStrip.Height - SplitterBar.Height - 4);
				}
			}
		}
		finally
		{
			_currentlySettingBounds = false;
		}
	}

	private void ScaleView(float percentage)
	{
		bool pathBarPresent = true;
		if (Orientation == Orientation.Horizontal)
		{
			pathBarPresent = percentage != 1f;
			int num = Height;
			if (pathBarPresent)
			{
				num -= _splitViewStrip.Height;
			}
			SizePanel((int)(percentage * num), pathBarPresent);
		}
		else
		{
			SizePanel((int)(percentage * _SplitContainer.Width), pathBarPresent);
		}
	}

	private void SizePanel(int distance, bool pathBarPresent)
	{
		_SplitContainer.SuspendLayout();

		try
		{
			if (Orientation == Orientation.Horizontal)
			{
				distance = Math.Max(0, distance);
				int num = Height;
				if (pathBarPresent && _pathStrip.Visible)
				{
					num -= _pathStrip.Height;
				}
				if (Splitter.Visible)
				{
					num = num - Splitter.MinExtra - Splitter.Height;
				}
				distance = Math.Min(distance, num);
				PanelTop.Height = distance;
			}
			else
			{
				distance = Math.Max(0, distance);
				int val = _SplitContainer.Width;
				if (Splitter.Visible)
				{
					val = _SplitContainer.Width - Splitter.MinExtra - Splitter.Width;
				}
				distance = Math.Min(distance, val);
				PanelTop.Width = distance;
			}
			if (Splitter.SplitPosition != distance)
			{
				Splitter.InvalidateSplitPosition();
			}
		}
		finally
		{
			_SplitContainer.ResumeLayout();
		}
	}

	private void SizePath(object sender, EventArgs e)
	{
	}

	private void Splitter_SplitterMoved(object sender, SplitterEventArgs e)
	{
		_mouseDownOnSplitter = false;
		if (Orientation == Orientation.Horizontal && _SplitContainer.Height > 0)
		{
			if (PanelBottom.Height - Splitter.MinExtra > 0)
			{
				PercentSplit = PanelTop.Height / (float)_SplitContainer.Height;
			}
		}
		else if (_SplitContainer.Width > 0 && PanelBottom.Width - Splitter.MinExtra > 0)
		{
			PercentSplit = PanelTop.Width / (float)_SplitContainer.Width;
		}
	}

	private void Splitter_DoubleClick(object sender, EventArgs e)
	{
		SplitterBar.ChevronButton.PerformClick();
	}

	private void SplitterBar_MouseMove(object sender, MouseEventArgs e)
	{
		if (_mouseDownOnSplitter && e.Button == MouseButtons.Left)
		{
			Size dragSize = SystemInformation.DragSize;
			if (Math.Abs(e.X - _startDragLocation.X) >= dragSize.Width || Math.Abs(e.Y - _startDragLocation.Y) >= dragSize.Height)
			{
				Point point = SplitterBar.PointToClient(Splitter.PointToScreen(Point.Empty));
				MouseEventArgs e2 = new MouseEventArgs(e.Button, 1, point.X, point.Y, e.Delta);
				SplitterBar.Capture = false;
				Splitter.SendMouseDown(SplitterBar, e2);
				Splitter.SendMouseMove(SplitterBar, e);
			}
		}
	}

	private void SplitterBar_MouseUp(object sender, MouseEventArgs e)
	{
		_mouseDownOnSplitter = false;
		SplitterBar.Capture = false;
	}

	private void SplitterBar_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && SplitterBar.SplitterRectangle.Contains(e.Location))
		{
			_mouseDownOnSplitter = true;
			_startDragLocation = e.Location;
			SplitterBar.Capture = true;
			if (Cursor.Current != Cursors.WaitCursor)
			{

				if (Orientation == Orientation.Horizontal)
				{
					Cursor.Current = Cursors.HSplit;
				}
				else
				{
					Cursor.Current = Cursors.VSplit;
				}
			}
		}
		else
		{
			_mouseDownOnSplitter = false;
		}
	}

	private void SplitterBar_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && SplitterBar.SplitterRectangle.Contains(e.Location))
		{
			Splitter_DoubleClick(sender, e);
		}
	}

	private void SplitterBar_ShowSplitterChanged(object sender, EventArgs e)
	{
		IsSplitterVisibleChangedEvent?.Invoke(this, e);
	}

	private void SplitterBar_SplitButtonClicked(object sender, EventArgs e)
	{
		ToolStripButton toolStripButton = sender as ToolStripButton;
		if (toolStripButton == SplitterBar.ChevronButton)
		{
			IsSplitterVisible = !SplitterBar.ShowSplitter;
		}
		else if (toolStripButton == SplitterBar.HSplitButton)
		{
			Orientation = Orientation.Horizontal;
			IsSplitterVisible = true;
		}
		else if (toolStripButton == SplitterBar.VSplitButton)
		{
			Orientation = Orientation.Vertical;
			IsSplitterVisible = true;
		}
	}

	private void SplitContainer_Layout(object sender, LayoutEventArgs e)
	{
		UpdateShowSplitter(sender, e);
	}

	public void SwapButtonsNoSuspendResume()
	{
		SplitterBar.Items.IndexOf(SplitterBar.Button1);
		SplitterBar.Items.IndexOf(SplitterBar.Button2);
		int num = 0;
		foreach (ToolStripButton secondaryPaneButton in SplitterBar.SecondaryPaneButtons)
		{
			SplitterBar.Items.Insert(num++, secondaryPaneButton);
		}
		SplitterBar.Items.Insert(num++, SplitterBar.SwapButton);
		foreach (ToolStripButton primaryPaneButton in SplitterBar.PrimaryPaneButtons)
		{
			SplitterBar.Items.Insert(num++, primaryPaneButton);
		}
	}

	public void SwapButtons()
	{
		SplitterBar.SuspendLayout();
		SwapButtonsNoSuspendResume();
		SplitterBar.ResumeLayout();
	}

	private void SwapButton_Click(object sender, EventArgs e)
	{
		SwapPanels();
	}

	public void SwapPanelsNoSuspendResume()
	{
		Panel panelTop = PanelTop;
		Size size = PanelTop.Size;
		DockStyle dock = PanelTop.Dock;
		Panel panelBottom = PanelBottom;
		Size size2 = PanelBottom.Size;
		DockStyle dock2 = PanelBottom.Dock;
		_panelBottom = panelTop;
		_panelBottom.Dock = dock2;
		_panelBottom.Size = size2;
		_panelBottom.Controls.Add(SplitterBar);
		_panelTop = panelBottom;
		_panelTop.Dock = dock;
		_panelTop.Size = IsSplitterVisible ? size : new Size(_SplitContainer.Width - Splitter.MinExtra, _SplitContainer.Height - Splitter.MinExtra);
		_SplitContainer.Controls.SetChildIndex(_panelBottom, 0);
		_SplitContainer.Controls.SetChildIndex(_splitter, 1);
		_SplitContainer.Controls.SetChildIndex(_panelTop, 2);
	}

	public void SwapPanels()
	{
		_SplitContainer.SuspendLayout();
		SplitterBar.SuspendLayout();
		_panelTop.SuspendLayout();
		_panelBottom.SuspendLayout();
		if (IsSplitterVisible)
		{
			SwapButtons();
		}
		SwapPanelsNoSuspendResume();
		UpdateSplitter();
		SplitterBar.ResumeLayout();
		_panelTop.ResumeLayout();
		_panelBottom.ResumeLayout();
		_SplitContainer.ResumeLayout();
		Update();
		PanelSwappedEvent?.Invoke(this, EventArgs.Empty);
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		bool isSplitterVisible = IsSplitterVisible;
		using (Microsoft.VisualStudio.Utilities.DpiAwareness.EnterDpiScope(Microsoft.VisualStudio.Utilities.DpiAwarenessContext.SystemAware))
		{
			_currentlySettingBounds = Width != width || Height != height;
			try
			{
				float percentage = isSplitterVisible ? PercentSplit : 1f;
				base.SetBoundsCore(x, y, width, height, specified);
				if (width > 0 && height > 0 && _currentlySettingBounds && Visible)
				{
					ScaleView(percentage);
				}
			}
			finally
			{
				_currentlySettingBounds = false;
			}
		}
	}

	private void ShowView(ToolStripButton designOrXamlButton)
	{
		if (SplitterBar.ShowSplitter)
		{
			if (designOrXamlButton != null && SplitterBar.SecondaryPaneButtons.Contains(designOrXamlButton))
			{
				SwapButton_Click(designOrXamlButton, EventArgs.Empty);
			}
			_SplitContainer.SuspendLayout();
			SplitterBar.ShowSplitter = false;
			LoadPathItems();
			ExpandPanel();
			_SplitContainer.ResumeLayout();
		}
		else if (!designOrXamlButton.Checked)
		{
			designOrXamlButton.PerformClick();
		}
	}

	public void ShowView(AbstractEditorTab editorTab)
	{
		ShowView(GetButton(editorTab));
	}

	public ToolStripButton GetButton(AbstractEditorTab editorTab)
	{
		ToolStripButton result = null;
		foreach (ToolStripButton item in SplitterBar.EnumerateAllButtons())
		{
			if ((Guid)item.Tag == editorTab.LogicalView)
			{
				result = item;
				break;
			}
		}
		return result;
	}

	private void UpdateShowSplitter(object sender, EventArgs e)
	{
		if (_currentlySettingBounds)
		{
			return;
		}
		bool showSplitter = SplitterBar.ShowSplitter;
		if (Orientation == Orientation.Horizontal)
		{
			if (Splitter.SplitPosition != -1 && Splitter.SplitPosition <= Splitter.MinExtra)
			{
				SplitterBar.SwapButton.PerformClick();
				if (!showSplitter)
				{
					LoadPathItems();
					ExpandPanel();
				}
			}
			if (PanelBottom.Height > 0 && Splitter.SplitPosition != -1)
			{
				SplitterBar.ShowSplitter = PanelBottom.Height > Splitter.MinExtra && Splitter.SplitPosition > Splitter.MinExtra;
			}
		}
		else
		{
			if (Splitter.SplitPosition != -1 && Splitter.SplitPosition <= Splitter.MinExtra)
			{
				SplitterBar.SwapButton.PerformClick();
				if (!showSplitter)
				{
					LoadPathItems();
					ExpandPanel();
				}
			}
			if (PanelBottom.Width > 0 && Splitter.SplitPosition != -1)
			{
				SplitterBar.ShowSplitter = PanelBottom.Width > Splitter.MinExtra && Splitter.SplitPosition > Splitter.MinExtra;
			}
		}
		if (SplitterBar.ShowSplitter != showSplitter)
		{
			LoadPathItems();
			if (SplitterBar.ShowSplitter && SplitterBar.PrimaryPaneButtons != null && ShouldSwapButtons())
			{
				SwapButtons();
			}
		}
	}

	private bool ShouldSwapButtons()
	{
		bool result = false;
		foreach (ToolStripButton secondaryPaneButton in SplitterBar.SecondaryPaneButtons)
		{
			if (secondaryPaneButton.Checked)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void UpdateActiveTab(Guid activeTabTypeGuid)
	{
		foreach (ToolStripButton item in SplitterBar.EnumerateAllButtons())
		{
			if ((Guid)item.Tag == activeTabTypeGuid)
			{
				item.Checked = true;
			}
			else
			{
				item.Checked = false;
			}
		}
	}

	private void UpdateSplitter()
	{
		if (_orientation == Orientation.Vertical)
		{
			PanelTop.Dock = DockStyle.Left;
			Splitter.Dock = DockStyle.Left;
			SplitterBar.Dock = DockStyle.Left;
			Splitter.MinExtra = SplitterBar.Width;
			Splitter.MinSize = 0;
			Splitter.Width = 1;
			Splitter.Height = 1;
		}
		else
		{
			PanelTop.Dock = DockStyle.Top;
			Splitter.Dock = DockStyle.Top;
			SplitterBar.Dock = DockStyle.Top;
			Splitter.MinExtra = SplitterBar.Height;
			Splitter.MinSize = 0;
			Splitter.Width = 1;
			Splitter.Height = 1;
		}
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		return GetService(serviceType);
	}
}
