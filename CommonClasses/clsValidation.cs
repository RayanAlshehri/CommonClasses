using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses
{
    public static class clsValidation
    {
        public static bool DoesEmailHaveBasicFormat(string Email)
        {
            int AtIndex = -1;
            int DotIndex = -1;

            for (int i = 0; i < Email.Length; i++)
            {
                if (Email[i] == '@')
                {
                    AtIndex = i;
                }

                if (Email[i] == '.')
                {
                    DotIndex = i;
                }

                if (AtIndex != -1 && DotIndex != -1)
                    break;
            }

            if (!(AtIndex != -1 && DotIndex != -1))
                return false;


            return AtIndex != 0
                && DotIndex != Email.Length - 1
                && DotIndex > AtIndex + 1;
        }

        public static bool IsPhoneNumberValid(string PhoneNumber)
        {
            return PhoneNumber[0] == '0' && PhoneNumber[1] == '5' && PhoneNumber.Length == 10;
        }
    }
}
