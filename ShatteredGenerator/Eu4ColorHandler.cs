//AFishyFez, 12/25/2014, Shattered Universalis: ShatteredGenerator
/*
 * This handles all functions and fields related to the Eu4ColorHandler.
 * The color handler itself basically holds a dictionary of all cultures,
 * and an associated color. We use this to make nation colors based off of
 * the state/province culture
 */

using System;
using System.Collections.Generic; //For the Dictionary data structure
using System.IO; //For reading in colors

namespace ShatteredGenerator
{
    internal sealed class Eu4ColorHandler
    {
        public const string COLOR_PATH = "./culture_colors.txt"; //The path to where all the colors are stored
        public const int RANDOM_OFFSET = 20; //When we make random colors, this is how much offset we applie to the color for each channel

        private Dictionary<string, Eu4Color> cultureColors; //Stores all the cultures and their respective colors
        private Random rooseBolton; //See GetRandomColor

        public Eu4ColorHandler()
        {
            cultureColors = new Dictionary<string, Eu4Color>();
            rooseBolton = new Random();
        }

        /// <summary>
        /// Loads all culture colors from culture_colors
        /// </summary>
        public void LoadColors()
        {
            StreamReader streamReader = new StreamReader(COLOR_PATH);
            string nextLine;
            int nextR, nextG, nextB;

            while(!streamReader.EndOfStream)
            {
                nextLine = streamReader.ReadLine();

                string[] values = nextLine.Split(new[] { ' ', '=', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
                nextR = Convert.ToInt32(values[1]);
                nextG = Convert.ToInt32(values[2]);
                nextB = Convert.ToInt32(values[3]);

                cultureColors.Add(values[0], new Eu4Color(nextR, nextG, nextB));
            }

            streamReader.Close();
        }

        /// <summary>
        /// Returns a culture's color, but with an offset applied to it for randomness
        /// </summary>
        /// <param name="culture">The culture who's color you'd like an offset of</param>
        /// <returns>A color with some randomness applied</returns>
        public Eu4Color GetRandomColor(string culture)
        {
            Eu4Color color = GetColor(culture);

            int r, g, b;
            r = color.Red + rooseBolton.Next(-RANDOM_OFFSET, RANDOM_OFFSET);
            r = r < 0 ? 0 : r;
            r = r > 255 ? 255 : r;

            g = color.Green + rooseBolton.Next(-RANDOM_OFFSET, RANDOM_OFFSET);
            g = g < 0 ? 0 : g;
            g = g > 255 ? 255 : g;

            b = color.Blue + rooseBolton.Next(-RANDOM_OFFSET, RANDOM_OFFSET);
            b = b < 0 ? 0 : b;
            b = b > 255 ? 255 : b;

            color.Red = (byte)r;
            color.Green = (byte)g;
            color.Blue = (byte)b;

            return color;
        }

        /// <summary>
        /// Gets a color from the list, based on a given culture. Does NOT return a random color, see GetRandomColor for that
        /// </summary>
        /// <param name="culture">The culture who's culture you'd like</param>
        /// <returns>A culture's color</returns>
        public Eu4Color GetColor(string culture)
        {
            Eu4Color color; //The color for the culture

            if (cultureColors.TryGetValue(culture, out color))
                return color;

            return new Eu4Color(255, 255, 255);
        }
    }
}
