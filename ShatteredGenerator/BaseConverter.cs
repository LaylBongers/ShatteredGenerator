using System;
using System.Collections.Generic;

namespace ShatteredGenerator
{
	public static class BaseConverter
	{
		public static readonly char[] CountryTagBase;

		static BaseConverter()
		{
			var countryTagChars = new List<char>();
			for (var currentChar = 'A'; currentChar < 'Z'; currentChar++)
			{
				countryTagChars.Add(currentChar);
			}
			CountryTagBase = countryTagChars.ToArray();
		}

		public static string IntToString(int value, char[] baseChars)
		{
			// 32 is the worst case buffer size for base 2 and int.MaxValue
			var i = 32;
			var buffer = new char[i];
			var targetBase = baseChars.Length;

			do
			{
				buffer[--i] = baseChars[value%targetBase];
				value = value/targetBase;
			} while (value > 0);

			var result = new char[32 - i];
			Array.Copy(buffer, i, result, 0, 32 - i);

			return new string(result);
		}
	}
}