﻿using System.Collections.Generic;
using BlackbirdSql.Sys.Ctl;


namespace BlackbirdSql.Sys.Interfaces;

public interface IBsEnumerableDescribers<T> : IEnumerable<Describer> where T : EnumeratorDescribers
{
	// IEnumerable<Describer> DescriberEnumerator { get; }
}
