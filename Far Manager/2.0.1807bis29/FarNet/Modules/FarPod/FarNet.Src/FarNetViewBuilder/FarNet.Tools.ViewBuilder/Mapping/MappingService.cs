using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using FarNet.Tools.ViewBuilder.Builders.AdditionalBuilders;
using FarNet.Tools.ViewBuilder.Builders.ContainerBuilders;
using FarNet.Tools.ViewBuilder.Common;
using FarNet.Tools.ViewBuilder.Mapping.Bases;
using System;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Mapping.Maps;

namespace FarNet.Tools.ViewBuilder.Mapping
{
    public class MappingService
    {
        private readonly static Dictionary<Type, BaseMap> _mappingHash;

        static MappingService()
        {
            _mappingHash = new Dictionary<Type, BaseMap>();

            // add mappings
            // main dialog 
            _mappingHash.Add(typeof(IDialog), new DialogMap());

            // standard controls            
            _mappingHash.Add(typeof(IButton), new ButtonMap());
            _mappingHash.Add(typeof(ICheckBox), new CheckBoxMap());
            _mappingHash.Add(typeof(IRadioButton), new RadioButtonMap());
            _mappingHash.Add(typeof(IText), new TextMap());
            _mappingHash.Add(typeof(IEdit), new EditMap());

            // list controls
            _mappingHash.Add(typeof(IListBox), new ListBoxMap());
            _mappingHash.Add(typeof(IComboBox), new ComboBoxMap());

            // conteiner controls | wrappers | groups
            _mappingHash.Add(typeof(IBox), new BoxMap());

            // create builder for 
            // IUserControl            

            // custom, non control builders
            _mappingHash.Add(typeof(SetItem), new FarItemMap());
            _mappingHash.Add(typeof(CollectionBuilder.ItemCollection), new ItemCollectionMap());
        }

        public static BaseMap Get(Type forType)
        {
            return _mappingHash[forType];
        }

        public static void Set(Type forType, BaseMap map)
        {
            _mappingHash[forType] = map;
        }
    }
}
