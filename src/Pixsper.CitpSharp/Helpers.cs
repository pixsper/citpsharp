using System;

namespace Pixsper.CitpSharp
{
	internal static class DateTimeHelpers
	{
		public static DateTime ConvertFromUnixTimestamp(ulong timestamp)
		{
			var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return origin.AddSeconds(timestamp);
		}

		public static ulong ConvertToUnixTimestamp(DateTime date)
		{
			var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			var diff = date.ToUniversalTime() - origin;
			return (ulong)Math.Floor(diff.TotalSeconds);
		}
	}



	internal static class ArrayHelpers
	{
		public static byte[][] Split(this byte[] arrayIn, int length)
		{
			bool even = arrayIn.Length % length == 0;
			int totalLength = arrayIn.Length / length;
			if (!even)
				++totalLength;

			var newArray = new byte[totalLength][];
			for (int i = 0; i < totalLength; ++i)
			{
				int allocLength = length;
				if (!even && i == totalLength - 1)
					allocLength = arrayIn.Length % length;

				newArray[i] = new byte[allocLength];
				Buffer.BlockCopy(arrayIn, i * length, newArray[i], 0, allocLength);
			}
			return newArray;
		}
	}
}