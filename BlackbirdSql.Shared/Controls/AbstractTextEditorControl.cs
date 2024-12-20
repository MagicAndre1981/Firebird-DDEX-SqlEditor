﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.ShellTextEditorControl

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Ctl.IO;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using Constants = Microsoft.VisualStudio.OLE.Interop.Constants;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;



namespace BlackbirdSql.Shared.Controls;


public abstract class AbstractTextEditorControl : Control, IDisposable, IOleCommandTarget, IVsWindowFrameNotify, IVsTextViewEvents
{

	public AbstractTextEditorControl()
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.AbstractShellTextEditorControl", "", null);
	}



	private Guid _MandatedXmlLanguageServiceClsid = Guid.Empty;

	protected static readonly string STName = "VSEditor";

	protected BorderStyle _BorderStyle = BorderStyle.Fixed3D;

	protected ShellTextBuffer _TextBuffer;

	private IntPtr _EditorHandle = IntPtr.Zero;

	protected ServiceProvider _OleServiceProvider;

	private bool _ParentedEditorWithParkingWindow;

	protected IOleCommandTarget _TextCmdTarget;

	protected IVsWindowPane _TextWindowPane;

	protected IVsWindowFrameNotify _TextWndFrameNotify;

	protected bool _WantCustomPopupMenu;

	protected Guid _ClsidLanguageService = Guid.Empty;

	protected Guid _ClsidLanguageServiceDefault;

	private static IVsTextManager _VsTextManager = null;

	private bool _WithEncoding;

	protected override CreateParams CreateParams
	{
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			// Evs.Trace(GetType(), "AbstractShellTextEditorControl.CreateParams", "", null);
			CreateParams createParams = base.CreateParams;
			if (_BorderStyle != 0)
			{
				createParams.Style |= 8388608;
			}

			createParams.Style |= 1174405120;
			return createParams;
		}
	}

	public object DocData
	{
		get
		{
			if (_TextBuffer != null)
			{
				return _TextBuffer.TextStream;
			}

			return null;
		}
	}


	protected virtual Guid MandatedXmlLanguageServiceClsid
	{
		get
		{
			if (_MandatedXmlLanguageServiceClsid == Guid.Empty)
				_MandatedXmlLanguageServiceClsid = new(VS.XmlLanguageServiceGuid);

			return _MandatedXmlLanguageServiceClsid;
		}
	}



	public ShellTextBuffer TextBuffer
	{
		get
		{
			if (_TextBuffer != null)
			{
				return _TextBuffer;
			}

			return null;
		}
	}

	public abstract IVsFindTarget FindTarget { get; }

	public abstract IVsTextView CurrentView { get; }

	public BorderStyle BorderStyle
	{
		get
		{
			return _BorderStyle;
		}
		set
		{
			// Evs.Trace(GetType(), "AbstractShellTextEditorControl.BorderStyle", "value = {0}", value.ToString());
			if (Enum.IsDefined(typeof(BorderStyle), value))
			{
				_BorderStyle = value;
				if (IsHandleCreated)
				{
					RecreateHandle();
				}
			}
		}
	}

	public bool WantCustomPopupMenu
	{
		get
		{
			return _WantCustomPopupMenu;
		}
		set
		{
			VerifyBeforeInstanceProperty();
			_WantCustomPopupMenu = true;
		}
	}

	public Guid ClsidLanguageService
	{
		get
		{
			return _ClsidLanguageService;
		}
		set
		{
			// Evs.Trace(GetType(), "AbstractShellTextEditorControl.LanguageService", "value = {0}", value.ToString("D", CultureInfo.CurrentCulture));
			if (!_ClsidLanguageService.Equals(value))
			{
				_ClsidLanguageService = value;
				if (!_ClsidLanguageService.Equals(Guid.Empty) && _TextBuffer != null)
				{
					ApplyLS(value);
				}
			}
		}
	}

	public Guid ClsidLanguageServiceDefault
	{
		get
		{
			return _ClsidLanguageServiceDefault;
		}
		set
		{
			// Evs.Trace(GetType(), "AbstractShellTextEditorControl.ClsidLanguageServiceDefault", "value = {0}", value.ToString("D", CultureInfo.CurrentCulture));
			_ClsidLanguageServiceDefault = value;
			_TextBuffer.DetectLangSid = false;
		}
	}


	public bool WithEncoding
	{
		get
		{
			return _WithEncoding;
		}
		set
		{
			_WithEncoding = value;
		}
	}

	protected IntPtr EditorHandle
	{
		get
		{
			return _EditorHandle;
		}
		set
		{
			_EditorHandle = value;
		}
	}

	protected abstract bool IsEditorInstanceCreated { get; }

	public event SpecialEditorCommandEventHandler ShowPopupMenuEvent;



	protected override void Dispose(bool disposing)
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.Dispose", "", null);
		if (!IsDisposed)
		{
			ShowPopupMenuEvent = null;
			if (disposing)
			{
				if (_TextBuffer != null)
				{
					_TextBuffer.Dispose();
					_TextBuffer = null;
				}

				if (_OleServiceProvider != null)
				{
					(_OleServiceProvider as IDisposable)?.Dispose();
					_OleServiceProvider = null;
				}
			}

			if (_EditorHandle != IntPtr.Zero)
			{
				_EditorHandle = IntPtr.Zero;
			}

			UnsinkEventsAndFreeInterfaces();
			if (_OleServiceProvider != null)
			{
				_OleServiceProvider = null;
			}

			base.Dispose(disposing);
		}
		else
		{
			// Evs.Trace(GetType(), "AbstractShellTextEditorControl.Dispose", "already disposed");
		}

		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.Dispose", "returning");
	}

	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);


	public virtual int QueryStatus(ref Guid guidGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
	{
		if (_TextCmdTarget != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _TextCmdTarget.QueryStatus(ref guidGroup, cCmds, prgCmds, pCmdText);
		}

		return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
	}

	public virtual int Exec(ref Guid guidGroup, uint nCmdId, uint nCmdExcept, IntPtr pobIn, IntPtr pvaOut)
	{
		if (_TextCmdTarget != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _TextCmdTarget.Exec(ref guidGroup, nCmdId, nCmdExcept, pobIn, pvaOut);
		}

		return (int)Constants.MSOCMDERR_E_UNKNOWNGROUP;
	}

	void IVsTextViewEvents.OnSetFocus(IVsTextView pView)
	{
		base.OnGotFocus(EventArgs.Empty);
		IContainerControl containerControl = GetContainerControl();
		if (containerControl != null)
		{
			containerControl.ActiveControl = this;
		}
	}

	void IVsTextViewEvents.OnKillFocus(IVsTextView pView)
	{
		base.OnLostFocus(EventArgs.Empty);
	}

	void IVsTextViewEvents.OnSetBuffer(IVsTextView pView, IVsTextLines pBuffer)
	{
	}

	void IVsTextViewEvents.OnChangeScrollInfo(IVsTextView pView, int iBar, int iMinUnit, int iMaxUnits, int iVisibleUnits, int iFirstVisibleUnit)
	{
	}

	void IVsTextViewEvents.OnChangeCaretLine(IVsTextView pView, int iNewLine, int iOldLine)
	{
	}

	public virtual int OnShow(int frameShow)
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.OnShow", "frameShow = {0}", frameShow);

		if (_TextWndFrameNotify != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _TextWndFrameNotify.OnShow(frameShow);
		}

		return VSConstants.S_OK;
	}

	public virtual int OnMove()
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.OnMove", "", null);

		if (_TextWndFrameNotify != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _TextWndFrameNotify.OnMove();
		}

		return VSConstants.S_OK;
	}

	public virtual int OnSize()
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.OnSize", "", null);

		if (_TextWndFrameNotify != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _TextWndFrameNotify.OnSize();
		}

		return VSConstants.S_OK;
	}

	public virtual int OnDockableChange(int fDockable)
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.OnDockableChange", "fDockable = {0}", fDockable);

		if (_TextWndFrameNotify != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _TextWndFrameNotify.OnDockableChange(fDockable);
		}

		return VSConstants.S_OK;
	}

	public int LoadViewState(IStream state)
	{
		if (_TextWindowPane != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _TextWindowPane.LoadViewState(state);
		}

		return VSConstants.E_NOTIMPL;
	}

	public int SaveViewState(IStream state)
	{
		// Evs.Trace(GetType(), nameof(SaveViewState));

		if (_TextWindowPane != null)
		{
			Diag.ThrowIfNotOnUIThread();

			return _TextWindowPane.SaveViewState(state);
		}

		return VSConstants.E_NOTIMPL;
	}

	public void PrepareForHandleRecreation()
	{
		if (!ShouldDistroyNativeControl())
		{
			using Control control = new Control();
			IntPtr parent = Native.GetParent(new HandleRef(control, control.Handle));
			SetParent(new HandleRef(this, Handle), new HandleRef(this, parent));
			_ParentedEditorWithParkingWindow = true;
		}
	}

	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg != Native.WM_NCPAINT || !DrawManager.DrawNCBorder(ref m))
		{
			base.WndProc(ref m);
		}
	}

	[UIPermission(SecurityAction.InheritanceDemand, Window = UIPermissionWindow.AllWindows)]
	protected override void CreateHandle()
	{
		base.CreateHandle();
		if (_ParentedEditorWithParkingWindow)
		{
			_ParentedEditorWithParkingWindow = false;
			if (IsWindow(_EditorHandle) && Handle != Native.GetParent(_EditorHandle))
			{
				SetParent(new HandleRef(this, _EditorHandle), new HandleRef(this, Handle));
			}
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.OnSizeChanged", "", null);
		if (_EditorHandle != IntPtr.Zero)
		{
			// Evs.Trace(GetType(), "AbstractShellTextEditorControl.OnSizeChanged", "adjusting text view size");

			Diag.ThrowIfNotOnUIThread();

			Rectangle clientRectangle = ClientRectangle;
			Native.SetWindowPos(_EditorHandle, IntPtr.Zero, clientRectangle.X, clientRectangle.Y, clientRectangle.Width, clientRectangle.Height, 4);
			OnSize();
		}

		base.OnSizeChanged(e);
	}

	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public override bool PreProcessMessage(ref Message msg)
	{
		return false;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);

		if (_EditorHandle != IntPtr.Zero)
			Native.SetFocus(_EditorHandle);
	}

	public void GetCoordinatesForPopupMenu(object[] vIn, out int x, out int y)
	{
		Point mousePosition = MousePosition;
		if (vIn != null && vIn.Length == 1 && vIn[0] != null)
		{
			x = (short)vIn[0];
			if (x == mousePosition.X)
			{
				y = mousePosition.Y;
				return;
			}

			try
			{
				IVsTextView currentView = CurrentView;
				___(currentView.GetCaretPos(out int piLine, out int piColumn));
				POINT[] array = new POINT[1];
				___(currentView.GetPointOfLineColumn(piLine, piColumn, array));

				if (Native.GetClientRect(currentView.GetWindowHandle(), out Native.UIRECTX val)
					&& (array[0].y < val.Top || array[0].x > val.Bottom || array[0].x < val.Left || array[0].y > val.Right))
				{
					array[0].x = 0;
					array[0].y = 0;
				}

				y = PointToScreen(new Point(array[0].x, array[0].y)).Y;
			}
			catch
			{
				y = mousePosition.Y;
			}
		}
		else
		{
			x = mousePosition.X;
			y = mousePosition.Y;
		}
	}

	public void CreateAndInitEditorWindow(object serviceProvider)
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.CreateAndInitEditorWindow", "", null);

		if (_TextBuffer == null)
		{
			CreateAndInitTextBuffer(serviceProvider, null);
		}

		Diag.ThrowIfNotOnUIThread();

		_OleServiceProvider = new ServiceProvider(serviceProvider as IOleServiceProvider);
		bool flag = true;
		if (EditorHandle == IntPtr.Zero)
		{
			CreateEditorWindow(serviceProvider);
			flag = false;
		}

		_VsTextManager ??= _OleServiceProvider.GetService(VS.CLSID_TextManager) as IVsTextManager;

		if (!flag)
		{
			SinkEventsAndCacheInterfaces();
		}

		if (_TextCmdTarget == null || _TextWindowPane == null || _VsTextManager == null)
		{
			Exception ex = new InvalidOperationException(ControlsResources.ExCannotInitNewEditorInst);
			Diag.ThrowException(ex);
		}
	}

	public void CreateAndInitTextBuffer(object sp, IVsTextStream existingDocData)
	{
		// Evs.Trace(GetType(), "AbstractShellTextEditorControl.CreateAndInitTextBuffer", "", null);
		if (existingDocData != null)
		{
			_TextBuffer = new(existingDocData, sp)
			{
				WithEncoding = _WithEncoding
			};
		}
		else
		{
			_TextBuffer = new()
			{
				WithEncoding = _WithEncoding
			};
			if (GetType().Name == "TextResultsViewContol")
			{
				_TextBuffer.InitContent = true;
			}

			_TextBuffer.SetSite(sp);
		}

		OnTextBufferCreated(_TextBuffer);
	}

	public static void ResetFontAndColor(Font font, Guid fontCategory, Guid colorCategory)
	{
		// Evs.Trace(typeof(AbstractShellTextEditorControl), "AbstractShellTextEditorControl.ResetFontAndColor", "", null);

		if (_VsTextManager != null)
		{
			Diag.ThrowIfNotOnUIThread();

			IConnectionPointContainer connectionPointContainer = _VsTextManager as IConnectionPointContainer;
			Microsoft.VisualStudio.OLE.Interop.CONNECTDATA[] array = new Microsoft.VisualStudio.OLE.Interop.CONNECTDATA[1];
			uint pcFetched = 1u;
			FONTCOLORPREFERENCES fONTCOLORPREFERENCES = default;
			IntPtr intPtr = (IntPtr)0;
			IntPtr intPtr2 = (IntPtr)0;
			try
			{
				Guid riid = typeof(IVsTextManagerEvents).GUID;
				connectionPointContainer.FindConnectionPoint(ref riid, out IConnectionPoint ppCP);
				ppCP.EnumConnections(out IEnumConnections ppEnum);
				intPtr = Marshal.AllocHGlobal(16);
				Marshal.Copy(fontCategory.ToByteArray(), 0, intPtr, 16);
				intPtr2 = Marshal.AllocHGlobal(16);
				Marshal.Copy(colorCategory.ToByteArray(), 0, intPtr2, 16);
				fONTCOLORPREFERENCES.pguidFontCategory = intPtr;
				fONTCOLORPREFERENCES.pguidColorCategory = intPtr2;
				if (font != null)
				{
					fONTCOLORPREFERENCES.hRegularViewFont = font.ToHfont();
					fONTCOLORPREFERENCES.hBoldViewFont = font.ToHfont();
				}

				while (pcFetched == 1)
				{
					___(ppEnum.Next(1u, array, out pcFetched));
					if (pcFetched == 1)
					{
						(array[0].punk as IVsTextManagerEvents).OnUserPreferencesChanged(null, null, null, [fONTCOLORPREFERENCES]);
					}
				}
			}
			catch (Exception e)
			{
				Diag.Ex(e);
			}
			finally
			{
				if (intPtr != (IntPtr)0)
				{
					Marshal.FreeHGlobal(intPtr);
				}

				if (intPtr2 != (IntPtr)0)
				{
					Marshal.FreeHGlobal(intPtr2);
				}
			}
		}
		else
		{
			// Evs.Trace(typeof(CommonUtils), "AbstractShellTextEditorControl.ResetFontAndColor", "s_vsTextManager is null");
		}
	}

	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);

	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool IsWindow(IntPtr hWnd);

	protected bool ShouldDistroyNativeControl()
	{
		if (Disposing)
		{
			return true;
		}

		for (Control parent = Parent; parent != null; parent = parent.Parent)
		{
			if (parent.Disposing)
			{
				return true;
			}

			if (parent.RecreatingHandle)
			{
				return false;
			}
		}

		return false;
	}

	protected void Release(object o)
	{
		if (Marshal.IsComObject(o))
		{
			for (int num = Marshal.ReleaseComObject(o); num > 0; num = Marshal.ReleaseComObject(o))
			{
			}
		}
	}

	protected void VerifyBeforeInstanceProperty()
	{
		if (IsEditorInstanceCreated)
		{
			InvalidOperationException ex = new("this property must be set BEFORE editor instance is created");
			Diag.Ex(ex);
			throw ex;
		}
	}

	public void OnSpecialEditorCommandEventHandler(object sender, SpecialEditorCommandEventArgs a)
	{
		if (a.CommandID == (int)VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU && ShowPopupMenuEvent != null)
		{
			ShowPopupMenuEvent(this, a);
		}
	}

	protected void ApplyLS(Guid lsGuid)
	{
		// Evs.Trace(GetType(), "SqlTextViewControl.ApplyLS", "lsGuid = {0}", lsGuid);

		if (_TextBuffer == null)
			return;

		if (MandatedXmlLanguageServiceClsid == lsGuid)
		{
			if (_TextBuffer.TextStream is IVsUserData vsUserData)
			{
				Guid riidKey = VS.CLSID_PropDisableXmlEditorPropertyWindowIntegration;
				___(vsUserData.SetData(ref riidKey, true));
				riidKey = VS.CLSID_PropOverrideXmlEditorSaveAsFileFilter;
				___(vsUserData.SetData(ref riidKey, ControlsResources.ShellTextViewControl_SaveAsXmlaFilterString));
			}
		}

		___(_TextBuffer.TextStream.SetLanguageServiceID(ref lsGuid));
	}

	protected abstract void CreateEditorWindow(object nativeSP);

	protected abstract void SinkEventsAndCacheInterfaces();

	protected abstract void UnsinkEventsAndFreeInterfaces();

	protected virtual void OnTextBufferCreated(ShellTextBuffer buf)
	{
	}
}
