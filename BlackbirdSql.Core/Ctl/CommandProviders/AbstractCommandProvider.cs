﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.Properties;


using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;

using EnNodeSystemType = BlackbirdSql.Core.Ctl.CommandProviders.CommandProperties.EnNodeSystemType;


namespace BlackbirdSql.Core.Ctl.CommandProviders;

// =========================================================================================================
//										AbstractCommandProvider Class
//
/// <summary>
/// The base IVsDataViewCommandProvider class
/// </summary>
// =========================================================================================================
public abstract class AbstractCommandProvider : DataViewCommandProvider
{

	// ---------------------------------------------------------------------------------
	#region Variables - AbstractCommandProvider
	// ---------------------------------------------------------------------------------


	private Hostess _Host;

	private readonly EnNodeSystemType _CommandNodeSystemType = EnNodeSystemType.None;


	#endregion Variables





	// =========================================================================================================
	#region Property Accessors - AbstractCommandProvider
	// =========================================================================================================


	// protected abstract Package DdexPackage { get; }
	protected IBAsyncPackage DdexPackage => Controller.DdexPackage;

	/// <summary>
	/// Abstract accessor to the command <see cref="EnNodeSystemType"/>.
	/// Identifies whether the target SE node is is a User, System or Global node.
	/// </summary>
	protected EnNodeSystemType CommandNodeSystemType => _CommandNodeSystemType;


	/// <summary>
	/// IDE host access class object
	/// </summary>
	protected Hostess Host
	{
		get
		{
			_Host ??= new(Site.ServiceProvider);

			return _Host;
		}
	}


	#endregion Property Accessors


	public AbstractCommandProvider(EnNodeSystemType nodeType) : base()
	{
		_CommandNodeSystemType = nodeType;
	}



	// =========================================================================================================
	#region Methods - AbstractCommandProvider
	// =========================================================================================================



	protected static bool CanExecute(IVsDataExplorerNode node)
	{
		if (node != null)
		{
			IVsDataObject @object = node.Object;
			if (@object == null || !MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.StoredProcedure,
				EnModelObjectType.Function))
			{
				return false;
			}
		}

