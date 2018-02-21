// Standard Library Imports.
using HeroesCollisionLibrary.Geometry;
using HeroesCollisionLibrary.Geometry.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Provides useful utilities used for working with Wavefront .obj files for the collision exporter/generator.
    /// </summary>
    public class ObjParser
    {
        /// <summary>
        /// Stores the contents of the Wavefront OBJ File.
        /// </summary>
        private string[] _objFileString;

        /// <summary>
        /// Defines the triangle array element which is currently being added.
        /// </summary>
        private int _triangleIndex = -1;

        /// <summary>
        /// Defines the vertex array element which is currently being added.
        /// </summary>
        private int _vertexIndex = -1;

        /// <summary>
        /// Match object for storing the results of each regex capture groups.
        /// </summary>
        private Match _regexMatch;

        /// <summary>
        /// [Constructor] Initializes the class.
        /// </summary>
        public ObjParser(string objFilePath)
        {
            // Load Actual OBJ File
            _objFileString = File.ReadAllLines(objFilePath);
        }

        /// <summary>
        /// Returns the complete version of the OBJ File with the necessary information to generate collision.
        /// After running, use GetCollisionFile to retrieve the information. (or GetVertices/GetTriangles)
        /// </summary>
        public void ReadObjFile()
        {
            // Calculate the vertices and triangles and return each of them.
            ReadVertices();
            ReadTriangles();
        }

        /// <summary>
        /// Works out all of the vertices from the Wavefront OBJ File.
        /// After running, use GetVertices to retrieve the information. 
        /// </summary>
        private void ReadVertices()
        {
            // Assigns Memory for Storing Vertex Data.
            _vertexIndex = -1;

            // Compile Regular Expressions for stripping spaces and definitions from faces and vertices.
            // Intended data goes into the first capture group.
            // Regex CheatSheet: https://www.cheatography.com/davechild/cheat-sheets/regular-expressions/
            // Learn Regex at: https://regexone.com/
            Regex vertexRegex = new Regex(@"v[ ]*(.*)", RegexOptions.Compiled);

            // In the case there are too many vertices.
            try 
            {
                // Parse the file line by line.
                foreach (String line in _objFileString)
                {
                    // If the line defines a vertex.
                    if (line.StartsWith("v")) 
                    {
                        // Get Regular Expression Matches.
                        _regexMatch = vertexRegex.Match(line);

                        // Group 0 contains entire matched expression, we only want first group.
                        string vertexCoordinates = _regexMatch.Groups[1].Value;

                        // Add vertex onto vertex list.
                        AddVertex(vertexCoordinates);
                    }
                }
            } 
            catch 
            { 
                "ERROR READING OBJ FILE VERTICES".PrintWarningMessage(); 
            }


            // Inform user of breeaking limits.
            if (_vertexIndex > 65535) { "YOUR COLLISION MODEL IS TOO COMPLEX FOR THE GAME, ENSURE YOUR MODEL HAS MAXIMUM 65535 VERTICES".PrintWarningMessage(); }
        }

        /// <summary>
        /// Works out all of the triangles' vertices from the Wavefront OBJ File.
        /// After running, use GetTriangles to retrieve the information. 
        /// </summary>
        private void ReadTriangles()
        {
            // Assigns memory for Storing Triangle Data
            GeometryData.Triangles = new List<HeroesTriangle>(UInt16.MaxValue); // Maximum amount in a Heroes Collision File.
            _triangleIndex = -1;

            // Compile Regular Expressions for stripping spaces and definitions from faces and vertices.
            // Intended data goes into the first capture group.
            // Regex CheatSheet: https://www.cheatography.com/davechild/cheat-sheets/regular-expressions/
            // Learn Regex at: https://regexone.com/
            Regex faceRegex = new Regex(@"f[ ]*(.*)", RegexOptions.Compiled);

            // In the case there are too many triangles.
            try
            {
                // Parse the file line by line.
                foreach (String line in _objFileString)
                {
                    // If the line defines a face.
                    if (line.StartsWith("f"))
                    {
                        // Get Regular Expression Matches.
                        _regexMatch = faceRegex.Match(line);

                        // Group 0 contains entire matched expression, we only want first group.
                        string faceCoordinates = _regexMatch.Groups[1].Value;

                        // Work out triangle faces.
                        AddTriangle(faceCoordinates);
                    }
                }
            } 
            catch 
            { 
                "ERROR READING OBJ FILE FACES".PrintWarningMessage(); 
            }

            // Inform user of breeaking limits.
            if (_triangleIndex > 65535) { "YOUR COLLISION MODEL IS TOO COMPLEX FOR THE GAME, ENSURE YOUR MODEL HAS MAXIMUM 65535 FACES".PrintWarningMessage(); }
        }

        /// <summary>
        /// Splits a string which defines three vertices and adds a vertex onto the vertex list.
        /// </summary>
        private void AddVertex(string vertexCoordinates)
        {
            // Split the vertex coordinates by spaces.
            string[] verticesString = vertexCoordinates.Split(' ');

            // Increment array index. (Pre-decrement, value starts at -1)
            _vertexIndex += 1;

            // Declare and assign the individual XYZ Positions
            Vertex vertices = new Vertex();

            vertices.X = Convert.ToSingle(verticesString[0]);
            vertices.Y = Convert.ToSingle(verticesString[1]);
            vertices.Z = Convert.ToSingle(verticesString[2]);

            // Add onto the vertices array.
            GeometryData.Vertices.Add(vertices);
        }

        /// <summary>
        /// Splits a string which defines three vertices and adds a triangle onto the triangle list.
        /// </summary>
        private void AddTriangle(string triangleVertices)
        {
            // Separate each triangle entry
            string[] trianglesString = triangleVertices.Split(' ');

            // Increment array index. (Pre-decrement, value starts at -1)
            _triangleIndex += 1;

            // Check if it's face vertex index value only, if it contains texture coordinates or normals, strip them from the string.
            for (int x = 0; x < trianglesString.Length; x++) 
            { 
                if (trianglesString[x].Contains("/")) 
                { 
                    trianglesString[x] = trianglesString[x].Substring(0, trianglesString[x].IndexOf("/")); 
                }
            }

            // Declare and assign the individual triangle vertices
            HeroesTriangle triangle = new HeroesTriangle();

            // Stores the individual vertex information.
            ushort vertexOne;
            ushort vertexTwo;
            ushort vertexThree;

            // NOTE: Our vertices array starts at 0, but the triangle vertices in the OBJ file start at 1, make sure this index subtraction is correct.
            // NOTE: Some OBJ Exporters may not assign a vertex to some faces, set them to 1 if that should turn to be so, try/catch.
            try { vertexOne = (ushort)(Convert.ToUInt16(trianglesString[0]) - 1); } catch {vertexOne = 1;}
            try { vertexTwo = (ushort)(Convert.ToUInt16(trianglesString[1]) - 1); } catch {vertexTwo = 1;}
            try { vertexThree = (ushort)(Convert.ToUInt16(trianglesString[2]) - 1); } catch {vertexThree = 1;}

            // Assign our vertices to the triangle.
            triangle.VertexOne = vertexOne;
            triangle.VertexTwo = vertexTwo;
            triangle.VertexThree = vertexThree;

            // Assign no collision flags
            triangle.FlagsPrimary = new byte[]{ 0x00, 0x00, 0x00, 0x00 };
            triangle.FlagsSecondary = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            // Add onto triangles array
            GeometryData.Triangles.Add(triangle);
        }
    }
}
