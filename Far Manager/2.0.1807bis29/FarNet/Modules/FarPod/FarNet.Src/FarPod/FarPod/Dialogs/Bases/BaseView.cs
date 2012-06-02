namespace FarPod.Dialogs.Bases
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using FarNet;
    using FarNet.Forms;
    using FarNet.Tools.ViewBuilder;
    using FarPod.Common;
    using System.Xml.Linq;
    using System.Xml;

    /// <summary>
    /// View super class
    /// </summary>
    abstract class BaseView : IDisposable, INotifyPropertyChanged
    {
        protected ViewFactory viewFactory
        {
            get;
            private set;
        }

        protected IDialog dialog
        {
            get;
            private set;
        }

        protected IBox box
        {
            get;
            private set;
        }

        public Action<BaseView> Initializer
        {
            get;
            set;
        }

        protected virtual void initView()
        {
            var xmlFullName = string.Format("FarPod.Dialogs.Views.{0}.xml", GetType().Name);

            var xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xmlFullName);

            viewFactory = new ViewFactory();

            var rootNode = XElement
                .Load(XmlReader.Create(xmlStream))
                .DescendantsAndSelf("Dialog")
                .FirstOrDefault();

            dialog = viewFactory.Create(this, FarPodContext.Current.ModuleManager, rootNode);

            box = getControls<IBox>().FirstOrDefault();
        }

        protected virtual void free()
        {

        }

        protected void setSize(int width, int height)
        {
            dialog.Rect = new Place(-1, -1, width, height);

            if (box != null) box.Rect = new Place(3, 1, width - 4, height - 2);
        }

        protected IEnumerable<T> getControls<T>()
        {
            return dialog.Controls.OfType<T>();
        }

        public virtual bool Show()
        {
            initView();

            if (Initializer != null) Initializer(this);

            return dialog.Show();
        }

        public virtual void Close()
        {
            dialog.Close();

            free();
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            dialog = null;
            box = null;

            if (viewFactory != null)
            {
                viewFactory.Dispose();
                viewFactory = null;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void firePropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;

            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
