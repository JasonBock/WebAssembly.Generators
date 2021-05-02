using Microsoft.CodeAnalysis;
using System.IO;

namespace WebAssembly.Generators
{
	[Generator]
	internal sealed class WasmTypeGenerator
		: ISourceGenerator
	{
		private const string ClassNameOption = "build_metadata.AdditionalFiles.ClassName";

		public void Execute(GeneratorExecutionContext context)
		{
			foreach (var additionalText in context.AdditionalFiles)
			{
				if (Path.GetExtension(additionalText.Path).ToLower() == ".wasm")
				{
					context.AnalyzerConfigOptions.GetOptions(additionalText)
						.TryGetValue(WasmTypeGenerator.ClassNameOption, out var className);

					if (string.IsNullOrWhiteSpace(className))
					{
						className = Path.GetFileNameWithoutExtension(additionalText.Path);
					}

					var text = WasmTypeBuilder.Build(additionalText.Path, className!);
					context.AddSource($"{className}.g.cs", text);
				}
			}
		}

		public void Initialize(GeneratorInitializationContext context) { }
	}
}