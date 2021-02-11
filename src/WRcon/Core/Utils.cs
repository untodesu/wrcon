using System;

namespace WRcon.Core
{
    internal static class Utils
    {
        public static bool IsNumeric(string input)
        {
            for(int i = 0; i < input.Length; i++) {
                if(!Char.IsDigit(input[i]))
                    return false;
            }
            return true;
        }
    }
}
