using System;
using System.Linq;
using System.Threading;
					
public class Program
{
	public static void Main()
	{
		Console.WriteLine("Starting C# pipeline.");
		
		// Arbitrary delay to test the portal's automatic status refreshing.
		foreach (var i in Enumerable.Range(1, 15).Reverse()) 
		{
			Console.WriteLine($"Sleeping for {i} seconds ...");
			Thread.Sleep(TimeSpan.FromSeconds(1));
		}
		
		Console.WriteLine("C# pipeline completed!");
	}
}