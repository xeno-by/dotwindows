using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<Control
    //  Name="Value"
    //  Disabled="Value"
    //  Hidden="Value"
    //  Text="Value"
    //  Rect="Value"

    //  Drawing="Event"
    //  Coloring="Event"
    //  GotFocus="Event"
    //  LosingFocus="Event"
    //  MouseClicked="Event"
    //  KeyPressed="Event"
    //  />

    class ControlMap : BaseMap
    {
        public override bool HasName
        {
            get
            {
                return true;
            }
        }

        protected override void init()
        {
            base.init();

            addMap("Disabled", PropertyMap.EKind.Value);
            addMap("Hidden", PropertyMap.EKind.Value);
            addMap("Text", PropertyMap.EKind.Value);
            addMap("Name", PropertyMap.EKind.Value);

            addMap("Drawing", PropertyMap.EKind.Event);
            addMap("Coloring", PropertyMap.EKind.Event);
            addMap("GotFocus", PropertyMap.EKind.Event);
            addMap("LosingFocus", PropertyMap.EKind.Event);
            addMap("MouseClicked", PropertyMap.EKind.Event);
            addMap("KeyPressed", PropertyMap.EKind.Event);

            //addMap("Rect", PropertyMap.EKind.Value);
        }
    }
}
