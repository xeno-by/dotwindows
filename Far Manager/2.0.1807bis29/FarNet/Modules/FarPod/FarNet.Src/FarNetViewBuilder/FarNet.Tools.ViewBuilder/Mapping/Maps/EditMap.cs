using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<Edit
    //  History="Value"
    //  IsPassword="Value"
    //  Fixed="Value"
    //  IsPath="Value"
    //  ManualAddHistory="Value"
    //  Mask="Value"
    //  NoAutoComplete="Value"
    //  NoFocus="Value"
    //  UseLastHistory="Value"
    //  />

    class EditMap : ControlMap
    {
        // include EditableMap

        protected override void init()
        {
            base.init();

            addMap("History", PropertyMap.EKind.Value);            
            addMap("IsPath", PropertyMap.EKind.Value);
            addMap("ManualAddHistory", PropertyMap.EKind.Value);
            addMap("Mask", PropertyMap.EKind.Value);
            addMap("NoAutoComplete", PropertyMap.EKind.Value);
            addMap("NoFocus", PropertyMap.EKind.Value);
            addMap("UseLastHistory", PropertyMap.EKind.Value);

            //addMap("IsPassword", PropertyMap.EKind.Value);
            //addMap("Fixed", PropertyMap.EKind.Value);

            include<EditableMap>();
        }
    }
}
