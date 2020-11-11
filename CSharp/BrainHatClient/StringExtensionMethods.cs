using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainHatClient
{
    public static class StringExtensionMethods
    {
        public static bool CheckHatResponse(this string value)
        {
            if (value != null && value.Length > 2)
            {
                var response = value.Substring(0, 3);
                if (response == "ACK")
                    return true;
            }
            return false;
        }

        public static string GetHatResponse(this string value)
        {
            if (value != null && value.Length > 3)
            {
                return value.Substring(3);
            }
            return "";
        }

       

    }
}
