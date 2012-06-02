using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<Dialog
    //  Cancel="Control"
    //  Default="Control"
    //  Focused="Control"
    //  HelpTopic="Value"
    //  IsSmall="Value"
    //  IsWarning="Value"
    //  KeepWindowTitle="Value"
    //  NoPanel="Value"
    //  NoShadow="Value"
    //  NoSmartCoordinates="Value"
    //  TypeId="Value"
    //  Rect="Value"
  
    //  Initialized="Event"
    //  Closing="Event"
    //  Idled="Event"
    //  MouseClicked="Event"
    //  KeyPressed="Event"
    //  ConsoleSizeChanged="Event">

    //  <!-- Controls -->

    //</Dialog>

    class DialogMap : BaseMap
    {
        protected override void init()
        {
            base.init();

            addMap("Cancel", PropertyMap.EKind.Control);
            addMap("Default", PropertyMap.EKind.Control);
            addMap("Focused", PropertyMap.EKind.Control);

            addMap("HelpTopic", PropertyMap.EKind.Value);
            addMap("IsSmall", PropertyMap.EKind.Value);
            addMap("IsWarning", PropertyMap.EKind.Value);
            addMap("KeepWindowTitle", PropertyMap.EKind.Value);
            addMap("NoPanel", PropertyMap.EKind.Value);
            addMap("NoShadow", PropertyMap.EKind.Value);
            addMap("NoSmartCoordinates", PropertyMap.EKind.Value);
            addMap("TypeId", PropertyMap.EKind.Value);

            //addMap("Rect", PropertyMap.EKind.Value);

            addMap("Initialized", PropertyMap.EKind.Event);
            addMap("Closing", PropertyMap.EKind.Event);
            addMap("Idled", PropertyMap.EKind.Event);
            addMap("MouseClicked", PropertyMap.EKind.Event);
            addMap("KeyPressed", PropertyMap.EKind.Event);
            addMap("ConsoleSizeChanged", PropertyMap.EKind.Event);
        }
    }
}
