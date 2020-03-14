using System;
using System.IO;
using System.Text;

using HeadingGenerator.ReflectiveCalling;

namespace HeadingGenerator.Generator {
    /// <summary>
    /// Manage a collection of additional settings profiles that can be swapped between while generating headings
    /// </summary>
    public sealed class ProfiledHeadingGenerator : HeadingGenerator {
        /*----------Variables----------*/
        //CONST

        /// <summary>
        /// Store the format that will be used when saving/loading profile information
        /// </summary>
        public const string SAVED_PROFILE_NAME = "{0}.data";

        //PRIVATE

        /// <summary>
        /// Store the directory to store the generated settings profiles at
        /// </summary>
        private string profileStorageLocation;

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Initialise this object with the default settings
        /// </summary>
        /// <param name="profileStorageLocation">The location on the device where the profile options are stored</param>
        public ProfiledHeadingGenerator(string profileStorageLocation) {
            this.profileStorageLocation = profileStorageLocation;
            ApplySettings(GeneratorSettings.Default);
        }

        /// <summary>
        /// Initialise this object with specified settings
        /// </summary>
        /// <param name="profileStorageLocation">The location on the device where the profile options are stored</param>
        /// <param name="settings">The default settings that this generator should be initialised with</param>
        public ProfiledHeadingGenerator(string profileStorageLocation, GeneratorSettings settings) : base(settings) {
            this.profileStorageLocation = profileStorageLocation;
        }

        /// <summary>
        /// Retrieve the disk location expected for a profle with the supplied name
        /// </summary>
        /// <param name="profileName">The name of the profile that a path will be generated for</param>
        /// <returns>Returns a path that can be used to save/load the profile</returns>
        public string GetProfilePath(string profileName) {
            return Path.Combine(
                profileStorageLocation,
                string.Format(SAVED_PROFILE_NAME, profileName)
            );
        }

        /// <summary>
        /// List all of the profiles that are saved to disk
        /// </summary>
        [ReflectiveCallable("-listProfiles")]
        public void ListProfiles() {
            //Create a listing of all of the profiles that can be found in the storage directory
            StringBuilder sb = new StringBuilder("Available Heading Generator Profiles:\n");

            //Try to find all of the profiles within the storage location
            try {
                //Get the directory information for the storage location
                DirectoryInfo dir = new DirectoryInfo(profileStorageLocation);

                //Retrieve all of the files within storage location that match the expected extension
                FileInfo[] profiles = dir.GetFiles($"*{Path.GetExtension(SAVED_PROFILE_NAME)}");

                //Add all of the profiles to the display listing
                for (int i = 0; i < profiles.Length; ++i)
                    sb.AppendLine($"\t{i + 1}:\t{profiles[i].Name}");
            }

            //If the directory can't be found, assume no saved profiles
            catch (DirectoryNotFoundException) { sb.AppendLine("\tNO PROFILES"); }

            //Anything else is an error that needs logging
            catch (Exception exec) { sb.AppendLine($"\tERROR: {exec.Message}"); }

            //Output the found profiles
            Console.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Delete the profile that is stored with the supplied name
        /// </summary>
        /// <param name="profileName">The name of the profile that is to be removed</param>
        [ReflectiveCallable("-deleteProfile")]
        public void DeleteProfile(string profileName) {
            //Attempt to delete the profile
            try { File.Delete(GetProfilePath(profileName)); }

            //If the file/path can't be found, assume it doesn't exist
            catch (DirectoryNotFoundException) {}

            //Otherwise, problem that needs reporting
            catch (Exception exec) { Console.WriteLine($"ERROR: Unable to delete the profile with the name '{profileName}'. {exec.Message}"); }
        }

        /// <summary>
        /// Save the current settings as a profile that can be reloaded
        /// </summary>
        /// <param name="profileName">The name to use for the new profile object</param>
        /// <returns>Returns true if the profile was saved successfully</returns>
        [ReflectiveCallable("-saveProfile")]
        public bool SaveProfile(string profileName) {
            //Attempt to save the current settings under the supplied name
            string path = GetProfilePath(profileName);

            //Try to write the settings to the data path
            return GeneratorSettings.SaveData(path, GetCurrentSettings());
        }

        /// <summary>
        /// Load the settings from the specified profile and apply them to this generator
        /// </summary>
        /// <param name="profileName">The name of the profile that is to be loaded</param>
        /// <returns>Returns true if the profile settings were loaded successfully</returns>
        [ReflectiveCallable("-loadProfile")]
        public bool LoadProfile(string profileName) {
            //Get the path that would be used for the profile
            string path = GetProfilePath(profileName);

            //Try to load the information from supplied path
            GeneratorSettings settings;
            if (!GeneratorSettings.LoadData(path, out settings)) {
                Console.WriteLine($"Unable to load the profile '{profileName}'");
                return false;
            }

            //Apply the loaded settings
            ApplySettings(settings);
            return true;
        }
    }
}
