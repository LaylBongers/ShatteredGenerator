using System.IO;
using ImageMagick;

namespace ShatteredGenerator
{
	public static class FlagGenerator
	{
		public static void Generate(string filePath, Eu4Color color)
		{
			using (var image = new MagickImage(new MagickColor(color.Red, color.Green, color.Blue), 128, 128))
			{
				var file = new FileInfo(filePath);
				var stream = file.Exists
					? file.OpenWrite()
					: file.Create();

				image.Write(stream, MagickFormat.Tga);

				stream.Close();
			}
		}
	}
}