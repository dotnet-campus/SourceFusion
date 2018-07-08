using System;
using System.Text;

namespace Cvte.Compiler.Command
{
    /// <summary>
    /// Specify a property to receive argument of command from the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandArgumentAttribute : Attribute
    {
        /// <summary>
        /// Gets the argument name of a command task.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the description of the argument.
        /// This will be shown when the user typed --help option.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specify a property to receive argument of command from the user.
        /// </summary>
        public CommandArgumentAttribute(string argumentName)
        {
            Name = argumentName;
        }
    }
}
