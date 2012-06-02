namespace FarPod.Dialogs
{
    using System;
    using FarPod.Resources;
    using FarPod.Services;

    /// <summary>
    /// FileCopier error dialog impl
    /// </summary>
    class FileCopierErrorDialog : OperationErrorDialog
    {
        private readonly FileCopierService _fileCopier;

        public string SourceText
        {
            get;
            protected set;
        }

        public string DestinationText
        {
            get;
            protected set;
        }

        public FileCopierErrorDialog(FileCopierService fc, Exception error)
            : base(error, true)
        {
            _fileCopier = fc;            
        }

        protected override void initView()
        {            
            SourceText = string.Format(MsgStr.MsgCopierSource,
                truncatePath(_fileCopier.SourcePath, TEXT_WIDTH - (MsgStr.MsgCopierSource.Length - 4)));

            DestinationText = string.Format(MsgStr.MsgCopierDestination,
                truncatePath(_fileCopier.DestinationPath, TEXT_WIDTH - (MsgStr.MsgCopierDestination.Length - 4)));

            base.initView();
        }
    }
}
