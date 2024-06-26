﻿
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Media.Imaging;
using BlackbirdSql.Core.Ctl;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Sys.Ctl;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;

using static BlackbirdSql.Sys.SysConstants;



namespace BlackbirdSql.Core;


// =========================================================================================================
//									AbstractPropertyAgent Class - Accessors
//
/// <summary>
/// The base class for all property based dispatcher and connection classes used in conjunction with
/// PropertySet static classes.
/// A conglomerate base class that supports dynamic addition of properties and parsing functionality for
/// <see cref="DbConnectionStringBuilder"/>, <see cref="Csb"/> and property strings as
/// well as support for the <see cref="IDisposable"/>, <see cref="ICustomTypeDescriptor"/>,
/// <see cref="IDataConnectionProperties"/>, <see cref="IComparable{T}"/>, <see cref="IEquatable{T}"/>,
/// <see cref="INotifyPropertyChanged"/>, <see cref="INotifyDataErrorInfo"/> and
/// <see cref="IWeakEventListener"/> interfaces.
/// </summary>
/// <remarks>
/// Steps for adding an additional property in descendents:
/// 1. Add the core property descriptor support using
/// <see cref="Add(string, Type, object)"/>.
/// 2. Add the property accessor using <see cref="GetProperty(string)"/> and 
/// <see cref="SetProperty(string, object)"/>.
/// 3. If the property type cannot be supported by the built-in types in 
/// <see cref="SetProperty(string, object)"/>, overload the method to support the new type.
/// 4. If (3) applies include a Set_[NewType]Property() method to support the new type using the builtin
/// Set_[Type]Property() methods as a template.
/// 5. If (4) applies add a Will[NewType]Change() method if an existing builtin method cannot be used.  
/// Note: Additional properties will not be included in the connection string or have actual property
/// descriptors. <see cref="ICustomTypeDescriptor"/> members will still need to be overloaded.
/// </remarks>
// =========================================================================================================
public abstract partial class AbstractPropertyAgent : IBPropertyAgent
{

	// ---------------------------------------------------------------------------------
	#region Accessor Fields - AbstractPropertyAgent
	// ---------------------------------------------------------------------------------


	protected bool _GetSetConnectionOpened = false;
	protected int _GetSetCardinal = 0;


	#endregion Accessor Fields





	// =========================================================================================================
	#region Property Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public virtual object this[string key]
	{
		get { return GetProperty(key); }
		set { SetProperty(key, value); }
	}

	public virtual BitmapImage IconImage => CoreIconsCollection.Instance.GetImage(
		Isset("Icon") ? (IBIconType)GetProperty("Icon") : CoreIconsCollection.Instance.Error_16);


	public DbConnectionStringBuilder ConnectionStringBuilder
	{
		set { Set_ConnectionStringBuilder(value); }
		get { return Get_ConnectionStringBuilder(false); }
	}


	public virtual DbConnection DataConnection
	{
		get
		{
			if (_DataConnection == null)
			{
				NotImplementedException ex = new("DataConnection");
				Diag.Dug(ex);
				throw ex;
			}

			return _DataConnection;
		}
	}


	public virtual DescriberDictionary Describers
	{
		get
		{
			if (_Describers == null)
				CreateAndPopulatePropertySet();

			return _Describers;
		}
	}




	// public abstract string[] EquivalencyKeys { get; }

	public bool HasErrors => _ValidationErrors != null && _ValidationErrors.Any();

	public long Id => _Id;

	public bool IsComplete
	{
		get
		{
			foreach (Describer describer in Describers.MandatoryKeys)
			{
				if (!_AssignedConnectionProperties.TryGetValue(describer.Name,
					out object value) || string.IsNullOrEmpty((string)value))
				{
					return false;
				}
			}

			return true;
		}
	}

	public bool IsExtensible => true;


	public virtual string PropertyString => ConnectionStringBuilder.ConnectionString;


	public IDictionary<string, string> ValidationErrors
		=> _ValidationErrors ??= new Dictionary<string, string>();


	#endregion Property Accessors





	// =========================================================================================================
	#region External (non-connection) Property Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public string DatasetKey
	{
		get { return (string)GetProperty("DatasetKey"); }
		set { SetProperty("DatasetKey", value); }
	}

	public string ConnectionKey
	{
		get { return (string)GetProperty("ConnectionKey"); }
		set { SetProperty("ConnectionKey", value); }
	}

	public string Dataset
	{
		get
		{
			string value = (string)GetProperty("Dataset");
			if (string.IsNullOrWhiteSpace(value))
			{
				value = Database;
				if (!string.IsNullOrWhiteSpace(value))
					value = Path.GetFileNameWithoutExtension(value);
			}
			return value;
		}
		set
		{
			SetProperty("Dataset", value);
		}
	}

