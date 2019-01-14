using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace dotnetCampus.SourceFusion
{
    public class AttributedTypeMetadataCollection<T, TAttribute> : Collection<AttributedTypeMetadata<T, TAttribute>>
        where TAttribute : Attribute
    {
        public static implicit operator List<(Type, TAttribute)>(
            AttributedTypeMetadataCollection<T, TAttribute> collection)
        {
            return collection.Select(x => (x.Type, x.Attribute)).ToList();
        }
    }

    public class AttributedTypeMetadata<T, TAttribute> where TAttribute : Attribute
    {
        internal AttributedTypeMetadata(Type type, Func<T> creator, TAttribute attribute)
        {
            Type = type;
            Creator = creator;
            Attribute = attribute;
        }

        public Type Type { get; }
        public Func<T> Creator { get; }
        public TAttribute Attribute { get; }
    }
}
