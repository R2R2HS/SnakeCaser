using dnlib.DotNet;
using Newtonsoft.Json.Serialization;

internal class Program
{
	private const string TEST_ASSEMBLY_PATH = "test.dll";

	private static void Main(string[] args)
	{
		string assemblyPath = args.Length == 0 ? TEST_ASSEMBLY_PATH : args[0];
		string snakedAssemblyPath = Path.Combine(Path.GetDirectoryName(assemblyPath), $"{Path.GetFileNameWithoutExtension(assemblyPath)}_snaked.dll");

		try
		{
			RenameElements(ModuleDefMD.Load(assemblyPath)).Write(snakedAssemblyPath);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}

		Console.WriteLine($"Assembly Modded: {snakedAssemblyPath}");
		Console.ReadLine();
	}

	private static ModuleDefMD RenameElements(ModuleDefMD module)
	{
		module.Name = GetSnakedString(module.Name, '.');

		foreach (var type in module.Types)
		{
			if (type.Namespace == "System.Runtime.CompilerServices" || type.Namespace == "Microsoft.CodeAnalysis")
			{
				continue;
			}

			type.Name = GetSnakedString(type.Name);
			type.Namespace = GetSnakedString(type.Namespace, '.');

			foreach (var parameter in type.GenericParameters)
			{
				parameter.Name = GetSnakedString(parameter.Name);
			}

			foreach (var method in type.Methods)
			{
				method.Name = GetSnakedString(method.Name);

                foreach (var parameter in method.Parameters)
                {
					parameter.Name = GetSnakedString(parameter.Name);
				}

				foreach (var parameter in method.GenericParameters)
				{
					parameter.Name = GetSnakedString(parameter.Name);
				}
			}

			foreach (var field in type.Fields)
			{
				field.Name = GetSnakedString(field.Name);
			}

			foreach (var property in type.Properties)
			{
				property.Name = GetSnakedString(property.Name);
			}

			foreach (var evt in type.Events)
			{
				evt.Name = GetSnakedString(evt.Name);
			}
		}

		return module;
	}

	private static string GetSnakedString(string input, char separator)
	{
		return string.Join(".", input.ToString().Split(separator).Select(GetSnakedString));
	}

	private static string GetSnakedString(string input)
	{
		return new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }.GetResolvedPropertyName(input);
	}
}