	public string DatasetId
	{
		get { return (string)GetProperty(C_KeyExDatasetId); }
		set { SetProperty(C_KeyExDatasetId, value); }
	}

	public string ConnectionName
	{
		get { return (string)GetProperty(C_KeyExConnectionName); }
		set { SetProperty(C_KeyExConnectionName, value); }
	}

	public EnConnectionSource ConnectionSource
	{
		get { return (EnConnectionSource)GetProperty("ConnectionSource"); }
		set { SetProperty("ConnectionSource", value); }
	}

	public string AdministratorLogin
	{
		get { return (string)GetProperty("AdministratorLogin"); }
		set { SetProperty("AdministratorLogin", value); }
	}


	public IBIconType Icon
	{
		get { return (IBIconType)GetProperty("Icon"); }
		set { SetProperty("Icon", value); }
	}


	public string OtherParams
	{
		get { return (string)GetProperty("OtherParams"); }
		set { SetProperty("OtherParams", value); }
	}


	public bool PersistPassword
	{
		get { return (bool)GetProperty("PersistPassword"); }
		set { SetProperty("PersistPassword", value); }
	}


	public EnEngineType ServerEngine
	{
		get { return (EnEngineType)GetProperty("ServerEngine"); }
		set { SetProperty("ServerEngine", (int)value); }
	}


	public string ServerFullyQualifiedDomainName
	{
		get { return (string)GetProperty("ServerFullyQualifiedDomainName"); }
		set { SetProperty("ServerFullyQualifiedDomainName", value); }
	}


	public Version ServerVersion
	{
		get { return (Version)GetProperty("ServerVersion"); }
		set { SetProperty("ServerVersion", value); }
	}

	public Version ClientVersion
	{
		get { return (Version)GetProperty("ClientVersion"); }
		set { SetProperty("ClientVersion", value); }
	}


	#endregion External (non-connection) Property Accessors





	// =========================================================================================================
	#region Descriptors Property Accessors - AbstractPropertyAgent
	// =========================================================================================================


	public string Database
	{
		get { return (string)GetProperty("Database"); }
		set { SetProperty("Database", value); }
	}


	public string DataSource
	{
		get { return (string)GetProperty("DataSource"); }
		set { SetProperty("DataSource", value); }
	}


	public string Password
	{
		get { return (string)GetProperty("Password"); }
		set { SetProperty("Password", value); }
	}


	public int Port
	{
		get { return (int)GetProperty("Port"); }
		set { SetProperty("Port", value); }
	}


	public EnServerType ServerType
	{
		get { return (EnServerType)GetProperty("ServerType"); }
		set { SetProperty("ServerType", value); }
	}


	public string UserID
	{
		get { return (string)GetProperty("UserID"); }
		set { SetProperty("UserID", value); }
	}


	#endregion 	Descriptors Property Accessors




	// =========================================================================================================
	#region Property Getters/Setters - AbstractPropertyAgent
	// =========================================================================================================


