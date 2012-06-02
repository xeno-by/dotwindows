using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<BaseList
    //  AutoAssignHotkeys="Value"
    //  NoAmpersands="Value"
    //  NoClose="Value"
    //  NoFocus="Value"
    //  Selected="Value"
    //  SelectLast="Value"
    //  WrapCursor="Value"
        
    //  Items="Value"
    //  SelectedItem="Value"
    //  />

    class BaseListMap : ControlMap
    {
        protected override void init()
        {
            base.init();

            addMap("AutoAssignHotkeys", PropertyMap.EKind.Value);
            addMap("NoAmpersands", PropertyMap.EKind.Value);
            addMap("NoClose", PropertyMap.EKind.Value);
            addMap("NoFocus", PropertyMap.EKind.Value);
            addMap("Selected", PropertyMap.EKind.Value);
            addMap("SelectLast", PropertyMap.EKind.Value);
            addMap("WrapCursor", PropertyMap.EKind.Value);

            addMap("Items", PropertyMap.EKind.Value);
            addMap("SelectedItem", PropertyMap.EKind.Value);
        }
    }
}
