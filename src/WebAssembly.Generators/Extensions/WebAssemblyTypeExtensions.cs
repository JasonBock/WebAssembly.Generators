using System.Collections.Generic;
using System.Linq;

namespace WebAssembly.Generators.Extensions
{
	internal static class WebAssemblyTypeExtensions
	{
		internal static string GetFunctionImportCreationCode(this WebAssemblyType self, string name)
		{
			// Something like this: new FunctionImport(new Action<int>(imports.{name}))
			var delegateType = self.Returns.Count > 0 ? "Func" : "Action";
			var genericParameterValues = new List<string>(self.Parameters.Count + self.Returns.Count);
			genericParameterValues.AddRange(self.Parameters.Select(_ => _.GetCSharpName()));

			if(self.Returns.Count > 0)
			{
				genericParameterValues.Add(self.Returns[0].GetCSharpName());
			}

			var genericParameters = genericParameterValues.Count > 0 ? $"<{string.Join(", ", genericParameterValues)}>" : string.Empty;

			return $"new FunctionImport(new {delegateType}{genericParameters}(imports.{name}))";
		}
	}
}