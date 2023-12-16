using MetaFarms.Libs.Generators.Templating;
using MetaFarms.Libs.Templating.Sample.Interfaces;

namespace MetaFarms.Libs.Templating.Sample;

[TypeMemberTemplate("IMethodOneImplementation.scriban")]
[TypeMemberTemplate("IMethodTwoImplementation.scriban")]
public partial class MethodTest 
    // : IHasMethodOne,
    //                               IHasMethodTwo<string>
{
}
