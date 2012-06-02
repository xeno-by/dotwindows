using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<FarItem
    //  Checked="Value"
    //  Disabled="Value"
    //  Grayed="Value"
    //  Hidden="Value"
    //  IsSeparator="Value"
    //  Text="Value"
    
    //  Click="Event">

    //  <!-- Data -->
    
    //</FarItem>

    class FarItemMap : BaseMap
    {
        protected override void init()
        {
            base.init();

            addMap("Checked", PropertyMap.EKind.Value);
            addMap("Disabled", PropertyMap.EKind.Value);
            addMap("Grayed", PropertyMap.EKind.Value);
            addMap("Hidden", PropertyMap.EKind.Value);
            addMap("IsSeparator", PropertyMap.EKind.Value);
            addMap("Text", PropertyMap.EKind.Value);

            addMap("Click", PropertyMap.EKind.Event);
        }
    }
}
