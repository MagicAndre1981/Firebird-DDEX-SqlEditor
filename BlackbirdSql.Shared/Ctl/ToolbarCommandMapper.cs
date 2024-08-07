﻿// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.TabbedEditorToolbarHandlerManager

using System;
using System.Collections.Generic;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Ctl;


public class ToolbarCommandMapper
{
	private readonly Dictionary<Type, Dictionary<GuidId, IBsToolbarCommandHandler>> _Mappings = [];



	public void AddMapping(Type tabbedWindowPaneType, IBsToolbarCommandHandler commandHandler)
	{
		if (!_Mappings.ContainsKey(tabbedWindowPaneType))
		{
			_Mappings.Add(tabbedWindowPaneType, []);
		}
		Dictionary<GuidId, IBsToolbarCommandHandler> dictionary = _Mappings[tabbedWindowPaneType];
		GuidId clsid = commandHandler.Clsid;

		if (dictionary.ContainsKey(clsid))
			_ = dictionary[clsid];
		else
			dictionary.Add(clsid, commandHandler);
	}



	public bool TryGetCommandHandler(Type tabbedWindowPaneType, GuidId clsid, out IBsToolbarCommandHandler commandHandler)
	{
		commandHandler = null;
		bool result = false;

		if (_Mappings.TryGetValue(tabbedWindowPaneType, out Dictionary<GuidId, IBsToolbarCommandHandler> dictionary))
		{
			result = dictionary.TryGetValue(clsid, out commandHandler);
		}

		return result;
	}


}
