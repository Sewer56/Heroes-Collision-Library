using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// A class that provides various utility methods used elsewhere in the program.
    /// </summary>
    public static class Utilities
    {
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

        /// <summary>
        /// Benchmarks an individual method call.
        /// </summary>
        public static void BenchmarkMethod(Action method, String actionText)
        {
            // Stopwatch to benchmark every action.
            Stopwatch performanceWatch = new Stopwatch();

            // Print out the action
            Console.Write(actionText + " | ");

            // Start the stopwatch.
            performanceWatch.Start();

            // Run the method.
            method();

            // Stop the stopwatch
            performanceWatch.Stop();

            // Print the results.
            Console.WriteLine(performanceWatch.ElapsedMilliseconds + "ms");
        }

        /// <summary>
        /// Extension method for booleans, verifies whether all supplied booleans have equal value.
        /// </summary>
        /// <returns>True if all of the supplied boolean values are equivalent</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllEqual(this bool firstValue, params bool[] otherValues)
        {
            // Determine whether all elements satisfy a condition (equal in our case)
            return otherValues.All(x => x == firstValue);
        }
    }
}