// Standard Library Imports.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Provides useful utilities used for working with Wavefront .obj files for the collision exporter/generator.
    /// </summary>
    public class OBJ_Utilities
    {
        /// <summary>
        /// Stores all of the necessary properties which define this .OBJ File
        /// </summary>
        private Geometry_Properties objFile;

        /// <summary>
        /// Stores the contents of the Wavefront OBJ File.
        /// </summary>
        private string[] objFileString;

        /// <summary>
        /// Defines the triangle array element which is currently being added (optimization).
        /// </summary>
        private int triangleIndex = -1;

        /// <summary>
        /// Defines the vertex array element which is currently being added (optimization).
        /// </summary>
        private int vertexIndex = -1;

        /// <summary>
        /// Match object for storing the results of each regex capture groups.
        /// </summary>
        Match regexMatch;

        /// <summary>
        /// [Constructor] Initializes the class.
        /// </summary>
        public OBJ_Utilities(string objFilePath)
        {
            // Initialize OBJ File
            objFile = new Geometry_Properties();

            // Load Actual OBJ File
            objFileString = File.ReadAllLines(objFilePath);
        }

        /// <summary>
        /// Retrieves the vertices of the loaded .OBJ File.
        /// </summary>
        public Geometry_Properties.Vertex[] GetVertices(){ return objFile.verticesArray; }

        /// <summary>
        /// Retrieves the triangles of the loaded .OBJ File.
        /// </summary>
        public Geometry_Properties.Triangle[] GetTriangles(){ return objFile.triangleArray; }

        /// <summary>
        /// Retrieves the complete collision file details.
        /// </summary>
        public Geometry_Properties GetCollisionFile(){ return objFile; }

        /// <summary>
        /// Returns the complete version of the OBJ File with the necessary information to generate collision.
        /// After running, use GetCollisionFile to retrieve the information. (or GetVertices/GetTriangles)
        /// </summary>
        public void CalculateCollisionFile()
        {
            // Calculate the vertices and triangles and return each of them.
            CalculateVertices();
            CalculateTriangles();
        }

        /// <summary>
        /// Works out all of the vertices from the Wavefront OBJ File.
        /// After running, use GetVertices to retrieve the information. 
        /// </summary>
        public void CalculateVertices()
        {
            // Assigns Memory for Storing Vertex Data.
            objFile.verticesArray = new Geometry_Properties.Vertex[UInt16.MaxValue]; // Maximum amount in a Heroes Collision File.
            vertexIndex = -1;

            // Compile Regular Expressions for stripping spaces and definitions from faces and vertices.
            // Intended data goes into the first capture group.
            // Regex CheatSheet: https://www.cheatography.com/davechild/cheat-sheets/regular-expressions/
            // Learn Regex at: https://regexone.com/
            Regex vertexRegex = new Regex(@"v[ ]*(.*)", RegexOptions.Compiled);

            // In the case there are too many vertices.
            try 
            {
                // Parse the file line by line.
                foreach (String line in objFileString)
                {
                    // Skip Comments
                    if (line.StartsWith("#")) { continue; }
                    // If the line defines a vertex.
                    else if (line.StartsWith("v")) 
                    {
                        // Get Regular Expression Matches.
                        regexMatch = vertexRegex.Match(line);

                        // Group 0 contains entire matched expression, we only want first group.
                        string vertexCoordinates = regexMatch.Groups[1].Value;

                        // Add vertex onto vertex list.
                        AddVertex(vertexCoordinates);

                        // Continue onto Next Iteration
                        continue;
                    }
                }
            } 
            catch 
            { 
                "YOUR COLLISION MODEL IS TOO COMPLEX, REDUCE THE AMOUNT OF VERTICES | MAX 65535".PrintWarningMessage(); 
            }

            // Trim the vertices down.
            TrimVertices();
        }

        /// <summary>
        /// Works out all of the triangles' vertices from the Wavefront OBJ File.
        /// After running, use GetTriangles to retrieve the information. 
        /// </summary>
        public void CalculateTriangles()
        {
            // Assigns memory for Storing Triangle Data
            objFile.triangleArray = new Geometry_Properties.Triangle[UInt16.MaxValue]; // Maximum amount in a Heroes Collision File.
            triangleIndex = -1;

            // Compile Regular Expressions for stripping spaces and definitions from faces and vertices.
            // Intended data goes into the first capture group.
            // Regex CheatSheet: https://www.cheatography.com/davechild/cheat-sheets/regular-expressions/
            // Learn Regex at: https://regexone.com/
            Regex faceRegex = new Regex(@"f[ ]*(.*)", RegexOptions.Compiled);

            // In the case there are too many triangles.
            try
            {
                // Parse the file line by line.
                foreach (String line in objFileString)
                {
                    // Skip Comments
                    if (line.StartsWith("#")) { continue; }

                    // If the line defines a face.
                    else if (line.StartsWith("f"))
                    {
                        // Get Regular Expression Matches.
                        regexMatch = faceRegex.Match(line);

                        // Group 0 contains entire matched expression, we only want first group.
                        string faceCoordinates = regexMatch.Groups[1].Value;

                        // Work out triangle faces.
                        AddTriangle(faceCoordinates);

                        // Continue onto Next Iteration
                        continue;
                    }
                }
            } 
            catch 
            { 
                "YOUR COLLISION MODEL IS TOO COMPLEX, REDUCE THE AMOUNT OF TRIANGLES | MAX 65535".PrintWarningMessage(); 
            }

            // Trim the triangles array down.
            TrimTriangles();
        }

        /// <summary>
        /// Splits a string which defines three vertices and adds a vertex onto the vertex list.
        /// </summary>
        private void AddVertex(string vertexCoordinates)
        {
            // Split the vertex coordinates by spaces.
            string[] verticesString = vertexCoordinates.Split(' ');

            // Increment array index. (Pre-decrement, value starts at -1)
            vertexIndex += 1;

            // Declare and assign the individual XYZ Positions
            Geometry_Properties.Vertex vertices = new Geometry_Properties.Vertex();

            vertices.X = Convert.ToSingle(verticesString[0]);
            vertices.Y = Convert.ToSingle(verticesString[1]);
            vertices.Z = Convert.ToSingle(verticesString[2]);

            // Add onto the vertices array.
            objFile.verticesArray[vertexIndex] = vertices;
        }

        /// <summary>
        /// Splits a string which defines three vertices and adds a triangle onto the triangle list.
        /// </summary>
        private void AddTriangle(string triangleVertices)
        {
            // Separate each triangle entry
            string[] trianglesString = triangleVertices.Split(' ');

            // Increment array index. (Pre-decrement, value starts at -1)
            triangleIndex += 1;

            // Check if it's face vertex index value only, if it contains texture coordinates or normals, strip them from the string.
            for (int x = 0; x < trianglesString.Length; x++) 
            { 
                if (trianglesString[x].Contains("/")) 
                { 
                    trianglesString[x] = trianglesString[x].Substring(0, trianglesString[x].IndexOf("/")); 
                }
            }

            // Declare and assign the individual triangle vertices
            Geometry_Properties.Triangle triangle = new Geometry_Properties.Triangle();

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
            triangle.vertexOne = vertexOne;
            triangle.vertexTwo = vertexTwo;
            triangle.vertexThree = vertexThree;

            // Assign no collision flags
            triangle.triangleFlagsI = 0;
            triangle.triangleFlagsII = 0;

            // Add onto triangles array
            objFile.triangleArray[triangleIndex] = triangle;
        }

        /// <summary>
        /// Trims the triangle array down to the necessary amount of array elements.
        /// </summary>
        private void TrimTriangles() { objFile.triangleArray = objFile.triangleArray.SubArray(0, triangleIndex + 1); }       

        /// <summary>
        /// Trims the vertex array down to the necessary amount of array elements.
        /// </summary>
        private void TrimVertices() { objFile.verticesArray = objFile.verticesArray.SubArray(0, vertexIndex + 1); } 
    }
}
