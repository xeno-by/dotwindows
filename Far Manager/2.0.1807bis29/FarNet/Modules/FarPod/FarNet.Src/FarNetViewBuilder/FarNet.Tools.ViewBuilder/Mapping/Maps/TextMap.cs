using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<Text
    //  BoxColor="Value"
    //  Centered="Value"
    //  CenterGroup="Value"
    //  Separator="Value"
    //  ShowAmpersand="Value"
    //  Vertical="Value"
    //  />

    class TextMap : ControlMap
    {
        protected override void init()
        {
            base.init();

            addMap("BoxColor", PropertyMap.EKind.Value);
            addMap("Centered", PropertyMap.EKind.Value);
            addMap("CenterGroup", PropertyMap.EKind.Value);
            addMap("Separator", PropertyMap.EKind.Value);
            addMap("ShowAmpersand", PropertyMap.EKind.Value);

            //addMap("Vertical", PropertyMap.EKind.Value);
        }
    }
}
