using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cvte.Compiler.Syntax
{
    internal class ClassDeclarationVisitor : CSharpSyntaxRewriter
    {
        private string _namespace;
        private readonly List<ClassDeclaration> _classes = new List<ClassDeclaration>();

        internal IReadOnlyList<ClassDeclaration> Classes { get; }

        public ClassDeclarationVisitor()
        {
            Classes = new ReadOnlyCollection<ClassDeclaration>(_classes);
        }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var nameSyntax = Visit(node.Name);
            _namespace = nameSyntax.ToFullString().Trim();
            return base.VisitNamespaceDeclaration(node);
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var identifier = VisitToken(node.Identifier);
            var list = VisitList(node.AttributeLists).SelectMany(x => x.Attributes).Select(x => x.Name.ToFullString());
            _classes.Add(new ClassDeclaration(
                identifier.ValueText,
                _namespace,
                list));
            return base.VisitClassDeclaration(node);
        }

        internal static IReadOnlyList<ClassDeclaration> VisiteSyntaxTree(SyntaxTree tree)
        {
            var classDeclarationVisitor = new ClassDeclarationVisitor();
            classDeclarationVisitor.Visit(tree.GetRoot());
            return classDeclarationVisitor.Classes;
        }
    }
}
