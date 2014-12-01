using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShatteredGenerator
{
	internal static class Program
	{
		private const string GameFolder = @"c:\Program Files (x86)\Steam\SteamApps\common\Europa Universalis IV";
		private const string OutputFolder = "./GeneratedUniversalis";
		private static bool _verbose = false;

		private static void Main(string[] args)
		{
			// ==================================================
			Console.WriteLine("\n------ Setting Up ------");

			// Clear previous output if exists
			if (Directory.Exists(OutputFolder))
			{
				Console.WriteLine("Clearing previous output...");
				Directory.Delete(OutputFolder);
			}

			var input = new DirectoryInfo(GameFolder);
			var output = Directory.CreateDirectory(OutputFolder);

			Console.WriteLine("Input:  " + input.FullName);
			Console.WriteLine("Output: " + output.FullName);

			// ==================================================
			Console.WriteLine("\n------ Loading Provinces ------");

			var provinceFiles = LoadProvinceFiles(input);
			var provincesRequiringCountry = new List<Eu4Province>();
			var clearCount = 0;
			var ownerCount = 0;

			foreach (var provinceFile in provinceFiles)
			{
				var province = provinceFile.Value;

				if (_verbose)
				{
					Console.Write(provinceFile.Key);

					var culture = province.Culture;

					if (culture == null)
					{
						Console.WriteLine(" with no Culture");
					}
					else
					{
						Console.WriteLine(" with Culture \"" + culture + "\"");
					}
				}

				// Clear the history so it doesn't mess with stuff
				clearCount += province.ClearHistory();

				// If this province has an owner, we will need to generate a new country for it
				if (province.Owner != null)
				{
					provincesRequiringCountry.Add(province);
					ownerCount++;
				}
			}

			Console.WriteLine("Cleared {0} History Entries", clearCount);
			Console.WriteLine("Requested {0} New Countries", ownerCount);

			Console.ReadKey();
		}

		private static IEnumerable<KeyValuePair<string, Eu4Province>> LoadProvinceFiles(DirectoryInfo input)
		{
			var inputRegions = input
				.GetDirectories("history")[0]
				.GetDirectories("provinces")[0];
			Console.WriteLine("Provinces Folder: " + inputRegions.FullName + "\n");

			var provinces = inputRegions.GetFiles()
				.Select(f => new KeyValuePair<string, Eu4Province>(
					f.Name,
					new Eu4Province(new Eu4FileData(File.ReadAllText(f.FullName)))));

			return provinces;
		}
	}
}