using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace HeadingGenerator.Utility {
    /// <summary>
    /// Store a collection of values that can be used to apply text processing functionality
    /// </summary>
    public static class TextUtility {
        /*----------Variables----------*/
        //CONST

        /// <summary>
        /// Store a set of the characters that can't be written out as a text character
        /// </summary>
        public static readonly HashSet<char> NON_VISUALISED_SET;

        /// <summary>
        /// Store the collection of non-visualisable characters as an array for split operations
        /// </summary>
        public static readonly char[] NON_VISUALISED_CHARS;

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Identify the characters that can't be represented visually on the screen
        /// </summary>
        static TextUtility() {
            //Store a set of all of the characters
            NON_VISUALISED_SET = new HashSet<char>();
            NON_VISUALISED_SET.Add(' ');

            //Store a set of all of the categories of character that will be ignored
            HashSet<UnicodeCategory> nonPrintableCategories = new HashSet<UnicodeCategory>(new UnicodeCategory[] {
                UnicodeCategory.Control,
                UnicodeCategory.OtherNotAssigned,
                UnicodeCategory.Surrogate
            });

            //Loop through and test all of the characters that need to be omitted
            for (int i = 0; i < short.MaxValue; ++i) {
                if (nonPrintableCategories.Contains(char.GetUnicodeCategory((char)i)))
                    NON_VISUALISED_SET.Add((char)i);
            }

            //Copy the found characters to the array
            NON_VISUALISED_CHARS = new char[NON_VISUALISED_SET.Count];
            NON_VISUALISED_SET.CopyTo(NON_VISUALISED_CHARS);
        }

        //PUBLIC

        /// <summary>
        /// Retrieve the supplied text with consistent '\n' newline characters
        /// </summary>
        /// <param name="text">The text that is to be processed for removal</param>
        /// <returns>Returns the supplied text with standard new-line characters</returns>
        public static string WithNormalisedEndLines(this string text) {
            return (text.Contains("\r\n") ?
                text.Replace("\r\n", "\n") :
                text
            );
        }

        /// <summary>
        /// Replace the vlaues with a string object if they match the supplied conditions
        /// </summary>
        /// <param name="text">The text that is to be processed for removal</param>
        /// <param name="toReplace">The sequence that will inserted if the supplied character matches the predicate</param>
        /// <param name="match">The predicate used to determine if a character should be kept</param>
        /// <returns>Returns a string with the replacements made to it</returns>
        public static string Replace(this string text, string toReplace, Predicate<char> match) {
            StringBuilder sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; ++i)
                sb.Append(match(text[i]) ? toReplace : text[i].ToString());
            return sb.ToString();
        }

        /// <summary>
        /// Retrieve a copy of the supplied text with the non-visulisable characters stripped out
        /// </summary>
        /// <param name="text">The text that is to be processed for the removal</param>
        /// <param name="replace">The character that should be inserted into place of the removed characters</param>
        /// <param name="preserveNewLine">Flags if newline characters should be preserved in the output text</param>
        /// <param name="preserveSpace">Flags if space characters should be preserved in the output text</param>
        /// <returns>Returns the supplied text with non-visualisable characters stripped out</returns>
        /// <remarks>This function assumes that new lines will be the standard '\n' character</remarks>
        public static string WithoutNonVisualisedCharacters(this string text, string replace = "", bool preserveNewLine = true, bool preserveSpace = true) {
            return text.Replace(replace, c => {
                //Check if this character is omitted
                if (c == '\n' && preserveNewLine) return false;
                else if (c == ' ' && preserveSpace) return false;

                //Otherwise, based on visualisable state
                return NON_VISUALISED_SET.Contains(c);
            });
        }

        /// <summary>
        /// Append the supplied sequence of characters the specified number of times to the string builder object
        /// </summary>
        /// <param name="sb">The StringBuilder target object for this operation</param>
        /// <param name="sequence">The sequence of characters to be appended to the string</param>
        /// <param name="count">The number of times the sequence of characters should be appended</param>
        /// <returns>Returns a reference to the String Builder that was modified</returns>
        public static StringBuilder Append(this StringBuilder sb, string sequence, int count) {
            for (int i = 0; i < count; ++i)
                sb.Append(sequence);
            return sb;
        }
    }
}
