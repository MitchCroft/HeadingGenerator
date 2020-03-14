using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using HeadingGenerator.Utility;

namespace HeadingGenerator.ReflectiveCalling {
    /// <summary>
    /// Identify elements within a supplied object that can be reflectivvly called with variable parameters
    /// </summary>
    /// <remarks>This is intended for the dynamic raising of functionality through the inputting of string values</remarks>
    public sealed class ReflectiveMethodCache {
        /*----------Variables----------*/
        //CONST

        /// <summary>
        /// Store an array of the types of parameters that can be processed and used by this object
        /// </summary>
        private static readonly HashSet<Type> USABLE_PARAM_TYPES = new HashSet<Type> {
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(char),
            typeof(string),
            typeof(bool),
        };

        /// <summary>
        /// Define the search flags that will be used when looking for methods that can be raised
        /// </summary>
        private const BindingFlags SEARCH_FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance;

        //PRIVATE

        /// <summary>
        /// Store a reference to all of the methods found that can be raised for operation
        /// </summary>
        private Dictionary<string, MethodInfo> cachedMethods;

        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// A reference to the object that was processed for raising functionality
        /// </summary>
        public object Target { get; private set; }

        /*----------Functions----------*/
        //PRIVATE

        /// <summary>
        /// Retrieve the individual parameter values from the supplied string
        /// </summary>
        /// <param name="dataString">The string of data that is to be processed into individual components</param>
        /// <returns>Returns an array of the identified data elements that were found</returns>
        private string[] GetParameterValues(string dataString) {
            //Create a list of the elements that where found
            List<string> found = new List<string>();

            //Loop through and process the string
            for (int prog = 0; prog < dataString.Length;) {
                //Check if this character is usable
                if (TextUtility.NON_VISUALISED_SET.Contains(dataString[prog])) {
                    ++prog;
                    continue;
                }

                //Find the segments within this string that are to be added as an individual section
                int startInd = -1, endInd = -1;

                //Flag if an entire segment could be found
                bool foundSegment = false;

                //If this character is a string opening, need to process until the matching character is found
                if (dataString[prog] == '"') {
                    //Look for a closing string opening to use
                    int end = -1;
                    for (int i = prog + 1; i < dataString.Length; ++i) {
                        if (dataString[i] == '"') {
                            end = i;
                            foundSegment = true;
                            break;
                        }
                    }

                    //If a segment could be found define the values to the extract
                    if (foundSegment) {
                        startInd = prog + 1;
                        endInd = end - 1;
                        prog = end + 1;
                    }
                }

                //If no segment could be found, just grab until the next break
                if (!foundSegment) {
                    //Use this character as the starting point
                    startInd = prog;

                    //Look for the endpoint
                    for (endInd = startInd + 1; endInd < dataString.Length && !TextUtility.NON_VISUALISED_SET.Contains(dataString[endInd]); ++endInd);

                    //Bring the end point back into the usable range
                    --endInd;

                    //Shift the progress to after the end point
                    prog = endInd + 1;
                }

                //Add the extracted section to the list
                found.Add(dataString.Substring(startInd, endInd - startInd + 1));
            }

            //Return the identified elements
            return found.ToArray();
        }

        //PUBLIC

