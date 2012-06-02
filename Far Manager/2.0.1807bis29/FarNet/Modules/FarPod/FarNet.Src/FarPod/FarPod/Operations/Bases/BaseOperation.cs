namespace FarPod.Operations.Bases
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using FarNet;
    using FarNet.Tools;
    using FarPod.Dialogs;
    using FarPod.Exceptions;
    using FarPod.Resources;

    /// <summary>
    /// Base operation super class
    /// </summary>
    abstract class BaseOperation
    {
        private int _percentage;

        private long _maximum = -1;
        private long _currentValue;

        private string _activity = string.Empty;

        private readonly List<Type> _skipErrorTypes = new List<Type>();

        protected ProgressBox progressBox;

        protected Stopwatch stopWatch;

        public bool IsCompleted
        {
            get;
            protected set;
        }

        public bool IsCanceled
        {
            get;
            protected set;
        }

        public virtual bool Execute()
        {
            try
            {
                prepare();

                IsCompleted = work();
            }
            catch (CancelOperationFarPodException e)
            {
                Far.Net.Message(e.ToString());
            }
            catch (Exception e)
            {
                throw new OperationFarPodException(GetType().Name, e);
            }
            finally
            {
                finish();
            }

            return IsCompleted;
        }

        private void updateProgressBox()
        {            
            if (progressBox != null)
            {
                progressBox.Title = MsgStr.FarPod;
                progressBox.Activity = _activity;
                progressBox.SetProgressValue(_currentValue, _maximum);
                progressBox.ShowProgress();
            }
        }

        protected virtual bool canExecute()
        {
            if (!IsCanceled && Far.Net.UI.ReadKeys(VKeyCode.Escape) == VKeyCode.Escape)
            {
                pauseTimer();

                IsCanceled = Far.Net.Message(MsgStr.MsgCancel, MsgStr.FarPod,
                    MsgOptions.Warning | MsgOptions.YesNo) == 0;

                resumeTimer();
            }

            return !IsCanceled;
        }

        protected void setProgress(long currentValue, long maximumValue = -1)
        {
            if (maximumValue > 0) _maximum = maximumValue;

            _currentValue = currentValue;

            if (_currentValue <= 0)
            {
                _percentage = 0;
            }
            else if (_maximum <= 0 || _currentValue >= _maximum)
            {
                _percentage = 100;
            }
            else
            {
                _percentage = (int)(_currentValue * 100 / _maximum);
            }

            updateProgressBox();
        }

        protected void setText(string message)
        {
            _activity = message;

            updateProgressBox();
        }

        protected virtual void prepare()
        {
            stopWatch = Stopwatch.StartNew();

            progressBox = new ProgressBox();
        }

        protected virtual void finish()
        {
            if (stopWatch != null)
            {
                stopWatch.Stop();
                stopWatch = null;
            }
            if (progressBox != null)
            {
                progressBox.Dispose();
                progressBox = null;
            }
        }

        protected void pauseTimer()
        {
            stopWatch.Stop();
        }

        protected void resumeTimer()
        {
            stopWatch.Start();
        }

        protected bool tryDo(System.Action task, bool canSkip = false, object obj = null)
        {            
            bool isBreak = false;
            bool isDone = false;

            Exception lastError = null;

            while (!isDone && !isBreak)
            {
                try
                {
                    task();

                    isDone = true;
                }
                catch (Exception e)
                {
                    lastError = e;

                    if (_skipErrorTypes.Contains(e.GetType()))
                    {
                        isBreak = true;
                    }
                    else
                    {
                        pauseTimer();

                        var ed = new OperationErrorDialog(e, canSkip);

                        if (obj != null)
                        {
                            ed.AdditionalObjectInfo = obj.ToString();
                        }

                        if (ed.Show())
                        {
                            isBreak = ed.ClickedButtonNumber != 0;

                            if (ed.RememberChoice == 1)
                            {
                                _skipErrorTypes.Add(e.GetType());
                            }
                        }
                        else
                        {
                            isBreak = true;
                            IsCanceled = true;
                        }

                        resumeTimer();
                    }
                }
            }

            if (IsCanceled)
            {
                OperationFarPodException e = new CancelOperationFarPodException(lastError);

                e.AdditionalObjectInfo = obj;

                throw e;
            }

            return isDone;
        }

        protected abstract bool work();
    }
}
