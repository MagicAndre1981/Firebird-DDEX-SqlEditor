﻿using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using EnvDTE;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;


namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.PackageControllerGuid)]
#if ASYNCRDTEVENTS_ENABLED
public interface IBsPackageController : IVsSolutionEvents3,
	IVsSelectionEvents, IVsRunningDocTableEvents3, IVsRunningDocTableEvents4, IVsRunningDocTableEvents7, IDisposable
#else
public interface IBsPackageController : IVsSolutionEvents3, // IVsSolutionEvents2, IVsSolutionEvents, */
	IVsSelectionEvents, IVsRunningDocTableEvents3, IVsRunningDocTableEvents4, IDisposable
#endif
{

	// Rdt Event Delegates

	delegate int AfterAttributeChangeDelegate(uint docCookie, uint grfAttribs);
	delegate int AfterAttributeChangeExDelegate(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld,
		uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew);

	delegate int AfterDocumentWindowHideDelegate(uint docCookie, IVsWindowFrame pFrame);
	delegate int AfterSaveDelegate(uint docCookie);
	delegate IVsTask AfterSaveAsyncDelegate(uint cookie, uint flags);

	delegate int BeforeDocumentWindowShowDelegate(uint docCookie, int fFirstShow, IVsWindowFrame pFrame);
	delegate int BeforeLastDocumentUnlockDelegate(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining,
		uint dwEditLocksRemaining);
	delegate int BeforeSaveDelegate(uint docCookie);
	delegate IVsTask BeforeSaveAsyncDelegate(uint cookie, uint flags, IVsTask saveTask);

	// Solution Event Delegates
	delegate int AfterOpenProjectDelegate(Project project, int fAdded);
	delegate void LoadSolutionOptionsDelegate(Stream stream);
	delegate void SaveSolutionOptionsDelegate(Stream stream);
	delegate int AfterCloseSolutionDelegate(object pUnkReserved);
	delegate int BeforeCloseProjectDelegate(IVsHierarchy pHierarchy, int fRemoved);
	delegate int BeforeCloseSolutionDelegate(object pUnkReserved);
	delegate int QueryCloseProjectDelegate(IVsHierarchy hierarchy, int removing, ref int cancel);
	delegate int QueryCloseSolutionDelegate(object pUnkReserved, ref int pfCancel);

	// Selection Event Delegates
	delegate int CmdUIContextChangedDelegate(uint dwCmdUICookie, int fActive);
	delegate int ElementValueChangedDelegate(uint elementid, object varValueOld, object varValueNew);
	delegate int SelectionChangedDelegate(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld,
		ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew,
		ISelectionContainer pSCNew);

	// Custom EventDelegates
	delegate int NewQueryRequestedDelegate(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType);


	// Solution events
	event AfterOpenProjectDelegate OnAfterOpenProjectEvent;

	event LoadSolutionOptionsDelegate OnLoadSolutionOptionsEvent;
	event SaveSolutionOptionsDelegate OnSaveSolutionOptionsEvent;

	event AfterCloseSolutionDelegate OnAfterCloseSolutionEvent;
	event BeforeCloseProjectDelegate OnBeforeCloseProjectEvent;
	event BeforeCloseSolutionDelegate OnBeforeCloseSolutionEvent;
	event QueryCloseProjectDelegate OnQueryCloseProjectEvent;
	event QueryCloseSolutionDelegate OnQueryCloseSolutionEvent;

	// Rdt events
	event AfterAttributeChangeDelegate OnAfterAttributeChangeEvent;
	event AfterAttributeChangeExDelegate OnAfterAttributeChangeExEvent;
	event AfterDocumentWindowHideDelegate OnAfterDocumentWindowHideEvent;
	event AfterSaveDelegate OnAfterSaveEvent;
	event AfterSaveAsyncDelegate OnAfterSaveAsyncEvent;
	event BeforeDocumentWindowShowDelegate OnBeforeDocumentWindowShowEvent;
	event BeforeLastDocumentUnlockDelegate OnBeforeLastDocumentUnlockEvent;
	event BeforeSaveDelegate OnBeforeSaveEvent;
	event BeforeSaveAsyncDelegate OnBeforeSaveAsyncEvent;

	// Selection Events
	event CmdUIContextChangedDelegate OnCmdUIContextChangedEvent;
	event ElementValueChangedDelegate OnElementValueChangedEvent;
	event SelectionChangedDelegate OnSelectionChangedEvent;

	// Custom Events
	event NewQueryRequestedDelegate OnNewQueryRequestedEvent;

	string UserDataDirectory { get; }

	IBsAsyncPackage PackageInstance { get; }

	DTE Dte { get; }

	object SolutionObject { get; }

	bool SolutionValidating { get; }

	IVsSolution VsSolution { get; }

	public bool IsToolboxInitialized { get; }


	bool IsCmdLineBuild { get; }


	IVsMonitorSelection SelectionMonitor { get; }

	IVsTaskStatusCenterService StatusCenterService { get; }


	void RegisterEventsManager(IBsEventsManager manager);


	IAsyncServiceContainer Services { get; }


	uint ToolboxCmdUICookie { get; }

	object LockGlobal { get; }




	abstract bool AdviseEvents();
	Task<bool> AdviseEventsAsync();


	void DisableRdtEvents();

	void EnableRdtEvents();

	string CreateConnectionUrl(IDbConnection connection);
	string CreateConnectionUrl(string connectionString);

	void EnsureMonitorSelection();

	TInterface EnsureService<TService, TInterface>() where TInterface : class;

	TInterface GetService<TService, TInterface>() where TInterface : class;

	Task<TInterface> GetServiceAsync<TService, TInterface>() where TInterface : class;

	Task<IVsTaskStatusCenterService> GetStatusCenterServiceAsync();

	string GetRegisterConnectionDatasetKey(IDbConnection connection);
	void InvalidateRctManager();
	bool IsConnectionEquivalency(string connectionString1, string connectionString2);
	bool IsWeakConnectionEquivalency(string connectionString1, string connectionString2);

	void ValidateSolution();


	void OnLoadSolutionOptions(Stream stream);
	int OnNewQueryRequested(IVsDataViewHierarchy site, EnNodeSystemType nodeSystemType);
	void OnSaveSolutionOptions(Stream stream);

}
