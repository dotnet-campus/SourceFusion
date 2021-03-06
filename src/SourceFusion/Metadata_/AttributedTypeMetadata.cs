﻿using System;

namespace dotnetCampus.SourceFusion
{
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
