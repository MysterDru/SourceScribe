using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetaFarms.Libs.Generators.Templating;

namespace MetaFarms.Libs.Templating.Sample;

public delegate void E(object sender, object args);

/// <summary>
/// Class that demonstrates the generation of an interface
/// </summary>
[TypeMemberTemplate("InterfaceGenerator.scriban")]
public partial class GenerateInterfaceExample : IGenerateInterfaceExample
{
    private List<string> _foo = new ();

    public event E EventExample;
    
    /// <summary>
    /// Indexer with single arg
    /// </summary>
    /// <param name="i"></param>
    public string this[int i]
    {
        get => _foo[i];
        set => _foo[i] = value;
    }
    
    /// <summary>
    /// indexer with multiple args
    /// </summary>
    /// <param name="i"></param>
    /// <param name="args"></param>
    public string this[int i, string[] args]
    {
        get => _foo[i];
        set => _foo[i] = value;
    }
    
    /// <summary>
    /// String property
    /// </summary>
    public string Foo { get; set; }
    
    /// <summary>
    /// Get and set, <see cref="Nullable{T}"/>
    /// </summary>
    public int? Bar { get; set; }
    
    /// <summary>
    /// Getter only
    /// </summary>
    public object Fizz { get; }
    
    /// <summary>
    /// Private getter, public setter
    /// </summary>
    public string Buzz { private get; set; }
 
    /// <summary>
    /// so something, no params
    /// </summary>
    public void DoSomething()
    {
    }

    /// <summary>
    /// Do something async
    /// </summary>
    /// <returns><see cref="Task{TResult}"/></returns>
    public Task DoSomethingAsync()
    {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Do something
    /// </summary>
    /// <param name="foo"></param>
    /// <returns></returns>
    public Task<string> DoSomethingAsync(string foo)
    {
        return Task.FromResult(foo);
    }
}
