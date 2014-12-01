using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShatteredGenerator
{
	internal static class Program
	{
		private const string GameFolder = @"C:\Program Files (x86)\Steam\SteamApps\common\Europa Universalis IV";
		private const string OutputFolder = "./GeneratedUniversalis";
		private const string TemplateDataFolder = "./TemplateData";
		private static bool _verbose = false;

		private static void Main(string[] args)
		{
			// ==================================================
			Console.WriteLine("\n------ Setting Up ------");

			// Clear previous output if exists
			if (Directory.Exists(OutputFolder))
			{
				Console.WriteLine("Clearing previous output...");
				try
				{
					Directory.Delete(OutputFolder, true);
				}
				catch
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("\nUnable to delete previous output, close any programs using this directory.");
					return;
				}
			}

			var input = new DirectoryInfo(GameFolder);
			var output = new DirectoryInfo(OutputFolder);

			Console.WriteLine("Copying template directory...");
			CopyDirectory(TemplateDataFolder, OutputFolder);
			DeleteFileRecursive(OutputFolder, ".dummy");

			Console.WriteLine("Input:  " + input.FullName);
			Console.WriteLine("Output: " + output.FullName);

			// ==================================================
			Console.WriteLine("\n------ Loading Provinces ------");

			var inputProvinceDirectory = input
				.GetDirectories("history")[0]
				.GetDirectories("provinces")[0];
			Console.WriteLine("Provinces Folder: " + inputProvinceDirectory.FullName + "\n");

			Console.WriteLine("Loading provinces...");
			var provinceFiles = LoadProvinceFiles(inputProvinceDirectory).ToList();
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

			// ==================================================
			Console.WriteLine("\n------ Creating Output ------");

			Console.WriteLine("Writing province files...");
			var outputProvinceDirectory = output
				.GetDirectories("history")[0]
				.GetDirectories("provinces")[0];
			foreach (var provinceFile in provinceFiles)
			{
				File.WriteAllText(
					outputProvinceDirectory.FullName + "/" + provinceFile.Key,
					provinceFile.Value.Serialize());
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\nDone!");
			Console.ReadKey();
		}

		private static IEnumerable<KeyValuePair<string, Eu4Province>> LoadProvinceFiles(DirectoryInfo inputProvinceDirectory)
		{
			var provinces = inputProvinceDirectory.GetFiles()
				.Select(f => new KeyValuePair<string, Eu4Province>(
					f.Name,
					new Eu4Province(new Eu4FileData(File.ReadAllText(f.FullName)))));

			return provinces;
		}

		private static void CopyDirectory(string sourceDirName, string targetDirName)
		{
			// Get the subdirectories for the specified directory.
			var dir = new DirectoryInfo(sourceDirName);
			var dirs = dir.GetDirectories();

			// If the destination directory doesn't exist, create it. 
			if (!Directory.Exists(targetDirName))
			{
				Directory.CreateDirectory(targetDirName);
			}

			// Get the files in the directory and copy them to the new location.
			var files = dir.GetFiles();
			foreach (var file in files)
			{
				var temppath = Path.Combine(targetDirName, file.Name);
				file.CopyTo(temppath, false);
			}

			// For subdirectories, them and their contents to new location. 
			foreach (var subDir in dirs)
			{
				var temppath = Path.Combine(targetDirName, subDir.Name);
				CopyDirectory(subDir.FullName, temppath);
			}
		}

		private static void DeleteFileRecursive(string directory, string pattern)
		{
			// Get the subdirectories for the specified directory.
			var dir = new DirectoryInfo(directory);
			var dirs = dir.GetDirectories();


			// Get the files in the directory that match and delete them.
			var files = dir.GetFiles(pattern);
			foreach (var file in files)
			{
				file.Delete();
			}

			// For subdirectories, them and their contents to new location. 
			foreach (var subDir in dirs)
			{
				DeleteFileRecursive(subDir.FullName, pattern);
			}
		}
	}
}