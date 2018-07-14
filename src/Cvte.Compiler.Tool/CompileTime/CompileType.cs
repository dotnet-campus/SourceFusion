using System;
using System.Collections.Generic;

namespace Cvte.Compiler.CompileTime
{
    /// <summary>
    /// 编译时找到的类型
    /// </summary>
    internal class CompileType : CompileMember, ICompileType
    {
        /// <summary>
        /// 创建编译时找到的类型
        /// </summary>
        /// <param name="name">类的名字</param>
        /// <param name="namespace">类的命名空间</param>
        /// <param name="attributes">类的特性</param>
        /// <param name="baseTypeList"></param>
        /// <param name="usingNamespaceList"></param>
        public CompileType(string name, string @namespace, IEnumerable<ICompileAttribute> attributes, List<string> baseTypeList, List<string> usingNamespaceList)
            : base(name, attributes)
        {
            Namespace = @namespace;
            BaseTypeList = baseTypeList;
            UsingNamespaceList = usingNamespaceList;
            FullName = $"{@namespace}.{name}";
            
        }

        /// <inheritdoc />
        /// <summary>
        /// 获取类的全名
        /// </summary>
        public string FullName { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> BaseTypeList { get; }

        /// <inheritdoc />
        public IReadOnlyList<string> UsingNamespaceList { get; }


        /// <summary>
        /// 类型的命名空间
        /// </summary>
        public string Namespace { get; }

 
   
        /// <summary>
        /// 类型包含的属性
        /// </summary>
        /// <returns></returns>
        public ICompileProperty[] GetProperties() => throw new NotImplementedException();

        /// <summary>
        /// 类型的所有成员
        /// </summary>
        /// <returns></returns>
        public ICompileMethod[] GetMethods() => throw new NotImplementedException();

        /// <summary>
        /// 类型的字段
        /// </summary>
        /// <returns></returns>
        public ICompileField[] GetFields() => throw new NotImplementedException();
    }
}