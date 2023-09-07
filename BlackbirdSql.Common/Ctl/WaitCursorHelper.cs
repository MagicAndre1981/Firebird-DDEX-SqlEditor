﻿#region Assembly Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;

namespace BlackbirdSql.Common.Ctl;


public sealed class WaitCursorHelper
{
	private class WaitCursor : IDisposable
	{
		private Cursor _currentCursor;

		public WaitCursor()
		{
			_currentCursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
		}

		~WaitCursor()
		{
			DisposeInternal();
		}

		public void Dispose()
		{
			DisposeInternal();
			GC.SuppressFinalize(this);
		}

		private void DisposeInternal()
		{
			if (!(_currentCursor != null))
			{
				return;
			}

			lock (this)
			{
				if (_currentCursor != null)
				{
					Cursor.Current = _currentCursor;
					_currentCursor = null;
				}
			}
		}
	}

	public static IDisposable NewWaitCursor()
	{
		return new WaitCursor();
	}
}