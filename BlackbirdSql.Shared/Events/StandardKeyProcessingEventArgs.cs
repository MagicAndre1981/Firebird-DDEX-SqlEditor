﻿// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.StandardKeyProcessingEventArgs

using System;
using System.Windows.Forms;



namespace BlackbirdSql.Shared.Events;


public delegate void StandardKeyProcessingEventHandler(object sender, StandardKeyProcessingEventArgs args);


public class StandardKeyProcessingEventArgs : EventArgs
{
	private readonly Keys m_Key;

	private readonly Keys m_Modifiers;

	private bool m_ShouldHandle = true;

	public Keys Key => m_Key;

	public Keys Modifiers => m_Modifiers;

	public bool ShouldHandle
	{
		get
		{
			return m_ShouldHandle;
		}
		set
		{
			m_ShouldHandle = value;
		}
	}

	public StandardKeyProcessingEventArgs(KeyEventArgs ke)
	{
		m_Key = ke.KeyCode;
		m_Modifiers = ke.Modifiers;
	}

	protected StandardKeyProcessingEventArgs()
	{
	}
}
