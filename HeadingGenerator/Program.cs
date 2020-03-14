using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

using HeadingGenerator.Utility;
using HeadingGenerator.Generator;
using HeadingGenerator.ReflectiveCalling;

namespace HeadingGenerator {
    /// <summary>
    /// Manage the running of the heading generation functionality
    /// </summary>
    static class Program {
        /*----------Variables----------*/
        //CONST

        /// <summary>
        /// Store the root location that will be used for all data created by this program
        /// </summary>
        private static readonly string STORAGE_LOCATION = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeadingGenerator\\");

        /// <summary>
        /// Store the location for the persistent data store that exists between sessions
        /// </summary>
        private static readonly string PERSISTING_DATA_LOC = Path.Combine(STORAGE_LOCATION, string.Format(ProfiledHeadingGenerator.SAVED_PROFILE_NAME, "persistent"));

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Initialise the generator and process user input
        /// </summary>
        /// <param name="args">The number of starting arguments that were supplied to the generator</param>
        /// <remarks>
        /// Every argument supplied as a command line entry will be processed individually like the regular input commands
        /// in the order that they were supplied to the program
        /// </remarks>
        [STAThread] static void Main(string[] args) {
            //Retrieve the persistent data settings that applied to start with
            GeneratorSettings persistentSettings;
            GeneratorSettings.LoadData(PERSISTING_DATA_LOC, out persistentSettings);

            //Create the heading generator object that will be used
            ProfiledHeadingGenerator generator = new ProfiledHeadingGenerator(Path.Combine(STORAGE_LOCATION, "Profiles\\"), persistentSettings);

            //Create the reflective cache of methods that can be called
            ReflectiveMethodCache methodCache = new ReflectiveMethodCache(generator);

            //Create a queue of all of the command line arguments that need to be processed
            Queue<string> commands = new Queue<string>(args.Length);
            for (int i = 0; i < args.Length; ++i)
                commands.Enqueue(args[i]);

            //Process all of the commands that are received by the program
            string command; bool running = true;
            do {
                //Start this entry with the text prompt
                Console.Write("Heading Generator -> ");

                //If there are still queued commands, use that
                if (commands.Count > 0) {
                    //Take the next one for processing
                    command = commands.Dequeue();

                    //Log the taken command to the console for transparency
                    Console.WriteLine(command);
                }

                //Otherwise, grab the information from the console
                else command = Console.ReadLine();

                //Split the text into two segments to get a starting test of what the command may be
                string[] split = command.Split(TextUtility.NON_VISUALISED_CHARS, 2, StringSplitOptions.RemoveEmptyEntries);

                //Check there is data to process
                if (split.Length > 0) {
                    //Lower the first segment to be tested with
                    split[0] = split[0].ToLowerInvariant();

                    //Check if the cache contains a method with the specified name
                    if (methodCache.HasMethod(split[0])) methodCache.Execute(command);

                    //Otherwise, check if there are some default handling elements that are needed
                    else {
                        switch (split[0]) {
                            //Display the collection of commands that are available 
                            case "-help":
                                Console.WriteLine($"Heading Generator Basic Options:\n\nMethod Name: -help\nMethod Description: See the various options available\n\nMethod Name: -quit\nMethod Description: Close the application, saving current settings for next time\n\nNOTE: Any unknown commands will be treated as text that should be converted into a 'header' text sequence\n\nHeader Generation Settings:\n\n{methodCache.GetMethodSummary()}");
                                break;

                            //Allow the user to close this application, saving persistent data
                            case "-quit":
                                running = false;
                                break;

                            //Anything else will be converted into a heading for the user
                            default:
                                //Ensure that all of the newline characters are normalised for the operation
                                command = command.WithNormalisedEndLines();

                                //Strip out all of the non-visualised characters that don't impact formatting
                                command = command.WithoutNonVisualisedCharacters(" ");

                                //Generate the heading text and save it to the computers clipboard
                                Clipboard.SetText(generator.GenerateHeading(command));

                                //Log that the clipboard copy operation occurred
                                Console.WriteLine($"Generated heading for '{command}' copied to clipboard");
                                break;
                        }
                    }
                }

                //Output an empty line to break up the flow
                Console.WriteLine();
            } while (running);

            //Try to save the current settings to the persistent data path
            GeneratorSettings.SaveData(PERSISTING_DATA_LOC, generator.GetCurrentSettings());
        }
    }
}
