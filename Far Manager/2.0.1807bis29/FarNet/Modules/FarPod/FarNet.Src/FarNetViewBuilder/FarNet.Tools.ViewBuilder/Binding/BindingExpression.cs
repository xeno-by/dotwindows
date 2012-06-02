using System;
using System.ComponentModel;
using System.Reflection;
using FarNet.Tools.ViewBuilder.Binding.Enums;
using FarNet.Tools.ViewBuilder.Interfaces;

namespace FarNet.Tools.ViewBuilder.Binding
{
    public class BindingExpression
    {
        public EBingingMode Mode
        {
            get;
            private set;
        }

        public object Source
        {
            get;
            private set;
        }

        public PropertyInfo SourceProperty
        {
            get;
            private set;
        }

        public object Target
        {
            get;
            private set;
        }

        public PropertyInfo TargetProperty
        {
            get;
            private set;
        }

        public IBindingValueConverter Converter
        {
            get;
            set;
        }

        public object ConverterParameter
        {
            get;
            set;
        }

        public BindingExpression(EBingingMode mode, object source, PropertyInfo sourceProperty, object target, PropertyInfo targetProperty)
        {            
            Mode = mode;
            Source = source;
            SourceProperty = sourceProperty;
            Target = target;
            TargetProperty = targetProperty;
        }

        public virtual void UpdateSource(EventArgs e)
        {
            try
            {
                setPropertyValue(SourceProperty, Source, getTargetValue());
            }
            catch
            {
            }
        }

        public virtual void UpdateTarget(EventArgs e)
        {
            try
            {
                setPropertyValue(TargetProperty, Target, getSourceValue());
            }
            catch
            {
            }
        }

        public virtual INotifyPropertyChanged GetSourceNotifyProvider()
        {
            return getNotifyProvider(Source);
        }

        public virtual INotifyPropertyChanged GetTargetNotifyProvider()
        {
            return getNotifyProvider(Target);
        }

        protected virtual INotifyPropertyChanged getNotifyProvider(object obj)
        {
            if (obj is INotifyPropertyChanged)
            {
                return (INotifyPropertyChanged)obj;
            }
            
            return null;
        }

        protected virtual object getSourceValue()
        {
            return getProperyValue(SourceProperty, Source);
        }

        protected virtual object getTargetValue()
        {
            return getProperyValue(TargetProperty, Target);
        }

        protected virtual object getProperyValue(PropertyInfo property, object obj)
        {
            return property.GetValue(obj, null);
        }

        protected virtual void setPropertyValue(PropertyInfo property, object obj, object value)
        {
            if (Converter != null)
            {
                if (property == TargetProperty)
                    value = Converter.Convert(value, property.PropertyType, ConverterParameter);
                else if (property == SourceProperty)
                    value = Converter.ConvertBack(value, property.PropertyType, ConverterParameter);
            }

            property.SetValue(obj, value, null);
        }
    }
}
