﻿// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.Host

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

using IObjectWithSite = Microsoft.VisualStudio.OLE.Interop.IObjectWithSite;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;


namespace BlackbirdSql.Core.Ctl;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "Class is UIThread compliant.")]

/// <summary>
/// Editor related Host services.
/// </summary>
public class Hostess : AbstractHostess
{


	public Hostess(IServiceProvider serviceProvider) : base(serviceProvider)
	{
		Tracer.Trace(GetType(), "Hostess.Hostess");
	}


	/// <summary>
	/// Activates or opens a virtual file for editing
	/// </summary>
	/// <param name="fileIdentifier"></param>
	/// <param name="doNotShowWindowFrame"></param>
	/// <returns></returns>
	internal IVsWindowFrame ActivateOrOpenVirtualDocument(IVsDataExplorerNode node, bool doNotShowWindowFrame)
	{
		Tracer.Trace(GetType(), "Hostess.ActivateOrOpenVirtualDocument");

		int result;

		MonikerAgent moniker = new(node);
		string source = MonikerAgent.GetDecoratedDdlSource(node, false);
		string mkDocument = moniker.DocumentMoniker;
		string path = moniker.ToPath(UserDataDirectory);
		uint grfIDO = (uint)__VSIDOFLAGS.IDO_ActivateIfOpen;


		IVsWindowFrame vsWindowFrame;

		Guid logicalViewGuid = VSConstants.LOGVIEWID_TextView;

		string physicalView = null;

		IVsUIHierarchy hierarchy;

		/*
		if (path.Length > 260)
		{
			NotSupportedException ex = new(Resources.SqlViewTextObjectCommandProvider_PathTooLong);
			Diag.Dug(ex);
			throw ex;
		}
		*/
		if (doNotShowWindowFrame)
			grfIDO = 0u;

		IVsUIShellOpenDocument service;
		try
		{
			service = HostService.GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (service == null)
		{
			ServiceUnavailableException ex = new(typeof(IVsUIShellOpenDocument));
			Diag.Dug(ex);
			throw ex;
		}

		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		// Check if the document is already open.
		try
		{
			_ = Native.WrapComCall(service.IsDocumentOpen(null, uint.MaxValue, mkDocument, ref logicalViewGuid,
				grfIDO, out hierarchy, null, out vsWindowFrame, out _));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (vsWindowFrame != null)
		{
			RegisterHierarchy(hierarchy);
			return vsWindowFrame;
		}

		if (!Directory.Exists(Path.GetDirectoryName(path)))
			Directory.CreateDirectory(Path.GetDirectoryName(path));


		if (File.Exists(path))
			File.SetAttributes(path, FileAttributes.Normal);


		FileStream fileStream = File.Open(path, FileMode.Create);
		using (fileStream)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(source);
			fileStream.Write(bytes, 0, bytes.Length);
		}

		File.SetAttributes(path, FileAttributes.Normal);


		try
		{
			// This will return the IOleServiceProvider, the hierarchy, it's itemid and window frame
			result = service.OpenDocumentViaProjectWithSpecific(path,
				(uint)(__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen | __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor | __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseView),
				MandatedSqlEditorFactoryClsid, physicalView, logicalViewGuid, out IOleServiceProvider ppSP, out hierarchy, out uint itemId,
				out vsWindowFrame);

			if (result != VSConstants.S_OK)
			{
				InvalidOperationException ex = new($"OpenDocumentViaProjectWithSpecific [{result}]");
				throw ex;
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			File.Delete(path);
			throw ex;
		}

		if (vsWindowFrame == null)
		{
			InvalidOperationException ex = new($"OpenDocumentViaProjectWithSpecific returned a null window frame [{result}].");
			Diag.Dug(ex);
			File.Delete(path);
			throw ex;
		}

		RegisterHierarchy(hierarchy);


		object pvar;

		try
		{
			Native.WrapComCall(vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out pvar));
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			File.Delete(path);
			throw ex;
		}

		IVsTextLines vsTextLines = pvar as IVsTextLines;
		if (vsTextLines == null)
		{
			(pvar as IVsTextBufferProvider)?.GetTextBuffer(out vsTextLines);
		}

		if (vsTextLines != null)
		{
			try
			{
				Native.WrapComCall(vsTextLines.GetStateFlags(out uint pdwReadOnlyFlags));
				pdwReadOnlyFlags |= (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY;
				Native.WrapComCall(vsTextLines.SetStateFlags(pdwReadOnlyFlags));
				if (MandatedSqlLanguageServiceClsid != Guid.Empty)
					vsTextLines.SetLanguageServiceID(MandatedSqlLanguageServiceClsid);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				File.Delete(path);
				throw ex;
			}
		}

		if (vsWindowFrame != null && !doNotShowWindowFrame)
		{
			try
			{
				Native.WrapComCall(vsWindowFrame.Show());
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				File.Delete(path);
				throw ex;
			}
		}

		// File.Delete(path);

		return vsWindowFrame;
	}


	internal void RegisterHierarchy(IVsUIHierarchy hierarchy)
	{
		IBPackageController controller;

		try
		{
			controller = GetService<IBPackageController>();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}

		if (controller == null)
		{
			ServiceUnavailableException ex = new(typeof(IBPackageController));
			Diag.Dug(ex);
			throw ex;
		}

		controller.RegisterMiscHierarchy(hierarchy);
	}




	/// <summary>
	/// Loads a string into a docData text buffer.
	/// </summary>
	protected int CreateDocDataFromText(Package package, Guid langGuid, string text, out IntPtr docData)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(exc);
			throw exc;
		}

		// Create a new IVsTextLines buffer.
		Type persistDocDataType = typeof(IVsPersistDocData);
		Guid riid = persistDocDataType.GUID;
		Guid clsid = typeof(VsTextBufferClass).GUID;

		// Create the text buffer
		IVsPersistDocData docDataObject = package.CreateInstance(ref clsid, ref riid, persistDocDataType) as IVsPersistDocData;

		// Site the buffer
		IObjectWithSite objectWithSite = (IObjectWithSite)docDataObject!;
		objectWithSite.SetSite(package);
		// objectWithSite.SetSite(ServiceProvider.GetService(typeof(IOleServiceProvider)));

		// Cast the buffer to a textlines buffer to load the text.
		IVsTextLines textLines = (IVsTextLines)docDataObject;
		textLines.InitializeContent(text, text.Length);

		if (langGuid != Guid.Empty)
			textLines.SetLanguageServiceID(langGuid);

		docData = Marshal.GetIUnknownForObject(docDataObject);

		return VSConstants.S_OK;

	}

