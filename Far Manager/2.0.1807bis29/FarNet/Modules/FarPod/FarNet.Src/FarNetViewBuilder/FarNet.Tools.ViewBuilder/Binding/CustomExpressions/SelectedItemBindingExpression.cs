using System;
using System.Linq;
using System.Reflection;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Binding.Enums;

namespace FarNet.Tools.ViewBuilder.Binding.CustomExpressions
{
    public class SelectedItemBindingExpression : BindingExpression
    {
        public SelectedItemBindingExpression(EBingingMode mode, object source, PropertyInfo sourceProperty, object target, PropertyInfo targetProperty)
            : base(mode, source, sourceProperty, target, targetProperty)
        {
            
        }

        public override void UpdateSource(EventArgs e)
        {
            // ничего не делать. контрол не может оповещать об изменения выбранного элемента
        }

        public override void UpdateTarget(EventArgs e)
        {
            var selectedItem = sourceItem;

            var selectedIndex = listControl.Items.TakeWhile(fi => fi != selectedItem).Count();

            listControl.Selected = selectedIndex;
        }

        private FarItem sourceItem
        {
            get
            {
                // нужно ли уже здесь применять Converter ???

                return (FarItem)getSourceValue();
            }
        }

        private IBaseList listControl
        {
            get
            {
                return (IBaseList)Target;
            }
        }
    }
}
