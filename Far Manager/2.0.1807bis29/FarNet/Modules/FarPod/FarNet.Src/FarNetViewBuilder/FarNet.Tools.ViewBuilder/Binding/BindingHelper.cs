using System;
using System.Reflection;

namespace FarNet.Tools.ViewBuilder.Binding
{
    static class BindingHelper
    {
        public static void SetValueIfViewHasProperty(Type typeOfContext, object context, string dataPath, object value)
        {
            BindingFlags defFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            PropertyInfo pi = null;

            foreach (string propertyName in dataPath.Split('.'))
            {
                if (context != null && pi != null)
                {
                    context = pi.GetGetMethod().Invoke(context, null);
                }

                if (context == null)
                {
                    break;
                }

                pi = context.GetType().GetProperty(propertyName, defFlags);
            }

            if (pi != null && pi.CanWrite && pi.PropertyType.IsAssignableFrom(value.GetType()))
            {
                pi.SetValue(context, value, null);
            }
        }

        public static object GetValueIfViewHasProperty(Type typeOfContext, object context, string dataPath)
        {
            BindingFlags defFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            PropertyInfo pi = null;

            foreach (string propertyName in dataPath.Split('.'))
            {
                if (context != null && pi != null)
                {
                    context = pi.GetGetMethod().Invoke(context, null);
                }

                if (context == null)
                {
                    break;
                }

                pi = context.GetType().GetProperty(propertyName, defFlags);
            }

            if (pi != null && pi.CanRead)
            {
                return pi.GetValue(context, null);
            }

            return null;
        }
    }
}
