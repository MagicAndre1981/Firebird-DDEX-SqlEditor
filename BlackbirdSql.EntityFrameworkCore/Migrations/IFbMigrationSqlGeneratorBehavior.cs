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

using Microsoft.EntityFrameworkCore.Migrations;

namespace BlackbirdSql.Data.Entity.Core;

public interface IFbMigrationSqlGeneratorBehavior
{
	void CreateSequenceTriggerForColumn(string columnName, string tableName, string schemaName, MigrationsSqlGenerationOptions options, MigrationCommandListBuilder builder);
	void DropSequenceTriggerForColumn(string columnName, string tableName, string schemaName, MigrationsSqlGenerationOptions options, MigrationCommandListBuilder builder);
}
