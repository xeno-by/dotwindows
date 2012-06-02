using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Binding;
using FarNet.Tools.ViewBuilder.Binding.CustomExpressions;
using FarNet.Tools.ViewBuilder.Binding.Enums;
using FarNet.Tools.ViewBuilder.Builders.ContainerBuilders.Bases;
using FarNet.Tools.ViewBuilder.Common;

namespace FarNet.Tools.ViewBuilder.Builders.ContainerBuilders
{
    public class ListControlBuilder<T> : BaseControlContainerBuilder
        where T : IBaseList
    {
        public override Type TypeOfResult
        {
            get { return typeof(T); }
        }

        protected override object create(ViewFactory factory, BuildContext context)
        {
            object resultControl = null;

            Place place = (Place)getAttributeValue(context.CurrentNode, typeof(Place), "Rect");

            if (place.Right < 0) place.Right = place.Left - place.Right;

            // ListBox
            // ComboBox

            if (typeof(T) == typeof(IListBox))
            {
                resultControl = context.Dialog.AddListBox(place.Left, place.Top, place.Right, place.Bottom, string.Empty);
            }
            else if (typeof(T) == typeof(IComboBox))
            {
                resultControl = context.Dialog.AddComboBox(place.Left, place.Top, place.Right, string.Empty);
            }

            return resultControl;
        }

        public override object Assembly(ViewFactory factory, BuildContext context)
        {
            var control = (T)base.Assembly(factory, context);

            foreach (object obj in assemblyInner(factory, context))
            {
                if (obj is FarItem) control.Items.Add((FarItem)obj);
            }

            return control;
        }

        protected override BindingExpression createBindingExpression(
            EBingingMode mode, 
            object source, 
            PropertyInfo sourceProperty, 
            object target,
            PropertyInfo targetProperty)
        {            
            if (typeof(IList<FarItem>).IsAssignableFrom(targetProperty.PropertyType) &&
                typeof(ObservableCollection<FarItem>).IsAssignableFrom(sourceProperty.PropertyType))
            {
                // create collection binding wrapper for IBaseList.Items ...
                return new CollectionBindingExpression(mode, source, sourceProperty, target, targetProperty);
            }

            if (targetProperty.Name == "SelectedItem")
            {
                // create binding for fake property SelectedItem typeof(FarItem)
                return new SelectedItemBindingExpression(mode, source, sourceProperty, target, targetProperty);
            }

            return base.createBindingExpression(mode, source, sourceProperty, target, targetProperty);
        }
    }
}
