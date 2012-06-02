using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using FarNet.Forms;
using FarNet.Tools.ViewBuilder.Binding.Enums;

namespace FarNet.Tools.ViewBuilder.Binding.CustomExpressions
{
    public class CollectionBindingExpression : BindingExpression
    {
        class CollectionPropertyChangedEventArgs : PropertyChangedEventArgs
        {
            public NotifyCollectionChangedEventArgs NotifyCollectionChanged { get; private set; }

            public CollectionPropertyChangedEventArgs(string propertyName, NotifyCollectionChangedEventArgs e)
                : base(propertyName)
            {
                NotifyCollectionChanged = e;
            }
        }

        class PropertyChangedProxy : INotifyPropertyChanged
        {
            readonly INotifyCollectionChanged _proxyObject;
            readonly string _fakePropertyName;

            public PropertyChangedProxy(string fakePropertyName, INotifyCollectionChanged obj)
            {
                _fakePropertyName = fakePropertyName;

                _proxyObject = obj;
                _proxyObject.CollectionChanged += CollectionChanged;                
            }

            void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                var propertyChanged = PropertyChanged;

                if (propertyChanged != null)
                {
                    propertyChanged(this, new CollectionPropertyChangedEventArgs(_fakePropertyName, e));
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;            

            #endregion
        }

        public CollectionBindingExpression(EBingingMode mode, object source, PropertyInfo sourceProperty, object target, PropertyInfo targetProperty)
            : base(mode, source, sourceProperty, target, targetProperty)
        {
            
        }

        public override void UpdateSource(EventArgs e)
        {
            // ничего не делать. коллекция на контроле не может менять 
        }

        public override void UpdateTarget(EventArgs e)
        {
            NotifyCollectionChangedEventArgs actualEventArg = null;

            if (e is CollectionPropertyChangedEventArgs)
            {
                actualEventArg = ((CollectionPropertyChangedEventArgs)e).NotifyCollectionChanged;
            }
            else
            {
                actualEventArg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            }

            switch (actualEventArg.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (FarItem i in actualEventArg.NewItems) targetCollection.Add(i);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (FarItem i in actualEventArg.OldItems) targetCollection.Remove(i);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        // что то делаем
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    {
                        // что то делаем
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        listControl.DetachItems();

                        targetCollection.Clear();

                        foreach (FarItem i in sourceCollection) targetCollection.Add(i);

                        listControl.AttachItems();
                    }
                    break;
            }
        }

        private IList<FarItem> targetCollection
        {
            get
            {
                // нужно ли уже здесь применять Converter ???

                return (IList<FarItem>)getTargetValue();
            }
        }

        private ObservableCollection<FarItem> sourceCollection
        {
            get
            {
                // нужно ли уже здесь применять Converter ???

                return (ObservableCollection<FarItem>)getSourceValue();
            }
        }

        private IBaseList listControl
        {
            get
            {
                return (IBaseList)Target;
            }
        }

        public override INotifyPropertyChanged GetSourceNotifyProvider()
        {
            return new PropertyChangedProxy(SourceProperty.Name, sourceCollection);
        }

        public override INotifyPropertyChanged GetTargetNotifyProvider()
        {
            return null;
        }
    }
}
