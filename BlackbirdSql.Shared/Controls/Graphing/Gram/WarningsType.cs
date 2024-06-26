// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.WarningsType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;

namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class WarningsType
{
	private object[] itemsField;

	private EnItemsChoiceType[] itemsElementNameField;

	private bool noJoinPredicateField;

	private bool noJoinPredicateFieldSpecified;

	private bool spatialGuessField;

	private bool spatialGuessFieldSpecified;

	private bool unmatchedIndexesField;

	private bool unmatchedIndexesFieldSpecified;

	private bool fullUpdateForOnlineIndexBuildField;

	private bool fullUpdateForOnlineIndexBuildFieldSpecified;

	[XmlElement("ColumnsWithNoStatistics", typeof(ColumnReferenceListType))]
	[XmlElement("ColumnsWithStaleStatistics", typeof(ColumnReferenceListType))]
	[XmlElement("ExchangeSpillDetails", typeof(ExchangeSpillDetailsType))]
	[XmlElement("HashSpillDetails", typeof(HashSpillDetailsType))]
	[XmlElement("MemoryGrantWarning", typeof(MemoryGrantWarningInfo))]
	[XmlElement("PlanAffectingConvert", typeof(AffectingConvertWarningType))]
	[XmlElement("SortSpillDetails", typeof(SortSpillDetailsType))]
	[XmlElement("SpillOccurred", typeof(SpillOccurredType))]
	[XmlElement("SpillToTempDb", typeof(SpillToTempDbType))]
	[XmlElement("Wait", typeof(WaitWarningType))]
	[XmlChoiceIdentifier("ItemsElementName")]
	public object[] Items
	{
		get
		{
			return itemsField;
		}
		set
		{
			itemsField = value;
		}
	}

	[XmlElement("ItemsElementName")]
	[XmlIgnore]
	public EnItemsChoiceType[] ItemsElementName
	{
		get
		{
			return itemsElementNameField;
		}
		set
		{
			itemsElementNameField = value;
		}
	}

	[XmlAttribute]
	public bool NoJoinPredicate
	{
		get
		{
			return noJoinPredicateField;
		}
		set
		{
			noJoinPredicateField = value;
		}
	}

	[XmlIgnore]
	public bool NoJoinPredicateSpecified
	{
		get
		{
			return noJoinPredicateFieldSpecified;
		}
		set
		{
			noJoinPredicateFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool SpatialGuess
	{
		get
		{
			return spatialGuessField;
		}
		set
		{
			spatialGuessField = value;
		}
	}

	[XmlIgnore]
	public bool SpatialGuessSpecified
	{
		get
		{
			return spatialGuessFieldSpecified;
		}
		set
		{
			spatialGuessFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool UnmatchedIndexes
	{
		get
		{
			return unmatchedIndexesField;
		}
		set
		{
			unmatchedIndexesField = value;
		}
	}

	[XmlIgnore]
	public bool UnmatchedIndexesSpecified
	{
		get
		{
			return unmatchedIndexesFieldSpecified;
		}
		set
		{
			unmatchedIndexesFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool FullUpdateForOnlineIndexBuild
	{
		get
		{
			return fullUpdateForOnlineIndexBuildField;
		}
		set
		{
			fullUpdateForOnlineIndexBuildField = value;
		}
	}

	[XmlIgnore]
	public bool FullUpdateForOnlineIndexBuildSpecified
	{
		get
		{
			return fullUpdateForOnlineIndexBuildFieldSpecified;
		}
		set
		{
			fullUpdateForOnlineIndexBuildFieldSpecified = value;
		}
	}
}
