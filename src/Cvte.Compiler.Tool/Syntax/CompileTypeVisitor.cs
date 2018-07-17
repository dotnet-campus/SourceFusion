using System.Collections.Generic;
using System.Linq;
using Cvte.Compiler.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cvte.Compiler.Syntax
{
    /// <summary>
    /// 访问编译的所有类和属性
    /// </summary>
    internal class CompileTypeVisitor : CSharpSyntaxRewriter
    {
        private string _namespace;
        private readonly List<ICompileType> _types = new List<ICompileType>();

        /// <summary>
        /// 所有找到的类型
        /// </summary>
        internal IReadOnlyList<ICompileType> Types => _types;

        private List<string> UsingNamespaceList { get; } = new List<string>();

        /// <summary>
        /// 创建文件编译访问
        /// </summary>
        /// <param name="visitIntoStructuredTrivia"></param>
        public CompileTypeVisitor(bool visitIntoStructuredTrivia = false) : base(visitIntoStructuredTrivia)
        {
        }

        /// <inheritdoc />
        public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var name = node.Name.ToString();

            UsingNamespaceList.Add(name);
            return base.VisitUsingDirective(node);
        }

        /// <summary>
        /// 获取命名空间
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
        /// 获取类
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var identifier = VisitToken(node.Identifier);

            var baseTypeList = new List<string>();
            if (node.BaseList != null)
            {
                foreach (var temp in node.BaseList.Types)
                {
                    var name = temp.Type.ToString();

                    baseTypeList.Add(name);
                }
            }

            //获取类的特性
            var attributeLists = GetCompileAttributeList(node.AttributeLists);

            _lastType = new CompileType
            (
                identifier.ValueText,
                _namespace,
                attributeLists,
                baseTypeList,
                UsingNamespaceList
            );

            _types.Add(_lastType);


            return base.VisitClassDeclaration(node);
        }

        private ICompileType _lastType;

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var attributeLists = GetCompileAttributeList(node.AttributeLists);

            // accessor 就是获取 Set get

            ICompileMethod get = null;
            ICompileMethod set = null;

            foreach (var temp in node.AccessorList.Accessors)
            {

                if (temp.Keyword.Text == "get")
                {
                    get = new CompileMethod(GetCompileAttributeList(temp.AttributeLists), "get");
                }
                else if (temp.Keyword.Text == "set")
                {
                    set=new CompileMethod(GetCompileAttributeList(temp.AttributeLists),"set");
                }
            }


            var compileProperty = new CompileProperty(node.Type.ToString(), attributeLists, node.Identifier.ToString())
            {
                SetMethod = set,
                GetMethod = get,
            };

            var type = _lastType;
            ((ICompileTypeProperty) type).CompilePropertyList.Add(compileProperty);

            return base.VisitPropertyDeclaration(node);
        }

        private ICompileAttribute[] GetCompileAttributeList(SyntaxList<AttributeListSyntax> attributeList)
        {
            return VisitList(attributeList).SelectMany(x => x.Attributes)
                .Select(x => new CompileAttribute(x.Name.ToFullString())).Cast<ICompileAttribute>().ToArray();
        }


    }
}