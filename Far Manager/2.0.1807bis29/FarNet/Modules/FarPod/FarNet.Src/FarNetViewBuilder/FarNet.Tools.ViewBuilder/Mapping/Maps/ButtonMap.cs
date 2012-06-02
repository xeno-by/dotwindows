using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<Button
    //  CenterGroup="Value"
    //  NoBrackets="Value"
    //  NoClose="Value"
    //  NoFocus="Value"
    //  ShowAmpersand="Value"
    
    //  ButtonClicked="Event"
    //  />

    class ButtonMap : ControlMap
    {
        protected override void init()
        {
            base.init();

            addMap("CenterGroup", PropertyMap.EKind.Value);
            addMap("NoBrackets", PropertyMap.EKind.Value);
            addMap("NoClose", PropertyMap.EKind.Value);
            addMap("NoFocus", PropertyMap.EKind.Value);
            addMap("ShowAmpersand", PropertyMap.EKind.Value);

            addMap("ButtonClicked", PropertyMap.EKind.Event);
        }
    }
}
