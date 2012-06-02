using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarNet.Forms;

namespace FarPod.Dialogs.Bases
{
    abstract class ActionDialog : BaseView
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

        public IEdit EditControl
        {
            get;
            set;
        }

        protected readonly int TEXT_WIDTH;

        public ActionDialog(string title)
        {
            Title = title;
            TEXT_WIDTH = 60;
        }

        public ActionDialog(string title, string text, string[] buttons)
            : this(title)
        {
            Text = text;
            Buttons = buttons;
        }

        protected override void initAndCreate()
        {
            base.initAndCreate();

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
