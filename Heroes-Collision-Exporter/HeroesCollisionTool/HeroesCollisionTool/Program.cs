using System;
using System.Diagnostics;
using HeroesCollisionLibrary;

namespace HeroesCollisionTool
{
    class Program
    {
        /// <summary>
        /// Flag deciding whether the application should present help to the end user.
        /// </summary>
        static byte action = 0;

        /// <summary>
        /// Creates an instance of the library for Collision Generation.
        /// </summary>
        static CollisionGenerator collisionGenerator = new CollisionGenerator();

        /// <summary>
        /// Creates an instance of the library for Collision Generation.
        /// </summary>
        static CollisionExporter collisionExporter = new CollisionExporter();

        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Check Arguments
            VerifyArguments(args);

            // Check if it is a CL or OBJ File for Exporting/Importing
            if (collisionGenerator.collisionGeneratorProperties.filePath != null)
            {
                if (collisionGenerator.collisionGeneratorProperties.filePath.EndsWith(".cl")) { action = 2; }
                else if (collisionGenerator.collisionGeneratorProperties.filePath.EndsWith(".obj")) { action = 1; }
            }

            // Switch all of the available action states..
            switch (action)
            {
                case 1:
                    collisionGenerator.LoadOBJFile();
                    collisionGenerator.GenerateCollision();
                    collisionGenerator.WriteFile();
                    collisionGenerator.PrintStatistics();
                    break;
                case 2:
                    collisionExporter.ReadColliison(collisionGenerator.collisionGeneratorProperties.filePath);
                    collisionExporter.WriteCollision(collisionGenerator.collisionGeneratorProperties.filePath + ".obj");
                    break;
                default:
                    ShowHelp();
                    break;
            }
        }

        /// <summary>
        /// Displays the help screen!
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Collision Generator X by Sewer56lol");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("OBJ => CL: --file <FilePath>");
            Console.WriteLine("CL => OBJ: --file <FilePath>");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Flags:");
            Console.WriteLine("--nodelevel <x>: Sets the depth level of the quadtree structure.");
            Console.WriteLine("--nodescale <x>: Increases node size to increase # of triangles per node. (Default: 1.05)");
            Console.WriteLine("--neighboursdisabled: Disables the neighbour finding algorithm.");
            Console.WriteLine("--adjacentsdisabled: Disables the adjacent triangle finding algorithm.");
            Console.WriteLine("--useAABB: Uses a slightly faster but less optimal triangle-node intersection algorithm.");
            Console.ReadLine();
        }

        /// <summary>
        /// Checks all of the supplied arguments to the application via startup parameters.
        /// </summary>
        private static void VerifyArguments(string[] args)
        {
            for (int x = 0; x < args.Length; x++)
            {
                if (args[x] == ("--file")) { collisionGenerator.collisionGeneratorProperties.filePath = args[x + 1]; }
                else if (args[x] == ("--nodelevel")) { collisionGenerator.collisionGeneratorProperties.depthLevel = Convert.ToByte(args[x + 1]); }
                else if (args[x] == ("--nodescale")) { collisionGenerator.collisionGeneratorProperties.nodeScale = Convert.ToSingle(args[x + 1]); }
                else if (args[x] == ("--neighboursdisabled")) { collisionGenerator.collisionGeneratorProperties.neighboursEnabled = false; }
                else if (args[x] == ("--adjacentsdisabled")) { collisionGenerator.collisionGeneratorProperties.adjacentsEnabled = false; }
                else if (args[x] == ("--useAABB")) { collisionGenerator.collisionGeneratorProperties.useAABB = true; }
                else if (args[x] == ("--basepower")) { collisionGenerator.collisionGeneratorProperties.basePower = Convert.ToByte(args[x + 1]); }
            }
        }
    }
}
