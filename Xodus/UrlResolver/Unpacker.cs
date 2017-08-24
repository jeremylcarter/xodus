using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UrlResolver
{
    public class Unpacker
    {
        private const string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string[] symtab;

        public (string, string[], int, int) FilterArgs(string source)
        {
            var reg = @"}\s*\('(.*)',\s*(.*?),\s*(\d+),\s*'(.*?)'\.split\('\|'\)";
            var re = new Regex(reg, RegexOptions.Singleline);
            var args = re.Match(source).Groups;
            var payload = args[1];
            var radix = args[2];
            var count = args[3];
            var symtab = args[4];

            return (payload.Value, symtab.Value.Split('|'), int.Parse(radix.Value), int.Parse(count.Value));
        }

        public string ReplaceStrings(string source)
        {
            var re = new Regex("var *(_\\w+)\\=\\[\"(.*?)\"\\];", RegexOptions.Singleline);
            var match = re.Match(source);
            return source;
        }

        public string Unpack(string source)
        {
            var args = FilterArgs(source);
            var payload = args.Item1;
            symtab = args.Item2;
            var radix = args.Item3;
            var count = args.Item4;

            if (symtab.Length != count)
                throw new ArgumentException();

            var regEx = new Regex(@"\b\w+\b", RegexOptions.Singleline);
            var result = regEx.Replace(payload, matchEval);
            return ReplaceStrings(result);
        }

        public static string Encode(long input)
        {
            if (input < 0) throw new ArgumentOutOfRangeException(nameof(input), input, "input cannot be negative");

            var clistarr = CharList.ToCharArray();
            var result = new Stack<char>();
            while (input != 0)
            {
                result.Push(clistarr[input % 36]);
                input /= 36;
            }
            return new string(result.ToArray());
        }

        /// <summary>
        ///     Decode the Base36 Encoded string into a number
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static long Decode(string input)
        {
            var reversed = input.ToLower().Reverse();
            long result = 0;
            var pos = 0;
            foreach (var c in reversed)
            {
                result += CharList.IndexOf(c) * (long) Math.Pow(36, pos);
                pos++;
            }
            return result;
        }

        public static string matchEval(Match m)
        {
            var x = m.Groups[0].Value;
            string y = null;

            try
            {
                var shit = (int) Decode(x);
                if (!string.IsNullOrEmpty(symtab[shit]))
                    return symtab[shit];
                return x;
            }
            catch (FormatException)
            {
                if (x.Length == 2)
                    try
                    {
                        var x1 = Convert.ToInt32(x, 16);
                        return symtab[x1];
                    }
                    catch (FormatException)
                    {
                        return x;
                    }
                //string y2 = DecimalToArbitrarySystem(x1, 36);
                y = x;
            }

            return y;
        }

        public static string DecimalToArbitrarySystem(long decimalNumber, int radix)
        {
            const int BitsInLong = 64;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (radix < 2 || radix > Digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length);

            if (decimalNumber == 0)
                return "0";

            var index = BitsInLong - 1;
            var currentNumber = Math.Abs(decimalNumber);
            var charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                var remainder = (int) (currentNumber % radix);
                charArray[index--] = Digits[remainder];
                currentNumber = currentNumber / radix;
            }

            var result = new string(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
                result = "-" + result;

            return result;
        }
    }
}