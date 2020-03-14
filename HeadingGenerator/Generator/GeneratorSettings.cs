using System;
using System.IO;

namespace HeadingGenerator.Generator {
    /// <summary>
    /// Store the collection of settings that will be used when rendering headings
    /// </summary>
    [Serializable] public struct GeneratorSettings {
        /*----------Variables----------*/
        //SHARED

        /// <summary>
        /// Store the default settings that will be used for header generation
        /// </summary>
        public static readonly GeneratorSettings Default = new GeneratorSettings {
            boundarySequence        = "/",
            fillSequence            = "-",
            verticalBoundarySize    = 1,
            horizontalBoundarySize  = 10,
            verticalBufferSize      = 0,
            horizontalBufferSize     = 0,
            sectionBufferSize       = 1,
            useFixedWidth           = true,
            fixedSize               = 80
        };

        //PUBLIC

        /// <summary>
        /// The sequence of characters that will be used to make up the characters in the boundary
        /// </summary>
        public string boundarySequence;

        /// <summary>
        /// The sequence of characters that will be used to fill in the negative space within the boundary
        /// </summary>
        public string fillSequence;

        /// <summary>
        /// The number of lines that will be used on the upper/lower ends of the boundary
        /// </summary>
        public int verticalBoundarySize;

        /// <summary>
        /// The number of columns that will be used on the left/right sides of the boundary
        /// </summary>
        public int horizontalBoundarySize;

        /// <summary>
        /// The number of lines that will be used above/below the display text between the boundary extents
        /// </summary>
        public int verticalBufferSize;

        /// <summary>
        /// The number of columns that will be used on the left/right sides of the display text between the boundary extens
        /// </summary>
        public int horizontalBufferSize;

        /// <summary>
        /// The number of lines that will be used between identified text sections
        /// </summary>
        public int sectionBufferSize;

        /// <summary>
        /// Flags if the width of the generated heading should be be a constant fixed size
        /// </summary>
        public bool useFixedWidth;

        /// <summary>
        /// The size of the display space that will be allocated to showing the supplied display text
        /// </summary>
        public int fixedSize;

        /*----------Functions----------*/
        //STATIC

        /// <summary>
        /// Save the supplied settings object to the specified path
        /// </summary>
        /// <param name="path">The path that defines where the data should be stored</param>
        /// <param name="settings">The settings values that should be saved to disk</param>
        /// <returns>Returns true if the settings where saved to the supplied path</returns>
        public static bool SaveData(string path, GeneratorSettings settings) {
            //Ensure that the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            //Try to write the data to disk
            try {
                //Create an output stream for the data
                using (FileStream stream = new FileStream(path, FileMode.Create)) {
                    using (BinaryWriter writer = new BinaryWriter(stream)) {
                        writer.Write(settings.boundarySequence);
                        writer.Write(settings.fillSequence);
                        writer.Write(settings.verticalBoundarySize);
                        writer.Write(settings.horizontalBoundarySize);
                        writer.Write(settings.verticalBufferSize);
                        writer.Write(settings.horizontalBufferSize);
                        writer.Write(settings.sectionBufferSize);
                        writer.Write(settings.useFixedWidth);
                        writer.Write(settings.fixedSize);
                    }
                }
                return true;
            }

            //If anything goes wrong, assume failure
            catch (Exception exec) {
                Console.WriteLine($"ERROR: Failed to save the settings to the location '{path}'. {exec.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load the settings from the supplied path
        /// </summary>
        /// <param name="path">The path where the data should be loaded from</param>
        /// <param name="settings">Passes out the loaded settings on success <see cref="Generator.GeneratorSettings.Default"/> on failure</param>
        /// <returns>Returns true if the settings where loaded properly from the location</returns>
        public static bool LoadData(string path, out GeneratorSettings settings) {
            //try to load the data from the path
            try {
                using (FileStream stream = new FileStream(path, FileMode.Open)) {
                    using (BinaryReader reader = new BinaryReader(stream)) {
                        settings.boundarySequence = reader.ReadString();
                        settings.fillSequence = reader.ReadString();
                        settings.verticalBoundarySize = reader.ReadInt32();
                        settings.horizontalBoundarySize = reader.ReadInt32();
                        settings.verticalBufferSize = reader.ReadInt32();
                        settings.horizontalBufferSize = reader.ReadInt32();
                        settings.sectionBufferSize = reader.ReadInt32();
                        settings.useFixedWidth = reader.ReadBoolean();
                        settings.fixedSize = reader.ReadInt32();
                    }
                }
                return true;
            }

            //If the file couldn't be found, silent failure
            catch (FileNotFoundException) {
                settings = Default;
                return false;
            }

            //If anything goes wrong, assume failure
            catch (Exception exec) {
                Console.WriteLine($"ERROR: Failed to load the settings from the location '{path}'. {exec.Message}");
                settings = Default;
                return false;
            }
        }
    }
}
