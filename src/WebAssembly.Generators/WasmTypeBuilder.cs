using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace WebAssembly.Generators
{
	internal static class WasmTypeBuilder
	{
		internal static (SourceText export, SourceText? import) Build(string wasmPath, string className, string? @namespace = null)
		{
			var module = Module.ReadFromBinary(wasmPath);

			var importText = WasmImportBuilder.Build(module, className, @namespace);
			var exportText = WasmExportBuilder.Build(module, className, @namespace, wasmPath);

			return (SourceText.From(exportText, Encoding.UTF8),
				importText is not null ? SourceText.From(importText, Encoding.UTF8) : null);
		}
	}
}