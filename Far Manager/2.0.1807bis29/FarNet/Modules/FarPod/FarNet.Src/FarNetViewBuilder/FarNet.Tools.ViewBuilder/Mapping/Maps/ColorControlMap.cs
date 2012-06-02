using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<ColorControl
    //  Background1="Value"
    //  Background2="Value"
    //  Background3="Value"
    //  Background4="Value"
    
    //  Foreground1="Value"
    //  Foreground2="Value"
    //  Foreground3="Value"
    //  Foreground4="Value"
    //  />

    class ColorControlMap : BaseMap
    {
        protected override void init()
        {
            base.init();

            addMap("Background1", PropertyMap.EKind.Value);
            addMap("Background2", PropertyMap.EKind.Value);
            addMap("Background3", PropertyMap.EKind.Value);
            addMap("Background4", PropertyMap.EKind.Value);

            addMap("Foreground1", PropertyMap.EKind.Value);
            addMap("Foreground2", PropertyMap.EKind.Value);
            addMap("Foreground2", PropertyMap.EKind.Value);
            addMap("Foreground4", PropertyMap.EKind.Value);
        }
    }
}