	public virtual void Set_ConnectionStringBuilder(DbConnectionStringBuilder csb)
	{
		Parse(csb);
	}
	public virtual DbConnectionStringBuilder Get_ConnectionStringBuilder(bool secure)
	{
		if (_ConnectionStringBuilder != null && !_ParametersChanged)
			return _ConnectionStringBuilder;

		_ParametersChanged = false;

		_ConnectionStringBuilder ??= new Csb();
		PopulateConnectionStringBuilder(_ConnectionStringBuilder, secure);

		return _ConnectionStringBuilder;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the DatsetKey property and sets and registers it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived DatasetKey else null and
	/// a boolean indicating wether or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public virtual (string, bool) GetSet_DatasetKey()
	{
		if (!IsComplete)
			return (null, false);

		Csb csa = RctManager.ShutdownState ? null : RctManager.CloneRegistered(this);
		if (csa == null)
			return (null, false);

		string datasetKey = csa.DatasetKey;

		if (string.IsNullOrEmpty(datasetKey))
			return (null, false);

		DatasetKey = datasetKey;
		Dataset = csa.Dataset;
		DatasetId = csa.DatasetId;

		return (datasetKey, false);
	}


	public virtual (IBIconType, bool) GetSet_Icon()
	{
		IBIconType iconType;

		if (ServerType == EnServerType.Embedded)
			iconType = CoreIconsCollection.Instance.EmbeddedDatabase_32;
		else
			iconType = CoreIconsCollection.Instance.ClassicServer_32;

		Icon = iconType;

		return (iconType, false);

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ServerEngine property and sets it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived ServerEngine else null and
	/// a boolean indicating wehther or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public virtual  (EnEngineType, bool) GetSet_ServerEngine()
	{
		EnEngineType serverEngine;

		if (ServerType == EnServerType.Embedded)
			serverEngine = EnEngineType.EmbeddedDatabase;
		else
			serverEngine = EnEngineType.ClassicServer;

		ServerEngine = serverEngine;

		return (serverEngine, false);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ServerVersion property and sets it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived ServerVersion else null and
	/// a boolean indicating wehther or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public (Version, bool) GetSet_ServerVersion()
	{
		if (DataConnection.State != ConnectionState.Open && !IsComplete)
			return (null, false);

		bool opened;
		Version version;

		(version, opened) = GetServerVersion(DataConnection);

		if (version != null)
			ServerVersion = version;

		return (version, opened);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the ClientVersion property and sets it if successful.
	/// </summary>
	/// <returns>Returns the value tuple of the derived ClientVersion else null and
	/// a boolean indicating wehther or not a connection was opened.</returns>
	// ---------------------------------------------------------------------------------
	public (Version, bool) GetSet_ClientVersion()
	{

		bool opened = false;
		Version version = new(NativeDb.ClientVersion);

		if (version != null)
			ClientVersion = version;

		return (version, opened);
	}



	protected virtual bool Set_StringProperty(string name, string value)
	{
		if (!WillStringPropertyChange(name, value, false))
			return false;

		if (Describers[name].DefaultEqualsOrEmpty(value))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}

	protected virtual bool Set_ObjectProperty(string name, object value)
	{
		if (!WillObjectPropertyChange(name, value, false))
			return false;

		if (Describers[name].DefaultEquals(value))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_IntProperty(string name, int value)
	{
		if (!WillIntPropertyChange(name, value, false))
			return false;

		if (value == Convert.ToInt32(Describers[name].DefaultValue))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_BoolProperty(string name, bool value)
	{
		if (!WillBoolPropertyChange(name, value, false))
			return false;

		if (value == Convert.ToBoolean(Describers[name].DefaultValue))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_EnumProperty(string name, object enumValue)
	{
		// We will always store as int32.

		object value = enumValue;

		if (enumValue != null)
		{
			if (enumValue is Enum enumType)
				value = enumType;
			else if (value is string s)
				value = Enum.Parse(Describers[name].PropertyType, s, true);
			else
				value = (Enum)value;
		}


		if (!WillEnumPropertyChange(name, value, false))
			return false;

		if (enumValue == null || Convert.ToInt32(value) == Convert.ToInt32(Describers[name].DefaultValue))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_ByteProperty(string name, object value)
	{
		if (!WillBytePropertyChange(name, value, false))
			return false;

		// We will always store as string.
		if (Cmd.IsNullValue(value))
			_AssignedConnectionProperties.Remove(name);
		else if (value.GetType() == typeof(byte[]))
			_AssignedConnectionProperties[name] = Encoding.Default.GetString((byte[])value);
		else
			_AssignedConnectionProperties[name] = (string)value;

		RaisePropertyChanged(name);

		return true;
	}



	protected virtual bool Set_PasswordProperty(string name, string value)
	{
		bool changed = WillPasswordPropertyChange(name, value, false);

		if (string.IsNullOrEmpty(value))
		{
			_AssignedConnectionProperties.Remove("InMemoryPassword");
			_AssignedConnectionProperties[name] = string.Empty;
		}
		else
		{
			_AssignedConnectionProperties["InMemoryPassword"] = value.ToSecure();
			_AssignedConnectionProperties.Remove("Password");
		}

		if (changed) RaisePropertyChanged(name);

		return changed;
	}



	protected virtual bool Set_VersionProperty(string name, Version value)
	{
		if (!WillStringPropertyChange(name, value?.ToString(), false))
			return false;

		if (Describers[name].DefaultEqualsOrEmptyString(value))
			_AssignedConnectionProperties.Remove(name);
		else
			_AssignedConnectionProperties[name] = (Version)value.Clone();

		RaisePropertyChanged(name);

		return true;
	}


	#endregion Property Getters/Setters





	// =========================================================================================================
	#region Accessor Methods - AbstractPropertyAgent
	// =========================================================================================================


	public virtual object GetProperty(string name)
	{
		if (name == "Password" && _AssignedConnectionProperties.TryGetValue("InMemoryPassword", out object secure))
			return ((SecureString)secure).ToReadable();


		if (_AssignedConnectionProperties.TryGetValue(name, out object value))
			return value;

		if (TryGetSetDerivedProperty(name, out value))
			return value;

		if (!TryGetDefaultValue(name, out object defaultValue))
		{
			ArgumentException ex = new($"Could not find a default value for property {name}.");
			Diag.Dug(ex);
			throw ex;
		}

		return defaultValue;
	}


	public virtual bool Isset(string property)
	{
		return _AssignedConnectionProperties.ContainsKey(property);
	}


	public virtual bool SetProperty(string name, object value)
	{

		bool changed;
		string propertyTypeName;


		if (GetPropertyType(name).IsSubclassOf(typeof(Enum)))
			propertyTypeName = "enum";
		else
			propertyTypeName = GetPropertyTypeName(name).ToLower();


		switch (propertyTypeName)
		{
			case "password":
				changed = Set_PasswordProperty(name, Convert.ToString(value));
				break;
			case "version":
				changed = Set_VersionProperty(name, (Version)value);
				break;
			case "enum":
				changed = Set_EnumProperty(name, value);
				break;
			case "int32":
				changed = Set_IntProperty(name, Convert.ToInt32(value));
				break;
			case "boolean":
				changed = Set_BoolProperty(name, Convert.ToBoolean(value));
				break;
			case "byte[]":
				changed = Set_ByteProperty(name, value);
				break;
			case "string":
				changed = Set_StringProperty(name, Convert.ToString(value));
				break;
			case "object":
				changed = Set_ObjectProperty(name, value);
				break;
			default:
				ArgumentException ex = new($"Property type {propertyTypeName} for property {name} is not supported");
				Diag.Dug(ex);
				throw ex;
		}


		return changed;
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Attempts to derive a value for a property and sets it if successful.
	/// </summary>
	/// <param name="name">The property name</param>
	/// <param name="value">The derived value or fallback value else null</param>
	/// <returns>
	/// Returns true if a useable value was derived, even if the value could not be used
	/// to set the property, else false if no useable property could be derived. An
	/// example of a useable derived property that will not be used to set the property
	/// is the ServerError Icon property when a connection cannot be established.
	/// </returns>
	/// <remarks>
	/// This method may be recursively called because a derived property may be dependent
	/// on other derived properties, any of which may require access to an open database
	/// connection. Each get/set method returns a boolean indicating whether or not it
	/// opened the connection. We track that and only close the connection (if necessary)
	/// when we exit the recursion to avoid repetitively opening and closing the connection. 
	/// </remarks>
	// ---------------------------------------------------------------------------------
	public virtual bool TryGetSetDerivedProperty(string name, out object value)
	{
		bool connectionOpened = false;
		bool result = false;

		_GetSetCardinal++;

		try
		{
			switch (name)
			{
				case "DatasetKey":
					(value, connectionOpened) = GetSet_DatasetKey();
					result = true;
					break;
				case "Icon":
					(value, connectionOpened) = GetSet_Icon();
					result = true;
					break;
				case "ServerEngine":
					(value, connectionOpened) = GetSet_ServerEngine();
					result = value != null;
					break;
				case "ServerVersion":
					(value, connectionOpened) = GetSet_ServerVersion();
					result = value != null;
					break;
				case "ClientVersion":
					(value, connectionOpened) = GetSet_ClientVersion();
					result = value != null;
					break;
				default:
					value = null;
					break;
			}
		}
		finally
		{
			_GetSetConnectionOpened |= connectionOpened;
			_GetSetCardinal--;

			if (_GetSetCardinal == 0 && _GetSetConnectionOpened)
			{
				try
				{
					DataConnection.Close();
				}
				catch { }

				_GetSetConnectionOpened = false;
			}
		}

		return result;

	}




	protected virtual bool WillPasswordPropertyChange(string name, string newValue, bool removing)
	{
		bool isSet = Isset(name);
		bool isSetInMemory = Isset("InMemoryPassword");

		if (removing)
		{
			if (!isSet && !isSetInMemory)
				return false;

			if (isSet)
				_AssignedConnectionProperties.Remove(name);
			if (isSetInMemory)
				_AssignedConnectionProperties.Remove("InMemoryPassword");

			_ParametersChanged = true;
			return true;
		}


		bool changed;
		string currValue = isSetInMemory
			? ((SecureString)_AssignedConnectionProperties["InMemoryPassword"]).ToReadable()
			: (isSet ? (string)_AssignedConnectionProperties[name] : null);

		if (Cmd.NullEquality(newValue, currValue) <= EnNullEquality.Equal)
		{
			changed = Cmd.NullEquality(newValue, currValue) == EnNullEquality.UnEqual;

			if (changed && IsParameter(name))
				_ParametersChanged = true;

			return changed;
		}

		changed = currValue != newValue;
		_ParametersChanged = true;

		return changed;
	}




	#endregion Accessor Methods


}
