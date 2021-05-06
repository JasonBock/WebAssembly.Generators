using System;
using System.Collections.Generic;
using GeneratedCollatz;
using WebAssembly.Runtime;

var collatz = CollatzTest.Create();
Console.Out.WriteLine(collatz.collatz(3));

var callbacks = new CollatzImports();
var collatzImports = CollatzWithCallbackTest.Create(callbacks);
collatzImports.collatz(7);
Console.Out.WriteLine(string.Join(", ", callbacks.Numbers));

using var q = Compile.FromBinary<CollatzTest>(@"collatz.wasm")(new ImportDictionary());

public sealed class CollatzImports : CollatzWithCallbackTestImports
{
	public override void collatzCallback(int a0) => this.Numbers.Add(a0);

	public List<int> Numbers { get; } = new();
}