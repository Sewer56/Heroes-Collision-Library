using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeroesCollisionLibrary.Geometry;
using HeroesCollisionLibrary.Geometry.Structures;

namespace HeroesCollisionLibrary.Collision
{
    public static class CLFileGenerator
    {
        /// <summary>
        /// Struct containing the offsets each CL file section.
        /// </summary>
        public struct ClFileOffsets
        {
            public int NodeSectionOffset;
            public int TriangleSectionOffset;
            public int VertexSectionOffset;
            public int TotalFileSize;
        }

        /// <summary>
        /// Writes the collision file out to a local file.
        /// </summary>
        /// <param name="clFile">The list of bytes to represent the collision file.</param>
        /// <param name="quadtreeData">Contains the quadtree information and details.</param>
        public static List<byte> GenerateFile(List<byte> clFile, QuadtreeGenerator quadtreeData)
        {
            // Allocate a buffer for the collision file.
            // The buffer size is 2MiB
            clFile = new List<byte>(2097152);

            // Calculate the file offsets.
            ClFileOffsets fileOffsets = CalculateClOffsets(quadtreeData);

            // Retrieve the header.
            CalculateCollisionHeader(fileOffsets, quadtreeData);

            // Write the file header.
            WriteCollisionFileHeader(clFile, quadtreeData);

            // Write the triangle reference list.
            WriteTriangleList(clFile, quadtreeData);

            // Write the quadnodes
            WriteQuadnodeList(clFile, quadtreeData);

            // Write all of the triangle entries.
            WriteTriangles(clFile);

            // Write the vertices.
            for (int x = 0; x < GeometryData.Vertices.Count; x++)
            {
                clFile.AddRange(BitConverter.GetBytes((float)GeometryData.Vertices[x].X).Reverse());
                clFile.AddRange(BitConverter.GetBytes((float)GeometryData.Vertices[x].Y).Reverse());
                clFile.AddRange(BitConverter.GetBytes((float)GeometryData.Vertices[x].Z).Reverse());
            }

            // Return collision file.
            return clFile;
        }

