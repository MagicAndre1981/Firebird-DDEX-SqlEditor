// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.FunctionType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class FunctionType
{
	private StmtBlockType statementsField;

	private string procNameField;

	private bool isNativelyCompiledField;

	private bool isNativelyCompiledFieldSpecified;

	public StmtBlockType Statements
	{
		get
		{
			return statementsField;
		}
		set
		{
			statementsField = value;
		}
	}

	[XmlAttribute]
	public string ProcName
	{
		get
		{
			return procNameField;
		}
		set
		{
			procNameField = value;
		}
	}

	[XmlAttribute]
	public bool IsNativelyCompiled
	{
		get
		{
			return isNativelyCompiledField;
		}
		set
		{
			isNativelyCompiledField = value;
		}
	}

	[XmlIgnore]
	public bool IsNativelyCompiledSpecified
	{
		get
		{
			return isNativelyCompiledFieldSpecified;
		}
		set
		{
			isNativelyCompiledFieldSpecified = value;
		}
	}
}
