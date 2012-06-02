using System;
using FarNet.Forms;

namespace FarNet.Tools.ViewBuilder.Wrappers
{
    class FarControlColorWrapper : IDisposable
    {
        // Summary:
        //     FarNet.Forms.IControl.Coloring event arguments.
        //
        // Remarks:
        //     Event handlers change the default colors provided by the event arguments.
        //     There are up to 4 color pairs (foreground and background).
        //     FarNet.Forms.IBox: 1: Title; 2: HiText; 3: Frame.
        //     FarNet.Forms.IText: 
        //        Normal text: 1: Title; 2: HiText; 3: Frame.  
        //        Vertical text: 1: Title.  The box color applies only to text items with the FarNet.Forms.IText.Separator
        //     flag set.
        //     FarNet.Forms.IEdit, FarNet.Forms.IComboBox: 1: EditLine; 2: Selected Text;
        //     3: Unchanged Color; 4: History and ComboBox pointer.
        //     FarNet.Forms.IButton, FarNet.Forms.ICheckBox, FarNet.Forms.IRadioButton:
        //     1: Title; 2: HiText.
        //     FarNet.Forms.IListBox recieves another event which is not yet exposed by
        //     FarNet.

        public ConsoleColor? Background1 { get; set; }        
        public ConsoleColor? Background2 { get; set; }        
        public ConsoleColor? Background3 { get; set; }        
        public ConsoleColor? Background4 { get; set; }
        
        public ConsoleColor? Foreground1 { get; set; }        
        public ConsoleColor? Foreground2 { get; set; }        
        public ConsoleColor? Foreground3 { get; set; }        
        public ConsoleColor? Foreground4 { get; set; }

        private IControl _realControl;

        public IControl RealControl
        {
            get
            {
                return _realControl;
            }
        }

        public FarControlColorWrapper(IControl control)
        {
            _realControl = control;

            _realControl.Coloring += coloring;
        }               

        public void Dispose()
        {
            if (_realControl == null) return;

            _realControl.Coloring -= coloring;

            _realControl = null;
        }

        private void coloring(object sender, ColoringEventArgs e)
        {
            if (Background1.HasValue) e.Background1 = Background1.Value;
            if (Background2.HasValue) e.Background2 = Background2.Value;
            if (Background3.HasValue) e.Background3 = Background3.Value;
            if (Background4.HasValue) e.Background4 = Background4.Value;

            if (Foreground1.HasValue) e.Foreground1 = Foreground1.Value;
            if (Foreground2.HasValue) e.Foreground2 = Foreground2.Value;
            if (Foreground3.HasValue) e.Foreground3 = Foreground3.Value;
            if (Foreground4.HasValue) e.Foreground4 = Foreground4.Value;

            _realControl.Coloring -= coloring;
        }
    }
}
