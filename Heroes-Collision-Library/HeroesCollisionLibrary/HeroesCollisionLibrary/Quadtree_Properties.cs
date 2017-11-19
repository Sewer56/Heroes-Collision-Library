using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Provides structures and set of common classes helpful for working with quadtrees.
    /// </summary>
    public class Quadtree_Properties
    {
        /// <summary>
        /// List of all of the quadnodes used in the program.
        /// </summary>
        public List<Quadnode> quadNodes;

        /// <summary>
        /// An array of a list of rectangles which define the node bounding boxes, used for fast collision checking.
        /// </summary>
        public Quadtree_Properties.NodeRectangle[] nodeBoxes;

        /// <summary>
        /// The depth level of the quadnode structure stored in the quadtree properties class.
        /// </summary>
        public int depthLevel;

        /// <summary>
        /// The base Offset Power Level for the quadnode structure. It is unknown what this does.
        /// </summary>
        public int basePower;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public Quadtree_Properties(int _depthLevel, int _basePower)
        {
            // Calculate number of quadnodes using Geometric Progression based on the depth level.
            int quadNodeCount = 1;

            // Add the amount of nodes at each level to the total node count.
            for (int x = 2; x <= _depthLevel; x++)
            {
                // Add a(1-r^n)/1-r
                quadNodeCount += (int)( 1 * (1 - Math.Pow(4, _depthLevel)) / (1-_depthLevel) );
            }

            // Instantiate the list.
            quadNodes = new List<Quadnode>(quadNodeCount);

            // Copy the depth level
            depthLevel = _depthLevel;

            // Set default base power.
            basePower = _basePower;
        }

        /// <summary>
        /// Struct which represents an individual quadtree node.
        /// </summary>
        public struct Quadnode
        {
            /// <summary>
            /// Individual uniquely identifiable index.
            /// </summary>
            public int nodeIndex;

            /// <summary>
            /// The parent node of the current node.
            /// </summary>
            public int nodeParent;

            /// <summary>
            /// The index of the first out of four children of the node, 0 if no child. 
            /// </summary>
            public int nodeChild;

            /// <summary>
            /// The right neighbour to the current node. 0 if edge/none. It can be either at the at the same or at another depth level.
            /// </summary>
            public int rightNeightbourIndex;

            /// <summary>
            /// The left neighbour to the current node. 0 if edge/none. It can be either at the at the same or at another depth level.
            /// </summary>
            public int leftNeightbourIndex;

            /// <summary>
            /// The bottom neighbour to the current node. 0 if edge/none. It can be either at the at the same or at another depth level.
            /// </summary>
            public int bottomNeighbourIndex;

            /// <summary>
            /// The top neighbouring node. 0 if edge/none. It can be either at the at the same or at another depth level.
            /// </summary>
            public int topNeighbourIndex;

            /// <summary>
            /// Offset to first triangle in triangle list for this node.
            /// </summary>
            public uint triangleListOffset;

            /// <summary>
            /// The offset positioning value of the node in the horizontal direction. Calculated by: 2^(base power-depth level) + Parent's Offset Value, depending on the node. See the wiki!
            /// </summary>
            public int positioningHorizontalOffset;

            /// <summary>
            /// The offset positioning value of the node in the horizontal direction. Calculated by: 2^(base power-depth level) + Parent's Offset Value, depending on the node. See the wiki!
            /// </summary>
            public int positioningVerticalOffset;

            /// <summary>
            /// The individual depth level of the QuadNode.
            /// </summary>
            public int depthLevel;

            /// <summary>
            /// Represents a listing of all of the triangles within the node.
            /// </summary>
            public List<Geometry_Properties.Triangle> trianglesInNode;
        }
        
        /// <summary>
        /// A struct which represents a rectangle, lightweight version of Drawing.Rectangle.
        /// </summary>
        public struct NodeRectangle
        {
            /// <summary>
            /// The minimum X coordinate which defines the rectangle.
            /// </summary>
            public float MinX;
            /// <summary>
            /// The minimum Z coordinate which defines the rectangle.
            /// </summary>
            public float MinZ;
            /// <summary>
            /// The minimum X coordinate which defines the rectangle.
            /// </summary>
            public float MaxX;
            /// <summary>
            /// The maximum Z coordinate which defines the rectangle.
            /// </summary>
            public float MaxZ;
        }

        /// <summary>
        /// Generates all of the quadnodes.
        /// </summary>
        public void GenerateQuadnodes()
        {
            // Setup first node (a dummy)
            InitializeRootNode();

            // Generates all of the children nodes.
            // GenerateChildNodesIterative(); 
            GenerateChildNodesRecursive(quadNodes[0]);

            // Print Quadnode Count
            Console.Write("Quadnode Count: " + quadNodes.Count() + " | ");
        }

        /// <summary>
        /// Returns the size of a specific quadnode based on its depth level.
        /// </summary>
        /// <returns></returns>
        public float GetNodeSize(float quadtreeSize, int depthLevel) 
        { 
            // Divide the node size by the amount of levels the node is deep in.
            for (int x = 0; x < depthLevel; x++)
            {
                quadtreeSize = quadtreeSize / 2.0F;
            }

            // Return the node size.
            return quadtreeSize; 
        }

        /// <summary>
        /// Returns an array of Quadnodes at a specified depth level
        /// </summary>
        public Quadnode[] GetQuadnodesAtDepth(int depthLevel)
        {
            // Find the amount of quadnodes which has the requested depth level.
            int quadNodeCount = 0;
            for (int x = 0; x < quadNodes.Count; x++)  {  if (quadNodes[x].depthLevel == depthLevel) { quadNodeCount += 1; } }

            // Return null if there are no nodes.
            if (quadNodeCount == 0) { return null; }

            // Allocate the list of quadNodes used for storage of nodes.
            Quadnode[] nodeList = new Quadnode[quadNodeCount];

            // Loop again to find the relevant nodes and assign them to the array.
            int nodeListIndex = 0;
            for (int x = 0; x < quadNodes.Count; x++)  
            {  
                if (quadNodes[x].depthLevel == depthLevel) 
                { 
                    nodeList[nodeListIndex] = quadNodes[x];
                    nodeListIndex += 1; 
                } 
            }

            // Return the quadnodes.
            return nodeList;
        }


        /// <summary>
        /// Calculates all of the bounding boxes (used for collision checking) for an array of nodes supplied.
        /// </summary>
        public void CalculateAllNodeBoundingBoxes(int basePower, Collision_Header collisionData, float nodeScale)
        {
            // Allocate space for all of the rectangles representing the individual nodes.
            nodeBoxes = new NodeRectangle[quadNodes.Count];

            // Iterate over all of the depth levels, calculating the size of nodes at each respective depth level
            for (int x = 0; x < Int32.MaxValue; x++)
            {
                // Obtain all of the quadnodes at the specified depth level.
                Quadnode[] quadnodesAtDepth = GetQuadnodesAtDepth(x); 

                // Break if there are no nodes at depth level.
                if (quadnodesAtDepth == null) { break; }

                // Retrieve all of the node bounding boxes.
                Quadtree_Properties.NodeRectangle[] boundingBoxes = CalculateNodeBoundingBoxes(basePower, nodeScale, x, collisionData, quadnodesAtDepth);

                // Assign each of the bounding boxes to nodeBoxes.
                // Length of quadnodesAtDepth and boundingBoxes is equal in the case it is not explicitly clear.
                for (int i = 0; i < boundingBoxes.Length; i++)
                {
                    nodeBoxes[quadnodesAtDepth[i].nodeIndex] = boundingBoxes[i]; 
                }
            }
        }

        /// <summary>
        /// Sets the neighbour relationship of all child nodes within each parent node.
        /// </summary>
        public void CalculateLocalNodeNeighbours()
        {
            // Iterate over all depths bar the final depth.
            for (int x = 0; x < depthLevel; x++)
            {
                // Obtain all of the quadnodes at the specified depth level.
                Quadnode[] quadnodesAtDepth = GetQuadnodesAtDepth(x); 

                // For each quadnode at the depth level, set the children neighbour relationships.
                for (int z = 0; z < quadnodesAtDepth.Length; z++)
                {
                    // Ignore if the node has no children/
                    if (quadnodesAtDepth[z].nodeChild == 0) { continue; }

                    // Get the children node.
                    int childNode = quadnodesAtDepth[z].nodeChild;

                    // Retrieve all of the children nodes.
                    Quadnode childNode1 = quadNodes[childNode];
                    Quadnode childNode2 = quadNodes[childNode + 1];
                    Quadnode childNode3 = quadNodes[childNode + 2];
                    Quadnode childNode4 = quadNodes[childNode + 3];
                    
                    // Childnode 1,2,3,4 = top left, top right, bottom left, bottom right.

                    // Set relative neighbour relationships.
                    childNode1.rightNeightbourIndex = childNode2.nodeIndex;
                    childNode1.bottomNeighbourIndex = childNode3.nodeIndex;

                    childNode2.leftNeightbourIndex = childNode1.nodeIndex;
                    childNode2.bottomNeighbourIndex = childNode4.nodeIndex;

                    childNode3.topNeighbourIndex = childNode1.nodeIndex;
                    childNode3.rightNeightbourIndex = childNode4.nodeIndex;
                    
                    childNode4.topNeighbourIndex = childNode2.nodeIndex;
                    childNode4.leftNeightbourIndex = childNode3.nodeIndex;

                    // Replace original quadnodes.
                    quadNodes[childNode] = childNode1;
                    quadNodes[childNode + 1] = childNode2;
                    quadNodes[childNode + 2] = childNode3;
                    quadNodes[childNode + 3] = childNode4;
                }
            }
        }

        /// <summary>
        /// Calculates all of the neighbours for each individual node.
        /// </summary>
        public void CalculateNodeNeighbours()
        {
            // Options for Multi-Core Looping
            ParallelOptions ParallelZ = new ParallelOptions(); // Create a new configuration for parallelism.
            ParallelZ.MaxDegreeOfParallelism = Environment.ProcessorCount; // Equal amount of CPU Threads.

            // Iterate over all of the depth levels, calculating the size of nodes at each respective depth level
            Parallel.For (0, depthLevel + 1, ParallelZ, x=>
            {
                // Obtain all of the quadnodes at the specified depth level.
                Quadnode[] quadnodesAtDepth = GetQuadnodesAtDepth(x); 

                // Break if there are no nodes at depth level.
                if (quadnodesAtDepth.Length == 0) { return; }

                // Obtain Horizontal/Vertical Positioning Offset between each node, each increment of this value points to next node.
                // e.g. if this is 512, node to the right will have 512 more than the previous node's offset, vertically or horizontally.
                int nextNodeOffset = (int)Math.Pow(2, (basePower - depthLevel));

                // Find all of the neighbours to this node.
                Quadnode[] newQuadnodesAtDepth =  FindNeighbours(quadnodesAtDepth, nextNodeOffset);

                // Reassign nodes back into the nodes array containing new neighbours.
                for (int z = 0; z < newQuadnodesAtDepth.Length; z++)
                {
                    int nodeIndex = newQuadnodesAtDepth[z].nodeIndex; 
                    quadNodes[nodeIndex] = newQuadnodesAtDepth[z];
                }
            }
            );
        }

        /// <summary>
        /// Annihilates the empty nodes from the quadtree structure.
        /// </summary>
        public void RemoveEmptyNodes()
        {
            // Generate List of new Quadnodes
            List<Quadnode> newQuadNodes = new List<Quadnode>(quadNodes.Count);

            // Find empty nodes
            IdentifyEmptyNodes();

            // Rewrite the quadnodes list without the removed nodes.
            newQuadNodes = quadNodes.Where(x => x.nodeIndex != -1).ToList();

            // Print statistics.
            Console.Write("Old Quadnodes: " + quadNodes.Count +  " | New Quadnodes: " + newQuadNodes.Count + " | ");

            // Fix the parent-child relations.
            newQuadNodes = FixChildParentRelations(newQuadNodes);
            
            // Now correct indexes of all nodes
            for (int x = 0; x < newQuadNodes.Count; x++)
            {
                Quadnode quadNode = newQuadNodes[x];
                quadNode.nodeIndex = x;
                newQuadNodes[x] = quadNode;
            }

            // Swap lists
            quadNodes = newQuadNodes;
        }

        /// <summary>
        /// Identifies the empty nodes within the quadtree structure, invalidating the index of all children and discarding the child index.
        /// </summary>
        private void IdentifyEmptyNodes()
        {
            // Iterate over all quadnode depths, identifying nodes without triangles.
            for (int x = depthLevel - 1; x >= 0; x--)
            {
                // Get all quadnodes at specified depth level.
                Quadnode[] quadnodesAtDepth = GetQuadnodesAtDepth(x);

                // Iterate over every node at the depth level set.
                for (int z = 0; z < quadnodesAtDepth.Length; z++)
                {   
                    // If this node has no triangles, invalidate all children and remove children index.
                    if (quadnodesAtDepth[z].trianglesInNode.Count == 0) 
                    {
                        // Invalidate children of node.
                        SetChildrenOfNodeInvalid(quadnodesAtDepth[z]); 

                        // Set node child index to 0 & place back into Quadnodes collection.
                        Quadnode tempNode = quadnodesAtDepth[z]; tempNode.nodeChild = 0; quadNodes[quadnodesAtDepth[z].nodeIndex] = tempNode;
                    }
                }
            }
        }
        
        /// <summary>
        /// Fixes the parent-child relations of a list of quadnodes given that the list of quadnodes 
        /// </summary>
        private List<Quadnode> FixChildParentRelations(List<Quadnode> newQuadNodes)
        {
            // Fixing parent-child relations.
            // Source Quadnode
            for (int x = 1; x < newQuadNodes.Count; x++)
            {
                // Get Index of Current/Source Node
                int parentNodeIndex = newQuadNodes[x].nodeIndex;

                // Count of all children nodes found (optimization)
                int foundNodes = 0;

                // Iterate over nodes until we find the child of the parent.
                // Start z at x because due to the order we have generated the quadtree in, the nodes child nodes will always be at further array indexes.
                // Target Investigation Quadnode
                for (int z = x; z < newQuadNodes.Count; z++)
                {
                    // Find child which has the parent node set to our current node.
                    if (newQuadNodes[z].nodeParent == parentNodeIndex)
                    {
                        // Fix child node's parent index.
                        Quadnode childNode = newQuadNodes[z];
                        childNode.nodeParent = x;
                        newQuadNodes[z] = childNode;

                        // Fix parent node's child index if it is incorrectly set.
                        Quadnode parentNode = newQuadNodes[x];
                        parentNode.nodeChild = z - 3; // Will correctly be set when last child element is hit (this will be hit on every child element)
                        newQuadNodes[x] = parentNode;

                        // Increment counter and break if all 4 children have been found.
                        foundNodes += 1;
                        if (foundNodes == 4) { break; }
                    }

                }
            }

            // Return list of QuadNodes
            return newQuadNodes;
        }


        /// <summary>
        /// Set all of the children nodes' indices of a passed in node to -1 to mark them for removal;
        /// </summary>
        private void SetChildrenOfNodeInvalid(Quadnode quadNode)
        {
            // If the node has no children, return.
            if (quadNode.nodeChild == 0) { return; }

            // Retrieve the quadnodes.
            Quadnode tempnode1 = quadNodes[quadNode.nodeChild];
            Quadnode tempnode2 = quadNodes[quadNode.nodeChild + 1];
            Quadnode tempnode3 = quadNodes[quadNode.nodeChild + 2];
            Quadnode tempnode4 = quadNodes[quadNode.nodeChild + 3];
            
            // Set indices
            tempnode1.nodeIndex = -1;
            tempnode2.nodeIndex = -1;
            tempnode3.nodeIndex = -1;
            tempnode4.nodeIndex = -1;

            // Set the nodes back with the invalid indices.
            quadNodes[quadNode.nodeChild] = tempnode1;
            quadNodes[quadNode.nodeChild + 1] = tempnode2;
            quadNodes[quadNode.nodeChild + 2] = tempnode3;
            quadNodes[quadNode.nodeChild + 3] = tempnode4;
        }

        /// <summary>
        /// Uses iteration to find the neighbours of each individual node.
        /// </summary>
        private Quadnode[] FindNeighbours(Quadnode[] quadnodesAtDepth, int nodeOffset)
        {
            Quadnode[] quadNodeList = quadnodesAtDepth;

            // Iterate over all of the source nodes twiceover to find neighbours.
            for (int x = 0; x < quadNodeList.Length; x++)
            {
                // If finding right is necessary
                if (quadNodeList[x].rightNeightbourIndex == 0)
                {
                    // Iterate over all of the target nodes.
                    for (int i = 0; i < quadNodeList.Length; i++)
                    {
                        // Right
                        if (quadNodeList[x].positioningHorizontalOffset + nodeOffset == quadNodeList[i].positioningHorizontalOffset) 
                        { 
                            quadNodeList[x].rightNeightbourIndex = quadNodeList[i].nodeIndex; 
                            quadNodeList[i].leftNeightbourIndex = quadNodeList[x].nodeIndex;
                            break;
                        }
                    }
                }

                // If finding left is necessary
                if (quadNodeList[x].leftNeightbourIndex == 0)
                {
                    // Iterate over all of the target nodes.
                    for (int i = 0; i < quadNodeList.Length; i++)
                    {
                        // Left
                        if (quadNodeList[x].positioningHorizontalOffset - nodeOffset == quadNodeList[i].positioningHorizontalOffset) 
                        { 
                            quadNodeList[x].leftNeightbourIndex = quadNodeList[i].nodeIndex; 
                            quadNodeList[i].rightNeightbourIndex = quadNodeList[x].nodeIndex;
                            break;
                        }
                    }
                }

                // If finding bottom is necessary
                if (quadNodeList[x].bottomNeighbourIndex == 0)
                {
                    // Iterate over all of the target nodes.
                    for (int i = 0; i < quadNodeList.Length; i++)
                    {
                        // Bottom
                        if (quadNodeList[x].positioningVerticalOffset + nodeOffset == quadNodeList[i].positioningVerticalOffset) 
                        { 
                            quadNodeList[x].bottomNeighbourIndex = quadNodeList[i].nodeIndex; 
                            quadNodeList[i].topNeighbourIndex = quadNodeList[x].nodeIndex;
                            break;
                        }

                    }
                }

                // If finding top is necessary
                if (quadNodeList[x].topNeighbourIndex == 0)
                {
                    // Iterate over all of the target nodes.
                    for (int i = 0; i < quadNodeList.Length; i++)
                    {
                        // Top
                        if (quadNodeList[x].positioningVerticalOffset - nodeOffset == quadNodeList[i].positioningVerticalOffset) 
                        { 
                            quadNodeList[x].topNeighbourIndex = quadNodeList[i].nodeIndex;
                            quadNodeList[i].bottomNeighbourIndex = quadNodeList[x].nodeIndex;
                            break;
                        }
                    }
                }
            }

            // Return the nodes
            return quadNodeList;
        }

        /// <summary>
        /// Calculates all of the bounding boxes (used for collision checking) for an array of nodes supplied.
        /// </summary>
        public Quadtree_Properties.NodeRectangle[] CalculateNodeBoundingBoxes(int basePower, float nodeScale, int depthLevel, Collision_Header collisionData, Quadnode[] quadNodes)
        {
            // Allocate space for all of the rectangles representing the individual nodes.
            NodeRectangle[] nodeRectangle = new NodeRectangle[quadNodes.Length];

            // Obtain the size of a node at this specified node depth.
            float nodeSize = GetNodeSize(collisionData.quadtreeSize, depthLevel);

            // Obtain Horizontal/Vertical Positioning Offset between each node, each increment of this value points to next node.
            // e.g. if this is 512, node to the right will have 512 more than the previous node's offset, vertically or horizontally.
            int nextNodeOffset = (int)Math.Pow(2, (basePower - depthLevel));

            // Represents the top edge of the quadtree. 
            float topEdge = collisionData.quadtreeCenter.Z - (collisionData.quadtreeSize / 2.0F);

            // Represents the left edge of the quadtree.
            float leftEdge = collisionData.quadtreeCenter.X - (collisionData.quadtreeSize / 2.0F);

            // Iterate over all of the quadnodes, obtaining the position of each edge.
            for (int x = 0; x < nodeRectangle.Length; x++)
            {
                // Calculate the amount of nodes offset from the top left node horizontally and vertically.
                int nodesRight = quadNodes[x].positioningHorizontalOffset / nextNodeOffset;
                int nodesDown = quadNodes[x].positioningVerticalOffset / nextNodeOffset;

                // Obtain Node Centers
                float nodeCenterX = leftEdge + (nodesRight * nodeSize) + (nodeSize / 2.0F);
                float nodeCenterY = topEdge + (nodesDown * nodeSize) + (nodeSize / 2.0F);

                // Set the minimums and maximums.
                nodeRectangle[x].MinX = nodeCenterX - ((nodeSize * nodeScale) / 2.0F);
                nodeRectangle[x].MaxX = nodeCenterX + ((nodeSize * nodeScale) / 2.0F);

                nodeRectangle[x].MinZ = nodeCenterY - ((nodeSize * nodeScale) / 2.0F); 
                nodeRectangle[x].MaxZ = nodeCenterY + ((nodeSize * nodeScale) / 2.0F); 
            }

            // Return all of the node rectangle bounding boxes.
            return nodeRectangle;
        }

        /// <summary>
        /// Initializes the topmost root node within the quadnode hierarchy.
        /// </summary>
        private void InitializeRootNode()
        {
            // Allocate the Quadnode.
            Quadnode node = new Quadnode();
            
            // Set defaults for first node.
            node.nodeIndex = 0;
            node.nodeParent = 0;
            node.nodeChild = 1;
            node.rightNeightbourIndex = 0;
            node.leftNeightbourIndex = 0;
            node.bottomNeighbourIndex = 0;
            node.topNeighbourIndex = 0;
            node.triangleListOffset = 0;
            node.positioningHorizontalOffset = 0;
            node.positioningVerticalOffset = 0;
            node.depthLevel = 0;

            // Add the individual node onto the nodes list.
            quadNodes.Add(node);
        }

        /// <summary>
        /// Initializes all of the children nodes to the quadnode, until the depth level is met.
        /// </summary>
        private void GenerateChildNodesIterative()
        {
            // This loop generates children of nodes, depth level x. 
            // Starts at 0 to match depth level of first created dummy node.
            for (int x = 0; x < depthLevel; x++)
            {
                // Retrieve all nodes within depth level x.
                Quadnode[] quadNodesLocal = GetQuadnodesAtDepth(x);

                // For each node of depth level X, generate the child nodes.
                foreach(Quadnode node in quadNodesLocal)
                {
                    GenerateChildNode(node, 1); // Top left
                    GenerateChildNode(node, 2); // Top right
                    GenerateChildNode(node, 3); // Bottom left
                    GenerateChildNode(node, 4); // Bottom right
                }
            }
        } 

        /// <summary>
        /// Generates an individual child node and its properties based off of the parent node.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="nodePosition"></param>
        private void GenerateChildNode(Quadnode parentNode, byte nodePosition)
        {
            // Allocate new quadNode
            Quadnode node = new Quadnode();

            // Set Properties!
            node.nodeIndex = quadNodes.Count(); // Length is 1 indexed, nodes are 0 indexed.
            node.nodeParent = parentNode.nodeIndex;
            node.nodeChild = 0;
            node.depthLevel = (byte)(parentNode.depthLevel + 1);

            // Calculate the node horiztontal and vertical offsets.
            // If a node is physically further right of the first node, add (2^basePower - depthLevel)
            switch (nodePosition)
            {
                case 1:
                    // Get parent node and assign child node. 
                    Quadnode tempNode = quadNodes[parentNode.nodeIndex];
                    tempNode.nodeChild = node.nodeIndex;
                    quadNodes[parentNode.nodeIndex] = tempNode;

                    // Calculate horizontal/vertical offset.
                    node.positioningHorizontalOffset = parentNode.positioningHorizontalOffset;
                    node.positioningVerticalOffset = parentNode.positioningVerticalOffset;
                    break;
                case 2:  

                    // Calculate horizontal/vertical offset.
                    node.positioningHorizontalOffset = (ushort)(parentNode.positioningHorizontalOffset + Math.Pow(2, (basePower - node.depthLevel)));
                    node.positioningVerticalOffset = parentNode.positioningVerticalOffset;
                    break;
                case 3:  

                    // Calculate horizontal/vertical offset.
                    node.positioningHorizontalOffset = parentNode.positioningHorizontalOffset;
                    node.positioningVerticalOffset = (ushort)(parentNode.positioningVerticalOffset + Math.Pow(2, (basePower - node.depthLevel)));
                    break;
                case 4:  

                    // Calculate horizontal/vertical offset.
                    node.positioningHorizontalOffset = (ushort)(parentNode.positioningHorizontalOffset + Math.Pow(2, (basePower - node.depthLevel)));
                    node.positioningVerticalOffset = (ushort)(parentNode.positioningVerticalOffset + Math.Pow(2, (basePower - node.depthLevel)));
                    break;
                default: break;
            }

            // Add node onto node list.
            quadNodes.Add(node);
        }

                /// <summary>
        /// Generates an individual child node and its properties based off of the parent node.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="nodePosition"></param>
        private void GenerateChildNodesRecursive(Quadnode parentNode)
        {
            // Allocate new quadNodes
            Quadnode topLeftNode = new Quadnode();
            Quadnode topRightNode = new Quadnode();
            Quadnode bottomLeftNode = new Quadnode();
            Quadnode bottomRightNode = new Quadnode();

            // Only for the 4 quadnodes which stem from the root of the function.
            topLeftNode.nodeIndex = quadNodes.Count();
            topRightNode.nodeIndex = quadNodes.Count() + 1;
            bottomLeftNode.nodeIndex = quadNodes.Count() + 2;
            bottomRightNode.nodeIndex = quadNodes.Count() + 3;

            // Set parent node of nodes 1-2-3-4
            topLeftNode.nodeParent = parentNode.nodeIndex;
            topRightNode.nodeParent = parentNode.nodeIndex;
            bottomLeftNode.nodeParent = parentNode.nodeIndex;
            bottomRightNode.nodeParent = parentNode.nodeIndex;

            // Set default child node of nodes 1-2-3-4
            topLeftNode.nodeChild = 0;
            topRightNode.nodeChild = 0;
            bottomLeftNode.nodeChild = 0;
            bottomRightNode.nodeChild = 0;

            // Specify node depth level of the child node.
            topLeftNode.depthLevel = (byte)(parentNode.depthLevel + 1);
            topRightNode.depthLevel = (byte)(parentNode.depthLevel + 1);
            bottomLeftNode.depthLevel = (byte)(parentNode.depthLevel + 1);
            bottomRightNode.depthLevel = (byte)(parentNode.depthLevel + 1);

            // Set child node for parent.
            Quadnode tempNode = quadNodes[parentNode.nodeIndex];
            tempNode.nodeChild = topLeftNode.nodeIndex;
            quadNodes[parentNode.nodeIndex] = tempNode;

            // Calculate the positioning horizontal and vertical offsets.
            // If a node is physically further right of the first node, add (2^basePower - depthLevel)

            // Node 1
            topLeftNode.positioningHorizontalOffset = parentNode.positioningHorizontalOffset;
            topLeftNode.positioningVerticalOffset = parentNode.positioningVerticalOffset;

            // Node 2
            topRightNode.positioningHorizontalOffset = (ushort)( parentNode.positioningHorizontalOffset + Math.Pow(2, (basePower - topRightNode.depthLevel)) );
            topRightNode.positioningVerticalOffset = parentNode.positioningVerticalOffset;

            // Node 3
            bottomLeftNode.positioningHorizontalOffset = parentNode.positioningHorizontalOffset;
            bottomLeftNode.positioningVerticalOffset = (ushort)( parentNode.positioningVerticalOffset + Math.Pow(2, (basePower - bottomLeftNode.depthLevel)) );

            // Node 4
            bottomRightNode.positioningHorizontalOffset = (ushort)( parentNode.positioningHorizontalOffset + Math.Pow(2, (basePower - bottomRightNode.depthLevel)) );
            bottomRightNode.positioningVerticalOffset = (ushort)( parentNode.positioningVerticalOffset + Math.Pow(2, (basePower - bottomRightNode.depthLevel)) );

            // Add the nodes 1-2-3-4 onto the quadnodes list.
            quadNodes.Add(topLeftNode);
            quadNodes.Add(topRightNode);
            quadNodes.Add(bottomLeftNode);
            quadNodes.Add(bottomRightNode);

            // Recursively generate nodes down the tree until the target depth level.
            if ( topLeftNode.depthLevel < depthLevel )
            {
                // Recursively generate all children of top left first, then top right, then bottom left and bottom right.
                GenerateChildNodesRecursive(topLeftNode);
                GenerateChildNodesRecursive(topRightNode);
                GenerateChildNodesRecursive(bottomLeftNode);
                GenerateChildNodesRecursive(bottomRightNode);
            }
            else
            {
                // Once we have generated enough nodes down, return.
                return;
            }
        }
    }
}