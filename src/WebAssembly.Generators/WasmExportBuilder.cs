using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebAssembly.Generators.Extensions;

namespace WebAssembly.Generators
{
	internal static class WasmExportBuilder
	{
		internal static string Build(Module module, string className, string? @namespace, string path)
		{
			using var writer = new StringWriter();
			using var indentWriter = new IndentedTextWriter(writer, "\t");

			indentWriter.WriteLine("using System;");
			indentWriter.WriteLine("using WebAssembly;");
			indentWriter.WriteLine("using WebAssembly.Runtime;");
			indentWriter.WriteLine();
			indentWriter.WriteLine("#nullable enable");
			indentWriter.WriteLine();

			if (!string.IsNullOrWhiteSpace(@namespace))
			{
				indentWriter.WriteLine($"namespace {@namespace}");
				indentWriter.WriteLine("{");
				indentWriter.Indent++;
			}

			indentWriter.WriteLine($"public abstract class {className}");
			indentWriter.Indent++;
			indentWriter.WriteLine(": IDisposable");
			indentWriter.Indent--;
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine("private bool disposed;");
			indentWriter.WriteLine($"private Instance<{className}>? capturedInstance;");
			indentWriter.WriteLine();
			WasmExportBuilder.BuildExportMethods(module, indentWriter);
			indentWriter.WriteLine();
			WasmExportBuilder.BuildCreateMethod(module, indentWriter, className, path);
			indentWriter.WriteLine();
			WasmExportBuilder.BuildGlobalProperties(module, indentWriter);
			indentWriter.WriteLine();
			WasmExportBuilder.BuildDisposeMethod(indentWriter);
			indentWriter.Indent--;
			indentWriter.WriteLine("}");

			if (!string.IsNullOrWhiteSpace(@namespace))
			{
				indentWriter.Indent--;
				indentWriter.WriteLine("}");
			}

			return writer.ToString();
		}

		private static void BuildDisposeMethod(IndentedTextWriter indentWriter)
		{
			indentWriter.WriteLine("public void Dispose()");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine("if (!this.disposed)");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine("this.disposed = true;");
			indentWriter.WriteLine("this.capturedInstance?.Dispose();");
			indentWriter.Indent--;
			indentWriter.WriteLine("}");
			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildCreateMethod(Module module, IndentedTextWriter writer, string className, string path)
		{
			var imports = module.Imports.OfType<Import.Function>().ToList();

			if (imports.Count == 0)
			{
				writer.WriteLine($"public static {className} Create(string path = @\"{path}\")");
				writer.WriteLine("{");
				writer.Indent++;
				writer.WriteLine($"var instance = Compile.FromBinary<{className}>(@\"{path}\")(new ImportDictionary());");
				writer.WriteLine("var exports = instance.Exports;");
				writer.WriteLine("exports.capturedInstance = instance;");
				writer.WriteLine("return exports;");
				writer.Indent--;
				writer.WriteLine("}");
			}
			else
			{
				var importClassName = WasmImportBuilder.GetImportClassName(className);
				writer.WriteLine($"public static {className} Create({importClassName} imports, string path = @\"{path}\")");
				writer.WriteLine("{");
				writer.Indent++;
				writer.WriteLine($"var instance = Compile.FromBinary<{className}>(@\"{path}\")(new ImportDictionary");
				writer.WriteLine("{");
				writer.Indent++;

				foreach(var import in imports)
				{
					var importType = module.Types[(int)import.TypeIndex];
					writer.WriteLine($"{{ \"{import.Module}\", \"{import.Field}\", {importType.GetFunctionImportCreationCode(import.Field)} }}");
				}

				writer.Indent--;
				writer.WriteLine("});");
				writer.WriteLine("var exports = instance.Exports;");
				writer.WriteLine("exports.capturedInstance = instance;");
				writer.WriteLine("return exports;");
				writer.Indent--;
				writer.WriteLine("}");
			}
		}
		private static void BuildGlobalProperties(Module module, IndentedTextWriter writer)
		{
			var exports = module.Exports.Where(_ => _.Kind == ExternalKind.Global).ToList();

			foreach(var export in exports)
			{
				var global = module.Globals[(int)export.Index];
				var accessors = $"get; {(global.IsMutable ? "set;" : string.Empty)}";
				writer.WriteLine($"public abstract {global.ContentType.GetCSharpName()} {export.Name} {{ {accessors} }}");
			}
		}

		private static void BuildExportMethods(Module module, IndentedTextWriter writer)
		{
			static IEnumerable<string> GetParameters(IList<WebAssemblyValueType> parameters)
			{
				var count = 0;

				foreach (var parameter in parameters)
				{
					yield return $"{parameter.GetCSharpName()} a{count}";
					count++;
				}
			}

			var exports = module.Exports.Where(_ => _.Kind == ExternalKind.Function).ToList();

			for (var i = 0; i < exports.Count; i++)
			{
				var export = exports[i];
				var function = module.Functions[(int)export.Index - module.Imports.Count];
				var type = module.Types[(int)function.Type];
				var parameters = string.Join(", ", GetParameters(type.Parameters));
				writer.WriteLine($"public abstract {(type.Returns.Count > 0 ? type.Returns[0].GetCSharpName() : "void")} {export.Name}({parameters});");

				if (i < exports.Count - 1)
				{
					writer.WriteLine();
				}
			}
		}
	}
}