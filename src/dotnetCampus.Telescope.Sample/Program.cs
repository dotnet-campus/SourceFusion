using System;

namespace dotnetCampus.Telescope.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var attributedTypes = AttributedTypes.FromAssembly<IDemo, DemoAttribute>(typeof(Program).Assembly);
            foreach (var attributedType in attributedTypes)
            {
                var type = attributedType.RealType;
                var attribute = attributedType.Attribute;
                var instance = attributedType.CreateInstance();
                Console.WriteLine($"Type = {type.FullName}, Attribute = {attribute.Value}, Instance = {instance.GetHashCode()}");
            }
        }
    }
}
