# SourceFusion

SourceFusion 是一套预编译框架。

当项目安装 SourceFusion 了之后，项目中即可开始书写预编译代码。通过执行这些预编译代码，项目可以在编译期间执行一些平时需要在运行时执行的代码。这种方式能够将耗时的运行时代码迁移到编译期执行，大幅度提升运行时性能。

## 入门

### 安装 NuGet 包

在 NuGet 源上搜索 `SourceFusion` 寻找已发布的 NuGet 包。由于目前尚未发布正式版，所以你需要指定搜索“预编译版本”才能搜索到此包。

在需要编写预编译代码的项目中安装此 NuGet 包即可。

### 编写预编译代码

你有两种编写预编译代码的方法：

1. 纯文本代码转换；
1. 模板转换。

以下代码为纯文本预编译代码的 Hello World 实现。HelloWorldTransformer.cs 中的 `Transform` 方法将在编译期间执行，用于将 HelloWorld.cs 文件中的输出改为 `Hello World!`。

```csharp
[CompileTimeCode("HelloWorld.cs")]
public class HelloWorldTransformer : IPlainCodeTransformer
{
    public string Transform(string originalText, TransformingContext context)
    {
        return originalText.Replace("Hello", "Hello World!");
    }
}
```

```csharp
using System;

public class HelloWorld
{
    public void SayHello()
    {
        Console.WriteLine("Hello");
    }
}
```

以下代码为模板转换的 Hello World 实现：

```csharp
using System;

namespace SourceFusion.Tests
{
    [CompileTimeTemplate]
    public class HelloWorld
    {
        public void SayHello()
        {
            var outputs = Placeholder.Array<string>(context =>
            {
                // 这里使用两个引号来转义一个引号，最终我们得到了一个包含三个单词部分的数组。
                return @"""Hello "", ""World"", ""!""";
            });
            foreach (var output in outputs)
            {
                Console.Write(output);
            }
        }
    }
}
```

## 为此项目开发

非常期望你能加入到 SourceFusion 的开发中来，请阅读 [如何为 SourceFusion 贡献代码](/docs/how-to-contribute.md) 了解开发相关的约定和技术要求。
