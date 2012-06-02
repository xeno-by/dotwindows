namespace FarPod.Operations.Bases
{
    using System;
    using FarPod.DataTypes;
    using FarPod.Dialogs;
    using FarPod.Helpers;
    using FarPod.Resources;
    using FarPod.Services;
    using SharePodLib;

    /// <summary>
    /// Copy file base operation
    /// </summary>
    abstract class CopyContentOperation : OnDeviceOperation
    {
        protected bool isMoveOperation;

        protected EExistCollisionStrategy collisionBehavior = EExistCollisionStrategy.None;
        protected bool skipAllCopierErrors;

        protected long totalByteToCopy;
        protected long alreadyByteCopy;

        protected CopyContentOperation(IPod dev) : base(dev)
        {

        }        

        protected virtual bool ifTargetExist()
        {
            return !processExist(getBehavior);
        }

        private EExistCollisionStrategy getBehavior()
        {
            EExistCollisionStrategy currentBehavior = collisionBehavior;

            if (currentBehavior == EExistCollisionStrategy.None)
            {
                // ask сollisionBehavior for file                

                CollisionBehaviorDialog cd = getCollisionBehaviorDialog();

                pauseTimer();

                cd.Show();

                resumeTimer();

                currentBehavior = cd.CollisionBehavior;

                if (cd.RememberChoice == 1)
                    collisionBehavior = currentBehavior;
            }

            return currentBehavior;
        }

        protected abstract CollisionBehaviorDialog getCollisionBehaviorDialog();

        protected abstract bool processExist(Func<EExistCollisionStrategy> getBehavior);        

        protected virtual string getStatusMessage()
        {
            if (stopWatch.Elapsed.TotalSeconds == 0)
                return string.Empty;

            double avgSpeed = (alreadyByteCopy / stopWatch.Elapsed.TotalSeconds);

            return string.Format(
                "\r{0} of {1}" +
                "\rElapsed time: {2}, speed: {3}/s",
                DisplayFormat.GetFileSizeString(alreadyByteCopy),
                DisplayFormat.GetFileSizeString(totalByteToCopy),
                DisplayFormat.GetTimeString(stopWatch.Elapsed.TotalSeconds),
                DisplayFormat.GetFileSizeString(avgSpeed));
        }

        protected virtual bool breakOnCopierError(FileCopierService cf, Exception e)
        {
            bool isRetry = false;

            if (!skipAllCopierErrors)
            {
                pauseTimer();

                var ed = new FileCopierErrorDialog(cf, e);

                if (ed.Show())
                {
                    skipAllCopierErrors = ed.RememberChoice == 1;

                    isRetry = ed.ClickedButtonNumber == 0;
                }
                else
                {
                    IsCanceled = true;

                    isRetry = false;
                }

                resumeTimer();
            }

            return !isRetry;
        }

        protected virtual bool copyFile(string sourcePath, string destinationPath)
        {
            bool isBreak = false;
            bool isDone = false;
            bool isHasError = false;

            FileCopierService cf = null;

            while (!isBreak && !isDone && canExecute())
            {
                isHasError = false;

                try
                {
                    cf = new FileCopierService(sourcePath, destinationPath);

                    cf.Start();

                    while (cf.CopyBlock() && canExecute())
                    {
                        alreadyByteCopy += cf.LastBlockTransferBytes;

                        setText(MsgStr.MsgCopyOrMoveToPath + getStatusMessage());
                    }

                    isDone = true;
                }
                catch (Exception e)
                {
                    isHasError = true;
                    isBreak = breakOnCopierError(cf, e);                    
                }
                finally
                {
                    if (cf != null) cf.Finish(IsCanceled || isHasError);
                }
            }

            return isDone;
        }
    }
}
