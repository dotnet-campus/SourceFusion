using System;
using System.Diagnostics;

namespace dotnetCampus.Telescope
{
    [DebuggerDisplay("AttributedTypeMetadata<{RealType}, {Attribute}>")]
    public class AttributedTypeMetadata<TBaseClassOrInterface, TAttribute>
        where TAttribute : Attribute
    {
        private readonly Func<TBaseClassOrInterface> _instanceCreator;

        public AttributedTypeMetadata(Type realType, TAttribute attribute, Func<TBaseClassOrInterface> instanceCreator)
        {
            RealType = realType ?? throw new ArgumentNullException(nameof(realType));
            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            _instanceCreator = instanceCreator ?? throw new ArgumentNullException(nameof(instanceCreator));
        }

        public Type RealType { get; }

        public TAttribute Attribute { get; }

        public TBaseClassOrInterface CreateInstance() => _instanceCreator();
    }
}