	/// <summary>
	/// Not operational. Do not use
	/// </summary>
	public IVsWindowFrame CreateDocumentWindow(int attributes, string documentMoniker, string ownerCaption,
		string editorCaption, Guid editorType, string physicalView, Guid commandUIGuid, object documentView,
		object documentData, int owningItemId, IVsUIHierarchy owningHierarchy, IOleServiceProvider serviceProvider)
	{
		IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(documentView);
		IntPtr iUnknownForObject2 = Marshal.GetIUnknownForObject(documentData);
		try
		{
			IVsUIShell service = HostService.GetService<SVsUIShell, IVsUIShell>();
			if (service == null)
			{
				ServiceUnavailableException ex = new(typeof(IVsUIShell));
				Diag.Dug(ex);
				throw ex;
			}

			if (!ThreadHelper.CheckAccess())
			{
				COMException exc = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
				Diag.Dug(exc);
				throw exc;
			}

			Native.WrapComCall(service.CreateDocumentWindow((uint)attributes, documentMoniker, owningHierarchy, (uint)owningItemId, iUnknownForObject, iUnknownForObject2, ref editorType, physicalView, ref commandUIGuid, serviceProvider, ownerCaption, editorCaption, null, out IVsWindowFrame ppWindowFrame));
			return ppWindowFrame;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			Marshal.Release(iUnknownForObject2);
			Marshal.Release(iUnknownForObject);
		}
	}

}