		return true;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source that can be opened in an IDE
	/// editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool CanOpen(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.Index, EnModelObjectType.ForeignKey))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
				{
					return true;
				}
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}

		}

		return false;
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source node object can be altered in an
	/// IDE editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool CanAlter(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Trigger")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}
		}

		return false;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// True if the node has an expression or source that can be opened in an IDE
	/// editor window else false.
	/// </summary>
	// ---------------------------------------------------------------------------------
	internal static bool HasScript(IVsDataExplorerNode node)
	{
		if (node != null && node.Object != null)
		{
			IVsDataObject @object = node.Object;

			if (@object.Type.Name.EndsWith("Column") || @object.Type.Name.EndsWith("Parameter")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.Index, EnModelObjectType.ForeignKey))
			{
				if ((bool)@object.Properties["IS_COMPUTED"])
				{
					return true;
				}
			}
			else if (@object.Type.Name.EndsWith("Trigger")
				|| MonikerAgent.ModelObjectTypeIn(@object, EnModelObjectType.View, EnModelObjectType.StoredProcedure, EnModelObjectType.Function))
			{
				return true;
			}

		}

		return false;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a command and delegate based on the SE itemId and commandId
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override MenuCommand CreateCommand(int itemId, CommandID commandId, object[] parameters)
	{
		// Tracer.Trace(GetType(), "AbstractCommandProvider.CreateCommand", "itemId: {0}, commandId: {1}", itemId, commandId);
		
		MenuCommand command = null;
		DataViewMenuCommand cmd = null;
		EnNodeSystemType commandNodeSystemType = EnNodeSystemType.None;
		IVsDataExplorerNode node;


		if (commandId.Equals(CommandProperties.NewQuery))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				node = Site.ExplorerConnection.FindNode(itemId);

				if (node == null)
				{
					// Tracer.Trace(GetType(), "AbstractCommandProvider.CreateCommand", "itemId: {0}, commandId: {1}. Node is null. Exiting.", itemId, commandId);
					return;
				}

				commandNodeSystemType = _CommandNodeSystemType;

				if (commandNodeSystemType < EnNodeSystemType.User)
				{
					EnNodeSystemType currentNodeSystemType = GetUnknownNodeSystemType(node);

					if (currentNodeSystemType != EnNodeSystemType.None)
						commandNodeSystemType = currentNodeSystemType;
				}
				if (cmd.Visible)
				{
					cmd.Properties["Text"] = GetResourceString("New", "Query", commandNodeSystemType);
				}
			}, delegate
			{
				if (itemId == int.MaxValue)
					OnNewQuery(itemId, commandNodeSystemType);
				else
					OnInterceptorNewQuery(itemId, commandNodeSystemType);
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.OpenTextObject))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
				cmd.Visible = HasScript(node);
				cmd.Enabled = CanOpen(node);

				if (cmd.Visible)
				{
					node = Site.ExplorerConnection.FindNode(itemId);
					cmd.Properties["Text"] = GetResourceString("Open", "Script", node);
				}
			}, delegate
			{
				OnOpen(itemId, false);
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.OpenAlterTextObject))
		{
			cmd = new DataViewMenuCommand(itemId, commandId, delegate
			{
				IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);
				cmd.Visible = HasScript(node);
				cmd.Enabled = CanAlter(node);

				if (cmd.Visible)
				{
					node = Site.ExplorerConnection.FindNode(itemId);

					cmd.Properties["Text"] = GetResourceString("Alter", "Script", node);
				}
			}, delegate
			{
				OnOpen(itemId, true);
			});
			command = cmd;
		}
		else if (commandId.Equals(CommandProperties.RightClick))
		{
			// Not working
			// Tracer.Trace(GetType(), "CreateCommand", "RightClick");
			command = base.CreateCommand(itemId, commandId, parameters);
		}
		else if (commandId.Equals(CommandProperties.DoubleClick))
		{
			// Not working
			// Tracer.Trace(GetType(), "CreateCommand", "DoubleClick");
			command = base.CreateCommand(itemId, commandId, parameters);
		}
		else if (commandId.Equals(CommandProperties.EnterKey))
		{
			// Not working
			// Tracer.Trace(GetType(), "CreateCommand", "EnterKey");
			command = base.CreateCommand(itemId, commandId, parameters);
		}
		else
		{
			command = base.CreateCommand(itemId, commandId, parameters);
		}


		return command;

	}


	/// <summary>
	/// Determines the IsSystemObject type of View, Procedure and Function node lists
	/// which may be mixed.
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	internal static EnNodeSystemType GetUnknownNodeSystemType(IVsDataExplorerNode node)
	{
		if (node == null || node.Object == null)
			return EnNodeSystemType.None;

		IVsDataObject @object = node.Object;

		if (@object.Type.Name == "View")
		{
			if ((short)@object.Properties["IS_SYSTEM_VIEW"] != 0)
				return EnNodeSystemType.System;

			return EnNodeSystemType.User;
		}
		else if (@object.Type.Name == "Procedure" || @object.Type.Name == "Function")
		{
			if ((int)@object.Properties["IS_SYSTEM_FLAG"] != 0)
				return EnNodeSystemType.System;

			return EnNodeSystemType.User;
		}

		return EnNodeSystemType.None;

	}



	public static string GetResourceString(string commandFunction, string scriptType, IVsDataExplorerNode node)
	{
		return GetResourceString(commandFunction, scriptType, MonikerAgent.GetNodeBaseType(node));

	}

	public static string GetResourceString(string commandFunction, string scriptType, EnNodeSystemType nodeSystemType)
	{
		return GetResourceString(commandFunction, scriptType, MonikerAgent.GetNodeSystemType(nodeSystemType));

	}


	public static string GetResourceString(string commandFunction, string scriptType, string nodeType)
	{
		string resource = $"CommandProvider_{commandFunction}{nodeType}{scriptType}";
		try
		{
			return Resources.ResourceManager.GetString(resource);
		}
		catch (Exception ex)

		{
			Diag.Dug(ex);
			return "Resource not found: " + resource;
		}
	}


	#endregion Methods





	// =========================================================================================================
	#region Event handlers - AbstractCommandProvider
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// New query command event handler.
	/// This roadblocks because key services in DataTools.Interop are protected.
	/// </summary>
	// ---------------------------------------------------------------------------------
	private void OnNewQuery(int itemId, EnNodeSystemType nodeSystemType)
	{
		// Tracer.Trace(GetType(), "AbstractCommandProvider.OnNewQuery", "itemId: {0}, nodeSystemType: {1}", itemId, nodeSystemType);

		Controller.Instance.OnNewQueryRequested(Site, nodeSystemType);
		// Host.QueryDesignerProviderTelemetry(qualityMetricProvider);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// New query intercept system command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnInterceptorNewQuery(int itemId, EnNodeSystemType nodeSystemType)
	{
		// Tracer.Trace(GetType(), "AbstractCommandProvider.OnInterceptorNewQuery", "itemId: {0}, nodeSystemType: {1}", itemId, nodeSystemType);

		IVsDataExplorerNode vsDataExplorerNode = Site.ExplorerConnection.FindNode(itemId);
		MenuCommand command = vsDataExplorerNode.GetCommand(CommandProperties.GlobalNewQuery);

		CommandProperties.CommandNodeSystemType = nodeSystemType;

		command.Invoke();
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Open text object command event handler.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected void OnOpen(int itemId, bool alternate)
	{
		// Tracer.Trace(GetType(), "AbstractCommandProvider.OnOpen", "itemId: {0}, alternate: {1}", itemId, alternate);

		IVsDataExplorerNode node = Site.ExplorerConnection.FindNode(itemId);

		if (SystemData.MandatedSqlEditorFactoryGuid.Equals(SystemData.DslEditorFactoryGuid, StringComparison.OrdinalIgnoreCase))
		{
			IBDesignerExplorerServices service = Controller.GetService<IBDesignerExplorerServices>()
				?? throw Diag.ServiceUnavailable(typeof(IBDesignerExplorerServices));

			service.ViewCode(node, alternate);
		}
		else
		{
			IBDesignerOnlineServices service = Controller.GetService<IBDesignerOnlineServices>()
				?? throw Diag.ServiceUnavailable(typeof(IBDesignerOnlineServices));

			MonikerAgent moniker = new(node);

			IList<string> identifierList = moniker.Identifier.ToArray();
			EnModelObjectType objectType = moniker.ObjectType;
			string script = MonikerAgent.GetDecoratedDdlSource(node, alternate);
			CsbAgent csb = new(node);

			service.ViewCode(csb, objectType, alternate, identifierList, script);

		}

		// Host.ActivateOrOpenVirtualDocument(node, false);
	}


	#endregion Event handlers

}