        /// <summary>
        /// Writes the triangle list of a collision CL file.
        /// </summary>
        /// <param name="ClFile">The list of bytes to represent the collision file.</param>
        private static void WriteTriangles(List<byte> clFile)
        {
            // Write the triangles
            for (int x = 0; x < GeometryData.Triangles.Count; x++)
            {
                // Add each of the triangle vertices.
                clFile.AddRange(BitConverter.GetBytes((ushort)GeometryData.Triangles[x].VertexOne).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)GeometryData.Triangles[x].VertexTwo).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)GeometryData.Triangles[x].VertexThree).Reverse());

                // Add each of the triangle's adjacents.
                clFile.AddRange(BitConverter.GetBytes((ushort)GeometryData.Triangles[x].AdjacentTriangles[0]).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)GeometryData.Triangles[x].AdjacentTriangles[1]).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)GeometryData.Triangles[x].AdjacentTriangles[2]).Reverse());

                // Write the vertex normals to the file
                clFile.AddRange(BitConverter.GetBytes((float)GeometryData.Triangles[x].Normals.X).Reverse());
                clFile.AddRange(BitConverter.GetBytes((float)GeometryData.Triangles[x].Normals.Y).Reverse());
                clFile.AddRange(BitConverter.GetBytes((float)GeometryData.Triangles[x].Normals.Z).Reverse());

                // Add dummies for collision flags.
                clFile.AddRange(GeometryData.Triangles[x].FlagsPrimary);
                clFile.AddRange(GeometryData.Triangles[x].FlagsSecondary);
            }
        }

        /// <summary>
        /// Writes the header of a collision CL file.
        /// </summary>
        /// <param name="ClFile">The list of bytes to represent the collision file.</param>
        /// <param name="quadtreeData">Contains the quadtree information and details.</param>
        private static void WriteCollisionFileHeader(List<byte> clFile, QuadtreeGenerator quadtreeData)
        {
            // Append the offsets of each of the individual file sections.
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.NumberOfBytes).Reverse());
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.QuadtreeOffset).Reverse());
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.TriangleOffset).Reverse());
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.VertexOffset).Reverse());

            // Append the quadtree center and the size to the header.
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.QuadtreeCenter.X).Reverse());
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.QuadtreeCenter.Y).Reverse());
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.QuadtreeCenter.Z).Reverse());
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.QuadtreeSize).Reverse());

            // Append the quadtree base power to the header as well as the length of each file section.
            clFile.AddRange(BitConverter.GetBytes(HeroesHeader.BasePower).Reverse());
            clFile.AddRange(BitConverter.GetBytes((ushort)GeometryData.Triangles.Count).Reverse());
            clFile.AddRange(BitConverter.GetBytes((ushort)GeometryData.Vertices.Count).Reverse());
            clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes.Length).Reverse());
        }

        /// <summary>
        /// Writes the triangle list to the output CL File.
        /// </summary>
        /// <param name="ClFile">The list of bytes to represent the collision file.</param>
        /// <param name="quadtreeData">Contains the quadtree information and details.</param>
        private static void WriteTriangleList(List<byte> clFile, QuadtreeGenerator quadtreeData)
        {
            // Retrieve all nodes with triangles.
            Quadnode[] quadnodesWithTriangles = QuadtreeGenerator.QuadNodes.Where(x => x.TrianglesInNode.Count > 0).ToArray();

            // Loop over all bottom level nodes, add triangles.
            for (int x = 0; x < quadnodesWithTriangles.Length; x++)
            {
                // Set triangle list offset.
                quadnodesWithTriangles[x].TriangleListOffset = (uint)clFile.Count;

                // Add triangles.
                foreach (HeroesTriangle triangle in quadnodesWithTriangles[x].TrianglesInNode)
                { clFile.AddRange(BitConverter.GetBytes((ushort)triangle.TriangleIndex).Reverse()); }

                // Replace the original quadnode list element
                QuadtreeGenerator.QuadNodes[quadnodesWithTriangles[x].NodeIndices.NodeIndex] = quadnodesWithTriangles[x];
            }
        }


        /// <summary>
        /// Writes the quadnode list to the output CL File.
        /// </summary>
        /// <param name="clFile"></param>
        /// <param name="quadtreeData">Contains the quadtree information and details.</param>
        private static void WriteQuadnodeList(List<byte> clFile, QuadtreeGenerator quadtreeData)
        {
            // For each node.
            for (int x = 0; x < QuadtreeGenerator.QuadNodes.Length; x++)
            {
                // Append the Node, Parent and Child node indices.
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].NodeIndices.NodeIndex).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].NodeIndices.NodeParent).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].NodeIndices.NodeChild).Reverse());

                // Appens the calculated neighbour indices to the node.
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].NodeNeighbours.RightNeightbourIndex).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].NodeNeighbours.LeftNeightbourIndex).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].NodeNeighbours.BottomNeighbourIndex).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].NodeNeighbours.TopNeighbourIndex).Reverse());

                // Append the amount of triangles present within this quadnode.
                // If the quadnode is not a bottom level node without children, we do not assign an offset.
                if (QuadtreeGenerator.QuadNodes[x].TriangleListOffset == 0)
                {
                    clFile.AddRange(BitConverter.GetBytes((ushort)0));
                }
                else
                {
                    clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].TrianglesInNode.Count).Reverse());
                }

                // Append the offset to the triangle list within the file.
                clFile.AddRange(BitConverter.GetBytes((uint)QuadtreeGenerator.QuadNodes[x].TriangleListOffset).Reverse());

                // Append the horizontal and vertical quadnode offset as well as the depth level.
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].PositioningHorizontalOffset).Reverse());
                clFile.AddRange(BitConverter.GetBytes((ushort)QuadtreeGenerator.QuadNodes[x].PositioningVerticalOffset).Reverse());
                clFile.Add((byte)(QuadtreeGenerator.QuadNodes[x].DepthLevel));

                // Dummy bytes that are not used in the original struct.
                clFile.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            }
        }

        /// <summary>
        /// Complete the file header for the output colliison struct based off of the already known data.
        /// </summary>
        /// <param name="quadtreeData">Contains the quadtree information and details.</param>
        private static void CalculateCollisionHeader(ClFileOffsets fileOffsets, QuadtreeGenerator quadtreeData)
        {
            // Fill in the necessary header data from all of the subclasses.
            HeroesHeader.BasePower = CollisionGenerator.Properties.BasePower;
            HeroesHeader.NumberOfBytes = (uint)fileOffsets.TotalFileSize;
            HeroesHeader.NumberOfNodes = (ushort)QuadtreeGenerator.QuadNodes.Length;
            HeroesHeader.NumberOfTriangles = (ushort)GeometryData.Triangles.Count;
            HeroesHeader.NumberOfVertices = (ushort)GeometryData.Vertices.Count;
            HeroesHeader.QuadtreeOffset = (uint)fileOffsets.NodeSectionOffset;
            HeroesHeader.TriangleOffset = (uint)fileOffsets.TriangleSectionOffset;
            HeroesHeader.VertexOffset = (uint)fileOffsets.VertexSectionOffset;
        }

        /// <summary>
        /// Calculates the offsets for the CL File.
        /// </summary>
        /// <param name="quadtreeData">Contains the quadtree information and details.</param>
        private static ClFileOffsets CalculateClOffsets(QuadtreeGenerator quadtreeData)
        {
            // File offsets.
            ClFileOffsets fileOffsets = new ClFileOffsets();

            // Current pointer in the nonexisting (yet) output file.
            int currentCursorPointer = HeroesHeader.HeaderLength;

            // Retrieve all nodes that contain triangles. (Nodes with triangles have no children)
            Quadnode[] quadnodesAtDepthLevel = QuadtreeGenerator.QuadNodes.Where(x => x.TrianglesInNode.Count > 0).ToArray();

            // Calculate offset for node section.
            foreach (var quadNode in quadnodesAtDepthLevel) {
                currentCursorPointer += (quadNode.TrianglesInNode.Count * 2);
            }

            // Set node section offset.
            fileOffsets.NodeSectionOffset = currentCursorPointer;

            // Calculate offset for triangle section.
            currentCursorPointer = currentCursorPointer + (0x20 * QuadtreeGenerator.QuadNodes.Length);

            // Set triangle section offset.
            fileOffsets.TriangleSectionOffset = currentCursorPointer;

            // Calculate Vertex Section Offset.
            currentCursorPointer = currentCursorPointer + (0x20 * GeometryData.Triangles.Count);

            // Set vertex section offset.
            fileOffsets.VertexSectionOffset = currentCursorPointer;

            // Calculate end of file
            currentCursorPointer = currentCursorPointer + (0x0C * GeometryData.Vertices.Count);

            // Set EOF.
            fileOffsets.TotalFileSize = currentCursorPointer;

            // Return the file offsets.
            return fileOffsets;
        }
    }
}
