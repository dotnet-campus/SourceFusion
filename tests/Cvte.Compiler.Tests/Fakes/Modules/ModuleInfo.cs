namespace Cvte.Compiler.Tests.Fakes.Modules
{
    public class ModuleInfo
    {
    }

    public class ModuleInfo<TModule> : ModuleInfo where TModule : IModule, new()
    {

    }
}
