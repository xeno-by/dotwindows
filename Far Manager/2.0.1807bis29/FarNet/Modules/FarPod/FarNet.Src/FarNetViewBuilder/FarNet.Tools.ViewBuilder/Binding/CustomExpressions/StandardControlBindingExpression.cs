using System;
using System.ComponentModel;
using System.Reflection;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Binding.Enums;
using FarNet.Tools.ViewBuilder.Wrappers;

namespace FarNet.Tools.ViewBuilder.Binding.CustomExpressions
{
    public class StandardControlBindingExpression : BindingExpression
    {
        private FarControlNotifyWrapper _notifyWrapper;

        public StandardControlBindingExpression(EBingingMode mode, object source, PropertyInfo sourceProperty, object target, PropertyInfo targetProperty)
            : base(mode, source, sourceProperty, target, targetProperty)
        {

        }

        public override void UpdateTarget(EventArgs e)
        {
            try
            {
                object sourceValue = getSourceValue();
                object targetValue = getTargetValue();

                if (Equals(sourceValue, targetValue)) return;

                if (sourceValue == null)
                {
                    if (TargetProperty.PropertyType == typeof(string))
                        sourceValue = string.Empty;
                    else if (TargetProperty.PropertyType == typeof(bool))
                        sourceValue = false;
                    else if (TargetProperty.PropertyType == typeof(int))
                        sourceValue = 0;
                }

                var editable = Target as IEditable;

                bool wasIsTouched = false;

                if (editable != null) wasIsTouched = editable.IsTouched;                

                setPropertyValue(TargetProperty, Target, sourceValue);

                if (editable != null) editable.IsTouched = wasIsTouched;
            }
            catch
            {
            }
        }

        public override void UpdateSource(EventArgs e)
        {
            try
            {
                object sourceValue = getSourceValue();
                object targetValue = getTargetValue();

                if (Equals(sourceValue, targetValue)) return;

                setPropertyValue(SourceProperty, Source, targetValue);
            }
            catch
            {
            }
        }

        protected override void setPropertyValue(PropertyInfo property, object obj, object value)
        {
            if (property.PropertyType == typeof(bool) && value.GetType() == typeof(int))
            {
                value = ((int)value == 1) ? true : false;
            }

            if (property.PropertyType == typeof(int) && value.GetType() == typeof(bool))
            {
                value = ((bool)value) ? 1 : 0;
            }

            base.setPropertyValue(property, obj, value);            
        }

        protected override INotifyPropertyChanged getNotifyProvider(object obj)
        {
            if (obj is IControl)
            {
                return _notifyWrapper ?? (_notifyWrapper = new FarControlNotifyWrapper((IControl)obj));
            }

            return base.getNotifyProvider(obj);
        }
    }
}
