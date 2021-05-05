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

			if (!string.IsNullOrWhiteSpace(@namespace))
			{
				indentWriter.WriteLine($"namespace {@namespace}");
				indentWriter.WriteLine("{");
				indentWriter.Indent++;
			}

			indentWriter.WriteLine($"public abstract class {className}");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			WasmExportBuilder.BuildExportMethods(module, indentWriter);
			indentWriter.WriteLine();
			WasmExportBuilder.BuildCreateMethod(module, indentWriter, className, path);
			indentWriter.Indent--;
			indentWriter.WriteLine("}");

			if (!string.IsNullOrWhiteSpace(@namespace))
			{
				indentWriter.Indent--;
				indentWriter.WriteLine("}");
			}

			return writer.ToString();
		}

		private static void BuildCreateMethod(Module module, IndentedTextWriter writer, string className, string path)
		{
			var imports = module.Imports.OfType<Import.Function>().ToList();

			if (imports.Count == 0)
			{
				writer.WriteLine($"public static {className} Create(string path = \"{path}\") =>");
				writer.Indent++;
				writer.WriteLine($"Compile.FromBinary<{className}>(\"{path}\")(new ImportDictionary()).Exports;");
				writer.Indent--;
			}
			else
			{
				var importClassName = WasmImportBuilder.GetImportClassName(className);
				writer.WriteLine($"public static {className} Create({importClassName} imports, string path = \"{path}\") =>");
				writer.Indent++;
				writer.WriteLine($"Compile.FromBinary<{className}>(\"{path}\")(new ImportDictionary");
				writer.WriteLine("{");
				writer.Indent++;

				foreach(var import in imports)
				{
					writer.WriteLine($"{{ \"{import.Module}\", \"{import.Field}\", new FunctionImport(callbacks.{import.Field})) }}");
				}

				writer.Indent--;
				writer.WriteLine("}).Exports;");
				writer.Indent--;
			}
		}

		private static void BuildExportMethods(Module module, IndentedTextWriter writer)
		{
			// TODO: May want to include these:
			// https://github.com/RyanLamansky/dotnet-webassembly/blob/main/Examples/GenerateClassFromWasm/Program.cs#L53
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