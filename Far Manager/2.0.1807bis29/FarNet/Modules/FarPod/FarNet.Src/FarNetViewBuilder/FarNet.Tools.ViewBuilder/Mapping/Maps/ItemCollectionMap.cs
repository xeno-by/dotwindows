using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Maps
{
  //<Collection
  //  Items="Value"
  //  TypeOfItem="Value"
  //  Orientation="Value"
  //  StartPlace="Value"       
  //  />

    class ItemCollectionMap : BaseMap
    {
        protected override void init()
        {
            base.init();

            addMap("Items", PropertyMap.EKind.Value);
            addMap("TypeOfItem", PropertyMap.EKind.Value);
            addMap("Orientation", PropertyMap.EKind.Value);
            addMap("StartPlace", PropertyMap.EKind.Value);
        }
    }
}
