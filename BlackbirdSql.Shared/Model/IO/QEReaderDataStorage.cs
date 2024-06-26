﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion


using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Shared.Controls.Grid;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;

namespace BlackbirdSql.Shared.Model.IO;


public sealed class QEReaderDataStorage : IBQEStorage, IBDataStorage, IDisposable
{
	private const int C_ColumnSizeIndex = 2;

	private long _RowCount;

	private readonly ArrayList _ColumnInfoArray;

	private bool _DataStorageEnabled = true;

	private StorageDataReader _StorageReader;

	private bool _IsClosed = true;

	private int _MaxCharsToStore = -1;

	private int _MaxXmlCharsToStore = -1;

	public StorageDataReader StorageReader => _StorageReader;

	public int MaxCharsToStore
	{
		get
		{
			return _MaxCharsToStore;
		}
		set
		{
			if (value < -1)
			{
				Exception ex = new ArgumentOutOfRangeException("value");
				Diag.ThrowException(ex);
			}

			_MaxCharsToStore = value;
		}
	}

	public int MaxXmlCharsToStore
	{
		get
		{
			return _MaxXmlCharsToStore;
		}
		set
		{
			if (value < 1)
			{
				Exception ex = new ArgumentOutOfRangeException("value");
				Diag.ThrowException(ex);
			}

			_MaxXmlCharsToStore = value;
		}
	}

	public event StorageNotifyDelegate StorageNotify;

	public QEReaderDataStorage()
	{
		_RowCount = 0L;
		_ColumnInfoArray = [];
		_IsClosed = true;
		_DataStorageEnabled = true;
	}

	public async Task<bool> StartConsumingDataWithoutStoringAsync(CancellationToken canceltoken)
	{
		// Tracer.Trace(GetType(), "QEDiskDataStorage.StartConsumingDataWithoutStoring", "", null);
		if (_StorageReader == null)
		{
			InvalidOperationException ex = new(QEResources.ErrQEStorageNoReader);
			Diag.Dug(ex);
			throw ex;
		}

		await ConsumeDataWithoutStoringAsync(canceltoken);

		return !canceltoken.IsCancellationRequested;
	}

	public void Dispose()
	{
		InitiateStopStoringData();
	}

	public IBStorageView GetStorageView()
	{
		QEStorageViewOnReader qEStorageViewOnReader = new QEStorageViewOnReader(this);
		if (MaxCharsToStore > 0)
		{
			qEStorageViewOnReader.MaxNumBytesToDisplay = MaxCharsToStore / C_ColumnSizeIndex;
		}

		return qEStorageViewOnReader;
	}

	public IBSortView GetSortView()
	{
		Exception ex = new NotImplementedException();
		Diag.ThrowException(ex);
		return null;
	}

	public long RowCount => _RowCount;


	public int ColumnCount => _ColumnInfoArray.Count;


	public IBColumnInfo GetColumnInfo(int iCol)
	{
		return (IBColumnInfo)_ColumnInfoArray[iCol];
	}

	public bool IsClosed()
	{
		return _IsClosed;
	}

	public async Task<bool> InitStorageAsync(IDataReader reader, CancellationToken cancelToken)
	{
		if (reader == null)
		{
			ArgumentNullException ex = new("reader");
			Diag.Dug(ex);
			throw ex;
		}

		_StorageReader = new StorageDataReader(reader);

		// Tracer.Trace(GetType(), "InitStorage()", "ASYNC GetSchemaTableAsync()");

		try
		{
			await _StorageReader.GetSchemaTableAsync(cancelToken);
		}
		catch (Exception ex)
		{
			if (ex is OperationCanceledException || cancelToken.IsCancellationRequested)
				return false;
			throw;
		}

		int fieldCount = _StorageReader.FieldCount;

		for (int i = 0; i < fieldCount; i++)
		{
			ColumnInfo columnInfo = new ColumnInfo();
			await columnInfo.InitializeAsync(_StorageReader, i, cancelToken);

			if (cancelToken.IsCancellationRequested)
				return false;

			_ColumnInfoArray.Add(columnInfo);
		}

		return true;
	}

	public async Task<bool> StartStoringDataAsync(CancellationToken cancelToken)
	{
		if (_StorageReader == null)
		{
			InvalidOperationException ex = new(QEResources.ErrQEStorageNoReader);
			Diag.Dug(ex);
			throw ex;
		}

		if (!_IsClosed)
		{
			InvalidOperationException ex = new(QEResources.ErrQEStorageAlreadyStoring);
			Diag.Dug(ex);
			throw ex;
		}

		_IsClosed = false;

		await GetDataFromReaderAsync(cancelToken);

		return !cancelToken.IsCancellationRequested;
	}

	public void InitiateStopStoringData()
	{
		_DataStorageEnabled = false;
	}

	private async Task<bool> ConsumeDataWithoutStoringAsync(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "QEReaderDataStorage.ConsumeDataWithoutStoring", "", null);
		_IsClosed = false;

		while (_DataStorageEnabled && await _StorageReader.ReadAsync(cancelToken));

		_DataStorageEnabled = false;
		OnStorageNotify(-1L, bStoredAllData: true);
		_IsClosed = true;

		return !cancelToken.IsCancellationRequested;
	}

	private async Task<bool> GetDataFromReaderAsync(CancellationToken cancelToken)
	{
		// Tracer.Trace(GetType(), "QEReaderDataStorage.GetDataFromReader", "", null);
		try
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QEReaderDataStorage.GetDataFromReader", "_DataStorageEnabled = {0}", _DataStorageEnabled);
			while (_DataStorageEnabled && await _StorageReader.ReadAsync(cancelToken))
			{
				Interlocked.Increment(ref _RowCount);
				OnStorageNotify(_RowCount, bStoredAllData: false);
			}
		}
		catch (Exception ex)
		{
			Diag.ThrowException(ex);
		}

		_DataStorageEnabled = false;
		OnStorageNotify(_RowCount, bStoredAllData: true);
		_IsClosed = true;

		return !cancelToken.IsCancellationRequested;
	}

	private void OnStorageNotify(long i64RowsInStorage, bool bStoredAllData)
	{
		StorageNotify?.Invoke(i64RowsInStorage, bStoredAllData);
	}
}
