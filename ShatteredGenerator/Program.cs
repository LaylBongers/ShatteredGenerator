using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ShatteredGenerator
{
	internal static class Program
	{
		private const string GameFolder = @"C:\Program Files (x86)\Steam\SteamApps\common\Europa Universalis IV";
		private const string OutputFolder = "./GeneratedUniversalis";
		private const string TemplateDataFolder = "./TemplateData";
		private static Encoding _encoding;

		private static void Main(string[] args)
		{
			// Western (Windows 1252)       
			_encoding = Encoding.GetEncoding("Windows-1252");

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
			var provinceFiles = LoadEu4Data(inputProvinceDirectory, d => new Eu4Province(d)).ToList();

			// ==================================================
			Console.WriteLine("\n------ Loading Countries ------");

			Console.WriteLine("Loading countries...");
			var inputCountryDirectory = input
				.GetDirectories("common")[0]
				.GetDirectories("countries")[0];
			var inputCountryFiles = LoadEu4Data(inputCountryDirectory, d => new Eu4Country(d)).ToList();

			Console.WriteLine("Loading country histories...");
			var inputCountryHistoryDirectory = input
				.GetDirectories("history")[0]
				.GetDirectories("countries")[0];
			var inputCountryHistoryFiles = LoadEu4Data(inputCountryHistoryDirectory, d => new Eu4CountryHistory(d)).ToList();

			Console.WriteLine("Loading country tags...");
			var inputCountryTagFile = input
				.GetDirectories("common")[0]
				.GetDirectories("country_tags")[0]
				.GetFiles("00_countries.txt")[0];
			var countryTags = Eu4DataConvert.Deserialize(File.ReadAllText(inputCountryTagFile.FullName));

			// ==================================================
			Console.WriteLine("\n------ Processing Data ------");

			Console.WriteLine("Processing provinces...");

			var clearCount = provinceFiles.Select(provinceFile => provinceFile.Value).Sum(province => province.ClearHistory());
			Console.WriteLine("Cleared {0} History Entries", clearCount);

			// ==================================================
			Console.WriteLine("\n------ Generating ------");

			Console.WriteLine("Generating new countries...");

			//var random = new Random();
			var colorHandler = new Eu4ColorHandler();
			//Must call this!
			colorHandler.LoadColors();

			var outputCountryFiles = new List<KeyValuePair<string, Eu4Country>>();
			var outputCountryHistoryFiles = new List<KeyValuePair<string, Eu4Country>>();
			var tagCountryDictionary = new Dictionary<string, Eu4Country>();
			var countryGenerationCount = 0;
			var countryTagNumber = 0;

			foreach (var provinceFile in provinceFiles
				// We only want to generate for countries with owners
				.Where(f => f.Value.Owner != null))
			{
				// Look up the country this province already has
				var oldCountryFilename = countryTags.One(provinceFile.Value.Owner).Split('/').Last();
				var oldCountry = inputCountryFiles.First(f => f.Key == oldCountryFilename).Value;
				var oldCountryHistory = inputCountryHistoryFiles.First(f => f.Key.StartsWith(provinceFile.Value.Owner)).Value;

				// Get the name of the province for later usage
				// TODO: Retrieve this from the EU4 localization file instead of from the file name?
				var provinceFileNameSplitted = provinceFile.Key.Split(new[] {'-', ' '}, StringSplitOptions.RemoveEmptyEntries);
				var provinceFileNameLocal = provinceFileNameSplitted.Last();
				var provinceName = provinceFileNameLocal.Substring(0, provinceFileNameLocal.Length - ".txt".Length);

				//Fix bad HRE status
				provinceFile.Value.RemoveBadHreStatus();

				// Clone the country and its history
				var newCountry = oldCountry.Clone();
				newCountry.ClearHistory();
				var newCountryHistory = oldCountryHistory.Clone();
				newCountryHistory.ClearHistory();

				//Make the culture and religion match the province
				if (provinceFile.Value.Culture != null)
					newCountryHistory.Culture = provinceFile.Value.Culture;

				if (provinceFile.Value.Religion != null)
					newCountryHistory.Religion = provinceFile.Value.Religion;

				// Give our new country a new shiny flag
				newCountry.Color = colorHandler.GetRandomColor(newCountryHistory.Culture);

				// Set the province # as capital
				var provinceNumber = int.Parse(provinceFileNameSplitted.First());
				newCountryHistory.Capital = provinceNumber;

				// Look up a free tag for the country and history
				string tag;
				while (true)
				{
					var testTag = BaseConverter.IntToString(countryTagNumber, BaseConverter.CountryTagBase);

					if (testTag.Length == 1)
						testTag = "AA" + testTag;
					if (testTag.Length == 2)
						testTag = "A" + testTag;

					// For some reason AUX and CON are special cases?
					// At any rate if we try to copy flags for that it causes issues.
					// Also blacklist AND because it will mess up any use of the word "AND".
					if (testTag == "AUX" || testTag == "CON" || testTag == "AND")
					{
						countryTagNumber++;
						continue;
					}

					if (countryTags.One(testTag) == null)
					{
						tag = testTag;
						break;
					}
					countryTagNumber++;
				}
				countryTagNumber++;
				tagCountryDictionary.Add(tag, newCountry);

				// Give the province that new tag as an owner
				provinceFile.Value.Owner = tag;

				// Give our new country a core on the province
				provinceFile.Value.AddCore(tag);

				// Generate filenames for the country and history and write them to the output lists
				var countryFilename = provinceName + ".txt";
				var countryHistoryFilename = tag + " - " + provinceName + ".txt";
				outputCountryFiles.Add(new KeyValuePair<string, Eu4Country>(countryFilename, newCountry));
				outputCountryHistoryFiles.Add(new KeyValuePair<string, Eu4Country>(countryHistoryFilename, newCountryHistory));

				// Write the new tag-file mapping to the output country-tag file
				var tagMappingFilename = "countries/" + countryFilename;
				countryTags.Set(tag, tagMappingFilename);

				// TODO: Write the name of the region to the localization file.

				countryGenerationCount++;
			}
			Console.WriteLine("Generated {0} New Countries", countryGenerationCount);

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
					provinceFile.Value.Serialize(), _encoding);
			}

			Console.WriteLine("Writing country files...");
			var outputCountryDirectory = output
				.GetDirectories("common")[0]
				.GetDirectories("countries")[0];
			foreach (var countryFile in outputCountryFiles)
			{
				File.WriteAllText(
					outputCountryDirectory.FullName + "/" + countryFile.Key,
					countryFile.Value.Serialize(), _encoding);
			}

			Console.WriteLine("Writing country history files...");
			var outputCountryHistoryDirectory = output
				.GetDirectories("history")[0]
				.GetDirectories("countries")[0];
			foreach (var countryHistoryFile in outputCountryHistoryFiles)
			{
				File.WriteAllText(
					outputCountryHistoryDirectory.FullName + "/" + countryHistoryFile.Key,
					countryHistoryFile.Value.Serialize(), _encoding);
			}

			Console.WriteLine("Writing country tags...");
			var outputCountryTagDirectory = output
				.GetDirectories("common")[0]
				.GetDirectories("country_tags")[0];
			File.WriteAllText(outputCountryTagDirectory.FullName + "/00_countries.txt", countryTags.Serialize(), _encoding);

			Console.WriteLine("Creating flags for new tags...");
			var outputFlagsDirectory = output
				.GetDirectories("gfx")[0]
				.GetDirectories("flags")[0];
			foreach (var tag in provinceFiles.Where(f => f.Value.Owner != null).Select(f => f.Value.Owner))
			{
				var outputFlag = outputFlagsDirectory.FullName + "\\" + tag + ".tga";
				FlagGenerator.Generate(outputFlag, tagCountryDictionary[tag].Color);
			}

			// ==================================================
			Console.WriteLine("\n------ Creating Localisation ------");

			Console.WriteLine("Loading original localisation file...");
			var inputLocalizationFile = input
				.GetDirectories("localisation")[0]
				.GetFiles("countries_l_english.yml")[0];
			var localizeText = new StringBuilder(File.ReadAllText(inputLocalizationFile.FullName));

			Console.WriteLine("Creating new localisation file...");
			foreach (var provinceFile in provinceFiles.Where(f => f.Value.Owner != null))
			{
				var provinceFileNameSplitted = provinceFile.Key.Split(new[] {'-', ' '}, StringSplitOptions.RemoveEmptyEntries);
				var provinceFileNameLocal = provinceFileNameSplitted.Last();
				var provinceName = provinceFileNameLocal.Substring(0, provinceFileNameLocal.Length - ".txt".Length);

				// | AMG: "Armagnac"|
				localizeText.Append(" ");
				localizeText.Append(provinceFile.Value.Owner);
				localizeText.Append(": \"");
				localizeText.Append(provinceName);
				localizeText.AppendLine("\"");

				// | AMG_ADJ: "Armagnac"|
				localizeText.Append(" ");
				localizeText.Append(provinceFile.Value.Owner);
				localizeText.Append("_ADJ: \"");
				localizeText.Append(provinceName);
				localizeText.AppendLine("\"");
			}

			// Actually write it to the file (in UTF-8, different from what the other files use)
			var outputLocalizationDirectory = output
				.GetDirectories("localisation")[0];
			File.WriteAllText(outputLocalizationDirectory.FullName + "/countries_l_english.yml", localizeText.ToString());

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\nDone!");
			Console.ReadKey();
		}

		private static IEnumerable<KeyValuePair<string, TOut>> LoadEu4Data<TOut>(DirectoryInfo inputDirectory,
			Func<Eu4Data, TOut> converter)
		{
			var provinces = inputDirectory.GetFiles()
				.Select(f => new KeyValuePair<string, TOut>(
					f.Name,
					converter(Eu4DataConvert.Deserialize(File.ReadAllText(f.FullName, _encoding)))));

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