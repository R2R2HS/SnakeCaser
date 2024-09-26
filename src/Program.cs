using dnlib.DotNet;
using Newtonsoft.Json.Serialization;

internal static class Program
{
	private const string TEST_ASSEMBLY_PATH = "test.dll";
	private const char DEFAULT_SEPARATOR = '.';

	private static void Main(string[] args)
	{
		string assemblyPath = args.Length == 0 ? TEST_ASSEMBLY_PATH : args[0];
		string snakedAssemblyPath = Path.Combine(Path.GetDirectoryName(assemblyPath), $"{GetSnakedString(Path.GetFileNameWithoutExtension(assemblyPath), DEFAULT_SEPARATOR)}_snaked.dll");

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
		module.Name = GetSnakedString(module.Name, DEFAULT_SEPARATOR);

		foreach (var type in module.Types)
		{
			if (IsInBlackListedNamespace(type))
			{
				continue;
			}

			if (HasBlackListedAttribute(type))
			{
				continue;
			}

			type.Name = GetSnakedString(type.Name);
			type.Namespace = GetSnakedString(type.Namespace, DEFAULT_SEPARATOR);

			foreach (var parameter in type.GenericParameters.Where(DoesNotHaveBlackListedAttribute))
			{
				parameter.Name = GetSnakedString(parameter.Name);
			}

			foreach (var method in type.Methods.Where(DoesNotHaveBlackListedAttribute))
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

			foreach (var field in type.Fields.Where(DoesNotHaveBlackListedAttribute))
			{
				field.Name = GetSnakedString(field.Name);
			}

			foreach (var property in type.Properties.Where(DoesNotHaveBlackListedAttribute))
			{
				property.Name = GetSnakedString(property.Name);
			}

			foreach (var evt in type.Events.Where(DoesNotHaveBlackListedAttribute))
			{
				evt.Name = GetSnakedString(evt.Name);
			}
		}

		return module;
	}

	private static bool IsInBlackListedNamespace(TypeDef type)
	{
		return type.Namespace == "System.Runtime.CompilerServices" || type.Namespace == "Microsoft.CodeAnalysis";
	}

	private static bool DoesNotHaveBlackListedAttribute(IHasCustomAttribute hasCustomAttribute)
	{
		return !HasBlackListedAttribute(hasCustomAttribute);
	}

	private static bool HasBlackListedAttribute(IHasCustomAttribute hasCustomAttribute)
	{
		return hasCustomAttribute.CustomAttributes.Find("System.Runtime.CompilerServices.CompilerGeneratedAttribute") != null;
	}

	private static string GetSnakedString(string input, char separator)
	{
		if (input.Contains(separator))
		{
			return string.Join(".", input.ToString().Split(separator).Select(GetSnakedString));
		}
		else
		{
			return GetSnakedString(input);
		}
	}

	private static string GetSnakedString(string input)
	{
		return new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }.GetResolvedPropertyName(input);
	}
}