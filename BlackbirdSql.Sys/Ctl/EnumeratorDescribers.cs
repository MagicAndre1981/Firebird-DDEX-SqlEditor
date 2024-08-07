﻿
using System.Collections;
using System.Collections.Generic;
using BlackbirdSql.Sys.Extensions;



namespace BlackbirdSql.Sys.Ctl;


public class EnumeratorDescribers : IEnumerator, IEnumerator<Describer>
{

	public EnumeratorDescribers(PublicValueCollection<string, Describer> values)
	{
		_Owner = values;
		_Enumerator = values.GetEnumerator();
		Reset();
	}

	public void Dispose()
	{
		_Owner = null;
		_Enumerator.Dispose();
		_Enumerator = default;
	}



	private PublicValueCollection<string, Describer> _Owner;
	private PublicValueCollection<string, Describer>.Enumerator _Enumerator;



	object IEnumerator.Current
	{
		get
		{
			return _Enumerator.Current;
		}
	}

	public Describer Current
	{
		get
		{
			return _Enumerator.Current;
		}
	}

	public bool MoveNext()
	{
		if (_Enumerator.Index >= _Owner.Count)
			return false;

		do
		{
			if (!_Enumerator.MoveNext())
				return false;

		} while (!IsValid(_Enumerator.Current));

		return true;
	}

	public virtual bool IsValid(Describer describer)
	{
		return !describer.IsInternalStore;
	}

	public void Reset()
	{
		_Enumerator.Reset();
	}
}
