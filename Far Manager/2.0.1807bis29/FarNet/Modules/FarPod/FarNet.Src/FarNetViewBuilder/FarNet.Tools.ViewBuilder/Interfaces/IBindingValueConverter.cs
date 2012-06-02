using System;

namespace FarNet.Tools.ViewBuilder.Interfaces
{
    public interface IBindingValueConverter
    {
        object Convert(object value, Type targetType, object parameter);

        object ConvertBack(object value, Type sourceType, object parameter);        
    }
}
