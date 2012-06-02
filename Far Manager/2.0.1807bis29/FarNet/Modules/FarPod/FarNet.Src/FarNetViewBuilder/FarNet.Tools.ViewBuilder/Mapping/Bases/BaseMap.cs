using System.Collections.Generic;
using FarNet.Tools.ViewBuilder.Mapping.Bases;

namespace FarNet.Tools.ViewBuilder.Mapping.Bases
{
    public abstract class BaseMap
    {
        public virtual bool HasName 
        { 
            get { return false; } 
        }

        private readonly List<PropertyMap> _propertyMaps;

        public List<PropertyMap> PropertyMaps
        {
            get { return _propertyMaps; }
        }        

        protected BaseMap()
        {            
            _propertyMaps = new List<PropertyMap>();

            init();
        }

        protected virtual void init()
        {

        }

        protected virtual void addMap(string name, PropertyMap.EKind kind)
        {
            var newMap = new PropertyMap(name, kind);

            if (!PropertyMaps.Contains(newMap)) PropertyMaps.Add(newMap);
        }

        protected virtual void include<T>() where T : BaseMap, new()
        {
            BaseMap includeMap = new T();

            foreach (var propertyMap in includeMap.PropertyMaps)
            {
                if (!PropertyMaps.Contains(propertyMap)) PropertyMaps.Add(propertyMap);
            }
        }
    }
}
