using System;
using System.Text;
using System.Collections.Generic;

using HeadingGenerator.Utility;
using HeadingGenerator.ReflectiveCalling;

namespace HeadingGenerator.Generator {
    /// <summary>
    /// Provide an interface for converting supplied text into a 'header' formated string block
    /// </summary>
    public class HeadingGenerator {
        /*----------Variables----------*/
        //PRIVATE

        /// <summary>
        /// The sequence of characters that will be used to make up the characters in the boundary
        /// </summary>
        private string boundarySequence;

        /// <summary>
        /// The sequence of characters that will be used to fill in the negative space within the boundary
        /// </summary>
        private string fillSequence;

        /// <summary>
        /// The number of lines that will be used on the upper/lower ends of the boundary
        /// </summary>
        private int verticalBoundarySize;

        /// <summary>
        /// The number of columns that will be used on the left/right sides of the boundary
        /// </summary>
        private int horizontalBoundarySize;

        /// <summary>
        /// The number of lines that will be used above/below the display text between the boundary extents
        /// </summary>
        private int verticalBufferSize;

        /// <summary>
        /// The number of columns that will be used on the left/right sides of the display text between the boundary extens
        /// </summary>
        private int horizontalBufferSize;

        /// <summary>
        /// The number of lines that will be used between identified text sections
        /// </summary>
        private int sectionBufferSize;

        /// <summary>
        /// The size of the display space that will be allocated to showing the supplied display text
        /// </summary>
        private int fixedSize;

        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// The sequence of characters that will be used to make up the characters in the boundary
        /// </summary>
        [ReflectiveCallable("-boundarySequence")]
        public string BoundarySequence {
            get { return boundarySequence; }
            set { boundarySequence = (string.IsNullOrEmpty(value) ? GeneratorSettings.Default.boundarySequence : value); }
        }

        /// <summary>
        /// The sequence of characters that will be used to fill in the negative space within the boundary
        /// </summary>
        [ReflectiveCallable("-fillSequence")]
        public string FillSequence {
            get { return fillSequence; }
            set { fillSequence = (string.IsNullOrEmpty(value) ? GeneratorSettings.Default.fillSequence : value); }
        }

        /// <summary>
        /// The number of lines that will be used on the upper/lower ends of the boundary
        /// </summary>
        [ReflectiveCallable("-verticalBoundarySize")]
        public int VerticalBoundarySize {
            get { return verticalBoundarySize; }
            set { verticalBoundarySize = Math.Max(0, value); }
        }

        /// <summary>
        /// The number of columns that will be used on the left/right sides of the boundary
        /// </summary>
        [ReflectiveCallable("-horizontalBoundarySize")]
        public int HorizontalBoundarySize {
            get { return horizontalBoundarySize; }
            set { horizontalBoundarySize = Math.Max(0, value); }
        }

