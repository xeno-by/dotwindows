using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FarNet.Tools.ViewBuilder.Binding;
using FarNet.Tools.ViewBuilder.Binding.Enums;
using FarNet.Tools.ViewBuilder.Common;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Builders
{
    public abstract class BaseBuilder
    {
        public abstract Type TypeOfResult
        {
            get;
        }

        public virtual object Assembly(ViewFactory factory, BuildContext context)
        {
            object control = create(factory, context);

            var mapping = factory.GetMapping(TypeOfResult);

            //Name="nameOfControl"            

            applyMapping(control.GetType(), control, mapping, factory, context);

            if (mapping.HasName)
            {
                string name = (string)getAttributeValue(context.CurrentNode, typeof(string), "Name");

                if (string.IsNullOrEmpty(name) == false)
                {
                    BindingHelper.SetValueIfViewHasProperty(context.TypeOfView, context.View, name, control);
                }
            }

            return control;
        }

        protected abstract object create(
            ViewFactory factory,
            BuildContext context);

        protected virtual void applyMapping(
            Type controlType,
            object control,
            BaseMap mapping,
            ViewFactory factory,
            BuildContext context)
        {
            foreach (var pm in mapping.PropertyMaps)
            {
                if (tryParseComplexValue(controlType, control, factory, context, pm)) continue;

                switch (pm.Kind)
                {
                    case PropertyMap.EKind.Value:
                        setControlProperty(controlType, control, factory, context, pm);
                        break;
                    case PropertyMap.EKind.Event:
                        addEventHandler(controlType, control, factory, context, pm);
                        break;
                }
            }
        }

        protected virtual bool tryParseComplexValue(
            Type typeOfControl,
            object control,
            ViewFactory factory,
            BuildContext context,
            PropertyMap pm)
        {
            var name = pm.Name;

            var binding = ComplexParser.GetValue(
                getAttributeValue(context.CurrentNode, typeof(string), name),
                "Binding",
                "Path");

            if (binding != null)
            {
                object source = null;
                PropertyInfo sourceProperty = null;

                // будет работать только если контрол, на который биндимся уже создали (пока что и так хватит) 
                if (binding.ContainsKey("SourceControl"))
                {
                    source = factory.GetControl(binding["SourceControl"]);
                    sourceProperty = getProperty(source.GetType(), binding["Path"]);
                }
                else
                {
                    source = context.View;
                    sourceProperty = getProperty(context.TypeOfView, binding["Path"]);
                }

                object target = control;
                PropertyInfo targetProperty = getProperty(typeOfControl, name);

                // тут targetProperty или sourceProperty могут быть фиктивными и потому не надо делать жосткой проверки
                // базовый BindingExpression сам выбросить исключение если надо 

                if (source != null &&
                    target != null)
                {
                    EBingingMode mode = EBingingMode.OneTime;

                    if (binding.ContainsKey("Mode")) mode = (EBingingMode)ValueParser.GetValue(typeof(EBingingMode), binding["Mode"]);

                    BindingExpression be = createBindingExpression(
                        mode,
                        source,
                        sourceProperty,
                        target,
                        targetProperty);

                    applyBindingExpression(
                        be,
                        factory,
                        binding);
                }

                return true;
            }

            var resource = ComplexParser.GetValue(
                getAttributeValue(context.CurrentNode, typeof(string), name),
                "Resource",
                "Key");

            if (resource != null)
            {
                var property = getProperty(typeOfControl, name);
                var value = factory.GetLocalString(resource["Key"]);

                if (property != null && property.CanWrite)
                {
                    property.SetValue(control, value, null);
                }

                return true;
            }

            return false;
        }

        protected virtual void setControlProperty(
            Type typeOfControl,
            object control,
            ViewFactory factory,
            BuildContext context,
            PropertyMap pm)
        {
            var name = pm.Name;

            PropertyInfo property = getProperty(typeOfControl, name);

            if (property == null || property.CanWrite == false) return;

            object value = getAttributeValue(context.CurrentNode, property.PropertyType, name);

            if (value != null) property.SetValue(control, value, null);
        }

        protected virtual BindingExpression createBindingExpression(
            EBingingMode mode,
            object source,
            PropertyInfo sourceProperty,
            object target,
            PropertyInfo targetProperty)
        {
            if (sourceProperty == null) throw new ArgumentNullException("sourceProperty");
            if (targetProperty == null) throw new ArgumentNullException("targetProperty");

            var be = new BindingExpression(
                        mode,
                        source,
                        sourceProperty,
                        target,
                        targetProperty);            

            return be;
        }

        protected virtual void applyBindingExpression(
            BindingExpression be,
            ViewFactory factory,
            Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey("Converter")) be.Converter = factory.GetBindingConverter(parameters["Converter"]);
            if (parameters.ContainsKey("ConverterParameter")) be.ConverterParameter = parameters["ConverterParameter"];

            factory.CreateBinding(be);
        }

        protected virtual void addEventHandler(
            Type typeOfControl,
            object control,
            ViewFactory factory,
            BuildContext context,
            PropertyMap pm)
        {
            var name = pm.Name;
            var eventHandlerName = (string)getAttributeValue(context.CurrentNode, typeof(string), name);

            if (string.IsNullOrEmpty(eventHandlerName) == false)
            {
                var ei = getEvent(typeOfControl, name);
                var mi = getEventHandler(context.TypeOfView, eventHandlerName);

                if (ei != null && mi != null)
                {
                    ei.AddEventHandler(control, Delegate.CreateDelegate(ei.EventHandlerType, context.View, mi));
                }
            }
        }

        protected virtual PropertyInfo getProperty(
            Type targetType,
            string dataPath,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            return targetType.GetProperty(dataPath, flags);
        }

        protected virtual EventInfo getEvent(
            Type targetType,
            string name,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            return targetType.GetEvent(name, flags);
        }

        protected virtual MethodInfo getEventHandler(
            Type targetType,
            string name,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            var methodList = targetType
                .GetMethods(flags)
                .Where(m => m.Name == name && m.ReturnType == typeof(void) && m.GetParameters().Length == 2)
                .FirstOrDefault();

            return methodList;
        }

        protected virtual object getAttributeValue(
            XElement node,
            Type propertyType,
            string attrName,
            object def = null)
        {
            XAttribute attr = node.Attribute(attrName);

            if (attr != null) return ValueParser.GetValue(propertyType, attr.Value);
            
            return def;
        }
    }
}
