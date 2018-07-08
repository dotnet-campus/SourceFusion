using System;

namespace Cvte.Compiler.Command
{
    /// <summary>
    /// Specify a property to receive an option of command from the user.
    /// The option template format can be "-n", "--name" or "-n|--name".
    /// The property type can be bool, string or List{string} (or any other base types).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandOptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the option template of this option.
        /// </summary>
        public string Template { get; }

        /// <summary>
        /// Gets or sets the description of the option.
        /// This will be shown when the user typed --help option.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specify a property to receive an option of command from the user.
        /// The option template format can be "-n", "--name" or "-n|--name".
        /// The property type can be bool, string or List{string} (or any other base types).
        /// </summary>
        public CommandOptionAttribute(string template)
        {
            Template = template;
        }
    }
}