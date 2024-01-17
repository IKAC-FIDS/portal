using System.Numerics;

namespace TES.Web.Core
{
    public static class AccountNumberExtensions
    {
        public static bool TryGenerateShebaNumber(string accountNo, out string result)
        {
            accountNo = accountNo.Trim();

            if (accountNo.Length < 19)
            {
                var zeroCountShouldAppend = 19 - accountNo.Length;
                var zeros = string.Empty;
                for (int i = 0; i < zeroCountShouldAppend; i++)
                {
                    zeros += "0";
                }
                accountNo = zeros + accountNo;
            }

            if (!BigInteger.TryParse($"058{accountNo}182700", out BigInteger tryParseOutput))
            {
                result = "WRONG";
                return false;
            }

            var cd = 98 - tryParseOutput % 97;
            var optimizedCd = cd >= 10 ? cd.ToString() : $"0{cd}";

            if (BigInteger.Parse($"058{accountNo}1827{optimizedCd}") % 97 != 1)
            {
                result = "WRONG";
                return false;
            }

            result = $"IR{optimizedCd}058{accountNo}";

            return true;
        }

        public static bool TryGenerateAccountNumberFromSheba(string shebaNumber, out string result)
        {
            result = null;

            if (string.IsNullOrEmpty(shebaNumber))
                return false;

            if (shebaNumber.Length != 26)
                return false;

            if (!shebaNumber.StartsWith("IR"))
                return false;

            result = $"{shebaNumber.Substring(8, 4)}-{shebaNumber.Substring(12, 3)}-{shebaNumber.Substring(15, 8)}-{shebaNumber.Substring(23, 3)}";

            return true;
        }

        public static void GenerateAccountNumber(string accountBranchCode, string accountType, string accountCustomerNumber, string accountRow, out string accountNumberWithDash, out string accountNumberWithoutDash)
        {
            accountBranchCode = accountBranchCode.PadLeft(4, '0');
            accountType = accountType.PadLeft(3, '0');
            accountCustomerNumber = accountCustomerNumber.PadLeft(8, '0');
            accountRow = accountRow.PadLeft(3, '0');

            accountNumberWithDash = $"{accountBranchCode}-{accountType}-{accountCustomerNumber}-{accountRow}";
            accountNumberWithoutDash = $"{accountBranchCode}{accountType}{accountCustomerNumber}{accountRow}";
        }
    }
}