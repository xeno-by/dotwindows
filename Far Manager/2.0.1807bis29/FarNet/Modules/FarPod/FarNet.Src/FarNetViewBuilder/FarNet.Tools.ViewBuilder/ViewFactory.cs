using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Binding;
using FarNet.Tools.ViewBuilder.Binding.Enums;
using FarNet.Tools.ViewBuilder.Builders;
using FarNet.Tools.ViewBuilder.Builders.AdditionalBuilders;
using FarNet.Tools.ViewBuilder.Builders.ContainerBuilders;
using FarNet.Tools.ViewBuilder.Builders.StandardControls;
using FarNet.Tools.ViewBuilder.Common;
using FarNet.Tools.ViewBuilder.Interfaces;
using FarNet.Tools.ViewBuilder.Mapping;
using FarNet.Tools.ViewBuilder.ValueConverters;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder
{
    public class ViewFactory : IDisposable
    {
        protected Dictionary<string, Type> _builderTypes;
        protected Dictionary<string, BaseBuilder> _builderList;
        protected Dictionary<string, IControl> _controlHash;

        protected Dictionary<string, IBindingValueConverter> _converterRegistry;
        
        protected BindingEngine _bindingEngine;
        protected IModuleManager _moduleManager;

        protected List<IDisposable> _garbageList;

        public ViewFactory()
        {            
            _controlHash = new Dictionary<string, IControl>();

            _builderList = new Dictionary<string, BaseBuilder>();
            _builderTypes = new Dictionary<string, Type>();

            initBuilders();

            _converterRegistry = new Dictionary<string, IBindingValueConverter>();

            initConverters();            

            _bindingEngine = new BindingEngine();
        }

        protected virtual void initConverters()
        {
            _converterRegistry.Add("BooleanNot", new BooleanNotConverter());
        }

        protected virtual void initBuilders()
        {
            // main dialog 
            _builderTypes.Add("Dialog", typeof(DialogBuilder));

            // standard controls            
            _builderTypes.Add("Button", typeof(CommonControlBuilder<IButton>));
            _builderTypes.Add("CheckBox", typeof(CommonControlBuilder<ICheckBox>));
            _builderTypes.Add("RadioButton", typeof(CommonControlBuilder<IRadioButton>));
            _builderTypes.Add("Text", typeof(CommonControlBuilder<IText>));
            _builderTypes.Add("Edit", typeof(CommonControlBuilder<IEdit>));

            // list controls
            _builderTypes.Add("ListBox", typeof(ListControlBuilder<IListBox>));
            _builderTypes.Add("ComboBox", typeof(ListControlBuilder<IComboBox>));

            // conteiner controls | wrappers | groups
            _builderTypes.Add("Box", typeof(BoxControlBuilder));

            // create builder for 
            // IUserControl            

            // custom, non control builders
            _builderTypes.Add("FarItem", typeof(FarItemBuilder));
            _builderTypes.Add("Collection", typeof(CollectionBuilder));
        }

        public virtual IDialog Create(object view, IModuleManager mm, XElement rootNode)
        {
            _moduleManager = mm;

            _controlHash.Clear();

            var context = new BuildContext();

            context.RootNode = rootNode;
            context.View = view;
            context.TypeOfView = view.GetType();            
            context.CurrentNode = context.RootNode;

            var resultDlg = (IDialog)GetBuilder("Dialog").Assembly(this, context);

            resultDlg.Initialized += onDialogInitialized;
            resultDlg.Closing += onDialogClosing;

            _bindingEngine.Prepare();

            _garbageList = new List<IDisposable>();

            _garbageList.Add(_bindingEngine);

            return resultDlg;
        }

        private void onDialogClosing(object sender, ClosingEventArgs e)
        {
            _bindingEngine.UpdateSource(EBingingMode.TwoWayOnClose);

            ((IDialog)sender).Closing -= onDialogClosing;
        }

        private void onDialogInitialized(object sender, InitializedEventArgs e)
        {
            _bindingEngine.UpdateSource(EBingingMode.AllTwoWay);

            ((IDialog)sender).Initialized -= onDialogInitialized;
        }

        public void UpdateSource(EBingingMode mode)
        {
            _bindingEngine.UpdateSource(mode);
        }

        public void UpdateTarget(EBingingMode mode)
        {
            _bindingEngine.UpdateTarget(mode);
        } 

        public virtual void AddBuilderType(string name, Type actualType)
        {
            if (_builderTypes.ContainsKey(name))
                _builderTypes[name] = actualType;
            else
                _builderTypes.Add(name, actualType);
        }

        public virtual BaseBuilder GetBuilder(string type)
        {
            if (_builderList.ContainsKey(type)) return _builderList[type];

            if (_builderTypes.ContainsKey(type))
            {
                _builderList.Add(type, createBuilder(_builderTypes[type]));

                return _builderList[type];
            }

            return null;
        }

        public virtual void AddControl(string name, IControl ctrl)
        {
            if (_controlHash.ContainsKey(name))
                _controlHash[name] = ctrl;
            else
                _controlHash.Add(name, ctrl);
        }

        public virtual IControl GetControl(string name)
        {
            if (_controlHash.ContainsKey(name))
                return _controlHash[name];
            else
                return null;
        }

        public virtual BaseMap GetMapping(Type forType)
        {
            return MappingService.Get(forType);
        }

        public virtual void CreateBinding(BindingExpression be)
        {
            _bindingEngine.Expressions.Add(be);
        }

        public virtual void AddBindingConverter(string name, IBindingValueConverter converter)
        {
            if (_converterRegistry.ContainsKey(name))
                _converterRegistry[name] = converter;
            else
                _converterRegistry.Add(name, converter);
        }

        public virtual IBindingValueConverter GetBindingConverter(string name)
        {
            if (_converterRegistry.ContainsKey(name))
                return _converterRegistry[name];
            else
                return null;
        }

        public virtual string GetLocalString(string key)
        {
            return _moduleManager.GetString(key);
        }

        protected virtual BaseBuilder createBuilder(Type typeOfBuilder)
        {
            return (BaseBuilder)Activator.CreateInstance(typeOfBuilder);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_garbageList != null)
            {
                while (_garbageList.Count > 0)
                {
                    IDisposable item = _garbageList[0];

                    _garbageList.Remove(item);

                    if (item != null)
                    {
                        item.Dispose();
                    }
                }
            }            
        }

        #endregion        
    }
}