        /// <summary>
        /// The number of lines that will be used above/below the display text between the boundary extents
        /// </summary>
        [ReflectiveCallable("-verticalBufferSize")]
        public int VerticalBufferSize {
            get { return verticalBufferSize; }
            set { verticalBufferSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The number of columns that will be used on the left/right sides of the display text between the boundary extens
        /// </summary>
        [ReflectiveCallable("-horizontalBufferSize")]
        public int HorizontalBufferSize {
            get { return horizontalBufferSize; }
            set { horizontalBufferSize = Math.Max(0, value); }
        }

        /// <summary>
        /// The number of lines that will be used between identified text sections
        /// </summary>
        [ReflectiveCallable("-sectionBufferSize")]
        public int SectionBufferSize {
            get { return sectionBufferSize; }
            set { sectionBufferSize = Math.Max(0, value); }
        }

        /// <summary>
        /// Flags if the width of the generated heading should be be a constant fixed size
        /// </summary>
        [ReflectiveCallable("-useFixedWidth")]
        public bool UseFixedWidth { get; set; }

        /// <summary>
        /// The size of the display space that will be allocated to showing the supplied display text
        /// </summary>
        [ReflectiveCallable("-fixedSize")]
        public int FixedSize {
            get { return fixedSize; }
            set { fixedSize = Math.Max(1, value); }
        }

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Break the supplied into sections based on the standard newline character ('\n')
        /// </summary>
        /// <param name="text">The text that is to be split into the sections</param>
        /// <param name="longest">Passes out the length of the longest section generated</param>
        /// <returns>Returns an array of the sections that were found in the text</returns>
        private string[] RetrieveTextSections(string text, out int longest) {
            //Store a collection of the sections that were found in the text
            List<string> found = new List<string>();

            //Reset the length passed out to a starting value to test against
            longest = 0;

            //Process the string to find all newlined sections
            for (int prog = 0; prog < text.Length;) {
                //Skip over any newline characters
                if (text[prog] == '\n') {
                    ++prog;
                    continue;
                }

                //This is the first character to be captured
                int startInd = prog, endInd = prog + 1;

                //Find the end of this section that needs to be captured
                for (; endInd < text.Length && text[endInd] != '\n'; ++endInd);

                //Bring the end point back into the usable range
                --endInd;

                //Shift the progress to after the endpoint
                prog = endInd + 1;

                //Siphon the substring into the found collection
                found.Add(text.Substring(startInd, endInd - startInd + 1));

                //Check if the length of the latest entry is larger then the current
                if (found[found.Count - 1].Length > longest)
                    longest = found[found.Count - 1].Length;
            }

            //Return the identified sections
            return found.ToArray();
        }

        /// <summary>
        /// Convert the supplied text value into the specified subsections that are needed to conform to the maximum length
        /// </summary>
        /// <param name="text">The text that is to be processed</param>
        /// <param name="maximumLength">The maximum length that a sub-section can be</param>
        /// <returns>Returns an array of the formatted sub-section lines that can be inserted into the final header</returns>
        private string[] BreakSectionIntoSubSections(string text, int maximumLength) {
            //Split the text into lines as required
            List<StringBuilder> lines = new List<StringBuilder>(1);

            //If the text is less then the maximum length, it doesn't need to be split
            if (text.Length <= maximumLength) lines.Add(new StringBuilder(text));

            //Otherwise, the line needs to be subdivided to meet the length requirement
            else {
                //Progress through the line of text to find the individual sections that can be seperated out
                int start = 0, end = maximumLength - 1;

                //Process each sub-section
                while (start != text.Length) {
                    //Move the start forward until a printable character is found
                    for (; start < end + 1 && TextUtility.NON_VISUALISED_SET.Contains(text[start]); ++start);

                    //If the start matches the end, skip to the next section
                    if (start == end) {
                        start = Math.Min(text.Length, start + 1);
                        end = Math.Min(text.Length - 1, start + maximumLength + 1);
                        continue;
                    }

                    //Back the progress up until a non-visualised character is found
                    int cur = end;
                    if (cur != text.Length - 1)
                        for (; cur > start && !TextUtility.NON_VISUALISED_SET.Contains(text[cur]); --cur);

                    //If no gap in text sections could be found, hard cutoff
                    if (cur == start) cur = end;

                    //Create the new line segment from the substring
                    lines.Add(new StringBuilder(text.Substring(start, cur - start + (text[cur] == ' ' ? 0 : 1))));

                    //Adjust to the new starting search point
                    start = Math.Min(text.Length, cur + 1);
                    end = Math.Min(text.Length - 1, start + maximumLength - 1);
                }
            }

            //Create an array to hold the individual values
            string[] formatted = new string[lines.Count];

            //Ensure that all of the identified lines meet the required length for this section, with horizontalBoundarySize included
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < lines.Count; ++i) {
                //Get the number of spaces that are required to be added to each line
                int needed = maximumLength - lines[i].Length + (horizontalBufferSize * 2);

                //If there is no needed buffer, don't bother
                if (needed == 0) {
                    formatted[i] = lines[i].ToString();
                    continue;
                }

                //Get half of the amount that is needed to buffer out the first part of this 
                int half = needed / 2;

                //Determine how many whole instances of the fill sequence can be added
                int fullCount = half / fillSequence.Length;

                //Clear the buffer element for the initial padding elements
                buffer.Clear();

                //Add the full sequences to the buffer
                if (fullCount > 0) buffer.Append(fillSequence, fullCount);

                //Add the substring section of the fill sequence as needed
                if (buffer.Length != half) buffer.Append(fillSequence.Substring(0, half - buffer.Length));

                //Prepend this buffer to the current line
                lines[i].Insert(0, buffer.ToString());

                //Add the remainder of the half divide to get the finishing sections requirements
                half += needed % 2;

                //Clear out the buffer for the ending section
                buffer.Clear();

                //Determine the remainder of the fill sequence that needs to be added to finish it off
                int remainder = lines[i].Length % fillSequence.Length;

                //Check if the fill sequence needs the ending added
                if (remainder > 0) buffer.Append(fillSequence.Substring(remainder, Math.Min(half, fillSequence.Length - remainder)));

                //Determine how many whole instances can be added to the space
                fullCount = (half - buffer.Length) / fillSequence.Length;

                //Add the full sequences to the buffer
                if (fullCount > 0) buffer.Append(fillSequence, fullCount);

                //Add the substring section of the fill sequence as needed to fill up the space
                if (buffer.Length != half) buffer.Append(fillSequence.Substring(0, half - buffer.Length));

                //Append the closing buffer to the current line
                lines[i].Append(buffer.ToString());

                //Save the final formatted string
                formatted[i] = lines[i].ToString();
            }

            //Return the formatted strings
            return formatted;
        }

        //PUBLIC

        /// <summary>
        /// Initialise this heading generator object with the default settings
        /// </summary>
        public HeadingGenerator() { ApplySettings(GeneratorSettings.Default); }

        /// <summary>
        /// Initialise this heading generator object with the supplied settings
        /// </summary>
        /// <param name="settings">The starting settings that should be assigned to this generator</param>
        public HeadingGenerator(GeneratorSettings settings) { ApplySettings(settings); }

        /// <summary>
        /// Restore the default settings to this object, overriding any current values
        /// </summary>
        [ReflectiveCallable("-restoreDefaultSettings")]
        public void RestoreDefaultSettings() { ApplySettings(GeneratorSettings.Default); }

        /// <summary>
        /// Retrieve the current settings applied to this object as a object
        /// </summary>
        /// <returns>Returns an object with the current values of this generator</returns>
        public GeneratorSettings GetCurrentSettings() {
            return new GeneratorSettings {
                boundarySequence = boundarySequence,
                fillSequence = fillSequence,
                verticalBoundarySize = verticalBoundarySize,
                horizontalBoundarySize = horizontalBoundarySize,
                verticalBufferSize = verticalBufferSize,
                horizontalBufferSize = horizontalBufferSize,
                sectionBufferSize = sectionBufferSize,
                useFixedWidth = UseFixedWidth,
                fixedSize = fixedSize
            };
        }

        /// <summary>
        /// Apply the supplied settings object to this generator
        /// </summary>
        /// <param name="settings">The collection of settings that should be applied</param>
        public void ApplySettings(GeneratorSettings settings) {
            BoundarySequence = settings.boundarySequence;
            FillSequence = settings.fillSequence;
            VerticalBoundarySize = settings.verticalBoundarySize;
            HorizontalBoundarySize = settings.horizontalBoundarySize;
            VerticalBufferSize = settings.verticalBufferSize;
            HorizontalBufferSize = settings.horizontalBufferSize;
            SectionBufferSize = settings.sectionBufferSize;
            UseFixedWidth = settings.useFixedWidth;
            FixedSize = settings.fixedSize;
        }

        /// <summary>
        /// Generate a heading using the current settings with the supplied display text
        /// </summary>
        /// <param name="displayText">The text that is to be displayed within the generated heading text</param>
        /// <returns>Returns a formatted string with the contained display text</returns>
        [ReflectiveCallable("-generateHeading")]
        public string GenerateHeading(string displayText) {
            //Split the supplied text up into the individual sections that need to be managed
            int longest;
            string[] sections = RetrieveTextSections(displayText, out longest);

            //If the display area size is fixed, adjust the identified length
            if (UseFixedWidth) longest = fixedSize;

            //Calculate the length of a single line in this header
            int totalLineLength = longest + horizontalBoundarySize * 2 + horizontalBufferSize * 2;

            //Create the line that will be used for the full top/bottom buffer bounds
            StringBuilder fullHorizontalBounds = new StringBuilder(totalLineLength);
            int fullCount = totalLineLength / boundarySequence.Length;
            if (fullCount > 0) fullHorizontalBounds.Append(boundarySequence, fullCount);
            if (fullHorizontalBounds.Length != totalLineLength) fullHorizontalBounds.Append(boundarySequence.Substring(0, totalLineLength - fullHorizontalBounds.Length));

            //Create the left bounding sequence of characters
            StringBuilder leftHorizontalBounds = new StringBuilder(horizontalBoundarySize);
            fullCount = horizontalBoundarySize / boundarySequence.Length;
            if (fullCount > 0) leftHorizontalBounds.Append(boundarySequence, fullCount);
            if (leftHorizontalBounds.Length != horizontalBoundarySize) leftHorizontalBounds.Append(boundarySequence.Substring(0, horizontalBoundarySize - leftHorizontalBounds.Length));

            //Create the right bounding sequence of characters
            StringBuilder rightHorizontalBounds = new StringBuilder(horizontalBoundarySize);
            int remainder = (horizontalBoundarySize + horizontalBufferSize + longest) % boundarySequence.Length;
            if (remainder > 0) rightHorizontalBounds.Append(boundarySequence.Substring(remainder, Math.Min(horizontalBoundarySize, boundarySequence.Length - remainder)));
            fullCount = (horizontalBoundarySize - rightHorizontalBounds.Length) / boundarySequence.Length;
            if (fullCount > 0) rightHorizontalBounds.Append(boundarySequence, fullCount);
            if (rightHorizontalBounds.Length != horizontalBoundarySize) rightHorizontalBounds.Append(boundarySequence.Substring(0, horizontalBoundarySize - rightHorizontalBounds.Length));

            //Create the buffer line sequence of characters
            StringBuilder bufferLine = new StringBuilder(totalLineLength);
            bufferLine.Append(leftHorizontalBounds.ToString());
            int displaySize = longest + horizontalBufferSize * 2;
            fullCount = displaySize / fillSequence.Length;
            if (fullCount > 0) bufferLine.Append(fillSequence, fullCount);
            if (bufferLine.Length != leftHorizontalBounds.Length + displaySize) bufferLine.Append(fillSequence.Substring(0, (leftHorizontalBounds.Length + displaySize) - bufferLine.Length));
            bufferLine.Append(rightHorizontalBounds.ToString());

            //Compile the final collection of lines
            StringBuilder compiled = new StringBuilder((verticalBoundarySize * 2 + verticalBufferSize * 2 + sections.Length * 2 - 1) * (totalLineLength + 1));

            //Add the starting full boundary lines
            for (int i = 0; i < verticalBoundarySize; ++i)
                compiled.AppendLine(fullHorizontalBounds.ToString());

            //Add the buffer spaces
            for (int i = 0; i < verticalBufferSize; ++i)
                compiled.AppendLine(bufferLine.ToString());

            //Process the inersetion of the display text
            for (int d = 0; d < sections.Length; ++d) {
                //Retrieve the formatted sub-section lines 
                string[] subSections = BreakSectionIntoSubSections(sections[d], longest);

                //Introduce the sub section text sections
                for (int i = 0; i < subSections.Length; ++i) {
                    //Add the section start
                    compiled.Append(leftHorizontalBounds.ToString());

                    //Add the display section
                    compiled.Append(subSections[i]);

                    //Finish up this section
                    compiled.AppendLine(rightHorizontalBounds.ToString());
                }

                //If there is another section following, add the buffer space
                if (d + 1 < sections.Length) {
                    for (int i = 0; i < sectionBufferSize; ++i)
                        compiled.AppendLine(bufferLine.ToString());
                }
            }

            //Add the buffer spaces
            for (int i = 0; i < verticalBufferSize; ++i)
                compiled.AppendLine(bufferLine.ToString());

            //Add the starting full boundary lines
            for (int i = 0; i < verticalBoundarySize; ++i)
                compiled.AppendLine(fullHorizontalBounds.ToString());

            //Return the final string value
            return compiled.ToString();
        }
    }
}
