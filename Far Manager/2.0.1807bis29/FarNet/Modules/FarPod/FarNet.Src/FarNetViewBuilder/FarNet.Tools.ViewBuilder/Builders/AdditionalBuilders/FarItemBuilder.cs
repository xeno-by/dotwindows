using System;
using FarNet.Tools.ViewBuilder.Common;

namespace FarNet.Tools.ViewBuilder.Builders.AdditionalBuilders
{
    public class FarItemBuilder : BaseBuilder
    {
        public override Type TypeOfResult
        {
            get { return typeof(SetItem); }
        }

        protected override object create(ViewFactory factory, BuildContext context)
        {
            return new SetItem();
        }

        public override object Assembly(ViewFactory factory, BuildContext context)
        {
            var item = (SetItem)base.Assembly(factory, context);

            // save node inner value as FarItem.Data or FarItem.Text
            if (string.IsNullOrEmpty(item.Text))
                item.Text = context.CurrentNode.Value;
            else
                item.Data = context.CurrentNode.Value;

            return item;
        }
    }
}
