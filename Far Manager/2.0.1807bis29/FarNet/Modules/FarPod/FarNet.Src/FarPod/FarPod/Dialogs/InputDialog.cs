namespace FarPod.Dialogs
{
    using FarNet.Forms;
    using FarPod.Dialogs.Bases;

    /// <summary>
    /// Simple input dialog impl
    /// </summary>
    class InputDialog : BaseActionDialog
    {       
        public string InputText
        {
            get;
            set;
        }

        public IEdit EditControl
        {
            get;
            set;
        }

        public string HistoryKey
        {
            get;
            set;
        }

        public InputDialog(string text, string title, string[] buttons, string defValue = "")
            : base(title, text, buttons)
        {
            InputText = defValue;
        }    
    }
}
