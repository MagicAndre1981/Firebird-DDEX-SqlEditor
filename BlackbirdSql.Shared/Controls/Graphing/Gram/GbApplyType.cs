// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.GbApplyType
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
public class GbApplyType : RelOpBaseType
{
	private ScalarExpressionType[] predicateField;

	private DefinedValuesListTypeDefinedValue[] aggFunctionsField;

	private RelOpType[] relOpField;

	private string joinTypeField;

	private string aggTypeField;

	[XmlElement("Predicate")]
	public ScalarExpressionType[] Predicate
	{
		get
		{
			return predicateField;
		}
		set
		{
			predicateField = value;
		}
	}

	[XmlArrayItem("DefinedValue", IsNullable = false)]
	public DefinedValuesListTypeDefinedValue[] AggFunctions
	{
		get
		{
			return aggFunctionsField;
		}
		set
		{
			aggFunctionsField = value;
		}
	}

	[XmlElement("RelOp")]
	public RelOpType[] RelOp
	{
		get
		{
			return relOpField;
		}
		set
		{
			relOpField = value;
		}
	}

	[XmlAttribute]
	public string JoinType
	{
		get
		{
			return joinTypeField;
		}
		set
		{
			joinTypeField = value;
		}
	}

	[XmlAttribute]
	public string AggType
	{
		get
		{
			return aggTypeField;
		}
		set
		{
			aggTypeField = value;
		}
	}
}
