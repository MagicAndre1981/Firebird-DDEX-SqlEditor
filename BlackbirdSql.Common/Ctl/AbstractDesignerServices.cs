﻿// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.DatabaseChangesManager
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Model.Enums;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common.Ctl;

// =========================================================================================================
//
//										AbstractDesignerServices Class
//
/// <summary>
/// Base class service for handling open query commands.
// =========================================================================================================
public abstract class AbstractDesignerServices
{

	// -----------------------------------------------------------------------------------------------------
	#region Constructors / Destructors - AbstractDesignerServices
	// -----------------------------------------------------------------------------------------------------

	public AbstractDesignerServices()
	{
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractDesignerServices
	// =========================================================================================================


	protected static Guid _DslEditorFactoryClsid = Guid.Empty;
	protected static event EventHandler<BeforeOpenDocumentEventArgs> _S_BeforeOpenDocumentEvent;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractDesignerServices
	// =========================================================================================================


	public static event EventHandler<BeforeOpenDocumentEventArgs> SBeforeOpenDocumentEvent
	{
		add { _S_BeforeOpenDocumentEvent += value; }
		remove { _S_BeforeOpenDocumentEvent -= value; }
	}


	protected static EventHandler<BeforeOpenDocumentEventArgs> S_BeforeOpenDocumentHandler => _S_BeforeOpenDocumentEvent;

	
	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractDesignerServices
	// =========================================================================================================






	// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// Microsoft.Data.Tools.Schema.Common.Threading.EventMarshaler:Invoke
	protected static void ExecuteDocumentLoadedCallback(Delegate method, params object[] args)
	{
		if (method == null)
			return;

		try
		{
			method.DynamicInvoke(args);
		}
		catch (MemberAccessException ex)
		{
			Diag.Dug(ex);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): Could not access target member {0}", ex);
			throw;
		}
		catch (TargetException ex2)
		{
			Diag.Dug(ex2);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): Attempted to invoke on null target or the member does not exist {0}", ex2);
			throw;
		}
		catch (TargetInvocationException ex3)
		{
			Diag.Dug(ex3);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): The target threw an exception.  {0}", ex3);
			if (ex3.InnerException != null)
			{
				Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "EventMarshaler.Invoke(...): The target threw an exception.  Inner exception {0}", ex3.InnerException);
				Exception innerException = ex3.InnerException;
				innerException.Data.Add("TargetSiteCallstack", innerException.StackTrace);
				innerException.Data.Add("OriginalException", ex3);
				ExceptionDispatchInfo.Capture(innerException).Throw();
			}
			throw;
		}
		catch (Exception ex4)
		{
			Diag.Dug(ex4);
			Tracer.TraceEvent(TraceEventType.Error, EnSqlTraceId.CoreServices, "Exception in EventMarshaler.Invoke(...): " + ex4.ToString());
			throw;
		}
	}



	public static void SetDocumentReadOnly(uint docCookie, bool readOnly)
	{
		if (RdtManager.TryGetDocDataFromCookie(docCookie, out var docData))
		{
			((IVsPersistDocData2)docData).SetDocDataReadOnly(readOnly ? 1 : 0);

			if (RdtManager.ServiceAvailable)
			{
				uint grfDocChanged = (uint)(readOnly
					? _VSRDTFLAGS4.RDT_PendingInitialization
					: _VSRDTFLAGS4.RDT_PendingHierarchyInitialization);

				RdtManager.NotifyDocumentChanged(docCookie, grfDocChanged);
			}
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handling - AbstractDesignerServices
	// =========================================================================================================



	protected static void RaiseBeforeOpenDocument(string mkDocument, DbConnectionStringBuilder csb,
		IList<string> identifierList, EnModelObjectType elementType, EnModelTargetType targetType,
		EventHandler<BeforeOpenDocumentEventArgs> handlers)
	{
		if (handlers == null)
			return;

		BeforeOpenDocumentEventArgs e = new BeforeOpenDocumentEventArgs(mkDocument, csb,
			identifierList, elementType, targetType);

		handlers(null, e);
	}


	#endregion Event handling
}
