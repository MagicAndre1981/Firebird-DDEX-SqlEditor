﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.BaseEditorFactory/SqlEditorFactory

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;



namespace BlackbirdSql.EditorExtension.Ctl;

[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Uses Diag.ThrowIfNotOnUIThread()")]


public abstract class AbstractEditorFactory(bool withEncoding) : AbstruseEditorFactory(withEncoding)
{

	protected static volatile int _EditorId;


	protected static uint dacTSqlEditorLaunchingCookie;

	protected static IVsMonitorSelection _monitorSelection;


	public override Guid ClsidEditorFactory
	{
		get
		{
			if (WithEncoding)
			{
				return new Guid(SystemData.EditorEncodedFactoryGuid);
			}

			return new Guid(SystemData.EditorFactoryGuid);
		}
	}

	protected string EditorId { get; set; }


	public override int CreateEditorInstance(uint createFlags, string moniker, string physicalView, IVsHierarchy hierarchy, uint itemId, IntPtr existingDocData, out IntPtr intPtrDocView, out IntPtr intPtrDocData, out string caption, out Guid cmdUIGuid, out int result)
	{


		RctManager.EnsureLoaded();

		intPtrDocView = IntPtr.Zero;
		intPtrDocData = IntPtr.Zero;
		caption = "";
		cmdUIGuid = Guid.Empty;
		result = 1;

		EditorId = "Editor" + _EditorId++;

		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			Cursor current = Cursor.Current;
			try
			{
				if (!string.IsNullOrEmpty(physicalView) && physicalView != "CodeFrame")
				{
					ArgumentException ex = new("physicalView is not CodeFrame or empty: " + physicalView);
					Diag.Dug(ex);
					// Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "physicalView is not null or empty- returning E_INVALIDARG");
					return VSConstants.E_INVALIDARG;
				}

				uint flagSilentOrOpen = (uint)(__VSCREATEEDITORFLAGS.CEF_OPENFILE | __VSCREATEEDITORFLAGS.CEF_SILENT);

				if ((createFlags & flagSilentOrOpen) == 0)
				{
					ArgumentException ex = new("invalid create flags: " + createFlags);
					Diag.Dug(ex);
					// Tracer.Trace(GetType(), "IVsEditorFactory.CreateEditorInstance", "invalid create flags - returning E_INVALIDARG");
					return VSConstants.E_INVALIDARG;
				}

				IVsTextLines vsTextLines = null;
				if (existingDocData != IntPtr.Zero)
				{
					if (WithEncoding)
					{
						// DataException ex = new("Data is not encoding compatible");
						// Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					object objectForIUnknown = Marshal.GetObjectForIUnknown(existingDocData);
					vsTextLines = objectForIUnknown as IVsTextLines;
					if (vsTextLines == null)
					{
						DataException ex = new("Data is not IVsTextLines compatible");
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					if (objectForIUnknown is not IVsUserData)
					{
						DataException ex = new("Data is not IVsUserData compatible");
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

					Guid pguidLangService = Guid.Empty;
					vsTextLines.GetLanguageServiceID(out pguidLangService);

					if (!(pguidLangService == MandatedSqlLanguageServiceClsid)
						&& !(pguidLangService == VS.CLSID_LanguageServiceDefault))
					{
						InvalidOperationException ex = new("Invalid language service: " + pguidLangService);
						Diag.Dug(ex);
						return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
					}

				}

				result = 0;
				int result2 = VSConstants.E_FAIL;

				Cursor.Current = Cursors.WaitCursor;
				IVsTextLines vsTextLines2 = null;

				if (vsTextLines == null)
				{
					Diag.ThrowIfNotOnUIThread();

					Guid clsid = typeof(VsTextBufferClass).GUID;
					Guid iid = VSConstants.IID_IUnknown;
					object obj = ((AsyncPackage)ApcManager.PackageInstance).CreateInstance(ref clsid, ref iid, typeof(object));

					if (WithEncoding)
					{
						IVsUserData obj2 = obj as IVsUserData;
						Guid riidKey = VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingPromptOnLoad_guid;
						obj2.SetData(ref riidKey, 1u);
					}

					(obj as IObjectWithSite)?.SetSite(OleServiceProvider);
					vsTextLines2 = obj as IVsTextLines;
				}
				else
				{
					vsTextLines2 = vsTextLines;
				}

				if (vsTextLines2 == null)
					return result2;

				EnsureAuxilliaryDocData(hierarchy, moniker, vsTextLines2);


				TabbedEditorWindowPane editorPane = CreateTabbedEditorPane(vsTextLines2, moniker);


				intPtrDocView = Marshal.GetIUnknownForObject(editorPane);
				intPtrDocData = Marshal.GetIUnknownForObject(vsTextLines2);
				caption = string.Empty;

				cmdUIGuid = VSConstants.GUID_TextEditorFactory;

				Guid clsidLangService = MandatedSqlLanguageServiceClsid;

				vsTextLines2.SetLanguageServiceID(ref clsidLangService);
				IVsUserData obj3 = (IVsUserData)vsTextLines2;
				Guid riidKey2 = VSConstants.VsTextBufferUserDataGuid.VsBufferDetectLangSID_guid;
				___(obj3.SetData(ref riidKey2, false));

				return VSConstants.S_OK;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
				if (ex is NullReferenceException || ex is ApplicationException || ex is ArgumentException || ex is InvalidOperationException)
				{
					MessageCtl.ShowEx(SharedResx.BaseEditorFactory_FailedToCreateEditor, ex);
					return VSConstants.E_FAIL;
				}

				throw;
			}
			finally
			{
				Cursor.Current = current;
			}
		}
	}

	protected virtual TabbedEditorWindowPane CreateTabbedEditorPane(IVsTextLines vsTextLines, string moniker)
	{
		return new TabbedEditorWindowPane(ServiceProvider, EditorExtensionPackage.Instance, vsTextLines, moniker);
	}

	protected virtual void EnsureAuxilliaryDocData(IVsHierarchy hierarchy, string documentMoniker, object docData)
	{
		EditorExtensionPackage.Instance.EnsureAuxilliaryDocData(hierarchy, documentMoniker, docData);
	}


}
