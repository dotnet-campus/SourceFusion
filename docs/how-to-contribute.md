# 如何为 SourceFusion 贡献代码

SourceFusion 基于 Roslyn 对代码进行编译期分析和执行。

## 核心程序集

核心程序集有两个：

- SourceFusion.Core
- SourceFusion.Tool

SourceFusion.Core 是抽象部分，当作为 NuGet 包安装后，会被目标程序集引用，这样目标程序集可以编写出编译期间运行的的代码。SourceFusion.Tool 是对以上抽象的具体实现部分，当作为 NuGet 包安装后，如果目标程序集执行编译操作，那么此程序集将被执行。也就是说，SourceFusion.Core 的存在是为了让目标程序集能够编写出编译期间执行的代码，而 SourceFusion.Tool 则是执行目标程序集中指定为编译期执行的代码。

这两个程序集有一些开发上的约定。

- SourceFusion.Core 由于会被目标项目通过 PackageReference 的方式引用，所以不要额外引入第三方依赖。
- SourceFusion.Tool 由于其控制台输出被用于特殊用途，所以请不要使用 `Console.WriteLine` 等进行调试。
