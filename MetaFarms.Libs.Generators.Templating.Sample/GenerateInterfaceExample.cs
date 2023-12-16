using System.Collections.Generic;
using System.Threading.Tasks;
using MetaFarms.Libs.Generators.Templating;

namespace MetaFarms.Libs.Templating.Sample;

[TypeMemberTemplate("InterfaceGenerator.scriban")]
public partial class GenerateInterfaceExample : IGenerateInterfaceExample
{
    private List<string> _foo = new (); 
    
    // public string X { get; init; }
    public string this[int i]
    {
        get => _foo[i];
        set => _foo[i] = value;
    }
    
    public string this[int i, string[] args]
    {
        get => _foo[i];
        set => _foo[i] = value;
    }
    
    public string Foo { get; set; }
    
    public int? Bar { get; set; }
    
    public object Fizz { get; }
    
    public string Buzz { private get; set; }
 
    public void DoSomething()
    {
    }

    public Task DoSomethingAsync()
    {
        return Task.CompletedTask;
    }
}
