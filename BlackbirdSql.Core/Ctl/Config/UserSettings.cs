﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Ctl.Extensions;
using BlackbirdSql.Core.Ctl.Interfaces;


namespace BlackbirdSql.Core.Ctl.Config;

// =========================================================================================================
//										UserSettings Class
//
/// <summary>
/// Consolidated single access point for daisy-chained packages settings models (IBSettingsModel).
/// As a rule we name descendent classes UserSettings as well. We hardcode bind the UserSettings descendent
/// tree from the top-level extension lib down to the Core. There is no point using services as this
/// configuration is fixed. ie:
/// VisualStudio.Ddex > Controller > [Intermediate Libs] > Core.
/// Current intermediate libs are: EditorExtension > Common.
/// </summary>
// =========================================================================================================
public abstract class UserSettings : IBUserSettings
{

	// ---------------------------------------------------------------------------------
	#region Variables
	// ---------------------------------------------------------------------------------

	protected static IBUserSettings _Instance = null;
	protected static Dictionary<string, object> _SettingsStore;

	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - UserSettings
	// =========================================================================================================

	public virtual object this[string name]
	{
		get
		{
			object value = null;

			_SettingsStore?.TryGetValue(name, out value);

			return value;
		}

		set
		{
			SettingsStore[name] = value;
		}
	}


	protected static Dictionary<string, object> SettingsStore => _SettingsStore ??= new();


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Dug"/> calls are logged
	/// </summary>
	/// <remarks>
	/// Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static bool EnableDiagnostics => (bool)GetSetting("DdexGeneralEnableDiagnostics", true);
#else
	public static bool EnableDiagnostics => (bool)GetSetting("DdexGeneralEnableDiagnostics", false);
#endif


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Returns a boolean indicating whether or not task results can be written to the
	/// output window.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool EnableTaskLog => (bool)GetSetting("DdexGeneralEnableTaskLog", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Tracer calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
#if BLACKBIRD
	public static bool EnableTracer => (bool)GetSetting("DdexDebugEnableTracer", false);
#else
	public static bool EnableTracer => (bool)GetSetting("DdexDebugEnableTracer", true);
#endif

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not <see cref="Diag.Trace"/> calls are logged
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static bool EnableTrace => (bool)GetSetting("DdexDebugEnableTrace", true) | EnableTracer;
#else
	public static bool EnableTrace => (bool)GetSetting("DdexDebugEnableTrace", false) | EnableTracer;
#endif


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not Firebird debug library diagnostics calls are logged
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public static bool EnableFbDiagnostics => (bool)GetSetting("DdexDebugEnableFbDiagnostics", true);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not diagnostics calls are logged to the log file.
	/// </summary>
	/// <remarks>
	/// Only applies to the Debug configuration. Debug Exceptions are always logged.
	/// </remarks>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static bool EnableDiagnosticsLog => (bool)GetSetting("DdexDebugEnableDiagnosticsLog", true);
#else
	public static bool EnableDiagnosticsLog => (bool)GetSetting("DdexDebugEnableDiagnosticsLog", false);
#endif


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Flag indicating whether or not the extrapolated TObjectSupport.xml, after
	/// imports, must be saved to the diagnostics logfile directory.
	/// </summary>
	// ---------------------------------------------------------------------------------
#if DEBUG
	public static bool EnableSaveExtrapolatedXml => (bool)GetSetting("DdexDebugEnableSaveExtrapolatedXml", true);
#else
	public static bool EnableSaveExtrapolatedXml => (bool)GetSetting("DdexDebugEnableSaveExtrapolatedXml", false);
#endif


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string LogFile => (string)GetSetting("DdexDebugLogFile", "/temp/vsdiag.log");



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// The Firebird log file path
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static string FbLogFile => (string)GetSetting("DdexDebugFbLogFile", "/temp/vsdiagfb.log");




#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - UserSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private singleton .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected UserSettings()
	{
		if (_Instance != null)
		{
			ArgumentException ex = new("Singleton UserSettings instance already created");
			Diag.Dug(ex);
			throw ex;
		}

		_Instance = this;
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Private live .ctor
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected UserSettings(bool live)
	{
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the singleton UserSettings instance
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static IBUserSettings Instance
	{
		get
		{
			if (_Instance == null)
			{
				NullReferenceException ex = new("Cannot instantiate UserSettings from abstract ancestor");
				Diag.Dug(ex);
				throw ex;
			}

			return _Instance;
		}
	}


	#endregion Constructors / Destructors



	// =========================================================================================================
	#region Methods - UserSettings
	// =========================================================================================================


	protected static object GetSetting(string name, object defaultValue)
	{
		try
		{
			if (_SettingsStore == null || !_SettingsStore.TryGetValue(name, out object value))
				value = defaultValue;

			return value;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}



	protected static bool HasSetting(string name)
	{
		return _SettingsStore != null && _SettingsStore.ContainsKey(name);
	}



	/// <summary>
	/// Adds the extension's SettingsSavedDelegate to a package settings models SettingsSavedEvents.
	/// Only implemented by packages that have settings models.
	/// </summary>
	public abstract void RegisterSettingsEventHandlers(IBUserSettings.SettingsSavedDelegate onSettingsSavedDelegate);


	/// <summary>
	/// Only implemented by packages that have settings models. Whenever a package
	/// settings model is saved it fires the extension's OnSettingsSaved event.
	/// That event handler then requests each package to populate SettingsEventArgs
	/// if it has settings relevant to the model.
	/// PopulateSettingsEventArgs is also called on loading by the extension without
	/// a specific model specified for a universal request for settings.
	/// </summary>
	public abstract bool PopulateSettingsEventArgs(ref PropagateSettingsEventArgs args);


	#endregion Methods




	// =========================================================================================================
	#region Event handlers - UserSettings
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Settings saved event handler - only the final descendent class implements this.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public abstract void OnSettingsSaved(object sender);



	/// <summary>
	/// Updates settings used by a library. This method will be initiated by the
	/// extension package and passed down through the chain of dll's to the Core.
	/// A dll will update settings relevant to itself from here.
	/// IOW these are push notifications of any settings loaded or saved throughout the
	/// extension.
	/// </summary>
	public virtual void PropagateSettings(PropagateSettingsEventArgs e)
	{
		foreach (MutablePair<string, object> pair in e.Arguments)
		{
			this[pair.Key] = pair.Value;
		}
	}

	#endregion Event handlers


}