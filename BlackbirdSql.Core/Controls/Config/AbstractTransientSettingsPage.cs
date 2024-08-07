﻿//
// Plagiarized from Community.VisualStudio.Toolkit extension
//
using System.Runtime.InteropServices;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model.Config;
using Microsoft.VisualStudio.Shell;


namespace BlackbirdSql.Core.Controls.Config;

// =========================================================================================================
//										AbstractTransientSettingsPage Class
//
/// <summary>
/// Base class for live user options settings which can be used with an implementation of
/// <see cref="AbstractTransientSettingsDialog"/>.
/// </summary>
// =========================================================================================================
[ComVisible(true)]
public abstract class AbstractTransientSettingsPage<TPage, T> : AbstractSettingsPage<T>
	where TPage : AbstractSettingsPage<T> where T : AbstractSettingsModel<T>, new()

{

	public AbstractTransientSettingsPage(IBsTransientSettings transientSettings)
	{
		// Tracer.Trace(GetType(), ".ctor");

		_Model = ThreadHelper.JoinableTaskFactory.Run(() => AbstractSettingsModel<T>.CreateAsync(transientSettings));
		_Model.SettingsResetEvent += OnResetSettings;

		EditControlGotFocusEvent += _Model.OnEditControlGotFocus;
		EditControlLostFocusEvent += _Model.OnEditControlLostFocus;
		AutomatorPropertyValueChangedEvent += _Model.OnAutomatorPropertyValueChanged;
	}


	protected override void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		if (_Model != null)
		{
			EditControlGotFocusEvent -= _Model.OnEditControlGotFocus;
			EditControlLostFocusEvent -= _Model.OnEditControlLostFocus;
			AutomatorPropertyValueChangedEvent -= _Model.OnAutomatorPropertyValueChanged;
			_Model = null;
		}

		base.Dispose(disposing);
	}

}
