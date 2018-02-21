using HeroesCollisionLibrary.Collision;
using HeroesCollisionLibrary.Geometry;
using HeroesCollisionLibrary.Geometry.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Provides structures and set of common classes helpful for working with quadtrees.
    /// </summary>
    public class QuadtreeGenerator
    {
        /// <summary>
        /// List of all of the quadnodes used in the program.
        /// </summary>
        public static Quadnode[] QuadNodes;
        
        /// <summary>
        /// An array of a list of rectangles which define the node bounding boxes, used for fast collision checking.
        /// </summary>
        public Rectangle[] NodeBoxes;

        /// <summary>
        /// The depth level of the quadnode structure stored in the quadtree properties class.
        /// </summary>
        public int DepthLevel;

        /// <summary>
        /// The base Offset Power Level for the quadnode structure. It is unknown what this does.
        /// </summary>
        public int BasePower;

        /// <summary>
        /// Local list of quadnodes.
        /// </summary>
        private List<Quadnode> _quadNodesLocal = new List<Quadnode>(65535);

        /// <summary>
        /// Constructor
        /// </summary>
        public QuadtreeGenerator(int depthLevel, int basePower)
        {
            // Calculate number of quadnodes using Geometric Progression based on the depth level.
            int quadNodeCount = 1;

            // Add the amount of nodes at each level to the total node count.
            for (int x = 2; x <= depthLevel; x++)
            {
                // Add a(1-r^n)/1-r
                quadNodeCount += (int)( 1 * (1 - Math.Pow(4, depthLevel)) / (1-depthLevel) );
            }

            // Copy the depth level
            DepthLevel = depthLevel;

            // Set default base power.
            BasePower = basePower;
        }

        /// <summary>
        /// Generates all of the quadnodes.
        /// </summary>
        public void GenerateQuadnodes()
        {
            // Setup first node (a dummy)
            InitializeRootNode();

            // Generates all of the children nodes.
            GenerateChildNodesRecursive(_quadNodesLocal[0]);

            // Thrash all but bottommost triangle references.
            for (int x = 0; x < _quadNodesLocal.Count; x++)
            {
                if (_quadNodesLocal[x].NodeIndices.NodeChild != 0)
                {
                    var quadNodeLocal = _quadNodesLocal[x];
                    quadNodeLocal.TrianglesInNode = new List<HeroesTriangle>();
                    _quadNodesLocal[x] = quadNodeLocal;
                }
            }

            // Copy quadnodes to array.
            QuadNodes = _quadNodesLocal.ToArray();

            // Find Neighbour Nodes Recursively
            FindNeighbourNodesRecursive(QuadNodes[0]);

            // Call GC
            GC.Collect();

            // Print Quadnode Count
            Console.Write("Quadnode Count: " + QuadNodes.Length + " | ");
        }

        /// <summary>
        /// Initializes the topmost root node within the quadnode hierarchy.
        /// </summary>
        private void InitializeRootNode()
        {
            // Allocate the Quadnode.
            Quadnode node = new Quadnode();

            // Set defaults for first node.
            node.NodeIndices = new NodeIndices();
            node.NodeNeighbours = new NodeNeighbours();

            node.NodeIndices.NodeIndex = 0;
            node.NodeIndices.NodeParent = 0;
            node.NodeIndices.NodeChild = 1;
            node.NodeNeighbours.RightNeightbourIndex = 0;
            node.NodeNeighbours.LeftNeightbourIndex = 0;
            node.NodeNeighbours.BottomNeighbourIndex = 0;
            node.NodeNeighbours.TopNeighbourIndex = 0;
            node.TriangleListOffset = 0;
            node.PositioningHorizontalOffset = 0;
            node.PositioningVerticalOffset = 0;
            node.DepthLevel = 0;

            // Add all triangles to initial node.
            node.TrianglesInNode = GeometryData.Triangles;

            // Add the individual node onto the nodes list.
            _quadNodesLocal.Add(node);
        }

        /// <summary>
        /// Generates an individual child node and its properties based off of the parent node.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="nodePosition"></param>
        private void GenerateChildNodesRecursive(Quadnode parentNode)
        {
            // Allocate new quadNodes
            #region Allocate New Quadnodes
            Quadnode topLeftNode = new Quadnode();
            Quadnode topRightNode = new Quadnode();
            Quadnode bottomLeftNode = new Quadnode();
            Quadnode bottomRightNode = new Quadnode();
            #endregion

            // Allocate index and neighbour location.
            #region Allocate Index & Neighbour Structs
            topLeftNode.NodeIndices = new NodeIndices();
            topLeftNode.NodeNeighbours = new NodeNeighbours();
            topRightNode.NodeIndices = new NodeIndices();
            topRightNode.NodeNeighbours = new NodeNeighbours();
            bottomLeftNode.NodeIndices = new NodeIndices();
            bottomLeftNode.NodeNeighbours = new NodeNeighbours();
            bottomRightNode.NodeIndices = new NodeIndices();
            bottomRightNode.NodeNeighbours = new NodeNeighbours();
            #endregion

            // Only for the 4 quadnodes which stem from the root of the function.
            #region Set Node Index
            topLeftNode.NodeIndices.NodeIndex = (ushort)_quadNodesLocal.Count();
            topRightNode.NodeIndices.NodeIndex = (ushort)(_quadNodesLocal.Count() + 1);
            bottomLeftNode.NodeIndices.NodeIndex = (ushort)(_quadNodesLocal.Count() + 2);
            bottomRightNode.NodeIndices.NodeIndex = (ushort)(_quadNodesLocal.Count() + 3);
            #endregion Set Node Index

            // Set parent node of nodes 1-2-3-4
            #region Set Node Parent
            topLeftNode.NodeIndices.NodeParent = parentNode.NodeIndices.NodeIndex;
            topRightNode.NodeIndices.NodeParent = parentNode.NodeIndices.NodeIndex;
            bottomLeftNode.NodeIndices.NodeParent = parentNode.NodeIndices.NodeIndex;
            bottomRightNode.NodeIndices.NodeParent = parentNode.NodeIndices.NodeIndex;
            #endregion Set Node Parent

            // Set default child node of nodes 1-2-3-4
            #region Default Child Node Pointer
            topLeftNode.NodeIndices.NodeChild = 0;
            topRightNode.NodeIndices.NodeChild = 0;
            bottomLeftNode.NodeIndices.NodeChild = 0;
            bottomRightNode.NodeIndices.NodeChild = 0;
            #endregion

            // Specify node depth level of the child node.
            #region Set Node Depth Level
            topLeftNode.DepthLevel = (byte)(parentNode.DepthLevel + 1);
            topRightNode.DepthLevel = (byte)(parentNode.DepthLevel + 1);
            bottomLeftNode.DepthLevel = (byte)(parentNode.DepthLevel + 1);
            bottomRightNode.DepthLevel = (byte)(parentNode.DepthLevel + 1);
            #endregion

            // Set child node for parent.
            #region Parent Node: Set Child Node
            Quadnode tempNode = _quadNodesLocal[parentNode.NodeIndices.NodeIndex];
            tempNode.NodeIndices.NodeChild = topLeftNode.NodeIndices.NodeIndex;
            _quadNodesLocal[parentNode.NodeIndices.NodeIndex] = tempNode;
            #endregion

            // Calculate the positioning horizontal and vertical offsets.
            // If a node is physically further right of the first node, add (2^basePower - depthLevel)
            #region Calculate Horizontal and Vertical Offset Values
            // Node 1
            topLeftNode.PositioningHorizontalOffset = parentNode.PositioningHorizontalOffset;
            topLeftNode.PositioningVerticalOffset = parentNode.PositioningVerticalOffset;

            // Node 2
            topRightNode.PositioningHorizontalOffset = (ushort)(parentNode.PositioningHorizontalOffset + Math.Pow(2, (BasePower - topRightNode.DepthLevel)));
            topRightNode.PositioningVerticalOffset = parentNode.PositioningVerticalOffset;

            // Node 3
            bottomLeftNode.PositioningHorizontalOffset = parentNode.PositioningHorizontalOffset;
            bottomLeftNode.PositioningVerticalOffset = (ushort)(parentNode.PositioningVerticalOffset + Math.Pow(2, (BasePower - bottomLeftNode.DepthLevel)));

            // Node 4
            bottomRightNode.PositioningHorizontalOffset = (ushort)(parentNode.PositioningHorizontalOffset + Math.Pow(2, (BasePower - bottomRightNode.DepthLevel)));
            bottomRightNode.PositioningVerticalOffset = (ushort)(parentNode.PositioningVerticalOffset + Math.Pow(2, (BasePower - bottomRightNode.DepthLevel)));
            #endregion

            // Set the local neighbours.
            #region Set Local Neighbours (Within Same Node)
            topLeftNode.NodeNeighbours.RightNeightbourIndex = topRightNode.NodeIndices.NodeIndex;
            topLeftNode.NodeNeighbours.BottomNeighbourIndex = bottomLeftNode.NodeIndices.NodeIndex;

            topRightNode.NodeNeighbours.LeftNeightbourIndex = topLeftNode.NodeIndices.NodeIndex;
            topRightNode.NodeNeighbours.BottomNeighbourIndex = bottomRightNode.NodeIndices.NodeIndex;

            bottomLeftNode.NodeNeighbours.TopNeighbourIndex = topLeftNode.NodeIndices.NodeIndex;
            bottomLeftNode.NodeNeighbours.RightNeightbourIndex = bottomRightNode.NodeIndices.NodeIndex;

            bottomRightNode.NodeNeighbours.TopNeighbourIndex = topRightNode.NodeIndices.NodeIndex;
            bottomRightNode.NodeNeighbours.LeftNeightbourIndex = bottomLeftNode.NodeIndices.NodeIndex;
            #endregion

            // Find triangle-node collisions for each node.
            #region Find Triangles Stored by Node
            QuadnodeUtilities.FindTriangles(ref topLeftNode, parentNode);
            QuadnodeUtilities.FindTriangles(ref topRightNode, parentNode);
            QuadnodeUtilities.FindTriangles(ref bottomLeftNode, parentNode);
            QuadnodeUtilities.FindTriangles(ref bottomRightNode, parentNode);
            #endregion

            // Add the nodes 1-2-3-4 onto the quadnodes list.
            _quadNodesLocal.Add(topLeftNode);
            _quadNodesLocal.Add(topRightNode);
            _quadNodesLocal.Add(bottomLeftNode);
            _quadNodesLocal.Add(bottomRightNode);

            // Recursively generate nodes down the tree until the target depth level.
            // Do so only if the child node has any triangles.
            if (topLeftNode.DepthLevel >= DepthLevel) return;

            // Recursively generate all children of top left first, then top right, then bottom left and bottom right.
            if (topLeftNode.TrianglesInNode.Count > 0) { GenerateChildNodesRecursive(topLeftNode); }
            if (topRightNode.TrianglesInNode.Count > 0) { GenerateChildNodesRecursive(topRightNode); }
            if (bottomLeftNode.TrianglesInNode.Count > 0) { GenerateChildNodesRecursive(bottomLeftNode); }
            if (bottomRightNode.TrianglesInNode.Count > 0) { GenerateChildNodesRecursive(bottomRightNode); }
        }

        /// <summary>
        /// Finds the neighbour nodes for the child nodes of the current nodes recursively.
        /// </summary>
        /// <param name="quadNode"></param>
        private void FindNeighbourNodesRecursive(Quadnode quadNode)
        {
            // Set the non-local neighbours.
            #region Set Non Local Neighbours (Relative to Parent - Optimization)

            // Obtain the children nodes to the current node.
            QuadnodeChildren childNodes = new QuadnodeChildren(quadNode);

            // If the current node has no children then return.
            if (childNodes.TopLeftNode == null) { return; }

            // Obtain the children for the children nodes.
            QuadnodeChildren topLeftChildren = new QuadnodeChildren(childNodes.TopLeftNode);
            QuadnodeChildren topRightChildren = new QuadnodeChildren(childNodes.TopRightNode);
            QuadnodeChildren bottomLeftChildren = new QuadnodeChildren(childNodes.BottomLeftNode);
            QuadnodeChildren bottomRightChildren = new QuadnodeChildren(childNodes.BottomRightNode);

            // Set relative neighbour relations for nodes in the quadtree struct.
            // NB: Looking at this code and are confused? You are just as I was when I was writing it.
            // Basically we are going down 2 depth levels from the current nodes, where the node total at the specified depth level
            // would be 16, and we are connecting the right side child nodes of the top left quadnodes to the left side child nodes
            // of the top right quadnodes. In the same way, we connect the bottom two quadnodes of the top left quadnode with the top 
            // two quadnodes of the bottom left quadnode etc. Fast neighbour finding.

            // The format of these statements
            // If (Node) Exists
            // Set the neighbour index to:
            // If exists, appropriate node, else parent of node (passed in ThisNode object).

            // Left-Right (Should be 4 sets)
            if (topLeftChildren.TopRightNode != null)
            { topLeftChildren.TopRightNode.NodeNeighbours.RightNeightbourIndex = topRightChildren.TopLeftNode != null ? topRightChildren.TopLeftNode.NodeIndices.NodeIndex : topRightChildren.ThisNode.NodeIndices.NodeIndex; }

            if (topRightChildren.TopLeftNode != null)
            { topRightChildren.TopLeftNode.NodeNeighbours.LeftNeightbourIndex = topLeftChildren.TopRightNode != null ? topLeftChildren.TopRightNode.NodeIndices.NodeIndex : topLeftChildren.ThisNode.NodeIndices.NodeIndex; }


            if (topLeftChildren.BottomRightNode != null)
            { topLeftChildren.BottomRightNode.NodeNeighbours.RightNeightbourIndex = topRightChildren.BottomLeftNode != null ? topRightChildren.BottomLeftNode.NodeIndices.NodeIndex : topRightChildren.ThisNode.NodeIndices.NodeIndex; }
            
            if (topRightChildren.BottomLeftNode != null)
            { topRightChildren.BottomLeftNode.NodeNeighbours.LeftNeightbourIndex = topLeftChildren.BottomRightNode != null ? topLeftChildren.BottomRightNode.NodeIndices.NodeIndex : topLeftChildren.ThisNode.NodeIndices.NodeIndex; }


            if (bottomLeftChildren.BottomRightNode != null)
            { bottomLeftChildren.BottomRightNode.NodeNeighbours.RightNeightbourIndex = bottomRightChildren.BottomLeftNode != null ? bottomRightChildren.BottomLeftNode.NodeIndices.NodeIndex : bottomRightChildren.ThisNode.NodeIndices.NodeIndex; }

            if (bottomRightChildren.BottomLeftNode != null)
            { bottomRightChildren.BottomLeftNode.NodeNeighbours.LeftNeightbourIndex = bottomLeftChildren.BottomRightNode != null ? bottomLeftChildren.BottomRightNode.NodeIndices.NodeIndex : bottomLeftChildren.ThisNode.NodeIndices.NodeIndex; }
            

            if (bottomRightChildren.TopLeftNode != null)
            { bottomRightChildren.TopLeftNode.NodeNeighbours.LeftNeightbourIndex = bottomLeftChildren.TopRightNode != null ? bottomLeftChildren.TopRightNode.NodeIndices.NodeIndex : bottomLeftChildren.ThisNode.NodeIndices.NodeIndex; }
            
            if (bottomLeftChildren.TopRightNode != null)
            { bottomLeftChildren.TopRightNode.NodeNeighbours.RightNeightbourIndex = bottomRightChildren.TopLeftNode != null ? bottomRightChildren.TopLeftNode.NodeIndices.NodeIndex : bottomRightChildren.ThisNode.NodeIndices.NodeIndex; }

            // Top-bottom (Should be 4 sets)
            if (topLeftChildren.BottomRightNode != null)
            { topLeftChildren.BottomRightNode.NodeNeighbours.BottomNeighbourIndex = bottomLeftChildren.TopRightNode != null ? bottomLeftChildren.TopRightNode.NodeIndices.NodeIndex : bottomLeftChildren.ThisNode.NodeIndices.NodeIndex; }

            if (bottomLeftChildren.TopRightNode != null)
            { bottomLeftChildren.TopRightNode.NodeNeighbours.TopNeighbourIndex = topLeftChildren.BottomRightNode != null ? topLeftChildren.BottomRightNode.NodeIndices.NodeIndex : topLeftChildren.ThisNode.NodeIndices.NodeIndex; }


            if (topLeftChildren.BottomLeftNode != null)
            { topLeftChildren.BottomLeftNode.NodeNeighbours.BottomNeighbourIndex = bottomLeftChildren.TopLeftNode != null ? bottomLeftChildren.TopLeftNode.NodeIndices.NodeIndex : bottomLeftChildren.ThisNode.NodeIndices.NodeIndex; }
            
            if (bottomLeftChildren.TopLeftNode != null)
            { bottomLeftChildren.TopLeftNode.NodeNeighbours.TopNeighbourIndex = topLeftChildren.BottomLeftNode != null ? topLeftChildren.BottomLeftNode.NodeIndices.NodeIndex : topLeftChildren.ThisNode.NodeIndices.NodeIndex; }
            

            if (topRightChildren.BottomRightNode != null)
            { topRightChildren.BottomRightNode.NodeNeighbours.BottomNeighbourIndex = bottomRightChildren.TopRightNode != null ? bottomRightChildren.TopRightNode.NodeIndices.NodeIndex : bottomRightChildren.ThisNode.NodeIndices.NodeIndex; }

            if (bottomRightChildren.TopRightNode != null)
            { bottomRightChildren.TopRightNode.NodeNeighbours.TopNeighbourIndex = topRightChildren.BottomRightNode != null ? topRightChildren.BottomRightNode.NodeIndices.NodeIndex : topRightChildren.ThisNode.NodeIndices.NodeIndex; }
            

            if (bottomRightChildren.TopLeftNode != null)
            { bottomRightChildren.TopLeftNode.NodeNeighbours.TopNeighbourIndex = topRightChildren.BottomLeftNode != null ? topRightChildren.BottomLeftNode.NodeIndices.NodeIndex : topRightChildren.ThisNode.NodeIndices.NodeIndex; }
            
            if (topRightChildren.BottomLeftNode != null)
            { topRightChildren.BottomLeftNode.NodeNeighbours.BottomNeighbourIndex = bottomRightChildren.TopLeftNode != null ? bottomRightChildren.TopLeftNode.NodeIndices.NodeIndex : bottomRightChildren.ThisNode.NodeIndices.NodeIndex; }

            // Write new quadnodes back to quadnodes array.
            topLeftChildren.ReplaceQuadnodes();
            topRightChildren.ReplaceQuadnodes();
            bottomLeftChildren.ReplaceQuadnodes();
            bottomRightChildren.ReplaceQuadnodes();
            #endregion

            // Find children.
            if (childNodes.TopLeftNode != null) { FindNeighbourNodesRecursive(childNodes.TopLeftNode); }
            if (childNodes.TopRightNode != null) { FindNeighbourNodesRecursive(childNodes.TopRightNode); }
            if (childNodes.BottomLeftNode != null) { FindNeighbourNodesRecursive(childNodes.BottomLeftNode); }
            if (childNodes.BottomRightNode != null) { FindNeighbourNodesRecursive(childNodes.BottomRightNode); }
        }

        /// <summary>
        /// Returns an array of Quadnodes at a specified depth level
        /// </summary>
        public Quadnode[] GetQuadnodesAtDepth(int depthLevel)
        {
            // Find the amount of quadnodes which has the requested depth level.
            int quadNodeCount = 0;
            for (int x = 0; x < QuadNodes.Length; x++)  {  if (QuadNodes[x].DepthLevel == depthLevel) { quadNodeCount += 1; } }

            // Return null if there are no nodes.
            if (quadNodeCount == 0) { return null; }

            // Allocate the list of quadNodes used for storage of nodes.
            Quadnode[] nodeList = new Quadnode[quadNodeCount];

            // Loop again to find the relevant nodes and assign them to the array.
            int nodeListIndex = 0;
            for (int x = 0; x < QuadNodes.Length; x++)  
            {  
                if (QuadNodes[x].DepthLevel == depthLevel) 
                { 
                    nodeList[nodeListIndex] = QuadNodes[x];
                    nodeListIndex += 1; 
                } 
            }

            // Return the quadnodes.
            return nodeList;
        }


        /// <summary>
        /// Calculates all of the neighbours for each individual node.
        /// </summary>
        public void CalculateNodeNeighbours()
        {
            // Retrieve nodes needing X,Y,Z neighbour.
            Quadnode[] nodesNeedingRight = QuadNodes.Where(x => x.NodeNeighbours.RightNeightbourIndex == 0).OrderByDescending(x => x.DepthLevel).ToArray();
            Quadnode[] nodesNeedingLeft = QuadNodes.Where(x => x.NodeNeighbours.LeftNeightbourIndex == 0).OrderByDescending(x => x.DepthLevel).ToArray();
            Quadnode[] nodesNeedingBottom = QuadNodes.Where(x => x.NodeNeighbours.TopNeighbourIndex == 0).OrderByDescending(x => x.DepthLevel).ToArray();
            Quadnode[] nodesNeedingTop = QuadNodes.Where(x => x.NodeNeighbours.BottomNeighbourIndex == 0).OrderByDescending(x => x.DepthLevel).ToArray();

            // For all nodes needing left, find nodes needing right.
            Thread searchRightThread = new Thread
            (x =>
            {
                for (int i = 0; i < nodesNeedingRight.Length; i++)
                {
                    // Get the offsets for the left node to this node.
                    int nodeOffset = (int)Math.Pow(2, (BasePower - nodesNeedingRight[i].DepthLevel));
                    int rightNodeOffset = (ushort)(nodesNeedingRight[i].PositioningHorizontalOffset + nodeOffset);

                    // Else search for correct right node.
                    for (int z = 0; z < nodesNeedingLeft.Length; z++)
                    {
                        if (nodesNeedingLeft[z].PositioningHorizontalOffset == rightNodeOffset)
                        {
                            nodesNeedingLeft[z].NodeNeighbours.LeftNeightbourIndex = nodesNeedingRight[i].NodeIndices.NodeIndex;
                            nodesNeedingRight[i].NodeNeighbours.RightNeightbourIndex = nodesNeedingLeft[z].NodeIndices.NodeIndex;
                            break;
                        }
                    }
                }
            }
            );

            // Start the Thread.
            searchRightThread.Start();

            // Search ourselves for top/bottom.
            for (int i = 0; i < nodesNeedingBottom.Length; i++)
            {
                // Get the offsets for the left node to this node.
                int nodeOffset = (int)Math.Pow(2, (BasePower - nodesNeedingBottom[i].DepthLevel));
                int bottomNodeOffset = (ushort)(nodesNeedingBottom[i].PositioningVerticalOffset + nodeOffset);

                // Else search for correct right node.
                for (int z = 0; z < nodesNeedingTop.Length; z++)
                {
                    if (nodesNeedingTop[z].PositioningVerticalOffset == bottomNodeOffset)
                    {
                        nodesNeedingTop[z].NodeNeighbours.TopNeighbourIndex = nodesNeedingBottom[i].NodeIndices.NodeIndex;
                        nodesNeedingBottom[i].NodeNeighbours.BottomNeighbourIndex = nodesNeedingTop[z].NodeIndices.NodeIndex;
                        break;
                    }
                }
            }


            // Wait for the Thread.
            searchRightThread.Join();

            // Write back the arrays to the main quadnodes struct.
            foreach (Quadnode quadNode in nodesNeedingLeft) { QuadNodes[quadNode.NodeIndices.NodeIndex].NodeNeighbours.LeftNeightbourIndex = quadNode.NodeNeighbours.LeftNeightbourIndex; }
            foreach (Quadnode quadNode in nodesNeedingRight) { QuadNodes[quadNode.NodeIndices.NodeIndex].NodeNeighbours.RightNeightbourIndex = quadNode.NodeNeighbours.RightNeightbourIndex; }
            foreach (Quadnode quadNode in nodesNeedingBottom) { QuadNodes[quadNode.NodeIndices.NodeIndex].NodeNeighbours.BottomNeighbourIndex = quadNode.NodeNeighbours.BottomNeighbourIndex; }
            foreach (Quadnode quadNode in nodesNeedingTop) { QuadNodes[quadNode.NodeIndices.NodeIndex].NodeNeighbours.TopNeighbourIndex = quadNode.NodeNeighbours.TopNeighbourIndex; }
        }
    }
}