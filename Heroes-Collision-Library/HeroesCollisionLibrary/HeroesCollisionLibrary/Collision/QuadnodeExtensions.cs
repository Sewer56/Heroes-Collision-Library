using System;
using System.Collections.Generic;
using System.Text;
using HeroesCollisionLibrary.Geometry;
using HeroesCollisionLibrary.Geometry.Structures;

namespace HeroesCollisionLibrary.Collision
{
    /// <summary>
    /// Provides various extension methods for elements of the quadnode structure,
    /// such as finding the intersecting triangles.
    /// </summary>
    public static class QuadnodeUtilities
    {
        /// <summary>
        /// Finds the intersecting triangles for a suppllied node and its parent node.
        /// </summary>
        /// <param name="parentNode">The parent node of the current node, for the transfer of the triangle list.</param>
        /// <param name="thisNode">The node we are finding the intersecting triangles for.</param>
        public static void FindTriangles(ref Quadnode thisNode, Quadnode parentNode)
        {
            // Allocate memory for this node's triangle list. Approximate 1/4th of parent.
            thisNode.TrianglesInNode = new List<HeroesTriangle>(parentNode.TrianglesInNode.Count / 4);

            // Retrieve parent's triangle list.
            List<HeroesTriangle> parentTriangle = parentNode.TrianglesInNode;

            // Retrieve the bounding box for the current node.
            Rectangle nodeBoundingBox = thisNode.CalculateNodeBoundingBox();

            // Check for collision against parent's triangle list.
            foreach (var triangle in parentTriangle)
            {
                // Index of the individual triangle within the array of triangle boxes.
                int triangleIndex = triangle.TriangleIndex;

                // Otherwise Compare Triangle-Node intersect Accurately.
                if (CollisionChecker.CheckCollision(GeometryData.Triangles[triangleIndex], nodeBoundingBox))
                {
                    thisNode.TrianglesInNode.Add(GeometryData.Triangles[triangleIndex]);
                }
            }
        }

        /// <summary>
        /// Calculates the bounding box of a supplied quadnode structure.
        /// </summary>
        /// <returns>A rectangle structure that represents the bounding box of the quadnode.</returns>
        public static Rectangle CalculateNodeBoundingBox(this Quadnode thisNode)
        {
            // Stores the bounding box of the current node.
            Rectangle boundingBox = new Rectangle();

            // Obtain the size of a node at this specified node depth.
            float thisNodeSize = thisNode.GetNodeSize(HeroesHeader.QuadtreeSize);

            // Represents the top edge of the quadtree. 
            float topQuadtreeEdge = HeroesHeader.QuadtreeCenter.Z - (HeroesHeader.QuadtreeSize / 2.0F);

            // Represents the left edge of the quadtree.
            float leftQuadtreeEdge = HeroesHeader.QuadtreeCenter.X - (HeroesHeader.QuadtreeSize / 2.0F);

            // Obtain Horizontal/Vertical Positioning Offset between each node, each increment of this value points to next node.
            // e.g. if this is 512, node to the right will have 512 more than the previous node's offset, vertically or horizontally.
            int nextNodeOffset = (int)Math.Pow(2, (CollisionGenerator.Properties.BasePower - thisNode.DepthLevel));

            // Calculate the amount of nodes offset from the top left node horizontally and vertically.
            int nodesRightOfLeftQuadtreeEdge = thisNode.PositioningHorizontalOffset / nextNodeOffset;
            int nodesDownOfTopQuadtreeEdge = thisNode.PositioningVerticalOffset / nextNodeOffset;

            // Obtain Node Centers
            float nodeCenterX = leftQuadtreeEdge + (nodesRightOfLeftQuadtreeEdge * thisNodeSize) + (thisNodeSize / 2.0F);
            float nodeCenterY = topQuadtreeEdge + (nodesDownOfTopQuadtreeEdge * thisNodeSize) + (thisNodeSize / 2.0F);

            // Set the minimums and maximums.
            boundingBox.MinX = nodeCenterX - (thisNodeSize / 2.0F);
            boundingBox.MaxX = nodeCenterX + (thisNodeSize / 2.0F);

            boundingBox.MinZ = nodeCenterY - (thisNodeSize / 2.0F);
            boundingBox.MaxZ = nodeCenterY + (thisNodeSize / 2.0F);

            // Account for the node overlap regions set in the properties
            boundingBox.MinX -= CollisionGenerator.Properties.NodeOverlapRegion;
            boundingBox.MinZ -= CollisionGenerator.Properties.NodeOverlapRegion;

            boundingBox.MaxX += CollisionGenerator.Properties.NodeOverlapRegion;
            boundingBox.MaxZ += CollisionGenerator.Properties.NodeOverlapRegion;

            // Return all of the node rectangle bounding boxes.
            return boundingBox;
        }

        /// <summary>
        /// Returns the size of the current quadnode, the size is both the length and
        /// width as the quadnode is a square.
        /// </summary>
        /// <returns>The size of the quadnode.</returns>
        public static float GetNodeSize(this Quadnode thisNode, float quadtreeSize)
        {
            // Divide the node size by the amount of levels the node is deep in.
            for (int x = 0; x < thisNode.DepthLevel; x++)
            {
                quadtreeSize = quadtreeSize / 2.0F;
            }

            // Return the node size.
            return quadtreeSize;
        }
    }
}
