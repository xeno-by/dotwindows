namespace FarPod.Dialogs
{
    using FarPod.DataTypes;
    using FarPod.Dialogs.Bases;
    using FarPod.Resources;

    /// <summary>
    /// Collision behavior dialog impl
    /// </summary>
    class CollisionBehaviorDialog : BaseActionDialog
    {
        public static CollisionBehaviorDialog ForFile(string objInfo)
        {
            return new CollisionBehaviorDialog(false, MsgStr.MsgFileExist, objInfo, MsgStr.FarPod, 
                new [] { MsgStr.BtnReplace, MsgStr.BtnSkip, MsgStr.BtnRename, MsgStr.BtnCancel });
        }

        public static CollisionBehaviorDialog ForTrack(string objInfo)
        {
            return new CollisionBehaviorDialog(true, MsgStr.MsgTrackExist, objInfo, MsgStr.FarPod,
                new [] { MsgStr.BtnReplace, MsgStr.BtnSkip, MsgStr.BtnAddExistToPlayList, MsgStr.BtnCancel });
        }        

        private readonly bool _isForTrack;

        public EExistCollisionStrategy CollisionBehavior
        {
            get;
            private set;
        }

        public string ObjectName
        {
            get;
            protected set;
        }

        public int RememberChoice
        {
            get;
            set;
        }

        private CollisionBehaviorDialog(bool isForTrack, string text, string objName, string title, string[] buttons)
            : base(title, text, buttons)
        {
            _isForTrack = isForTrack;

            ObjectName = truncateText(objName, TEXT_WIDTH);

            CollisionBehavior = EExistCollisionStrategy.Break;            
        }

        public override bool Show()
        {
            bool dlgResult = base.Show();

            if (dlgResult)
            {                
                switch (ClickedButtonNumber)
                {
                    case 0:
                        CollisionBehavior = EExistCollisionStrategy.Replace;
                        break;
                    case 1:
                        CollisionBehavior = EExistCollisionStrategy.Skip;
                        break;
                    case 2:
                        CollisionBehavior = _isForTrack ? EExistCollisionStrategy.AddExistToPlayList : EExistCollisionStrategy.Rename;
                        break;
                    case 3:
                        CollisionBehavior = EExistCollisionStrategy.Break;
                        break;
                }
            }

            return dlgResult;
        }
    }
}