        /// <summary>
        /// Initialise this cache with the target object that will be managed
        /// </summary>
        /// <param name="target">The target object will be used to generate the cache of methods</param>
        public ReflectiveMethodCache(object target) {
            //Store the reference to the object that is being targeted
            Target = target;

            //Get the type description of the object being targeted
            Type type = target.GetType();

            //Store a collection of all of the methods that are to be processed for inclusion
            List<Tuple<ReflectiveCallableAttribute, MethodInfo>> toSetup = new List<Tuple<ReflectiveCallableAttribute, MethodInfo>>();

            //Find all of the methods within the type that can be processed
            MethodInfo[] methods = type.GetMethods(SEARCH_FLAGS);
            for (int i = 0; i < methods.Length; ++i) {
                //Look for a callable cache attribute that can be used
                ReflectiveCallableAttribute att = methods[i].GetCustomAttribute<ReflectiveCallableAttribute>();

                //If no attribute, don't bother
                if (att == null) continue;

                //Add it to the processing list
                toSetup.Add(new Tuple<ReflectiveCallableAttribute, MethodInfo>(att, methods[i]));
            }

            //Find all of the property setters within the type that can be processed
            PropertyInfo[] properties = type.GetProperties(SEARCH_FLAGS);
            for (int i = 0; i < properties.Length; ++i) {
                //Look for a callable cache attribute that can be used
                ReflectiveCallableAttribute att = properties[i].GetCustomAttribute<ReflectiveCallableAttribute>();

                //If no attribute, don't bother
                if (att == null) continue;

                //Check that there is a setter method to be processed
                if (properties[i].SetMethod == null) {
                    Console.WriteLine($"ERROR: Unable to process property '{properties[i].Name}' tagged as Reflective Callable, there is no set method available");
                    continue;
                }

                //Add it to the processing list
                toSetup.Add(new Tuple<ReflectiveCallableAttribute, MethodInfo>(att, properties[i].SetMethod));
            }

            //Create the collection for the methods to be cached
            cachedMethods = new Dictionary<string, MethodInfo>(toSetup.Count);

            //Process all the elements that were found for setting up
            foreach (var pair in toSetup) {
                //Check that this method is usable with the system
                if (pair.Item2.IsGenericMethod || pair.Item2.ContainsGenericParameters) {
                    Console.WriteLine($"ERROR: Unable to process method '{GenerateMethodSignature(pair.Item2)}' tagged as Reflective Callable, generic methods are not permitted");
                    continue;
                }

                //Check that it's parameters are valid for calling
                bool paramsValid = true;
                foreach (ParameterInfo param in pair.Item2.GetParameters()) {
                    //If the parameter type isn't in the list, no can do
                    if (!USABLE_PARAM_TYPES.Contains(param.ParameterType)) {
                        paramsValid = false;
                        break;
                    }
                }

                //If the parameters aren't valid, don't bother
                if (!paramsValid) {
                    Console.WriteLine($"ERROR: Unable to process the method '{GenerateMethodSignature(pair.Item2)}' tagged as Reflective Callable, unsuported parameter type(s)");
                    continue;
                }

                //Get the name that will be used to represent this method
                string name = pair.Item1.CallableName;

                //If this name is not overriden, grab the name from the method
                if (!pair.Item1.OverrideDefaultName) {
                    name = (pair.Item2.IsSpecialName ?
                        pair.Item2.Name.Substring(pair.Item2.Name.LastIndexOf('_') + 1) :
                        pair.Item2.Name
                    );
                }

                //If there is no name assigned, can't use it
                if (string.IsNullOrWhiteSpace(name)) {
                    Console.WriteLine($"ERROR: Unable to process the method '{GenerateMethodSignature(pair.Item2)}' tagged as Reflective Callable, no name has been assigned");
                    continue;
                }

                //Check that the name hasn't already been used
                if (cachedMethods.ContainsKey(name)) {
                    Console.WriteLine($"ERROR: Unable to process the method '{GenerateMethodSignature(pair.Item2)}' tagged as Reflective Callable as the assigned name '{name}' has already been assigned to the method '{GenerateMethodSignature(cachedMethods[name])}'");
                    continue;
                }

                //Add this option to the list
                cachedMethods.Add(name.ToLowerInvariant(), pair.Item2);
            }
        }

        /// <summary>
        /// Get a summary of all of the methods that can be executed within this object
        /// </summary>
        /// <returns>Returns a string of all of the methods and their possible options</returns>
        public string GetMethodSummary() {
            //Store all of the data in a string Builder for use
            StringBuilder sb = new StringBuilder();

            //Loop through and process all of the entries in the dictionary
            foreach (var pair in cachedMethods) {
                //Add the basic information for this entry
                sb.AppendLine($"Method Name: {pair.Key}");
                sb.AppendLine($"Method Signature: {GenerateMethodSignature(pair.Value)}");

                //Retrieve all of the parameters that are expected
                ParameterInfo[] parameters = pair.Value.GetParameters();
                
                //Append all of the parameter information to the entry
                for (int i = 0; i < parameters.Length; ++i) 
                    sb.AppendLine($"\t{i}:\tName = {parameters[i].Name}\tType = {parameters[i].ParameterType.Name}");

                //Add a blank line at the end of the entry
                sb.AppendLine();
            }

            //Return the final string
            return sb.ToString();
        }

        /// <summary>
        /// Check to see if the cache contains a method under the supplied name
        /// </summary>
        /// <param name="name">The name of the method to be looked for</param>
        /// <returns>Returns true if there is a method with the assigned name in the cache</returns>
        public bool HasMethod(string name) { return cachedMethods.ContainsKey(name.ToLowerInvariant()); }

        /// <summary>
        /// Try to execute the cached method with the supplied collection of data
        /// </summary>
        /// <param name="dataString">The complete string of data that starts with the method to be raised and the subsequent parameters to be parsed</param>
        /// <returns>Returns true if the method was executed successfully</returns>
        public bool Execute(string dataString) {
            //Check there is a string to process
            if (string.IsNullOrWhiteSpace(dataString)) {
                Console.WriteLine("ERROR: Unable to run ReflectiveMethodCache.Execute(dataString) command with an empty dataString supplied");
                return false;
            }

            //Try to parse the data string into a collection of individual commands
            string[] segments = dataString.Split(TextUtility.NON_VISUALISED_CHARS, 2, StringSplitOptions.RemoveEmptyEntries);

            //If there are no segments then we have a problem
            if (segments.Length == 0) {
                Console.WriteLine("ERROR: Unable to run ReflectiveMethodCache.Execute(dataString) command with an empty dataString supplied");
                return false;
            }

            //Create an array for the other segments to be processed as parameters
            string[] parameters = (segments.Length > 1 ?
                GetParameterValues(segments[1]) :
                null
            );

            //Try to run the supplied functionality
            return Execute(segments[0], parameters);
        }

