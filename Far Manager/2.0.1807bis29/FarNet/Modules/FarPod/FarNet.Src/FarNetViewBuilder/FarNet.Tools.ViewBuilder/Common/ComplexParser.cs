using System;
using System.Collections.Generic;

namespace FarNet.Tools.ViewBuilder.Common
{
    static class ComplexParser
    {
        public static Dictionary<string, string> GetValue(object value, string complexName, string defaultParamName)
        {
            if (!IsComplex(value)) return null;

            string strValue = value.ToString();

            strValue = strValue.Substring(1, strValue.Length - 2).Trim();

            string nameOfComplex = strValue.Substring(0, strValue.IndexOf(" "));

            if (string.Compare(nameOfComplex, complexName, true) == 0)
            {
                strValue = strValue.Substring(nameOfComplex.Length);

                var result = new Dictionary<string, string>();

                foreach (string paramPair in strValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] paramElements = paramPair.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                    string name = paramElements.Length == 2 ? paramElements[0].Trim() : defaultParamName;
                    string val = paramElements[paramElements.Length - 1];

                    result.Add(name.Trim(), val.Trim());
                }

                return result;
            }

            return null;
        }

        public static bool IsComplex(object value)
        {
            if (value == null) return false;

            string strValue = value.ToString();

            return (string.IsNullOrEmpty(strValue) == false &&
                    strValue.Length > 3 &&
                    strValue.StartsWith("{") &&
                    strValue.EndsWith("}"));
        }
    }
}
