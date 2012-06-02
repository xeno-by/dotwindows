using System;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Builders.ContainerBuilders.Bases;
using FarNet.Tools.ViewBuilder.Common;

namespace FarNet.Tools.ViewBuilder.Builders.ContainerBuilders
{
    public class BoxControlBuilder : BaseControlContainerBuilder
    {
        public override Type TypeOfResult
        {
            get { return typeof(IBox); }
        }

        protected override object create(ViewFactory factory, BuildContext context)
        {
            //Box

            //Rect="left,top,right,bottom"

            var place = (Place)getAttributeValue(context.CurrentNode, typeof(Place), "Rect");

            if (place.Right < 0) place.Right = place.Left - place.Right;

            object resultControl = context.Dialog.AddBox(place.Left, place.Top, place.Right, place.Bottom, string.Empty);

            return resultControl;
        }

        public override object Assembly(ViewFactory factory, BuildContext context)
        {
            object control = base.Assembly(factory, context);

            assemblyInner(factory, context);

            return control;
        }
    }
}
