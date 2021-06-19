# WebAssembly.Generators

A package to generate code based on the [WebAssembly](https://github.com/RyanLamansky/dotnet-webassembly) package.

## Installation

You can install it from NuGet [here](https://www.nuget.org/packages/WebAssembly.Generators/). 

## Overview

To use it, you add an `<AdditionalFile>` entry into your C# project file to reference a .wasm file:

```
<AdditionalFiles Include="collatz.wasm" ClassName="CollatzTest" Namespace="GeneratedCollatz" />
```

The `ClassName` and `Namespace` properties are optional. If you don't specify `ClassName`, the generator will use the name of the file as the name of the class that contains the exported functions (if imports exist, that class name will be appended with `Imports`). `Namespace` allows you to specify a namespace for the generated classes - if it's not provided, they will exist in the global namespace.

The exported class will also have a static `Create()` method that will create an instance for you:

```
using var collatz = CollatzTest.Create();
Console.Out.WriteLine(collatz.collatz(3));
```

If the module defined imports, `Create()` will require an object that implements the generated import class:

```
var callbacks = new CollatzImports();
using var collatzImports = CollatzWithCallbackTest.Create(callbacks);
collatzImports.collatz(7);
Console.Out.WriteLine(string.Join(", ", callbacks.Numbers));

public sealed class CollatzImports 
  : CollatzWithCallbackTestImports
{
  public override void collatzCallback(int a0) => this.Numbers.Add(a0);

  public List<int> Numbers { get; } = new();
}
```

## Project Layout

Here's a brief description of the projects. Note that this will probably change over time.

* `WebAssembly.Generators` - Defines the generator to create the export class (and import class if needed)
* `WebAssembly.Generators.Tests` - Defines tests for `WebAssembly.Generators`
* `WebAssembly.Generators.Host` - A console application to view the generated code
* `WebAssembly.Generators.NuGetHost` - A console application that references `WebAssembly.Generators`
