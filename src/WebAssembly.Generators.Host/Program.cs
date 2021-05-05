using System;
using WebAssembly.Generators;
using WebAssembly.Runtime;

//var (exportText, importText) = WasmTypeBuilder.Build("collatz.wasm", "CollatzTest", "WATest");
var (exportText, importText) = WasmTypeBuilder.Build("collatzWithCallback.wasm", "CollatzWithCallbackTest", "WATest");

Console.Out.WriteLine("Export:");
Console.Out.Write(exportText.ToString());

Console.Out.WriteLine();

if (importText is not null)
{
	Console.Out.WriteLine("Import:");
	Console.Out.Write(importText.ToString());
}

public abstract class CollatzWithCallbackTest
{
	public abstract void collatz(int a0);

	public static CollatzWithCallbackTest Create(CollatzWithCallbackTestImports imports, string path = "collatzWithCallback.wasm") =>
		Compile.FromBinary<CollatzWithCallbackTest>("collatzWithCallback.wasm")(new ImportDictionary
		{
			{ "imports", "collatzCallback", new FunctionImport(new Action<int>(a0 => imports.collatzCallback(a0))) }
		}).Exports;
}

public abstract class CollatzWithCallbackTestImports
{
	public abstract void collatzCallback(int a0);
}