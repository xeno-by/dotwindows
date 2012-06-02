using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FarNet.Forms;

namespace FarNet.Tools.ViewBuilder.Common
{
    public class FactoryContext
    {
        public Type TypeOfView
        {
            get;
            set;
        }

        public object View
        {
            get;
            set;
        }

        public XElement RootNode
        {
            get;
            set;
        }

        public XElement CurrentNode
        {
            get;
            set;
        }

        public IDialog Dialog
        {
            get;
            set;
        }

        //public IControl ParentControl
        //{
        //    get;
        //    set;
        //}
    }
}
