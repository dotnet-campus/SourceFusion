using System;

namespace Cvte.Compiler.Command
{
    /// <summary>
    /// Specify a unique name of a command and when user typped a command
    /// with this name the Run method of this class will be executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandMetadataAttribute : Attribute
    {
        /// <summary>
        /// Gets the unique name of a command task.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the description of the command task.
        /// This will be shown when the user typed --help option.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specify a unique name of a command and when user typped a command
        /// with this name the Run method of this class will be executed.
        /// </summary>
        public CommandMetadataAttribute(string commandName)
        {
            Name = commandName;
        }
    }
}