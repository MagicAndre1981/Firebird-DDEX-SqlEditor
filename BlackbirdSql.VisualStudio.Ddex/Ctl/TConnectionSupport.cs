// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Data;
using System.Data.Common;
using System.ComponentModel.Design;
using System.Threading;

using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using FirebirdSql.Data.FirebirdClient;

using BlackbirdSql.Core;
using BlackbirdSql.VisualStudio.Ddex.Extensions;
using System.Diagnostics;
using System.ServiceProcess;

namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//										TConnectionSupport Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionSupport"/> interface
/// </summary>
// =========================================================================================================
public class TConnectionSupport : AdoDotNetConnectionSupport
{





	// =====================================================================================================
	#region Property Accessors - TConnectionSupport
	// =====================================================================================================


	public override string ConnectionString
	{
		get
		{
			return Connection.ConnectionString;
		}
		set
		{
			if (Connection.State != 0)
				Close();

			try
			{
				Connection.ConnectionString = value;
			}
			catch (Exception ex)
			{
				ArgumentException exa = new(ex.Message, "value", ex);
				Diag.Dug(exa);
				throw exa;
			}
		}
	}


	public override int ConnectionTimeout
	{
		get { return Connection.ConnectionTimeout; }
		set { throw new NotSupportedException(); }
	}


	public override DataConnectionState State => Connection.State switch
	{
		ConnectionState.Closed => DataConnectionState.Closed,
		ConnectionState.Open => DataConnectionState.Open,
		ConnectionState.Broken => DataConnectionState.Broken,
		_ => DataConnectionState.Closed,
	};



	#endregion Property Accessors





	// =========================================================================================================
	#region Constructors / Destructors - TConnectionSupport
	// =========================================================================================================


	public TConnectionSupport() : base()
	{
		// Diag.Trace();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TConnectionSupport
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates a new service object based on the specified interface service type.
	/// </summary>
	/// <param name="container">
	/// A service provider object to contain the service.
	/// </param>
	/// <param name="serviceType">A System.Type of the service to create.</param>
	/// <returns>The service object.</returns>
	// ---------------------------------------------------------------------------------
	protected override object CreateService(IServiceContainer container, Type serviceType)
	{
		// Diag.Trace();

		/* Uncomment this and change PackageSupportedObjects._UseFactoryOnly to true to debug implementations
		 * Don't forget to do the same for the ProviderObjectFactory if you do.
		 * 
		if (serviceType == typeof(IVsDataSourceInformation))
		{
			// Diag.Trace();
			return new TSourceInformation(Site);
		}
		else if (serviceType == typeof(IVsDataObjectSelector))
		{
			return new TObjectSelector(Site);
		}
		else if (serviceType == typeof(IVsDataObjectMemberComparer))
		{
			// Diag.Trace();
			return new TObjectMemberComparer(Site);
		}
		else if (serviceType == typeof(IVsDataObjectIdentifierConverter))
		{
			// Diag.Trace();
			return new TObjectIdentifierConverter(Site);
		}
		else if (serviceType == typeof(IVsDataMappedObjectConverter))
		{
			// Diag.Trace();
			return new TMappedObjectConverter(Site);
		}
		*/



		// Diag.Trace(serviceType.FullName + " is not directly supported");

		return base.CreateService(container, serviceType);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Opens the specified data connection.
	/// </summary>
	/// <param name="doPromptCheck">
	/// Indicates whether the call to the Open method should return false for specified
	/// errors that relate to missing connection information.</param>
	/// <param name="serviceType">A <see cref="Type"/> of the service to create.</param>
	/// <returns>
	/// true if the connection opened successfully and does not require a prompt, false
	/// if the connection is missing required connection information and a prompt should
	/// be displayed to obtain the missing information form the user.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public override bool Open(bool doPromptCheck)
	{

		try
		{
			// Diag.Trace("Prompt: " + doPromptCheck + " IsOpen: " + State + " ConnectionString: " + ConnectionString);
			if (State == DataConnectionState.Open)
				return true;

			IVsDataConnectionUIProperties vsDataConnectionUIProperties =
				((IVsDataSiteableObject<IVsDataProvider>)this).Site.CreateObject<IVsDataConnectionUIProperties>(Site.Source);

			vsDataConnectionUIProperties.Parse(ConnectionString);

			if (doPromptCheck && !vsDataConnectionUIProperties.IsComplete)
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		return base.Open(doPromptCheck);
	}


	#endregion Method Implementations

}