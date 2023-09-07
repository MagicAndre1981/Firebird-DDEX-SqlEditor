﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.MenuCommandsService

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Model.QueryExecution;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Constants = Microsoft.VisualStudio.OLE.Interop.Constants;




namespace BlackbirdSql.Common.Ctl;


public class MenuCommandsService : Collection<MenuCommand>, IDisposable, IMenuCommandService, Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget
{
	private ServiceProvider m_cachedServiceProvider;

	DesignerVerbCollection IMenuCommandService.Verbs
	{
		get
		{
			NotSupportedException ex = new();
			Diag.Dug(ex);
			throw ex;
		}
	}

	public MenuCommandsService()
	{
	}

	public MenuCommandsService(ServiceProvider serviceProvider)
	{
		m_cachedServiceProvider = serviceProvider;
	}

	public void Dispose()
	{
		if (m_cachedServiceProvider != null)
		{
			m_cachedServiceProvider.Dispose();
			m_cachedServiceProvider = null;
		}

		GC.SuppressFinalize(this);
	}

	void IMenuCommandService.AddVerb(DesignerVerb verb)
	{
		NotSupportedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}

	void IMenuCommandService.RemoveVerb(DesignerVerb verb)
	{
		NotSupportedException ex = new();
		Diag.Dug(ex);
		throw ex;
	}

	public void AddCommand(MenuCommand command)
	{
		Add(command);
	}

	public MenuCommand FindCommand(CommandID cmdID)
	{
		using (IEnumerator<MenuCommand> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				MenuCommand current = enumerator.Current;
				if (current.CommandID.Equals(cmdID))
				{
					return current;
				}
			}
		}

		return null;
	}


	public bool GlobalInvoke(CommandID commandID)
	{
		MenuCommand menuCommand = FindCommand(commandID);
		if (menuCommand != null)
		{
			menuCommand.Invoke();
			return true;
		}

		if (m_cachedServiceProvider != null)
		{
			IVsUIShell vsUIShell = (IVsUIShell)m_cachedServiceProvider.GetService(typeof(SVsUIShell));
			if (vsUIShell != null)
			{
				try
				{
					object pvaIn = null;
					Guid pguidCmdGroup = commandID.Guid;
					Native.ThrowOnFailure(vsUIShell.PostExecCommand(ref pguidCmdGroup, (uint)commandID.ID, 0u, ref pvaIn), (string)null);
					return true;
				}
				catch (Exception)
				{
				}
			}
		}

		return false;
	}

	public void RemoveCommand(MenuCommand command)
	{
		if (Contains(command))
		{
			Remove(command);
		}
	}

	public void ShowContextMenu(CommandID menuID, int x, int y)
	{
		if (m_cachedServiceProvider != null)
		{
			ShowContextMenu(m_cachedServiceProvider, menuID, x, y);
		}
	}

	public void ShowContextMenu(ServiceProvider sp, CommandID menuID)
	{
		Point mousePosition = Control.MousePosition;
		ShowContextMenu(sp, menuID, mousePosition.X, mousePosition.Y);
	}

	public void ShowContextMenu(ServiceProvider sp, CommandID menuID, int x, int y)
	{
		if (sp == null)
		{
			Exception ex = new ArgumentException("service provider cannot be null", "sp");
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		IOleComponentUIManager oleComponentUIManager = (IOleComponentUIManager)sp.GetService(typeof(SOleComponentUIManager));
		if (oleComponentUIManager != null)
		{
			POINTS pOINTS = default;
			pOINTS.x = (short)x;
			pOINTS.y = (short)y;
			Guid rclsidActive = menuID.Guid;
			Native.ThrowOnFailure(oleComponentUIManager.ShowContextMenu(VS.dwReserved, ref rclsidActive, menuID.ID, new POINTS[1] { pOINTS }, this), (string)null);
		}
	}

	public virtual int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		CommandID cmdID = new CommandID(pguidCmdGroup, (int)nCmdID);
		MenuCommand menuCommand = FindCommand(cmdID);
		if (menuCommand != null)
		{
			menuCommand.Invoke();
			return VSConstants.S_OK;
		}

		return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
	}

	public virtual int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, Microsoft.VisualStudio.OLE.Interop.OLECMD[] prgCmds, IntPtr pCmdText)
	{
		CommandID cmdID = new CommandID(pguidCmdGroup, (int)prgCmds[0].cmdID);
		MenuCommand menuCommand = FindCommand(cmdID);

		if (menuCommand != null)
		{
			prgCmds[0].cmdf = (uint)menuCommand.OleStatus;
			if (menuCommand is MenuCommandTextChanges)
			{
				MenuCommandTextChanges menuCommandTextChanges = menuCommand as MenuCommandTextChanges;
				if (OLECMDTEXT.GetText(pCmdText) != menuCommandTextChanges.Text)
				{
					OLECMDTEXT.SetText(pCmdText, menuCommandTextChanges.Text);
				}
			}

			if (prgCmds[0].cmdf == 0)
			{
				return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
			}

			return VSConstants.S_OK;
		}

		return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
	}

	public MenuCommandsService(MenuCommandsService value)
	{
		AddRange(value);
	}

	public MenuCommandsService(MenuCommand[] value)
	{
		AddRange(value);
	}

	public void AddRange(MenuCommand[] cmds)
	{
		for (int i = 0; i < cmds.Length; i++)
		{
			Add(cmds[i]);
		}
	}

	public void AddRange(MenuCommandsService value)
	{
		for (int i = 0; i < value.Count; i++)
		{
			Add(value[i]);
		}
	}
}