/*
 *  Visual Studio DDEX Provider for FirebirdClient (BlackbirdSql)
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.blackbirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;

[Guid(Configuration.PackageData.ObjectFactoryServiceGuid)]


public interface IProviderObjectFactory
{
}

public sealed class DdexProviderObjectFactory : DataProviderObjectFactory, IProviderObjectFactory
{
	#region � Constructors �

	public DdexProviderObjectFactory() : base()
	{
		Diag.Trace();
	}

	#endregion

	#region � Methods �

	public override object CreateObject(Type objType)
	{
		/* Uncomment this and change SupportedObjects._useFactoryOnly to true to debug implementations
		 * Don't forget to do the same for DdexConnectionSupport if you do.
		 * 
		if (objType == typeof(IVsDataConnectionSupport))
		{
			Diag.Trace();
			return new DdexConnectionSupport();
		}
		else if (objType == typeof(IVsDataConnectionUIControl))
		{
			Diag.Trace();
			return new DdexConnectionUIControl();
		}
		else if (objType == typeof(IVsDataConnectionPromptDialog))
		{
			Diag.Trace();
			return new DdexConnectionPromptDialog();
		}
		else if (objType == typeof(IVsDataConnectionProperties))
		{
			Diag.Trace();
			return new DdexConnectionProperties();
		}
		else if (objType == typeof(IVsDataConnectionUIProperties))
		{
			Diag.Trace();
			return new DdexConnectionUIProperties();
		}
		else if (objType == typeof(IVsDataObjectIdentifierResolver))
		{
			Diag.Trace();
			return new DdexObjectIdentifierResolver((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataObjectSupport))
		{
			Diag.Trace();
			return new DdexObjectSupport((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataSourceInformation))
		{
			Diag.Trace();
			return new DdexSourceInformation();
		}
		else if (objType == typeof(IVsDataViewSupport))
		{
			Diag.Trace();
			return new DataViewSupport("BlackbirdSql.VisualStudio.Ddex.DdexViewSupport", typeof(ProviderObjectFactory).Assembly);
			// return new DdexViewSupport();
		}
		else if (objType == typeof(IVsDataConnectionEquivalencyComparer))
		{
			Diag.Trace();
			return new DdexConnectionEquivalencyComparer();
		}
		*/

		Diag.Dug(true, objType.FullName + " is not supported");
		return null;
	}

	#endregion
}