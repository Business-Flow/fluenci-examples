using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
					
public class Program
{
	public static void Main()
	{
		Console.WriteLine("Starting C# pipeline.");
		var AZURE_CLIENT_ID = GetEnvironmentVariableOrFail("AZURE_CLIENT_ID");
		var AZURE_SECRET = GetEnvironmentVariableOrFail("AZURE_SECRET");
		var AZURE_TENANT= GetEnvironmentVariableOrFail("AZURE_TENANT");
		var AZURE_SUBSCRIPTION_ID = GetEnvironmentVariableOrFail("AZURE_SUBSCRIPTION_ID");

		var projectCodeDirectory = "csharp/csharp_project";
		Console.WriteLine($"Changing to directory: {projectCodeDirectory}");
		Directory.SetCurrentDirectory(projectCodeDirectory);

		Console.WriteLine($"Current directory is: {Directory.GetCurrentDirectory()}");

		Console.WriteLine("Logging in to Azure.");
		RunCommand("az", $"login --service-principal -u {AZURE_CLIENT_ID} -p {AZURE_SECRET} --tenant {AZURE_TENANT}");

		Console.WriteLine("Setting Azure Subscription.");
		RunCommand("az", $"account set --subscription {AZURE_SUBSCRIPTION_ID}");

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
				RedirectStandardOutput = false,
				RedirectStandardError = false,
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
		process.WaitForExit();

		if (process.ExitCode != 0)
		{
			Console.WriteLine($"Command '{command}' failed with exit code {process.ExitCode}.  Terminating.");
			Environment.Exit(process.ExitCode)
		}
	}
}
