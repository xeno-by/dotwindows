using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<CheckBox
    //  CenterGroup="Value"
    //  NoFocus="Value"
    //  Selected="Value"
    //  ShowAmpersand="Value"
    //  ThreeState="Value"
    
    //  ButtonClicked="Event"
    //  />

    class CheckBoxMap : ControlMap
    {
        protected override void init()
        {
            base.init();

            addMap("CenterGroup", PropertyMap.EKind.Value);
            addMap("NoFocus", PropertyMap.EKind.Value);
            addMap("Selected", PropertyMap.EKind.Value);
            addMap("ShowAmpersand", PropertyMap.EKind.Value);
            addMap("ThreeState", PropertyMap.EKind.Value);

            addMap("ButtonClicked", PropertyMap.EKind.Event);
        }
    }
}
