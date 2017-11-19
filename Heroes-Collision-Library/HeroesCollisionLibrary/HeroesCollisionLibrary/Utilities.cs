using System;
using System.Text;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// A class that provides various utility methods used elsewhere in the program.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// A generics based method which allows us to retrieve subarrays from arrays.
        /// </summary>
        /// <param name="index">The first element of the array.</param>
        /// <param name="length">The amount of elements we want from the array.</param>
        /// <returns>A subarray of the original array/</returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        /// <summary>
        /// Generates and prints an important warning message.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static void PrintWarningMessage(this string text)
        {
            try
            {
                // Stringbuilder for the borders of the box.
                StringBuilder borderBuilder = new StringBuilder(text.Length + 6);

                // Generate bottom/top borders.
                for (int x = 0; x < borderBuilder.Length; x++) { borderBuilder.Append("/"); }

                // Write to screen.
                Console.WriteLine(borderBuilder);
                Console.WriteLine("// " + text + " //");
                Console.WriteLine(borderBuilder);
            } catch {}
        }
    }
}