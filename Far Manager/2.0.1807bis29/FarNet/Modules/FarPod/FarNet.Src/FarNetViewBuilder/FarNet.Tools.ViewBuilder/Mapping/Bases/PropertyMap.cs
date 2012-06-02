using System;

namespace FarNet.Tools.ViewBuilder.Mapping.Bases
{
    public class PropertyMap
    {
        [Flags]
        public enum EKind
        {
            Value,
            Event,
            Control
        }

        public string Name
        {
            get;
            private set;
        }

        public EKind Kind
        {
            get;
            private set;
        }

        public PropertyMap(string name, EKind kind)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            Name = name;
            Kind = kind;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Kind, Name);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Kind.GetHashCode();
        }
    }
}
