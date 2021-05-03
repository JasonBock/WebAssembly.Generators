using Microsoft.CodeAnalysis;
using System.IO;

namespace WebAssembly.Generators
{
	[Generator]
	internal sealed class WasmTypeGenerator
		: ISourceGenerator
	{
		private const string ClassNameOption = "build_metadata.AdditionalFiles.ClassName";
		private const string NamespaceOption = "build_metadata.AdditionalFiles.Namespace";

		public void Execute(GeneratorExecutionContext context)
		{
			foreach (var additionalText in context.AdditionalFiles)
			{
				if (Path.GetExtension(additionalText.Path).ToLower() == ".wasm")
				{
					var options = context.AnalyzerConfigOptions.GetOptions(additionalText);
					options.TryGetValue(WasmTypeGenerator.ClassNameOption, out var className);
					options.TryGetValue(WasmTypeGenerator.NamespaceOption, out var @namespace);

					if (string.IsNullOrWhiteSpace(className))
					{
						className = Path.GetFileNameWithoutExtension(additionalText.Path);
					}

					var text = WasmTypeBuilder.Build(additionalText.Path, className!, @namespace);
					context.AddSource($"{className}.g.cs", text);
				}
			}
		}

		public void Initialize(GeneratorInitializationContext context) { }
	}
}