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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlackbirdSql.Data.Entity.Core.Query.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace BlackbirdSql.Data.Entity.Core.Query.ExpressionTranslators.Internal;

public class FbStringIndexOfTranslator : IMethodCallTranslator
{
	readonly FbSqlExpressionFactory _fbSqlExpressionFactory;

	public FbStringIndexOfTranslator(FbSqlExpressionFactory fbSqlExpressionFactory)
	{
		_fbSqlExpressionFactory = fbSqlExpressionFactory;
	}

	public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
	{
		if (method.DeclaringType == typeof(string) && method.Name == nameof(string.IndexOf))
		{
			var args = new List<SqlExpression>();
			args.Add(_fbSqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]));
			args.Add(instance);
			foreach (var a in arguments.Skip(1))
			{
				args.Add(_fbSqlExpressionFactory.ApplyDefaultTypeMapping(a));
			}
			return _fbSqlExpressionFactory.Subtract(
				_fbSqlExpressionFactory.Function("POSITION", args, true, Enumerable.Repeat(true, args.Count), typeof(int)),
				_fbSqlExpressionFactory.Constant(1));
		}
		return null;
	}
}
