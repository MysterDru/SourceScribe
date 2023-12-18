# CodeModel

The files in this folder are a fork of the [NTypewriter CodeModel](https://github.com/NeVeSpl/NTypewriter/blob/master/Documentation/CodeModel.md). Originally, the intention was to use the [NTypewriter.CodeModel nuget package](https://www.nuget.org/packages/NTypewriter.CodeModel) however, the implementation provided was missing key features for rendering templates properly. As a result, the code was forked and modified to meet the needs of the MetaFarms.Libs.Generators.Templating project.

The included files provide an abstraction of the Roslyn Symbol classes and provide a way for consuming templates to interact with the symantic code model in a way that is similar to using the System.Reflection namespace and types.

Various objects from the code model will be passed to the templates as a parameter and allow the template to render based on current state of the compilation.