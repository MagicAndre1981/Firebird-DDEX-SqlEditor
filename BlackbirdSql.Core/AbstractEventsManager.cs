﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdSql.Core.Ctl.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;



namespace BlackbirdSql.Core;


// =========================================================================================================
//										AbstractEventsManager Class
//
/// <summary>
/// Base class for and extension or service ide event handling manager.
/// </summary>
/// <remarks>
/// Each service or extension that handles ide events should derive an events manager from
/// AbstractEventsManager. All solution, rdt and selection events are routed through PackageController.
/// The events manager can handle an ide event by hooking onto Controller.On[event]. 
/// </remarks>
// =========================================================================================================
public abstract class AbstractEventsManager : IBEventsManager
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractEventsManager
	// ---------------------------------------------------------------------------------

	protected AbstractEventsManager(IBPackageController controller)
	{
		if (InternalInstance != null)
		{
			TypeInitializationException ex = new TypeInitializationException(GetType().FullName, new ArgumentException("Instance already exists."));
			Diag.Dug(ex);
			throw ex;
		}

		InternalInstance = this;
		_Controller = controller;

		controller.RegisterEventsManager(this);
		Initialize();
	}


	/// <summary>
	/// AbstractEventsManager disposal
	/// </summary>
	/// <remarks>
	/// Example
	/// <code>_Controller.OnExampleEvent -= OnExample;</code>
	/// </remarks>
	public abstract void Dispose();


	#endregion Constructors / Destructors




	// =========================================================================================================
	#region Fields - BlackbirdSqlDdexExtension
	// =========================================================================================================

	private readonly IBPackageController _Controller;

	protected string _TaskHandlerTaskName = "Task";
	protected TaskProgressData _ProgressData = default;
	protected ITaskHandler _TaskHandler = null;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractEventsManager
	// =========================================================================================================

	/// <summary>
	/// Access to the static at the instance local level. This allows the base class to access and update
	/// the localized static instance.
	/// </summary>
	protected abstract IBEventsManager InternalInstance { get; set; }

	public IBPackageController Controller => _Controller;
	public IBAsyncPackage DdexPackage => _Controller.DdexPackage;
	public IVsMonitorSelection SelectionMonitor => _Controller.SelectionMonitor;


	/// <summary>
	/// The name of the running task if the object is currently using the task handler.
	/// </summary>
	public string TaskHandlerTaskName => _TaskHandlerTaskName;


	#endregion Property accessors




	// =========================================================================================================
	#region Methods - AbstractEventsManager
	// =========================================================================================================


	/// <summary>
	/// ThrowOnFailure token
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Initializes the event manager
	/// validation.
	/// </summary>
	/// Example
	/// <code>_Controller.OnExampleEvent += OnExample;</code>
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public abstract void Initialize();


	#endregion Methods




	// =========================================================================================================
	#region Utility Methods - AbstractEventsManager
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Provides a callback to the client's ProgressData.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public TaskProgressData GetProgressData()
	{
		return _ProgressData;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar
	/// with updated TaskHandlerTaskName information.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public ITaskHandler GetTaskHandler()
	{
		return _TaskHandler;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar
	/// with project update information.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public virtual bool TaskHandlerProgress(string text)
	{
		if (_ProgressData.PercentComplete == null)
		{
			_ProgressData.PercentComplete = 0;
			if (_TaskHandler != null)
				Diag.TaskHandlerProgress(this, "Started");
		}

		Diag.TaskHandlerProgress(this, text);

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Moves back onto the UI thread and updates the IDE task handler progress bar.
	/// </summary>
	/// <param name="progress">The % completion of TaskHandlerTaskName.</param>
	/// <param name="elapsed">The time taken to complete the stage.</param>
	// ---------------------------------------------------------------------------------
	public virtual bool TaskHandlerProgress(int progress, int elapsed)
	{
		bool completed = false;
		string text;

		if (progress == 0)
		{
			text = "Started";
		}
		else if (progress == 100)
		{
			completed = true;
			text = $"Completed. Validation took {elapsed}ms.";
		}
		else if (_TaskHandler.UserCancellation.IsCancellationRequested)
		{
			completed = true;
			text = $"Cancelled. {progress}% completed. Validation took {elapsed}ms.";
		}
		else
		{
			text = $"{progress}% completed after {elapsed}ms.";
		}

		_ProgressData.PercentComplete = progress;

		Diag.TaskHandlerProgress(this, text, completed);

		return true;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the VisualStudio status bar.
	/// </summary>
	/// <param name="message">Thye status bar display text.</param>
	/// <param name="complete">Tags this message as a completion message.</param>
	/// <returns></returns>
	// ---------------------------------------------------------------------------------
	public virtual bool UpdateStatusBar(string message, bool complete = false)
	{
		return Diag.UpdateStatusBar(message, complete);
	}


	#endregion Utility Methods




	// =========================================================================================================
	#region IVs Events Implementation and Event handling - AbstractEventsManager
	// =========================================================================================================


	// Example
	// public int OnExample(int arg)


	#endregion IVs Events Implementation and Event handling


}