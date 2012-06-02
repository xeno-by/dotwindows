using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Binding;
using FarNet.Tools.ViewBuilder.Builders.ContainerBuilders.Bases;
using FarNet.Tools.ViewBuilder.Common;
using FarNet.Tools.ViewBuilder.Mapping;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Builders.ContainerBuilders
{
    public class CollectionBuilder : BaseContainerBuilder
    {
        public class ItemCollection
        {
            public Place StartPlace
            {
                get;
                set;
            }

            public int Orientation
            {
                get;
                set;
            }

            public string TypeOfItem
            {
                get;
                set;
            }

            public IEnumerable Items
            {
                get;
                set;
            }
        }

        public override Type TypeOfResult
        {
            get { return typeof(ItemCollection); }
        }

        protected override object create(ViewFactory factory, BuildContext context)
        {
            return new ItemCollection();
        }

        public override object Assembly(ViewFactory factory, BuildContext context)
        {
            var ic = (ItemCollection)base.Assembly(factory, context);

            BaseMap mapping = factory.GetMapping(TypeOfResult);

            //applyMapping(ic.GetType(), ic, mapping, factory, context);

            if (ic.Items == null || !ic.Items.Cast<object>().Any()) return null;

            BaseBuilder childBuilder = factory.GetBuilder(ic.TypeOfItem);

            if (childBuilder == null) return null;

            if (childBuilder.TypeOfResult == ic.Items.Cast<object>().First().GetType())
            {
                return ic.Items;
            }

            // build new xml

            XElement etalonNode = new XElement(context.CurrentNode);

            foreach (PropertyMap pm in mapping.PropertyMaps)
            {
                var attr = etalonNode.Attribute(pm.Name);

                if (attr != null) attr.Remove();
            }

            bool isFirstItem = true;

            foreach (object childItem in ic.Items)
            {
                var childNode = new XElement(ic.TypeOfItem);

                // calc Rect
                var rect = new Place();

                if (ic.Orientation == 0)
                {
                    // in one line
                    if (isFirstItem)
                    {
                        rect = ic.StartPlace;

                        isFirstItem = false;
                    }
                }
                else if (ic.Orientation == 1)
                {
                    // line by line
                    rect = ic.StartPlace;
                }

                // add rect
                childNode.Add(new XAttribute("Rect", string.Empty));

                childNode.Attribute("Rect").Value = string.Format("{0},{1},{2},{3}", rect.Left, rect.Top, rect.Right, rect.Bottom);

                // add text
                childNode.Add(new XAttribute("Text", string.Empty));

                childNode.Attribute("Text").Value = childItem.ToString();

                etalonNode.Add(childNode);
            }

            XElement currentNode = context.CurrentNode;

            context.CurrentNode = etalonNode;

            var buildResult = assemblyInner(factory, context);

            int index = 0;

            foreach (object result in buildResult)
            {
                if (result is IControl) ((IControl)result).Data = index++;
            }

            context.CurrentNode = currentNode;

            return buildResult;
        }

        protected override void applyBindingExpression(
            BindingExpression be, 
            ViewFactory factory, 
            Dictionary<string, string> parameters)
        {
            base.applyBindingExpression(be, factory, parameters);

            // update UpdateTarget immediately in applyMapping call
            be.UpdateTarget(EventArgs.Empty);
        }
    }
}
