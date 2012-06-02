namespace FarPod.Extensions
{
    using System;

    /// <summary>
    /// Generic Enum Extensions
    /// </summary>
    static class EnumExtensions
    {
        public static bool HasFlag<T>(this Enum en, T flag)
        {
            return ((int)(object)en & (int)(object)flag) == (int)(object)flag;            
        }

        public static T AddFlag<T>(this Enum en, T flag)
        {
            return (T)(object)((int)(object)en | (int)(object)flag);
        }

        public static T RemoveFlag<T>(this Enum en, T flag)
        {
            return (T)(object)((int)(object)en ^ (int)(object)flag);
        }
    }
}
