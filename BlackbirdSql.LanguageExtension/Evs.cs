﻿
using System.Diagnostics.Tracing;
using BlackbirdSql.Sys.Ctl.Diagnostics;



namespace BlackbirdSql.LanguageExtension;

[EventSource(Name = C_Name)]


public class Evs : AbstractEvs<Evs>
{
	public Evs() : base()
	{
	}

	private const string C_Name = "BlackbirdSql.LanguageExtension.Etw";
}