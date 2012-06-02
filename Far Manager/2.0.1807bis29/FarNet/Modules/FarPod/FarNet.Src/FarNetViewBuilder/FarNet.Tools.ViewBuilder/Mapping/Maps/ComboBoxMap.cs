using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
  //<ComboBox
  //  DropDownList="Value">

  //  <!-- FarItems -->

  //</ComboBox>

    class ComboBoxMap : BaseListMap
    {        
        // include EditableMap

        protected override void init()
        {
            base.init();

            addMap("DropDownList", PropertyMap.EKind.Value);

            include<EditableMap>();
        }
    }
}
