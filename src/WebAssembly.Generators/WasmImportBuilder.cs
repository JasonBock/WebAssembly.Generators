using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebAssembly.Generators.Extensions;

namespace WebAssembly.Generators
{
	internal static class WasmImportBuilder
	{
		internal static string GetImportClassName(string className) => $"{className}Imports";

		internal static string? Build(Module module, string className, string? @namespace)
		{
			if (module.Imports.Count > 0)
			{
				using var writer = new StringWriter();
				using var indentWriter = new IndentedTextWriter(writer, "\t");

				if (!string.IsNullOrWhiteSpace(@namespace))
				{
					indentWriter.WriteLine($"namespace {@namespace}");
					indentWriter.WriteLine("{");
					indentWriter.Indent++;
				}

				var importClassName = WasmImportBuilder.GetImportClassName(className);
				indentWriter.WriteLine($"public abstract class {importClassName}");
				indentWriter.WriteLine("{");
				WasmImportBuilder.BuildImportMethods(module, indentWriter);
				indentWriter.WriteLine("}");

				if (!string.IsNullOrWhiteSpace(@namespace))
				{
					indentWriter.Indent--;
					indentWriter.WriteLine("}");
				}

				return writer.ToString();
			}

			return null;
		}

		private static void BuildImportMethods(Module module, IndentedTextWriter writer)
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

			var imports = module.Imports.OfType<Import.Function>().ToList();

			for (var i = 0; i < imports.Count; i++)
			{
				var import = imports[i]!;
				var type = module.Types[(int)import.TypeIndex];
				var parameters = string.Join(", ", GetParameters(type.Parameters));
				writer.Indent++;
				writer.WriteLine($"public abstract {(type.Returns.Count > 0 ? type.Returns[0].GetCSharpName() : "void")} {import.Field}({parameters});");
				writer.Indent--;

				if (i < imports.Count - 1)
				{
					writer.WriteLine();
				}
			}
		}
	}
}