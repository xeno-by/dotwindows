using System;
using System.ComponentModel;
using FarNet.Forms;

namespace FarNet.Tools.ViewBuilder.Wrappers
{
    class FarControlNotifyWrapper : INotifyPropertyChanged, IDisposable
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private readonly IControl _realControl;

        public IControl RealControl
        {
            get
            {
                return _realControl;
            }
        }

        public FarControlNotifyWrapper(IControl control)
        {
            _realControl = control;

            if (_realControl is IEditable)
            {
                (_realControl as IEditable).TextChanged += controlNotifyWrapper_TextChanged;
            }
            else if (_realControl is ICheckBox)
            {
                (_realControl as ICheckBox).ButtonClicked += controlNotifyWrapper_ButtonClicked;
            }
            else if (_realControl is IRadioButton)
            {
                (_realControl as IRadioButton).ButtonClicked += controlNotifyWrapper_ButtonClicked;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_realControl == null)
                return;

            if (_realControl is IEditable)
            {
                (_realControl as IEditable).TextChanged -= controlNotifyWrapper_TextChanged;
            }
            else if (_realControl is ICheckBox)
            {
                (_realControl as ICheckBox).ButtonClicked -= controlNotifyWrapper_ButtonClicked;
            }
            else if (_realControl is IRadioButton)
            {
                (_realControl as IRadioButton).ButtonClicked -= controlNotifyWrapper_ButtonClicked;
            }
        }

        #endregion

        private void controlNotifyWrapper_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            firePropertyChanged(_realControl, "Selected");
        }

        private void controlNotifyWrapper_TextChanged(object sender, TextChangedEventArgs e)
        {
            firePropertyChanged(_realControl, "Text");
        }

        private void firePropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }
    }
}
