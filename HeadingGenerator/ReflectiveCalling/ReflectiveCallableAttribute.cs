using System;

namespace HeadingGenerator.ReflectiveCalling {
    /// <summary>
    /// Mark a field/attribute that can be called via reflective means
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ReflectiveCallableAttribute : Attribute {
        /*----------Properties----------*/
        //PUBLIC

        /// <summary>
        /// Flag if the default name should be overriden and the assigned used instead
        /// </summary>
        public bool OverrideDefaultName { get; private set; }

        /// <summary>
        /// The name that will be used instead of the default name
        /// </summary>
        public string CallableName { get; private set; }

        /*----------Functions----------*/
        //PUBLIC

        /// <summary>
        /// Default constructor to allow creating a basic attribute with no overrides
        /// </summary>
        public ReflectiveCallableAttribute() { }

        /// <summary>
        /// Mark the attribute with an override name that will be used for the object instead
        /// </summary>
        /// <param name="callableName">The name that will be attached to the marked Property|Method instead</param>
        public ReflectiveCallableAttribute(string callableName) {
            CallableName = callableName;
            OverrideDefaultName = true;
        }
    }
}
