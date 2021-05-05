using System;
using WebAssembly.Generators;

//var (exportText, importText) = WasmTypeBuilder.Build("collatz.wasm", "CollatzTest", "WATest");
var (exportText, importText) = WasmTypeBuilder.Build("collatzWithCallback.wasm", "CollatzWithCallbackTest", "WATest");

Console.Out.WriteLine("Export:");
Console.Out.Write(exportText.ToString());

Console.Out.WriteLine();

if(importText is not null)
{
	Console.Out.WriteLine("Import:");
	Console.Out.Write(importText.ToString());
}