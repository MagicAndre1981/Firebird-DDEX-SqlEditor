// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.UI.PropertyGridUtilities.GlobalBoolConverter
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;
using BlackbirdSql.Core.Controls.Events;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Interfaces;
using BlackbirdSql.Core.Model.Interfaces;
using BlackbirdSql.Core.Properties;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public abstract class AbstractBoolConverter : BooleanConverter, IBAutomationConverter, IDisposable
{
	private IBSettingsModel _Model = null;
	private string _PropertyName;
	private bool _IsAutomator = false;

	private IDictionary<string, bool> _Dependents = null;



	protected virtual System.Resources.ResourceManager ResMgr => AttributeResources.ResourceManager;

	public abstract string LiteralFalseResource { get; }
	public abstract string LiteralTrueResource { get; }

	public AbstractBoolConverter()
	{
		// Tracer.Trace(GetType(), ".ctor");
	}

	public void Dispose()
	{
		if (_Model != null)
		{
			if (_IsAutomator)
				_Model.AutomationPropertyValueChangedEvent -= OnAutomationPropertyValueChanged;
			_Model = null;
		}
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		string text = value as string;

		if (!string.IsNullOrEmpty(text))
		{
			if (ResMgr.GetResourceSet(culture, createIfNotExists: true, tryParents: true) != null)
			{
				if (string.Equals(text, GetGlobalizedLiteralFalse(culture), StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				if (string.Equals(text, GetGlobalizedLiteralTrue(culture), StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			throw new FormatException();
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType != null && value != null && value is bool bvalue)
		{
			RegisterModel(context, bvalue);


			if (destinationType == typeof(string)
				&& ResMgr.GetResourceSet(culture, createIfNotExists: true, tryParents: true) != null)
			{
				if (bvalue)
				{
					return GetGlobalizedLiteralTrue(culture);
				}
				return GetGlobalizedLiteralFalse(culture);
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}



	public bool UpdateReadOnly(object oldValue, object newValue)
	{
		bool bOldValue = (bool)oldValue;
		bool bNewValue = (bool)newValue;

		if (bOldValue == bNewValue)
			return false;

		bool result = false;
		bool readOnly;

		PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(_Model);

		foreach (KeyValuePair<string, bool> pair in _Dependents)
		{
			PropertyDescriptor descriptor = descriptors[pair.Key];

			if (descriptor.Attributes[typeof(ReadOnlyAttribute)] is ReadOnlyAttribute attr)
			{
				// Tracer.Trace($"Automator {_PropertyName} updating dependent: {pair.Key} from {fld.GetValue(attr)} to {(pair.Value ? bvalue : !bvalue)}.");

				readOnly = pair.Value ? bNewValue : !bNewValue;

				FieldInfo fieldInfo = Reflect.GetFieldInfo(attr, "isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);

				if ((bool)Reflect.GetFieldInfoValue(attr, fieldInfo) != readOnly)
				{
					result = true;
					Reflect.SetFieldInfoValue(attr, fieldInfo, readOnly);
				}

			}
		}

		return result;
	}


	private bool RegisterModel(ITypeDescriptorContext context, bool value)
	{
		if (_Model != null || context.Instance is not IBSettingsModel model)
			return false;

		_Model = model;
		_Model.Disposed += OnModelDisposed;

		_PropertyName = context.PropertyDescriptor.Name;


		if (context.PropertyDescriptor.Attributes[typeof(AutomationAttribute)] is not AutomationAttribute attr
			|| attr.Automator != null)
		{
			return true;
		}

		_IsAutomator = true;
		_Model.AutomationPropertyValueChangedEvent += OnAutomationPropertyValueChanged;

		// Tracer.Trace($"Registering automator {_PropertyName}.");

		_Dependents = new Dictionary<string, bool>(model.PropertyWrappers.Count);

		foreach (IBSettingsModelPropertyWrapper property in model.PropertyWrappers)
		{
			if (property.Automator != null && property.Automator == _PropertyName)
			{
				// Tracer.Trace($"Registering automation dependent for {_PropertyName}: Dependent: {property.PropertyName}.");
				_Dependents.Add(property.PropertyName, property.InvertAutomation);
			}
		}

		UpdateReadOnly(!value, value);

		return true;
	}

	private string GetGlobalizedLiteralTrue(CultureInfo culture)
	{
		return ResMgr.GetString(LiteralTrueResource, culture);
	}

	private string GetGlobalizedLiteralFalse(CultureInfo culture)
	{
		return ResMgr.GetString(LiteralFalseResource, culture);
	}

	public void OnAutomationPropertyValueChanged(object sender, AutomationPropertyValueChangedEventArgs e)
	{
		if (e.ChangedItem.PropertyDescriptor.Name != _PropertyName)
			return;

		e.ReadOnlyChanged = UpdateReadOnly((bool)e.OldValue, (bool)e.ChangedItem.Value);
	}

	public void OnModelDisposed(object sender, EventArgs e)
	{
		_Model = null;
	}

}
