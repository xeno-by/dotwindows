using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
    //<Editable
    //  ExpandEnvironmentVariables="Value"
    //  IsTouched="Value"
    //  ReadOnly="Value"
    //  SelectOnEntry="Value"
    
    //  TextChanged="Event"
    //  />

    class EditableMap : BaseMap
    {
        protected override void init()
        {
            base.init();

            addMap("ExpandEnvironmentVariables", PropertyMap.EKind.Value);
            addMap("IsTouched", PropertyMap.EKind.Value);
            addMap("ReadOnly", PropertyMap.EKind.Value);
            addMap("SelectOnEntry", PropertyMap.EKind.Value);

            addMap("TextChanged", PropertyMap.EKind.Event);
        }
    }
}
