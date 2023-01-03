﻿/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using Microsoft.EntityFrameworkCore.Storage;

namespace BlackbirdSql.Data.Entity.Core.Storage.Internal;

public class FbDateOnlyTypeMapping : DateOnlyTypeMapping
{
	public FbDateOnlyTypeMapping(string storeType)
		: base(storeType)
	{ }

	protected FbDateOnlyTypeMapping(RelationalTypeMappingParameters parameters)
		: base(parameters)
	{ }

	protected override string GenerateNonNullSqlLiteral(object value)
	{
		return $"CAST('{value:yyyy-MM-dd}' AS DATE)";
	}

	protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
		=> new FbDateOnlyTypeMapping(parameters);
}
