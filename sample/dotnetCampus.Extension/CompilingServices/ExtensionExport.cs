using System.Collections.Generic;
using System.Linq;
using dotnetCampus.SourceFusion;

namespace dotnetCampus.CompilingServices
{
    [CompileTimeTemplate]
    public class ExtensionExport
    {
        public IReadOnlyList<IInteresting> Interestings = Placeholder.Array<IInteresting>(context =>
        {
            return new CompileCodeSnippet(@"new {0}(),
", context.Assembly.GetTypes()
                .Where(type => type.Attributes.Any(x => x.Match("Interesting")))
                .Select(x => x.FullName));
        });
    }
}
