using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Class which provides utilities for the reading of already existing Sonic Heroes Collision Files.
    /// </summary>
    public class CollisionExporter
    {
        /// <summary>
        /// Represents the Sonic Heroes collision file.
        /// </summary>
        private byte[] _clFile;

        /// <summary>
        /// Represents the Sonic Heroes collision file.
        /// </summary>
        private List<string> _objFile;
        
        /// <summary>
        /// Reads in the supplied collision file or data.
        /// </summary>
        public void ReadColliison(string filePath) { _clFile = File.ReadAllBytes(filePath); ReadCollisionInternal(); }

        /// <summary>
        /// Reads in the supplied collision file or data.
        /// </summary>
        public void ReadColliison(byte[] collisionData) { _clFile = collisionData; ReadCollisionInternal(); }

        /// <summary>
        /// Reads in the supplied collision file or data.
        /// </summary>
        private void ReadCollisionInternal()
        {
            // Set the new OBJ File
            _objFile = new List<string>(100000);

            // Retrieve Vertex Section Offset
            uint triangleSectionOffset = ReadValue<uint>(8, sizeof(uint), true);
            uint vertexSectionOffset = ReadValue<uint>(12, sizeof(uint), true);

            // Get Number of Vertices and Triangles
            UInt16 numberOfTriangles = ReadValue<ushort>(34, sizeof(ushort), true);
            UInt16 numberOfVertices = ReadValue<ushort>(36, sizeof(ushort), true);

            // Set initial file pointers for reading file sections.
            int vertexSectionPointer = (int)vertexSectionOffset;
            int triangleSectionPointer = (int)triangleSectionOffset;

            // Write File Header
            _objFile.Add("# Exported by the Heroes Collision Library | Written by Sewer56");
            _objFile.Add("# Number of Vertices: " + numberOfVertices);
            _objFile.Add("# Number of Triangles: " + numberOfTriangles);
            _objFile.Add("");

            // Retrieve all of the Vertices
            for (int x = 0; x < numberOfVertices; x++)
            {
                // Get each of the vertex coordinates
                float xCoordinate = ReadValue<float>(vertexSectionPointer, sizeof(float), true);
                float yCoordinate = ReadValue<float>(vertexSectionPointer + sizeof(float), sizeof(float), true);
                float zCoordinate = ReadValue<float>(vertexSectionPointer + (sizeof(float)*2), sizeof(float), true);

                // Build the vertex entry string.
                string vertexEntry = "v " + xCoordinate + " " + yCoordinate + " " + zCoordinate;
                
                // Add string to OBJ File.
                _objFile.Add(vertexEntry);

                // Go to next Vertex.
                vertexSectionPointer += 12;
            }

            // Declare the object
            _objFile.Add("");
            _objFile.Add("o Collision");
            _objFile.Add("");

            // Retrieve all of the triangles
            for (int x = 0; x < numberOfTriangles; x++)
            {
                // Retrieve triangle indices.
                ushort triangleVertexOne = ReadValue<ushort>(triangleSectionPointer, sizeof(ushort), true);
                ushort triangleVertexTwo = ReadValue<ushort>(triangleSectionPointer + sizeof(ushort), sizeof(ushort), true);
                ushort triangleVertexThree = ReadValue<ushort>(triangleSectionPointer + (sizeof(ushort)*2), sizeof(ushort), true);
                
                // OBJ Triangles are 1 indexed, add 1.
                triangleVertexOne += 1;
                triangleVertexTwo += 1;
                triangleVertexThree += 1;

                // Build the triangle entry string.
                string triangleEntry = "f " + triangleVertexOne + " " + triangleVertexTwo + " " + triangleVertexThree;

                // Append onto the file.
                _objFile.Add(triangleEntry);

                // Go to next Triangle.
                triangleSectionPointer += 32;
            }
        }

        /// <summary>
        /// Writes the OBJ collision file out to external storage..
        /// </summary>
        /// <param name="filePath"></param>
        public void WriteCollision(string filePath)
        {
            File.WriteAllLines(filePath, _objFile);
        }

        /// <summary>
        /// Retrieves the OBJ collision file out to external storage..
        /// </summary>
        /// <param name="filePath"></param>
        public List<string> GetCollision(string filePath)
        {
            return _objFile;
        }

        /// <summary>
        /// Reads a value from the CLFile array and retrieves it in the desired format.
        /// </summary>
        /// <returns></returns>
        private T ReadValue<T>(int startIndex, int length, bool isReverseEndian)
        {
            // Retrieve the type of the passed in Generic Parameter
            Type genericType = typeof(T);

            // Declare an array for storing the data.
            byte[] data = new byte[length];

            // Copy the requested bytes onto the data array.
            Array.Copy(_clFile, startIndex, data, 0, length);

            // Reverse endians if necessary.
            if (isReverseEndian) { data = data.Reverse().ToArray(); }
            
            // Use this base object for the storage of the value we are retrieving.
            object value;
        
            // Convert the requested data into our type as specified and return.
            switch (genericType.Name)
            {
                case "String": value = BitConverter.ToString(data, 0); return (T)Convert.ChangeType(value, typeof(T));
                case "Boolean": value = BitConverter.ToBoolean(data, 0); return (T)Convert.ChangeType(value, typeof(T));
                case "Char": value = BitConverter.ToChar(data, 0); return (T)Convert.ChangeType(value, typeof(T));
                case "Byte": return (T)Convert.ChangeType(data[0], typeof(T));
                case "Single": value = BitConverter.ToSingle(data, 0); return (T)Convert.ChangeType(value, typeof(T));
                case "Double": value = BitConverter.ToDouble(data, 0); return (T)Convert.ChangeType(value, typeof(T));
                case "Int32": value = BitConverter.ToInt32(data, 0); return (T)Convert.ChangeType(value, typeof(T));
                case "UInt32": value = BitConverter.ToUInt32(data, 0); return (T)Convert.ChangeType(value, typeof(T));
                case "UInt16": value = BitConverter.ToUInt16(data, 0); return (T)Convert.ChangeType(value, typeof(T));
                case "Int16": value = BitConverter.ToInt16(data, 0); return (T)Convert.ChangeType(value, typeof(T));
            }
            return (T)Convert.ChangeType(0, typeof(T));
        }
    }
}