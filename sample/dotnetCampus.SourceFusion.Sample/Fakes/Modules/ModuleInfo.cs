﻿namespace dotnetCampus.SourceFusion.Sample.Fakes.Modules
{
    public class ModuleInfo
    {
    }

    public class ModuleInfo<TModule> : ModuleInfo where TModule : IModule, new()
    {

    }
}
