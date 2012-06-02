namespace FarPod.Dialogs
{
    using System;
    using System.Collections.Generic;
    using FarPod.Dialogs.Bases;
    using FarPod.Resources;

    /// <summary>
    /// Operation error dialog impl
    /// </summary>
    class OperationErrorDialog : BaseActionDialog
    {                
        private readonly Exception _error;

        public List<string> MessageLines
        {
            get;
            protected set;
        }

        public int RememberChoice
        {
            get;
            set;
        }

        public string AdditionalObjectInfo
        {
            get;
            set;
        }

        public OperationErrorDialog(Exception error)
            : this(error, false)
        {            
            
        }

        public OperationErrorDialog(Exception error, bool canSkip)
            : base(MsgStr.FarPod)
        {
            _error = error;

            RememberChoice = 0;

            if (canSkip)
            {
                Buttons = new [] { MsgStr.BtnRetry, MsgStr.BtnSkip, MsgStr.BtnSkipAll, MsgStr.BtnCancel };
            }
            else
            {
                Buttons = new [] { MsgStr.BtnRetry, MsgStr.BtnCancel };
            }
        }

        protected override void initView()
        {            
            string textMessage = truncateText(_error.Message, TEXT_WIDTH * 4);

            MessageLines = new List<string>();

            while (textMessage.Length > 0)
            {
                string line;

                if (textMessage.Length > TEXT_WIDTH)
                {
                    line = textMessage.Substring(0, textMessage.LastIndexOf(' ', TEXT_WIDTH, TEXT_WIDTH));

                    textMessage = textMessage.Remove(0, line.Length >= textMessage.Length ? textMessage.Length : line.Length);
                }
                else
                {
                    line = textMessage;

                    textMessage = string.Empty;
                }

                MessageLines.Add(line);
            }            

            base.initView();

            setSize(dialog.Rect.Width, dialog.Rect.Height + MessageLines.Count - 1);
        }

        public override bool Show()
        {
            bool dlgResult = base.Show();

            if (dlgResult)
            {
                RememberChoice = ClickedButtonNumber == 2 ? 1 : 0;
            }

            return dlgResult;
        }
    }
}
