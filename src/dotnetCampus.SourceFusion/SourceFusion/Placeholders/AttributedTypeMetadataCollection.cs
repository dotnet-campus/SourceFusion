using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace dotnetCampus.SourceFusion.Placeholders
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
}
