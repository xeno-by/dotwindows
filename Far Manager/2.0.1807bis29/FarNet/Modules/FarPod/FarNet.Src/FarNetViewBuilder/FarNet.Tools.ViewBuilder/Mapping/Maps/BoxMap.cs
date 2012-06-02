using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<Box
    //  LeftText="Value"
    //  ShowAmpersand="Value"
    //  Single="Value">

    //  <!-- Controls -->

    //</Box>

    class BoxMap : ControlMap
    {
        protected override void init()
        {
            base.init();

            addMap("LeftText", PropertyMap.EKind.Value);
            addMap("ShowAmpersand", PropertyMap.EKind.Value);
            addMap("Single", PropertyMap.EKind.Value);
        }
    }
}
