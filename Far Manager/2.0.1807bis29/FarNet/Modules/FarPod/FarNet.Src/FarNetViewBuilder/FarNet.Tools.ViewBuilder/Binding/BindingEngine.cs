using System;
using System.Collections.Generic;
using System.ComponentModel;
using FarNet.Tools.ViewBuilder.Binding.Enums;

namespace FarNet.Tools.ViewBuilder.Binding
{
    public class BindingEngine : IDisposable
    {
        public List<BindingExpression> Expressions
        {
            get; private set;
        }        

        private readonly List<INotifyPropertyChanged> _subscribeSourceList;
        private readonly List<INotifyPropertyChanged> _subscribeTargetList;

        public BindingEngine()
        {
            Expressions = new List<BindingExpression>();

            _subscribeSourceList = new List<INotifyPropertyChanged>();
            _subscribeTargetList = new List<INotifyPropertyChanged>();
        }

        public void Prepare()
        {
            if (Expressions.Count == 0) return;            

            foreach (BindingExpression be in Expressions)
            {
                be.UpdateTarget(EventArgs.Empty);

                if (be.Mode != EBingingMode.OneTime)
                {
                    INotifyPropertyChanged s = be.GetSourceNotifyProvider();

                    if (s != null && !_subscribeSourceList.Contains(s))
                    {
                        s.PropertyChanged += SourcePropertyChanged;

                        _subscribeSourceList.Add(s);
                    }
                }

                if (be.Mode == EBingingMode.TwoWay)
                {
                    INotifyPropertyChanged t = be.GetTargetNotifyProvider();

                    if (t != null && !_subscribeTargetList.Contains(t))
                    {
                        t.PropertyChanged += TargetPropertyChanged;

                        _subscribeTargetList.Add(t);
                    }
                }
            }
        }

        internal void UpdateSource(EBingingMode mode)
        {
            if (Expressions.Count == 0) return;

            foreach (BindingExpression be in Expressions)
            {
                if ((mode & be.Mode) == be.Mode) be.UpdateSource(EventArgs.Empty);
            }
        }

        internal void UpdateTarget(EBingingMode mode)
        {
            if (Expressions.Count == 0) return;

            foreach (BindingExpression be in Expressions)
            {
                if ((mode & be.Mode) == be.Mode) be.UpdateTarget(EventArgs.Empty);
            }
        }

        private BindingExpression _initialUpdateExpr;

        private void TargetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (BindingExpression be in Expressions)
            {
                if (be.Target == sender && compareProperty(e.PropertyName, be.TargetProperty.Name))
                {
                    if (_initialUpdateExpr == null)
                    {
                        _initialUpdateExpr = be;
                        be.UpdateSource(e);
                        _initialUpdateExpr = null;
                    }
                    else if (_initialUpdateExpr != be)
                    {
                        be.UpdateSource(e);
                    }
                }
            }
        }

        private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (BindingExpression be in Expressions)
            {
                if (be.Source == sender && compareProperty(e.PropertyName, be.SourceProperty.Name))
                {
                    if (_initialUpdateExpr == null)
                    {
                        _initialUpdateExpr = be;
                        be.UpdateTarget(e);
                        _initialUpdateExpr = null;
                    }
                    else if (_initialUpdateExpr != be)
                    {
                        be.UpdateTarget(e);
                    }
                }
            }
        }

        private bool compareProperty(string propertyMask, string property)
        {
            return Far.Net.MatchPattern(property, propertyMask);
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (INotifyPropertyChanged obj in _subscribeSourceList)
            {                
                obj.PropertyChanged -= SourcePropertyChanged;
            }

            foreach (INotifyPropertyChanged obj in _subscribeTargetList)
            {                
                obj.PropertyChanged -= SourcePropertyChanged;
            }

            Expressions.Clear();
        }

        #endregion
    }
}
