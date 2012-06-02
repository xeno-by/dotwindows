namespace FarPod.Explorers
{
    using System;
    using FarNet;
    using FarPod.DataTypes;
    using FarPod.Extensions;
    using FarPod.Services;

    /// <summary>
    /// Base file-like explorer
    /// </summary>
    abstract class FileFarPodExplorerBase : FarPodExplorerBase
    {
        protected FileFarPodExplorerBase(Guid typeId)
            : base(typeId)
        {
            Functions =
                ExplorerFunctions.ExportFiles |
                ExplorerFunctions.ImportFiles |
                ExplorerFunctions.AcceptFiles |
                ExplorerFunctions.DeleteFiles;
        }

        public override void ExportFiles(ExportFilesEventArgs args)
        {
            FarPodOperationService s = get(args);

            if (!Far.Net.Panel2.RealNames && !s.IsInternalOperation())
            {
                args.Result = JobResult.Ignore;
                return;
            }

            OperationResult or = s.CopyOrMoveContentFromSelf(args.Files, args.DirectoryName, args.Move);

            args.Result = processResult(or, args.Mode.HasFlag(ExplorerModes.Silent), args.FilesToStay);
        }

        public override void ImportFiles(ImportFilesEventArgs args)
        {
            FarPodOperationService s = get(args);

            if (!Far.Net.Panel.RealNames)
            {
                args.Result = JobResult.Ignore;
                return;
            }

            OperationResult or = s.CopyOrMoveContentToSelf(args.Files, args.DirectoryName, args.Move);

            args.Result = processResult(or, args.Mode.HasFlag(ExplorerModes.Silent), args.FilesToStay);
        }

        public override void DeleteFiles(DeleteFilesEventArgs args)
        {
            OperationResult or = get(args).DeleteContent(args.Files, args.Force);

            args.Result = processResult(or, args.Mode.HasFlag(ExplorerModes.Silent), args.FilesToStay);
        }

        public override void AcceptFiles(AcceptFilesEventArgs args)
        {            
            if (args.Explorer as FarPodExplorerBase == null)
            {
                args.Result = JobResult.Ignore;
                return;
            }

            FarPodOperationService s = get(args, (FarPodExplorerBase)args.Explorer);

            OperationResult or = s.DirectCopyOrMoveToDevice(args.Files, this, args.Move);

            args.Result = processResult(or, args.Mode.HasFlag(ExplorerModes.Silent), args.FilesToStay);
        }
    }
}
