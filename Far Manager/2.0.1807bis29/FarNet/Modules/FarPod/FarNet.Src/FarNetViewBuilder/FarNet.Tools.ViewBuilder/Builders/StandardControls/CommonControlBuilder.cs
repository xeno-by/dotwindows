using System;
using System.Linq;
using System.Reflection;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Binding;
using FarNet.Tools.ViewBuilder.Binding.CustomExpressions;
using FarNet.Tools.ViewBuilder.Binding.Enums;
using FarNet.Tools.ViewBuilder.Common;
using FarNet.Tools.ViewBuilder.Mapping.Bases;
using FarNet.Tools.ViewBuilder.Wrappers;
using FarNet.Tools.ViewBuilder.Mapping.Maps;

namespace FarNet.Tools.ViewBuilder.Builders.StandardControls
{
    public class CommonControlBuilder<T> : BaseBuilder
        where T : IControl
    {
        public override Type TypeOfResult
        {
            get { return typeof(T); }
        }

        protected override object create(ViewFactory factory, BuildContext context)
        {
            object resultControl = null;

            //Rect="left,top,right,bottom"

            var place = (Place)getAttributeValue(context.CurrentNode, typeof(Place), "Rect");

            if (place.Right < 0) place.Right = place.Left - place.Right;

            //Button
            //CheckBox
            //RadioButton
            //Text    
            //Edit

            if (typeof(T) == typeof(IButton))
            {
                resultControl = context.Dialog.AddButton(place.Left, place.Top, string.Empty);
            }
            else if (typeof(T) == typeof(ICheckBox))
            {
                resultControl = context.Dialog.AddCheckBox(place.Left, place.Top, string.Empty);
            }
            else if (typeof(T) == typeof(IRadioButton))
            {
                resultControl = context.Dialog.AddRadioButton(place.Left, place.Top, string.Empty);
            }
            else if (typeof(T) == typeof(IText))
            {
                var isVertical = (bool)getAttributeValue(context.CurrentNode, typeof(bool), "Vertical", false);

                if (isVertical)
                {
                    resultControl = context.Dialog.AddVerticalText(place.Left, place.Top, place.Right, string.Empty);
                }
                else
                {
                    resultControl = context.Dialog.AddText(place.Left, place.Top, place.Right, string.Empty);
                }
            }
            else if (typeof(T) == typeof(IEdit))
            {
                var isFixed = (bool)getAttributeValue(context.CurrentNode, typeof(bool), "Fixed", false);
                var isPassword = (bool)getAttributeValue(context.CurrentNode, typeof(bool), "IsPassword", false);

                if (isFixed)
                {
                    resultControl = context.Dialog.AddEditFixed(place.Left, place.Top, place.Right, string.Empty);
                }
                else if (isPassword)
                {
                    resultControl = context.Dialog.AddEditPassword(place.Left, place.Top, place.Right, string.Empty);
                }
                else
                {
                    resultControl = context.Dialog.AddEdit(place.Left, place.Top, place.Right, string.Empty);
                }
            }

            return resultControl;
        }

        public override object Assembly(ViewFactory factory, BuildContext context)
        {
            var control = (T)base.Assembly(factory, context);      

            string controlName = (string)getAttributeValue(context.CurrentNode, typeof(string), "Name");

            if (string.IsNullOrEmpty(controlName) == false)
            {
                factory.AddControl(controlName, control);
            }

            // add color wrapper
            if (context.CurrentNode.Attributes().Select(a => a.Name.LocalName).Any(a => a.StartsWith("Background") || a.StartsWith("Foreground")))
            {
                var colorWrapper = new FarControlColorWrapper(control);

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

        protected override bool tryParseComplexValue(Type typeOfControl, object control, ViewFactory factory, BuildContext context, PropertyMap pm)
        {
            if (base.tryParseComplexValue(typeOfControl, control, factory, context, pm)) return true;

            var name = pm.Name;

            // allow direct call any parameterless method
            var command = ComplexParser.GetValue(
                getAttributeValue(context.CurrentNode, typeof(string), name),
                "Command",
                "Name");

            if (command != null)
            {
                if (pm.Kind == PropertyMap.EKind.Event)
                {
                    // TODO
                }
            }

            return false;
        }
    }
}
