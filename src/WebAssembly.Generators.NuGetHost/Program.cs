using System;
using WebAssembly.Runtime;

var collatz = Compile.FromBinary<CollatzTest>("collatz.wasm");
Console.Out.WriteLine(collatz(new ImportDictionary()).Exports.collatz(3));