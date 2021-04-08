using System;

namespace Butjok {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, Inherited = false,
        AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute {

        public readonly string Name;
        public readonly string[] Arguments;
        public readonly string HelpText;

        public CommandAttribute(string name = null, string[] arguments = null, string helpText = null) {
            Name = name;
            Arguments = arguments;
            HelpText = helpText;
        }
    }
}