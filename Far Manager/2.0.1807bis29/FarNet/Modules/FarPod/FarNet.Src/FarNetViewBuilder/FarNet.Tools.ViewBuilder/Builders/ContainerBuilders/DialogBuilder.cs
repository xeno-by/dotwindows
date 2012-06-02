using System;
using System.Xml.Linq;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Builders.ContainerBuilders.Bases;
using FarNet.Tools.ViewBuilder.Common;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Builders.ContainerBuilders
{
    public class DialogBuilder : BaseContainerBuilder
    {
        public override Type TypeOfResult
        {
            get { return typeof(IDialog); }
        }

        protected override object create(ViewFactory factory, BuildContext context)
        {
            //Rect="left,top,right,bottom"

            var place = (Place)getAttributeValue(context.CurrentNode, typeof(Place), "Rect");

            return Far.Net.CreateDialog(place.Left, place.Top, place.Right, place.Bottom);
        }

        public override object Assembly(ViewFactory factory, BuildContext context)
        {
            var dialog = (IDialog)base.Assembly(factory, context);

            var typeOfDlg = dialog.GetType();

            var mapping = factory.GetMapping(TypeOfResult);            

            context.Dialog = dialog;

            assemblyInner(factory, context);

            //Cancel="IButton"
            //Default="IControl"
            //Focused="IControl"
            
            foreach (var pm in mapping.PropertyMaps)
            {                
                if (pm.Kind == PropertyMap.EKind.Control) setPropertyIfHasControl(typeOfDlg, dialog, context.CurrentNode, factory, pm.Name);
            }            

            return dialog;
        }

        protected override bool tryParseComplexValue(Type typeOfControl, object control, ViewFactory factory, BuildContext context, PropertyMap pm)
        {
            return base.tryParseComplexValue(typeOfControl, control, factory, context, pm);
        }

        protected virtual void setPropertyIfHasControl(Type typeOfDlg, object dialog, XElement node, ViewFactory factory, string name)
        {
            var controlName = (string)getAttributeValue(node, typeof(string), name);

            if (string.IsNullOrEmpty(controlName) == false)
            {
                var control = factory.GetControl(controlName);

                if (control != null) getProperty(typeOfDlg, name).SetValue(dialog, control, null);                
            }
        }
    }
}
