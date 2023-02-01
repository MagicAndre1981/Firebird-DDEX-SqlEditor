﻿/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Hennadii Zabula, Jiri Cincura (jiri@cincura.net)

using System;
using System.Diagnostics.Contracts;
using BlackbirdSql.Data.Common;

namespace BlackbirdSql.Data.Client.Native.Handles;

// public visibility added, because auto-generated assembly can't work with internal types
public class StatementHandle : DslHandle
{
	protected override bool ReleaseHandle()
	{
		Contract.Requires(FbClient != null);

		if (IsClosed)
		{
			return true;
		}

		var statusVector = new IntPtr[IscCodes.ISC_STATUS_LENGTH];
		var @ref = this;
		FbClient.isc_dsql_free_statement(statusVector, ref @ref, IscCodes.DSQL_drop);
		handle = @ref.handle;

		var exception = StatusVectorHelper.ParseStatusVector(statusVector, Charset.DefaultCharset);
		return exception == null || exception.IsWarning;
	}
}