﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.IO;
using System.Xml;
using BlackbirdSql.Core.Ctl.Config;
using Microsoft.VisualStudio.Data.Core;


namespace BlackbirdSql.Core.Extensions;

// =========================================================================================================
//											XmlParser Class
//
/// <summary>
/// Xml parser utility methods
/// </summary>
// =========================================================================================================
public static class XmlParser
{
	#region Fields


	#endregion Fields





	// =========================================================================================================
	#region Methods - XmlParser
	// =========================================================================================================



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has the db provider invariant configured in the app.config
	/// and configures it if it doesn't
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if app,config was modified else false.</returns>
	// ---------------------------------------------------------------------------------
	public static bool ConfigureDbProvider(string xmlPath, Type factoryClass)
	{
		bool modified;


		try
		{
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(xmlPath);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				return false;
			}


			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);

			if (!xmlNs.HasNamespace("confBlackbirdNs"))
			{
				xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);
			}


			modified = ConfigureDbProviderFactory(xmlDoc, xmlNs, xmlRoot, factoryClass);


			if (modified)
			{
				try
				{
					xmlDoc.Save(xmlPath);
					// Evs.Debug(typeof(XmlParser), "ConfigureDbProvider()", $"app.config save: {xmlPath}.");
				}
				catch (Exception ex)
				{
					modified = false;
					Diag.Ex(ex);
					return false;
				}

			}
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		return modified;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks if a project has database EntityFramework configured in the app.config
	/// and configures it if it doesn't.
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <param name="configureDbProvider">
	///	If set to true then the client DBProvider factory will also be configured
	/// </param>
	/// <returns>true if app,config was modified else false.</returns>
	// ---------------------------------------------------------------------------------
	public static bool ConfigureEntityFramework(string xmlPath, bool configureDbProvider, Type factoryClass)
	{
		bool modified = false;


		try
		{
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(xmlPath);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				return false;
			}


			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);

			if (!xmlNs.HasNamespace("confBlackbirdNs"))
			{
				xmlNs.AddNamespace("confBlackbirdNs", xmlRoot.NamespaceURI);
			}

			if (configureDbProvider)
				modified = ConfigureDbProviderFactory(xmlDoc, xmlNs, xmlRoot, factoryClass);

			modified |= ConfigureEntityFrameworkProviderServices(xmlDoc, xmlNs, xmlRoot);


			if (modified)
			{
				try
				{
					xmlDoc.Save(xmlPath);
					// Evs.Debug(typeof(XmlParser), "ConfigureEntityFramework()", $"app.config save: {xmlPath}.");
				}
				catch (Exception ex)
				{
					modified = false;
					Diag.Ex(ex);
					return false;
				}

			}
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		return modified;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the app.config xml EntityFramework section.
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if xml was modified else false</returns>
	// ---------------------------------------------------------------------------------
	private static bool ConfigureEntityFrameworkProviderServices(XmlDocument xmlDoc, XmlNamespaceManager xmlNs, XmlNode xmlRoot)
	{
		bool modified = false;


		try
		{
			// For anyone watching, you have to denote your private namespace after every forwardslash in
			// the markup tree.
			// Q? Does this mean you can use different namespaces within the selection string?
			XmlNode xmlNode = null, xmlParent;
			XmlAttribute xmlAttr;

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbirdNs:entityFramework", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "entityFramework", "");
				xmlParent = xmlRoot.AppendChild(xmlNode);
			}
			else
			{
				xmlParent = xmlNode;
			}


			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:defaultConnectionFactory", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "defaultConnectionFactory", "");
				xmlAttr = xmlDoc.CreateAttribute("type");
				xmlAttr.Value = NativeDb.EFConnectionFactory + ", " + NativeDb.EFProvider;
				xmlNode.Attributes.Append(xmlAttr);
				xmlParent.AppendChild(xmlNode);
			}


			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:providers", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "providers", "");
				xmlParent = xmlParent.AppendChild(xmlNode);
			}
			else
			{
				xmlParent = xmlNode;
			}

			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:provider[@invariantName='" + NativeDb.Invariant + "']", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "provider", "");
				xmlAttr = xmlDoc.CreateAttribute("invariantName");
				xmlAttr.Value = NativeDb.Invariant;
				xmlNode.Attributes.Append(xmlAttr);
				xmlAttr = xmlDoc.CreateAttribute("type");
				xmlAttr.Value = NativeDb.EFProviderServices + ", " + NativeDb.EFProvider;
				xmlNode.Attributes.Append(xmlAttr);
				xmlParent.AppendChild(xmlNode);
			}
			else
			{
				xmlAttr = (XmlAttribute)xmlNode.Attributes.GetNamedItem("invariantName");
				if (xmlAttr == null)
				{
					modified = true;
					xmlAttr = xmlDoc.CreateAttribute("invariantName");
					xmlAttr.Value = NativeDb.Invariant;
					xmlNode.Attributes.Append(xmlAttr);
				}
				else if (xmlAttr.Value != NativeDb.Invariant)
				{
					modified = true;
					xmlAttr.Value = NativeDb.Invariant;
				}

				xmlAttr = (XmlAttribute)xmlNode.Attributes.GetNamedItem("type");
				if (xmlAttr == null)
				{
					modified = true;
					xmlAttr = xmlDoc.CreateAttribute("type");
					xmlAttr.Value = NativeDb.EFProviderServices + ", " + NativeDb.EFProvider;
					xmlNode.Attributes.Append(xmlAttr);
				}
				else if (xmlAttr.Value.ToLower().Replace(" ", "") != NativeDb.EFProviderServices.ToLower() + "," + NativeDb.EFProvider.ToLower())
				{
					modified = true;
					xmlAttr.Value = NativeDb.EFProviderServices + ", " + NativeDb.EFProvider;
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}


		return modified;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates the app.config xml system.data section with the db provider invariant.
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if the app.config could not be successfully verified/updated
	/// </exception>
	/// <returns>true if xml was modified else false</returns>
	// ---------------------------------------------------------------------------------
	private static bool ConfigureDbProviderFactory(XmlDocument xmlDoc, XmlNamespaceManager xmlNs, XmlNode xmlRoot, Type factoryClass)
	{
		bool modified = false;


		try
		{
			// For anyone watching, you have to denote your private namespace after every forwardslash in
			// the markup tree.
			// Q? Does this mean you can use different namespaces within the selection string?
			XmlNode xmlNode = null, xmlParent;
			XmlAttribute xmlAttr;

			xmlNode = xmlRoot.SelectSingleNode("//confBlackbirdNs:system.data", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "system.data", "");
				xmlParent = xmlRoot.AppendChild(xmlNode);
			}
			else
			{
				xmlParent = xmlNode;
			}


			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:DbProviderFactories", xmlNs);

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "DbProviderFactories", "");
				xmlParent = xmlParent.AppendChild(xmlNode);
			}
			else
			{
				xmlParent = xmlNode;
			}

			xmlNode = xmlParent.SelectSingleNode("confBlackbirdNs:add[@invariant='" + NativeDb.Invariant + "']", xmlNs);

			// We're using the current latest version of the client (on this build it's 9.1.1.0)
			string factoryQualifiedNameType;
			string factoryNameType = NativeDb.ProviderFactoryClassName + ", " + NativeDb.Invariant;

			if (xmlNode == null)
			{
				modified = true;

				xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "add", "");

				xmlAttr = xmlDoc.CreateAttribute("invariant");
				xmlAttr.Value = NativeDb.Invariant;
				xmlNode.Attributes.Append(xmlAttr);

				xmlAttr = xmlDoc.CreateAttribute("name");
				xmlAttr.Value = NativeDb.ProviderFactoryName;
				xmlNode.Attributes.Append(xmlAttr);

				xmlAttr = xmlDoc.CreateAttribute("description");
				xmlAttr.Value = NativeDb.ProviderFactoryDescription;
				xmlNode.Attributes.Append(xmlAttr);

				xmlAttr = xmlDoc.CreateAttribute("type");
				xmlAttr.Value = factoryNameType;
				xmlNode.Attributes.Append(xmlAttr);

				xmlParent.AppendChild(xmlNode);
			}
			else
			{
				xmlAttr = (XmlAttribute)xmlNode.Attributes.GetNamedItem("type");
				if (xmlAttr == null)
				{
					modified = true;
					xmlAttr = xmlDoc.CreateAttribute("type");
					xmlAttr.Value = factoryNameType;
					xmlNode.Attributes.Append(xmlAttr);
				}
				else if (xmlAttr.Value.Replace(" ", "") != factoryNameType.Replace(" ", ""))
				{
					// Check if it's not using the fully qualified name - must be current latest version (build is 9.1.1)
					factoryQualifiedNameType = factoryClass.AssemblyQualifiedName;

					if (xmlAttr.Value.Replace(" ", "") != factoryQualifiedNameType.Replace(" ", ""))
					{
						modified = true;
						xmlAttr.Value = factoryNameType;
					}
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}


		return modified;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Establishes if an edmx is a valid extension database edmx.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static bool IsValidEdmx(string xmlPath)
	{

		try
		{
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(xmlPath);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				throw;
			}

			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);


			if (!xmlNs.HasNamespace("edmxBlackbird"))
				xmlNs.AddNamespace("edmxBlackbird", xmlRoot.NamespaceURI);
			if (!xmlNs.HasNamespace("ssdlBlackbird"))
				xmlNs.AddNamespace("ssdlBlackbird", "http://schemas.microsoft.com/ado/2009/11/edm/ssdl");



			XmlNode xmlNode = null;


			// You have to denote your private namespaces after every forwardslash in the markup tree.
			try
			{
				xmlNode = xmlRoot.SelectSingleNode("edmxBlackbird:Runtime/edmxBlackbird:StorageModels/ssdlBlackbird:Schema[@Provider='" + NativeDb.Invariant + "']", xmlNs);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				return false;
			}

			return xmlNode != null;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Updates an edmx if it was using the legacy database client.
	/// </summary>
	/// <param name="project"></param>
	/// <exception cref="Exception">
	/// Throws an exception if there weere errors.
	/// </exception>
	/// <returns>true if edmx was modified else false.</returns>
	// ---------------------------------------------------------------------------------
	public static bool UpdateEdmx(string xmlPath)
	{

		try
		{
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(xmlPath);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				throw;
			}

			XmlNode xmlRoot = xmlDoc.DocumentElement;
			XmlNamespaceManager xmlNs = new XmlNamespaceManager(xmlDoc.NameTable);


			if (!xmlNs.HasNamespace("edmxBlackbird"))
				xmlNs.AddNamespace("edmxBlackbird", xmlRoot.NamespaceURI);
			if (!xmlNs.HasNamespace("ssdlBlackbird"))
				xmlNs.AddNamespace("ssdlBlackbird", "http://schemas.microsoft.com/ado/2009/11/edm/ssdl");



			XmlNode xmlNode = null;
			XmlAttribute xmlAttr;


			// You have to denote your private namespaces after every forwardslash in the markup tree.
			try
			{
				xmlNode = xmlRoot.SelectSingleNode("edmxBlackbird:Runtime/edmxBlackbird:StorageModels/ssdlBlackbird:Schema[@Provider='" + NativeDb.Invariant + "']", xmlNs);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				return false;
			}

			if (xmlNode == null)
				return false;


			xmlNode = xmlRoot.SelectSingleNode("//edmxBlackbird:Designer/edmxBlackbird:Options/edmxBlackbird:DesignerInfoPropertySet/edmxBlackbird:DesignerProperty[@Name='UseLegacyProvider']", xmlNs);

			if (xmlNode == null)
				return false;


			xmlAttr = (XmlAttribute)xmlNode.Attributes.GetNamedItem("Value");

			if (xmlAttr != null && xmlAttr.Value == "false")
				return false;

			if (xmlAttr == null)
			{
				xmlAttr = xmlDoc.CreateAttribute("Value");
				xmlAttr.Value = "false";
				xmlNode.Attributes.Append(xmlAttr);
			}
			else
			{
				xmlAttr.Value = "false";
			}

			try
			{
				xmlDoc.Save(xmlPath);
				// Evs.Debug(typeof(XmlParser), "UpdateEdmx()", $"edmx Xml saved: {xmlPath}.");
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
				throw;
			}
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		return true;

	}

	#endregion Methods

}