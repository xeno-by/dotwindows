using System.Linq;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Common;
using FarNet.Tools.ViewBuilder.Wrappers;
using FarNet.Tools.ViewBuilder.Mapping.Maps;
using FarNet.Tools.ViewBuilder.Binding;
using FarNet.Tools.ViewBuilder.Binding.CustomExpressions;
using System.Reflection;
using FarNet.Tools.ViewBuilder.Binding.Enums;

namespace FarNet.Tools.ViewBuilder.Builders.ContainerBuilders.Bases
{
    public abstract class BaseControlContainerBuilder : BaseContainerBuilder
    {
        public override object Assembly(ViewFactory factory, BuildContext context)
        {
            object control = base.Assembly(factory, context);            

            string controlName = (string)getAttributeValue(context.CurrentNode, typeof(string), "Name");

            if (string.IsNullOrEmpty(controlName) == false)
            {
                factory.AddControl(controlName, (IControl)control);
            }

            // add color wrapper
            if (context.CurrentNode.Attributes().Select(a => a.Name.LocalName).Any(a => a.StartsWith("Background") || a.StartsWith("Foreground")))
            {
                var colorWrapper = new FarControlColorWrapper((IControl)control);

                applyMapping(colorWrapper.GetType(), colorWrapper, factory.GetMapping(typeof(ColorControlMap)), factory, context);
            }

            // save node inner value as IControl.Data
            ((IControl)control).Data = context.CurrentNode.Value;  
               
            return control;
        }

        protected override BindingExpression createBindingExpression(
            EBingingMode mode,
            object source,
            PropertyInfo sourceProperty,
            object target,
            PropertyInfo targetProperty)
        {
            return new StandardControlBindingExpression(mode, source, sourceProperty, target, targetProperty);
        }
    }
}
