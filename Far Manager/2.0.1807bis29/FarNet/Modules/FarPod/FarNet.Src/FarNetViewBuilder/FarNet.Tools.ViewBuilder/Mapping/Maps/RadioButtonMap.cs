using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<RadioButton
    //  CenterGroup="Value"
    //  Group="Value"
    //  MoveSelect="Value"
    //  NoFocus="Value"
    //  Selected="Value"
    //  ShowAmpersand="Value"
    
    //  ButtonClicked="Event"
    //  />

    class RadioButtonMap : ControlMap
    {
        protected override void init()
        {
            base.init();

            addMap("CenterGroup", PropertyMap.EKind.Value);
            addMap("Group", PropertyMap.EKind.Value);
            addMap("MoveSelect", PropertyMap.EKind.Value);
            addMap("NoFocus", PropertyMap.EKind.Value);
            addMap("Selected", PropertyMap.EKind.Value);
            addMap("ShowAmpersand", PropertyMap.EKind.Value);

            addMap("ButtonClicked", PropertyMap.EKind.Event);
        }
    }
}
