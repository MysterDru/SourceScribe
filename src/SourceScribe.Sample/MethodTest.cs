using SourceScribe;
using SourceScribe.Sample.Interfaces;

namespace SourceScribe.Sample;

[TypeMemberTemplate("IMethodOneImplementation.scriban")]
[TypeMemberTemplate("IMethodTwoImplementation.scriban")]
public partial class MethodTest 
    // : IHasMethodOne,
    //                               IHasMethodTwo<string>
{
}
