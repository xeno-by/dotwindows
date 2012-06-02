using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<ListBox
    //  Bottom="Value"
    //  NoBox="Value"
    //  Title="Value">

    //  <!-- FarItems -->

    //</ListBox>

    class ListBoxMap : BaseListMap
    {
        protected override void init()
        {
            base.init();

            addMap("Bottom", PropertyMap.EKind.Value);
            addMap("NoBox", PropertyMap.EKind.Value);
            addMap("Title", PropertyMap.EKind.Value);
        }
    }
}
