namespace dotnetCampus.TelescopeTask
{
    public abstract class CommandLineTask
    {
        public void Run()
        {
            RunCore();
        }

        protected abstract void RunCore();
    }
}
