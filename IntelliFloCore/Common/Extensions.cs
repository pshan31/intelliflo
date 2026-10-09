using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliFloCore.Common
{
    public static class Extensions
    {
        public static bool IsNumeric(this string text, out double test)
        {
            return double.TryParse(text, out test);
        }

        public static bool IsNumeric(this string text)
        {
            double test;
            return double.TryParse(text, out test);
        }
    }
}