        /// <summary>
        /// Try to execute the cached method with the supplied name and parameters
        /// </summary>
        /// <param name="name">The name of the method that is to be raised</param>
        /// <param name="parameters">The string of parameters that are to be supplied to the raising method</param>
        /// <returns>Returns true if the method was executed successfully</returns>
        public bool Execute(string name, string parameters) {
            //Check that the name exists
            if (string.IsNullOrWhiteSpace(name)) {
                Console.WriteLine("ERROR: Unable to run ReflectiveMethodCache.Execute(name, parameters) command with an empty name");
                return false;
            }

            //Check that there is a method with the supplied name
            if (!cachedMethods.ContainsKey(name.ToLowerInvariant())) {
                Console.WriteLine($"ERROR: Unable to run ReflectiveMethodCache.Execute(name, parameters) command as the method '{name}' doesn't exist");
                return false;
            }

            //Divide the paramaters string into individual segments
            string[] parameterSegments = (string.IsNullOrWhiteSpace(parameters) ?
                null :
                GetParameterValues(parameters)
            );

            //Try to run the supplied functionality
            return Execute(name, parameterSegments);
        }

        /// <summary>
        /// Try to execute the cached method with the supplied name and parameters
        /// </summary>
        /// <param name="name">The name of the method that is to be raised</param>
        /// <param name="parameters">The individual values that will be converted into the data formats for raising the method</param>
        /// <returns>Returns true if the method was executed successfully</returns>
        public bool Execute(string name, params string[] parameters) {
            //Check that the name exists
            if (string.IsNullOrWhiteSpace(name)) {
                Console.WriteLine("ERROR: Unable to run execute command with an empty name");
                return false;
            }

            //Check that there is a method with the supplied name
            if (!cachedMethods.ContainsKey(name.ToLowerInvariant())) {
                Console.WriteLine($"ERROR: Unable to run execute command as the method '{name}' doesn't exist");
                return false;
            }

            //Get the method that is to be run
            MethodInfo method = cachedMethods[name.ToLowerInvariant()];

            //Get the parameters within the method for testing
            ParameterInfo[] paramInfo = method.GetParameters();

            //Store the number of parameter values there are to parse
            int paramCount = (parameters != null ? parameters.Length : 0);

            //Check there are enough parameter sections to attempt parsing
            if (paramCount < paramInfo.Length) {
                Console.WriteLine($"ERROR: Unable to run execute command as the method '{GenerateMethodSignature(method, true)}' requires more parameter information then the supplied count ({paramCount})");
                return false;
            }

            //Try to parse the supplied values into usable objects
            object[] parameterValues = new object[paramInfo.Length];
            for (int i = 0; i < parameterValues.Length; ++i) {
                try { parameterValues[i] = Convert.ChangeType(parameters[i], paramInfo[i].ParameterType); }
                catch (Exception exec) {
                    Console.WriteLine($"ERROR: Unable to run execute command for the method '{GenerateMethodSignature(method, true)}' due to a failure to parse the supplied value '{parameters[i]}' (Index {i}) could not be converted to '{paramInfo[i].ParameterType.Name}'. {exec.Message}");
                    return false;
                }
            }

            //If there are additional values, log the ones being ignored
            if (paramCount > paramInfo.Length) {
                StringBuilder sb = new StringBuilder($"WARNING: Execute command for method '{GenerateMethodSignature(method, true)}' is being run but the additional supplied parameter values will be ignored. Values are:\n");
                for (int i = paramInfo.Length; i < paramCount; ++i)
                    sb.Append($"\t'{parameters[i]}'\n");
                Console.Write(sb.ToString());
            }

            //Try to run the defined method
            try { method.Invoke(Target, parameterValues); return true; }

            //If anything goes wrong, log the failure
            catch (Exception exec) {
                Console.WriteLine($"ERROR: Unable to successfully run execute command for the method '{GenerateMethodSignature(method, true)}' due to an exception thrown in the method. {exec.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create a string that defines the method that was supplied
        /// </summary>
        /// <param name="method">The method that is to be converted into a representative string</param>
        /// <param name="includeParamNames">Flags if the parameter names should be included in the string</param>
        /// <returns>Returns a string that describes the method supplied</returns>
        public static string GenerateMethodSignature(MethodInfo method, bool includeParamNames = false) {
            //Create the string for this method
            StringBuilder sb = new StringBuilder($"{method.DeclaringType.Name}.{method.Name}");

            //Retrieve the parameters for this method
            ParameterInfo[] parameters = method.GetParameters();

            //If there are parameters to process, add them to the string
            if (parameters.Length > 0) {
                //Add a buffer for the parameters
                sb.Append(" (");

                //Loop through and add the parameters
                for (int i = 0; i < parameters.Length; ++i) {
                    //Add the different parameter elements
                    if (includeParamNames) sb.Append($"{parameters[i].Name} = {parameters[i].ParameterType.Name}");
                    else sb.Append(parameters[i].ParameterType.Name);

                    //Check if the punctuation needs to be added
                    if (i != parameters.Length - 1) sb.Append(", ");
                }

                //End the collection
                sb.Append(")");
            }
            return sb.ToString();
        }
    }
}
