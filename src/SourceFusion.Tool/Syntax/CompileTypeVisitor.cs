using System.Collections.Generic;
using System.Linq;
using dotnetCampus.SourceFusion.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace dotnetCampus.SourceFusion.Syntax
{
    /// <summary>
    ///     访问编译的所有类和属性
    /// </summary>
    internal class CompileTypeVisitor : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     创建文件编译访问
        /// </summary>
        /// <param name="visitIntoStructuredTrivia"></param>
        public CompileTypeVisitor(bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
        {
        }

        /// <summary>
        ///     所有找到的类型
        /// </summary>
        internal IReadOnlyList<ICompileType> Types => _types;

        /// <summary>
        ///     此文件中的所有命名空间（不含语法性的 using 和分号）。
        /// </summary>
        internal IReadOnlyList<string> UsingNamespaceList => _usingNamespaceList;

        /// <summary>
        ///     所有程序集级别的特性（Attribute）。
        /// </summary>
        internal IReadOnlyList<ICompileAttribute> AssemblyAttributes => _assemblyAttributes;

        /// <inheritdoc />
        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.StaticKeyword.Value != null)
            {
                // 形如 using static System.Math;
                var name = node.Name.ToString();
                _usingNamespaceList.Add($"static {name}");
            }
            else if (node.Alias != null)
            {
                // 形如 using Math = System.Math;
                var alias = node.Alias.ToFullString();
                var name = node.Name.ToString();
                _usingNamespaceList.Add($"{alias}{name}");
            }
            else
            {
                // 形如 using System;
                var name = node.Name.ToString();
                _usingNamespaceList.Add(name);
            }

            return base.VisitUsingDirective(node);
        }

        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var attributes = GetCompileAttributeList(SyntaxFactory.List(node.ChildNodes().OfType<AttributeListSyntax>()));
            foreach (var attribute in attributes)
            {
                _assemblyAttributes.Add(attribute);
            }

            return base.VisitCompilationUnit(node);
        }

        /// <summary>
        ///     获取命名空间
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var nameSyntax = Visit(node.Name);
            // 命名空间
            _namespace = nameSyntax.ToFullString().Trim();

            //可能有多个命名空间
            return base.VisitNamespaceDeclaration(node);
        }

        /// <summary>
        ///     获取文件命名空间
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            var nameSyntax = Visit(node.Name);
            // 命名空间
            _namespace = nameSyntax.ToFullString().Trim();

            return base.VisitFileScopedNamespaceDeclaration(node);
        }

        /// <summary>
        ///     获取类
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _lastType = GetCompileType(node);

            _types.Add(_lastType);

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            _lastType = GetCompileType(node);

            _types.Add(_lastType);

            return base.VisitStructDeclaration(node);
        }

        /// <inheritdoc />
        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            _lastType = GetCompileType(node);

            _types.Add(_lastType);

            return base.VisitInterfaceDeclaration(node);
        }

        /// <summary>
        ///     获取属性
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var attributeLists = GetCompileAttributeList(node.AttributeLists);

            // accessor 就是获取 Set get

            ICompileMethod get = null;
            ICompileMethod set = null;

            if (node.AccessorList != null)
            {
                foreach (var temp in node.AccessorList.Accessors)
                {
                    if (temp.Keyword.Text == "get")
                    {
                        get = new CompileMethod(GetCompileAttributeList(temp.AttributeLists), "get");
                    }
                    else if (temp.Keyword.Text == "set")
                    {
                        set = new CompileMethod(GetCompileAttributeList(temp.AttributeLists), "set");
                    }
                }
            }

            var compileProperty = new CompileProperty(node.Type.ToString(), attributeLists, node.Identifier.ToString())
            {
                SetMethod = set,
                GetMethod = get
            };

            foreach (var temp in node.Modifiers)
            {
                compileProperty.MemberModifiers |= SyntaxKindToMemberModifiers(temp.Kind());
            }

            var type = _lastType;
            type.AddCompileProperty(compileProperty);

            return base.VisitPropertyDeclaration(node);
        }


        /// <inheritdoc />
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var type = _lastType;

            type.AddCompileMethod
            (
                new CompileMethod(GetCompileAttributeList(node.AttributeLists), node.ToString())
                {
                    MemberModifiers = SyntaxKindListToMemberModifiers(node.Modifiers.Select(temp => temp.Kind()))
                }
            );

            return base.VisitMethodDeclaration(node);
        }

        /// <inheritdoc />
        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var type = _lastType;

            type.AddCompileField
            (
                new CompileField(node.ToString(), GetCompileAttributeList(node.AttributeLists))
                {
                    MemberModifiers = SyntaxKindListToMemberModifiers(node.Modifiers.Select(temp => temp.Kind()))
                }
            );

            return base.VisitFieldDeclaration(node);
        }

        private readonly List<ICompileType> _types = new List<ICompileType>();

        private readonly List<string> _usingNamespaceList = new List<string>();

        private readonly List<ICompileAttribute> _assemblyAttributes = new List<ICompileAttribute>();

        private CompileType _lastType;
        private string _namespace;

        private CompileType GetCompileType(TypeDeclarationSyntax type)
        {
            // 获取类型的名称。
            var identifier = type.Identifier.ValueText;

            // 对于内部类，将父类的名称叠加到前面。
            var parent = type.Parent;
            while (parent is ClassDeclarationSyntax cds)
            {
                identifier = $"{cds.Identifier.ValueText}.{identifier}";
                parent = cds.Parent;
            }

            // 添加基类列表。
            var baseTypeList = new List<string>();
            if (type.BaseList != null)
            {
                foreach (var temp in type.BaseList.Types)
                {
                    var name = temp.Type.ToString();
                    baseTypeList.Add(name);
                }
            }

            //获取类的特性
            var attributeLists = GetCompileAttributeList(type.AttributeLists);

            return new CompileType
            (
                identifier,
                _namespace,
                attributeLists,
                baseTypeList,
                _usingNamespaceList
            );
        }

        private ICompileAttribute[] GetCompileAttributeList(SyntaxList<AttributeListSyntax> attributeList)
        {
            if (!attributeList.Any())
            {
                return new ICompileAttribute[0];
            }

            return VisitList(attributeList)
                .SelectMany(x => x.Attributes)
                .Select(x => new CompileAttribute(
                    x.Name.ToFullString(),
                    x.ArgumentList?.Arguments.Select(a =>
                    {
                        // Attribute 中形如 Property = "Value" 的语法。
                        var property = a.NameEquals?.Name.Identifier.ToString();
                        var value = a.Expression.ToString();
                        return new KeyValuePair<string, string>(property, value);
                    })))
                .Cast<ICompileAttribute>().ToArray();
        }

        private MemberModifiers SyntaxKindListToMemberModifiers(IEnumerable<SyntaxKind> kind)
        {
            var modifiers = MemberModifiers.Unset;
            foreach (var temp in kind)
            {
                modifiers |= SyntaxKindToMemberModifiers(temp);
            }

            return modifiers;
        }

        private MemberModifiers SyntaxKindToMemberModifiers(SyntaxKind kind)
        {
            var kindList = new Dictionary<SyntaxKind, MemberModifiers>
            {
                {SyntaxKind.PublicKeyword, MemberModifiers.Public},
                {SyntaxKind.PrivateKeyword, MemberModifiers.Private},
                {SyntaxKind.ProtectedKeyword, MemberModifiers.Protected},
                {SyntaxKind.InternalKeyword, MemberModifiers.Internal}
            };

            if (kindList.ContainsKey(kind))
            {
                return kindList[kind];
            }

            return MemberModifiers.Unset;
        }
    }
}