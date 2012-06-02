using System;

namespace FarNet.Tools.ViewBuilder.Common
{
    static class ValueParser
    {
        public static object GetValue(Type type, string strValue)
        {
            bool isNullable = false;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                isNullable = true;

                type = type.GetGenericArguments()[0];
            }

            object result;

            if (type.IsEnum)
            {
                result = Enum.Parse(type, strValue);
            }
            else if (type == typeof(Place))
            {
                var placeParts = strValue.Split(',');
                var place = new int[4];

                for (int i = 0; i < placeParts.Length; i++)
                {
                    place[i] = Convert.ToInt32(placeParts[i] ?? "0");
                }

                result = new Place(place[0], place[1], place[2], place[3]);
            }
            else if (type == typeof(Guid))
            {
                result = new Guid(strValue);
            }
            else if (type == typeof(bool))
            {
                if (strValue.Length == 1)
                    result = strValue[0] == '1';
                else
                    result = bool.Parse(strValue);
            }
            else if (type == typeof(string))
            {
                result = strValue;
            }
            else if (type.IsValueType)
            {
                result = Convert.ChangeType(strValue, type);
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (isNullable) return Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(type), result);
            
            return result;
        }
    }
}
