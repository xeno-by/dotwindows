using System;
using FarNet.Tools.ViewBuilder.Interfaces;

namespace FarNet.Tools.ViewBuilder.ValueConverters
{
    public class BooleanNotConverter : IBindingValueConverter
    {
        #region IBindingValueConverter Members

        public object Convert(object value, Type targetType, object parameter)
        {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type sourceType, object parameter)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
