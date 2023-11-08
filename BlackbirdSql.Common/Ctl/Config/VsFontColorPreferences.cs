﻿// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Integration.VsFontColorPreferences

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using System.Windows.Threading;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Win32;



namespace BlackbirdSql.Common.Ctl.Config;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]
[SuppressMessage("Usage", "VSTHRD001:Avoid legacy thread switching APIs")]
[SuppressMessage("Usage", "VSTHRD110:Observe result of async calls")]

public class VsFontColorPreferences : IVsTextManagerEvents, IDisposable
{
	private ConnectionPointCookie _TextManagerEventsCookie;

	private bool _IsFireEventPending;

	public static Font EnvironmentFont
	{
		get
		{
			Font font = null;

			if (Package.GetGlobalService(typeof(IUIService)) is IUIService iUIService)
			{
				font = iUIService.Styles["DialogFont"] as Font;

				if (font != null)
					return font;
			}

			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			if (Package.GetGlobalService(typeof(SUIHostLocale)) is IUIHostLocale2 iUIHostLocale)
			{
				UIDLGLOGFONT[] array = new UIDLGLOGFONT[1];
				ErrorHandler.ThrowOnFailure(iUIHostLocale.GetDialogFont(array));

				if (array.Length != 0)
					font = FontFromUIDLGLOGFONT(array[0]);
			}

			return font;
		}
	}

	public event EventHandler PreferencesChangedEvent;

	public VsFontColorPreferences()
	{
		IVsTextManager source = Package.GetGlobalService(typeof(SVsTextManager)) as IVsTextManager;
		_TextManagerEventsCookie = new ConnectionPointCookie(source, this, typeof(IVsTextManagerEvents));
		SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
	}

	~VsFontColorPreferences()
	{
		SqlTracer.AssertTraceEvent(condition: false, TraceEventType.Warning, EnSqlTraceId.VSShell, "VsFontColorPreferences must be explictly disposed");
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (_TextManagerEventsCookie != null)
			{
				_TextManagerEventsCookie.Dispose();
				_TextManagerEventsCookie = null;
			}
			SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
		}
	}

	void IVsTextManagerEvents.OnRegisterMarkerType(int iMarkerType)
	{
	}

	void IVsTextManagerEvents.OnRegisterView(IVsTextView pView)
	{
	}

	void IVsTextManagerEvents.OnUnregisterView(IVsTextView pView)
	{
	}

	void IVsTextManagerEvents.OnUserPreferencesChanged(VIEWPREFERENCES[] pViewPrefs, FRAMEPREFERENCES[] pFramePrefs, LANGPREFERENCES[] pLangPrefs, FONTCOLORPREFERENCES[] pColorPrefs)
	{
		if (pColorPrefs != null)
		{
			FireVsFontColorPreferencesChanged();
		}
	}

	private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		if (e.Category == UserPreferenceCategory.General)
		{
			FireVsFontColorPreferencesChanged();
		}
	}

	private void FireVsFontColorPreferencesChanged()
	{
		if (_IsFireEventPending)
			return;

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		_IsFireEventPending = true;

		Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate
		{
			PreferencesChangedEvent?.Invoke(this, EventArgs.Empty);
			_IsFireEventPending = false;
		});
	}

	private static Font FontFromUIDLGLOGFONT(UIDLGLOGFONT logFont)
	{
		char[] array = new char[logFont.lfFaceName.Length];
		int num = 0;
		ushort[] lfFaceName = logFont.lfFaceName;
		foreach (ushort num2 in lfFaceName)
		{
			array[num++] = (char)num2;
		}
		string familyName = new string(array);
		float emSize = -logFont.lfHeight;
		FontStyle fontStyle = FontStyle.Regular;
		if (logFont.lfItalic > 0)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (logFont.lfUnderline > 0)
		{
			fontStyle |= FontStyle.Underline;
		}
		if (logFont.lfStrikeOut > 0)
		{
			fontStyle |= FontStyle.Strikeout;
		}
		if (logFont.lfWeight > 400)
		{
			fontStyle |= FontStyle.Bold;
		}
		GraphicsUnit unit = GraphicsUnit.Pixel;
		byte lfCharSet = logFont.lfCharSet;
		return new Font(familyName, emSize, fontStyle, unit, lfCharSet);
	}
}
