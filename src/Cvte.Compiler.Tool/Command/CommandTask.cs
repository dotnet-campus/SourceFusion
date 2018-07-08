namespace Cvte.Compiler.Command
{
    /// <summary>
    /// Provide a base class for all tasks that can run command from command line.
    /// </summary>
    public abstract class CommandTask
    {
        /// <summary>
        /// Run command when derived class override this method.
        /// </summary>
        /// <returns>
        /// Return value of the whole application.
        /// </returns>
        public virtual int Run()
        {
            return 0;
        }
    }
}