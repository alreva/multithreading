using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;

namespace Dir.Read
{
    public static class EnumStringFormatter
    {
        public static string ToString2<T>(this T enumValue)
            where T: struct
        {
            if (!typeof (T).IsEnum)
            {
                throw new ArgumentException($"You can only supply Enum values here, but supplied {enumValue} of {typeof(T)}");
            }

            var intValue = (int)Convert.ChangeType(enumValue, typeof (int));

            string baseToString = enumValue.ToString();

            if (Regex.IsMatch(baseToString, "[^0-9]"))
            {
                return baseToString;
            }

            var customString = new StringBuilder($"custom ({baseToString}): ");

            List<string> permissions = new List<string>();

            foreach (int permission in Enum.GetValues(typeof(FileSystemRights)))
            {
                if ((intValue & permission) > 0)
                {
                    permissions.Add(((FileSystemRights)permission).ToString());
                }
            }

            permissions.Sort();
            customString.Append(string.Join(", ", permissions.Distinct()));

            customString.Append(" + possibly something else");

            return customString.ToString();
        }
    }
}