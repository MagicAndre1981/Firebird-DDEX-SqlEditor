// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MissingIndexGroupType
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
public class MissingIndexGroupType
{
	private MissingIndexType[] missingIndexField;

	private double impactField;

	[XmlElement("MissingIndex")]
	public MissingIndexType[] MissingIndex
	{
		get
		{
			return missingIndexField;
		}
		set
		{
			missingIndexField = value;
		}
	}

	[XmlAttribute]
	public double Impact
	{
		get
		{
			return impactField;
		}
		set
		{
			impactField = value;
		}
	}
}
