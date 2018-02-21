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
        private static CollisionGenerator collisionGenerator;

        /// <summary>
        /// Creates an instance of the library for Collision Generation.
        /// </summary>
        private static CollisionExporter collisionExporter;

        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Instantiate Generator & Exporter
            collisionExporter = new CollisionExporter();
            collisionGenerator = new CollisionGenerator();

            // Check Arguments
            VerifyArguments(args);

            // Check if it is a CL or OBJ File for Exporting/Importing
            if (CollisionGenerator.Properties.FilePath != null)
            {
                if (CollisionGenerator.Properties.FilePath.EndsWith(".cl")) { action = 2; }
                else if (CollisionGenerator.Properties.FilePath.EndsWith(".obj")) { action = 1; }
            }

            // Switch all of the available action states..
            switch (action)
            {
                case 1:
                    collisionGenerator.LoadObjFile();
                    collisionGenerator.GenerateCollision();
                    collisionGenerator.WriteFile();
                    break;
                case 2:
                    collisionExporter.ReadColliison(CollisionGenerator.Properties.FilePath);
                    collisionExporter.WriteCollision(CollisionGenerator.Properties.FilePath + ".obj");
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
            Console.WriteLine("--nodeoverlapregion <x>: Defines an overlap region in floating point units\n " +
                              "                         where triangles in quadnodes overlap, default value 25.0\n" +
                              "                         This is used to prevent cracks in floors.");
            Console.WriteLine("--neighboursdisabled: Disables the neighbour finding algorithm for nodes that are not at the same depth.");
            Console.WriteLine("--adjacentsdisabled: Disables the adjacent triangle finding algorithm.");
            Console.ReadLine();
        }

        /// <summary>
        /// Checks all of the supplied arguments to the application via startup parameters.
        /// </summary>
        private static void VerifyArguments(string[] args)
        {
            for (int x = 0; x < args.Length; x++)
            {
                if (args[x] == ("--file")) { CollisionGenerator.Properties.FilePath = args[x + 1]; }
                else if (args[x] == ("--nodelevel")) { CollisionGenerator.Properties.DepthLevel = Convert.ToByte(args[x + 1]); }
                else if (args[x] == ("--nodeoverlapregion")) { CollisionGenerator.Properties.NodeOverlapRegion = Convert.ToSingle(args[x + 1]); }
                else if (args[x] == ("--neighboursdisabled")) { CollisionGenerator.Properties.EnableNeighbours = false; }
                else if (args[x] == ("--adjacentsdisabled")) { CollisionGenerator.Properties.EnableAdjacents = false; }
                else if (args[x] == ("--basepower")) { CollisionGenerator.Properties.BasePower = Convert.ToByte(args[x + 1]); }
            }
        }
    }
}
