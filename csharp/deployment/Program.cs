using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
					
public class Program
{
	public static void Main()
	{
		Console.WriteLine("Starting C# pipeline.");
		var azureClientId = GetEnvironmentVariableOrFail("AZURE_CLIENT_ID");
		var azureSecret = GetEnvironmentVariableOrFail("AZURE_SECRET");
		var azureTenant= GetEnvironmentVariableOrFail("AZURE_TENANT");
		var azureSubscriptionId = GetEnvironmentVariableOrFail("AZURE_SUBSCRIPTION_ID");

		var projectCodeDirectory = "csharp/csharp_project";
		Console.WriteLine($"Changing to directory: {projectCodeDirectory}");
		Directory.SetCurrentDirectory(projectCodeDirectory);

		Console.WriteLine($"Current directory is: {Directory.GetCurrentDirectory()}");

		Console.WriteLine("Publishing Function App ...");
		RunCommand("func", "azure functionapp publish fluenci-csharp-demo  --dotnet-isolated --dotnet-version 8.0");
	}

	public static string GetEnvironmentVariableOrFail(string name)
	{
		var value = Environment.GetEnvironmentVariable(name);

		if (value is null)
			throw new Exception($"Missing environment variable: '{name}'.");

		return value;
	}

	public static void RunCommand(string command, string arguments)
	{
		var process = new Process 
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = command,
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};	

		process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine("Output: " + e.Data);
				Console.Out.Flush();
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.WriteLine("Error: " + e.Data);
				Console.Out.Flush();
            }
        };

		process.Start();

		// string result = process.StandardOutput.ReadToEnd();
		// string error = process.StandardError.ReadToEnd();

		// process.BeginOutputReadLine();
		// process.BeginErrorReadLine();

		process.WaitForExit();

		// Console.WriteLine("Output: \n" + result);
		// if (!string.IsNullOrWhiteSpace(error))
		// {
		// 	Console.WriteLine("Error: \n" + error);
		// }
	}
}
