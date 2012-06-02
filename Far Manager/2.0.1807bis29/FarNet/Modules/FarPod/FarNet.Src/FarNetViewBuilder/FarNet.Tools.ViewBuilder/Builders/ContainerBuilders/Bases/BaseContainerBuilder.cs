using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using FarNet.Tools.ViewBuilder.Common;

namespace FarNet.Tools.ViewBuilder.Builders.ContainerBuilders.Bases
{
    public abstract class BaseContainerBuilder : BaseBuilder
    {
        protected virtual IEnumerable<object> assemblyInner(ViewFactory factory, BuildContext context)
        {
            var resultList = new List<object>();

            XElement parentNode = context.CurrentNode;            

            foreach (XElement childNode in context.CurrentNode.Elements())
            {
                BaseBuilder childBuilder = factory.GetBuilder(childNode.Name.LocalName);

                if (childBuilder != null)
                {
                    XElement fullChildNode = applyParentParameters(parentNode, childNode);

                    context.CurrentNode = fullChildNode;

                    object childBuildResult = childBuilder.Assembly(factory, context);
                    
                    resultList.AddRange(getFlatResult(childBuildResult));
                }
            }

            context.CurrentNode = parentNode;

            return resultList;
        }

        private IEnumerable<object> getFlatResult(object obj)
        {
            if (obj == null) yield break;

            if (obj is IEnumerable)
            {
                foreach (object child in ((IEnumerable)obj))
                {
                    foreach (object v in getFlatResult(child))
                    {
                        yield return v;
                    }                    
                }
            }
            else
                yield return obj;
        }

        private XElement applyParentParameters(XElement parent, XElement child)
        {
            var result = new XElement(child);

            foreach (XAttribute attr in parent.Attributes())
            {
                if (attr.Name.LocalName.StartsWith(child.Name.LocalName))
                {
                    string name = attr.Name.LocalName.Substring(child.Name.LocalName.Length + 1);

                    result.Add(new XAttribute(name, attr.Value));
                }
            }

            return result;
        }
    }
}
