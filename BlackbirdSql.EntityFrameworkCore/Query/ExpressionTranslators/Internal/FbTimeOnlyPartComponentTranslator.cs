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

using System;
using System.Collections.Generic;
using System.Reflection;
using BlackbirdSql.EntityFrameworkCore.Query.Expressions.Internal;
using BlackbirdSql.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace BlackbirdSql.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

public class FbTimeOnlyPartComponentTranslator : IMemberTranslator
{
	const string SecondPart = "SECOND";
	const string MillisecondPart = "MILLISECOND";
	private static readonly Dictionary<MemberInfo, string> MemberMapping = new Dictionary<MemberInfo, string>
		{
			{  typeof(TimeOnly).GetProperty(nameof(TimeOnly.Hour)), "HOUR" },
			{  typeof(TimeOnly).GetProperty(nameof(TimeOnly.Minute)), "MINUTE" },
			{  typeof(TimeOnly).GetProperty(nameof(TimeOnly.Second)), SecondPart },
			{  typeof(TimeOnly).GetProperty(nameof(TimeOnly.Millisecond)), MillisecondPart },
		};

	readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

	public FbTimeOnlyPartComponentTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
	{
		_fbSqlExpressionFactory = fbSqlExpressionFactory;
	}

	public SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (!MemberMapping.TryGetValue(member, out var part))
			return null;

		var result = (SqlExpression)_fbSqlExpressionFactory.SpacedFunction(
			"EXTRACT",
			new[] { _fbSqlExpressionFactory.Fragment(part), _fbSqlExpressionFactory.Fragment("FROM"), instance },
			true,
			new[] { false, false, true },
			typeof(int));
		if (part == SecondPart || part == MillisecondPart)
		{
			result = _fbSqlExpressionFactory.Function("TRUNC", new[] { result }, true, new[] { true }, typeof(int));
		}
		return result;
	}
}
