using HeroesCollisionLibrary.Geometry.Structures;
using System.Collections.Generic;

namespace HeroesCollisionLibrary.Collision
{
    /// <summary>
    /// Structure which represents an individual quadtree node.
    /// </summary>
    public class Quadnode
    {
        /// <summary>
        /// Stores the individual node indexes which contain the parent, self and child node.
        /// </summary>
        public NodeIndices NodeIndices;

        /// <summary>
        /// Stores the neighbouring nodes to this current node structure.
        /// </summary>
        public NodeNeighbours NodeNeighbours;

        /// <summary>
        /// The offset positioning value of the node in the horizontal direction. Calculated by: 2^(base power-depth level) + Parent's Offset Value, depending on the node. See the wiki!
        /// </summary>
        public ushort PositioningHorizontalOffset;

        /// <summary>
        /// The offset positioning value of the node in the horizontal direction. Calculated by: 2^(base power-depth level) + Parent's Offset Value, depending on the node. See the wiki!
        /// </summary>
        public ushort PositioningVerticalOffset;

        /// <summary>
        /// The individual depth level of the QuadNode.
        /// </summary>
        public byte DepthLevel;

        /// <summary>
        /// Offset to first triangle in triangle list for this node.
        /// </summary>
        public uint TriangleListOffset;

        /// <summary>
        /// Represents a listing of all of the triangles within the node.
        /// </summary>
        public List<HeroesTriangle> TrianglesInNode;
    }

    /// <summary>
    /// Defines the individual indexes of the node's neighbouring nodes.
    /// </summary>
    public struct NodeNeighbours
    {
        /// <summary>
        /// The right neighbour to the current node. 0 if edge/none. It can be either at the at the same or at another depth level.
        /// </summary>
        public ushort RightNeightbourIndex;

        /// <summary>
        /// The left neighbour to the current node. 0 if edge/none. It can be either at the at the same or at another depth level.
        /// </summary>
        public ushort LeftNeightbourIndex;

        /// <summary>
        /// The bottom neighbour to the current node. 0 if edge/none. It can be either at the at the same or at another depth level.
        /// </summary>
        public ushort BottomNeighbourIndex;

        /// <summary>
        /// The top neighbouring node. 0 if edge/none. It can be either at the at the same or at another depth level.
        /// </summary>
        public ushort TopNeighbourIndex;
    }

    /// <summary>
    /// Defines the individual indexes of the node's neighbouring nodes.
    /// </summary>
    public struct NodeIndices
    {
        /// <summary>
        /// Individual uniquely identifiable index.
        /// </summary>
        public ushort NodeIndex;

        /// <summary>
        /// The parent node of the current node.
        /// </summary>
        public ushort NodeParent;

        /// <summary>
        /// The index of the first out of four children of the node, 0 if no child. 
        /// </summary>
        public ushort NodeChild;
    }

    /// <summary>
    /// Structure which stores 4 child quadnodes, used for the simplification of code involving
    /// cross depth level node operations.
    /// </summary>
    public class QuadnodeChildren
    {
        public Quadnode TopLeftNode;
        public Quadnode TopRightNode;
        public Quadnode BottomLeftNode;
        public Quadnode BottomRightNode;
        public Quadnode ThisNode;

        public QuadnodeChildren(Quadnode currentNode)
        {
            ThisNode = currentNode;
            if (currentNode.NodeIndices.NodeChild != 0)
            {
                TopLeftNode = QuadtreeGenerator.QuadNodes[currentNode.NodeIndices.NodeChild + (int)QuadtreePosition.topLeft];
                TopRightNode = QuadtreeGenerator.QuadNodes[currentNode.NodeIndices.NodeChild + (int)QuadtreePosition.topRight];
                BottomLeftNode = QuadtreeGenerator.QuadNodes[currentNode.NodeIndices.NodeChild + (int)QuadtreePosition.bottomLeft];
                BottomRightNode = QuadtreeGenerator.QuadNodes[currentNode.NodeIndices.NodeChild + (int)QuadtreePosition.bottomRight];
            }
        }

        /// <summary>
        /// Replaces the original quadnodes in the Quadnodes Array with the Quadnodes from the current QuadnodeChildren struct.
        /// </summary>
        public void ReplaceQuadnodes()
        {
            if (TopLeftNode != null) { QuadtreeGenerator.QuadNodes[TopLeftNode.NodeIndices.NodeIndex] = TopLeftNode; }
            if (TopRightNode != null) { QuadtreeGenerator.QuadNodes[TopRightNode.NodeIndices.NodeIndex] = TopRightNode; }
            if (BottomLeftNode != null) { QuadtreeGenerator.QuadNodes[BottomLeftNode.NodeIndices.NodeIndex] = BottomLeftNode; }
            if (BottomRightNode != null) { QuadtreeGenerator.QuadNodes[BottomRightNode.NodeIndices.NodeIndex] = BottomRightNode; }
        }
    }

    /// <summary>
    /// Defines the relative quadtree location relative to the parent node of the current node.
    /// </summary>
    public enum QuadtreePosition
    {
        topLeft,
        topRight,
        bottomLeft,
        bottomRight
    }
}
