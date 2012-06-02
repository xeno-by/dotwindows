namespace FarPod.Dialogs.Bases
{
    using System.Linq;
    using FarNet.Forms;

    /// <summary>
    /// Define base action dialog
    /// </summary>
    abstract class BaseActionDialog : BaseView
    {
        public int ClickedButtonNumber
        {
            get;
            protected set;
        }

        public string Title
        {
            get;
            protected set;
        }

        public string Text
        {
            get;
            protected set;
        }

        public string[] Buttons
        {
            get;
            protected set;
        }

        protected readonly int TEXT_WIDTH;

        protected BaseActionDialog(string title)
        {
            Title = title;
            TEXT_WIDTH = 60;
        }

        protected BaseActionDialog(string title, string text, string[] buttons)
            : this(title)
        {
            Text = text;
            Buttons = buttons;
        }

        protected override void initView()
        {
            base.initView();

            dialog.Cancel = getControls<IButton>().FirstOrDefault(p => p.Text == Buttons[Buttons.Length - 1]);
        }

        public override bool Show()
        {
            ClickedButtonNumber = -1;

            bool dlgResult = base.Show();

            if (ClickedButtonNumber == -1)
                ClickedButtonNumber = dlgResult ? 0 : Buttons.Length - 1;

            return dlgResult;
        }

        protected void onButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            ClickedButtonNumber = (int)e.Control.Data;
        }

        protected string truncateText(string text, int length)
        {
            if (text.Length > length)
            {
                text = text.Substring(0, length - 3) + "...";
            }

            return text;
        }

        protected string truncatePath(string text, int length)
        {
            if (text.Length > length)
            {
                text = text.Substring(0, length - 3) + "...";
            }

            return text;
        }
    }
}
