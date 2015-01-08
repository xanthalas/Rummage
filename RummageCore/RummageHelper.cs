using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RummageCore
{
    public class RummageHelper
    {
        /// <summary>
        /// Validate that all the non-blank lines in the text passed in contain valid regexes.
        /// </summary>
        /// <param name="text">The set of text to check.</param>
        /// <returns>Empty string if all regexes are valid, otherwise the first invalid regex line</returns>
        public static string AreRegexesValid(string text)
        {
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            return validateRegexes(lines);
        }


        /// <summary>
        /// Validate that all the non-blank lines passed in contain valid regexes.
        /// </summary>
        /// <param name="text">The lines to check.</param>
        /// <returns>True if all the lines contain valid regexes, otherwise false</returns>        
        public static string AreRegexesValid(IEnumerable<string> lines)
        {
            return validateRegexes(lines);
        }


        private static string validateRegexes(IEnumerable<string> lines)
        {
            string invalidRegex = string.Empty;

            foreach (var regexline in lines)
            {
                string line = regexline.Trim();
                if (line.Length > 0)
                {
                    try
                    {
                        Regex rx = new Regex(line);
                    }
                    catch (ArgumentException)
                    {
                        invalidRegex = line;
                        break;
                    }
                }
            }
            return invalidRegex;
        }

    }
}