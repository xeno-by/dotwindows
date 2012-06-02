namespace FarPod.Dialogs
{
    using FarNet.Forms;

    /// <summary>
    /// Double line input dialog impl
    /// </summary>
    class DoubleInputDialog : InputDialog
    {
        public string Text2
        {
            get;
            protected set;
        }

        public IEdit EditControl2
        {
            get;
            set;
        }

        public string InputText2
        {
            get;
            set;
        }

        public string HistoryKey2
        {
            get;
            set;
        }

        public DoubleInputDialog(string text, string text2, string title, string[] buttons, string defValue = "", string defValue2 = "")
            : base(text, title, buttons, defValue)
        {
            Text2 = text2;
            InputText2 = defValue2;
        }
    }
}
