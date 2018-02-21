using HeroesCollisionLibrary.Geometry;
using HeroesCollisionLibrary.Geometry.Structures;
using System;
using System.Runtime.CompilerServices;

namespace HeroesCollisionLibrary.Collision
{
    /// <summary>
    /// Class which provides methods which allow us to check collision betweeen various objects such as triangles and nodes.
    /// </summary>
    public class CollisionChecker
    {
        /// <summary>
        /// Accurately checks node-triangle collisions, down to the individual vertices.
        /// </summary>
        /// <param name="triangle">The triangle to check passed in node collision against.</param>
        /// <param name="nodeRectangle">The bounding box square representing the current node (performance optimization).</param>
        /// <returns>True if there is an intersection, otherwise false.</returns>
        public static bool CheckCollision(HeroesTriangle triangle, Rectangle nodeRectangle)
        {
            // First check if the bounding boxes intersect, if they don't, discard the operation.
            if (!BoundingBoxIntersect(nodeRectangle, GeometryData.TriangleBoxes[triangle.TriangleIndex])) { return false; }

            // Check all vertices for presence in rectangle for possible fast return.
            // If any of the vertices is inside the node, then there must be a collision.
            if (IsVertexInRectangle(GeometryData.Vertices[triangle.VertexOne], nodeRectangle)) { return true; }
            if (IsVertexInRectangle(GeometryData.Vertices[triangle.VertexTwo], nodeRectangle)) { return true; }
            if (IsVertexInRectangle(GeometryData.Vertices[triangle.VertexThree], nodeRectangle)) { return true; }

            // Define vertices of the Rectangles.
            Vertex topLeftNodeEdge = new Vertex(nodeRectangle.MinX, 0, nodeRectangle.MinZ);
            Vertex topRightNodeEdge = new Vertex(nodeRectangle.MaxX, 0, nodeRectangle.MinZ);
            Vertex bottomRightNodeEdge = new Vertex(nodeRectangle.MaxX, 0, nodeRectangle.MaxZ);
            Vertex bottomLeftNodeEdge = new Vertex(nodeRectangle.MaxX, 0, nodeRectangle.MaxZ);

            // Define triangle vertices.
            Vertex triangleVertexOne = GeometryData.Vertices[triangle.VertexOne];
            Vertex triangleVertexTwo = GeometryData.Vertices[triangle.VertexTwo];
            Vertex triangleVertexThree = GeometryData.Vertices[triangle.VertexThree];

            // Then check if any of the node line segments intersect the triangle line segments.
            // We basically check if there are intersections between any of the three line segments of the triangle
            // i.e. A=>B, B=>C and C=>A and the four edges of the rectangle.
            #region Line Intersection Tests: Triangle line segments (vertex X to vertex Y) against node up down left right edges.

            // Check the top edge of the against the triangle line segments.
            if (LineIntersectionTest(triangleVertexOne, triangleVertexTwo, topLeftNodeEdge, topRightNodeEdge)) { return true; }
            if (LineIntersectionTest(triangleVertexTwo, triangleVertexThree, topLeftNodeEdge, topRightNodeEdge)) { return true; }
            if (LineIntersectionTest(triangleVertexThree, triangleVertexOne, topLeftNodeEdge, topRightNodeEdge)) { return true; }

            // Check the left edge of the against the triangle line segments.
            if (LineIntersectionTest(triangleVertexOne, triangleVertexTwo, topLeftNodeEdge, bottomLeftNodeEdge)) { return true; }
            if (LineIntersectionTest(triangleVertexTwo, triangleVertexThree, topLeftNodeEdge, bottomLeftNodeEdge)) { return true; }
            if (LineIntersectionTest(triangleVertexThree, triangleVertexOne, topLeftNodeEdge, bottomLeftNodeEdge)) { return true; }

            // Check the bottom edge of the against the triangle line segments.
            if (LineIntersectionTest(triangleVertexOne, triangleVertexTwo, bottomLeftNodeEdge, bottomRightNodeEdge)) { return true; }
            if (LineIntersectionTest(triangleVertexTwo, triangleVertexThree, bottomLeftNodeEdge, bottomRightNodeEdge)) { return true; }
            if (LineIntersectionTest(triangleVertexThree, triangleVertexOne, bottomLeftNodeEdge, bottomRightNodeEdge)) { return true; }

            // Check the right edge of the against the triangle line segments.
            if (LineIntersectionTest(triangleVertexOne, triangleVertexTwo, topRightNodeEdge, bottomRightNodeEdge)) { return true; }
            if (LineIntersectionTest(triangleVertexTwo, triangleVertexThree, topRightNodeEdge, bottomRightNodeEdge)) { return true; }
            if (LineIntersectionTest(triangleVertexThree, triangleVertexOne, topRightNodeEdge, bottomRightNodeEdge)) { return true; }

            #endregion Line Intersection Tests: Triangle line segments (vertex X to vertex Y) against node up down left right edges.

            // Understanding the code below:
            // Consider the triangle as three vectors, in a fixed rotation order A=>B, B=>C, C=>A
            // Now consider each vertex of the triangle and each square vertex, compute the cross product between each triangle edge and rectangle
            // vector (3*4=12 comparisons in total). If all of the cross products are of the same sign, or zero, the triangle is inside.

            // Conceptually we are determining whether the vertices are on the left or right side of the line, albeit the side is not cared.
            // We do not care whether it is left or right specifically, or whether it is in clockwise or anticlockwise order, only that all of the vertices
            // of the square are on the same side of the lines.
            // i.e. if all of the vertices are on the same side of each line, the vertices are "trapped" between the 3 lines, meaning that they must be
            // inside the triangle.
            #region Node Inside Triangle Tests: Determine if all of the node vertices are on the right side of the line, with vertices going clockwise.

            // Vertex One   = A
            // Vertex Two   = B
            // Vertex Three = C
            // Note: Variable names are completely arbitrary, to make code less confusing/long

            // Compare line A=>B with all edge vertices.
            bool v1 = IsPointRightOfLine(triangleVertexOne, triangleVertexTwo, topLeftNodeEdge);
            bool v2 = IsPointRightOfLine(triangleVertexOne, triangleVertexTwo, topRightNodeEdge);
            bool v3 = IsPointRightOfLine(triangleVertexOne, triangleVertexTwo, bottomRightNodeEdge);
            bool v4 = IsPointRightOfLine(triangleVertexOne, triangleVertexTwo, bottomLeftNodeEdge);

            // Compare line B=>C with all edge vertices.
            bool v5 = IsPointRightOfLine(triangleVertexTwo, triangleVertexThree, topLeftNodeEdge);
            bool v6 = IsPointRightOfLine(triangleVertexTwo, triangleVertexThree, topRightNodeEdge);
            bool v7 = IsPointRightOfLine(triangleVertexTwo, triangleVertexThree, bottomRightNodeEdge);
            bool v8 = IsPointRightOfLine(triangleVertexTwo, triangleVertexThree, bottomLeftNodeEdge);

            // Compare line C=>A with all edge vertices.
            bool v9 = IsPointRightOfLine(triangleVertexThree, triangleVertexOne, topLeftNodeEdge);
            bool v10 = IsPointRightOfLine(triangleVertexThree, triangleVertexOne, topRightNodeEdge);
            bool v11 = IsPointRightOfLine(triangleVertexThree, triangleVertexOne, bottomRightNodeEdge);
            bool v12 = IsPointRightOfLine(triangleVertexThree, triangleVertexOne, bottomLeftNodeEdge);

            // Check whether the node is inside the triangle.
            if (v1.AllEqual(v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12))
            {
                return true;
            }

            #endregion Node Inside Triangle Tests: Determine if all of the node vertices are on the right side of the line, with vertices going clockwise.

            // Else return false;
            return false;
        }

        /// <summary>
        /// Verifies whether two lines intersect with each other. (Only X & Z are used)
        /// </summary>
        /// <param name="lineVertex1">The vertex which marks the start of the first line.</param>
        /// <param name="lineVertex2">The vertex which marks the end of the first line.</param>
        /// <param name="targetVertex1">The vertex which marks the start of the second line.</param>
        /// <param name="targetVertex2">The vertex which marks the end of the second line.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LineIntersectionTest(Vertex lineVertex1, Vertex lineVertex2, Vertex targetVertex1, Vertex targetVertex2)
        {
            // Check for bounding box intersections of lines first.
            Rectangle boundingBox1 = GetBoundingBox(lineVertex1, lineVertex2);
            Rectangle boundingBox2 = GetBoundingBox(targetVertex1, targetVertex2);

            // If the bounding boxes do not intersect, return false.
            if (!BoundingBoxIntersect(boundingBox1, boundingBox2)) { return false; }

            // Check if the line segments defining the triangle's edges and the square's edges intersect.
            if (!LineSegmentTouchesOrCrossesLine(lineVertex1, lineVertex2, targetVertex1, targetVertex2)) { return false; }

            // Lines intersect.
            return true;
        }

        /// <summary>
        /// Checks collision between two boxes, if they collide, return true.
        /// The checks are performed on the edges of the boxes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BoundingBoxIntersect(Rectangle thisRectangle, Rectangle nodeRectangle)
        {
            if (thisRectangle.MaxX < nodeRectangle.MinX) return false; // if a is left of b
            if (thisRectangle.MinX > nodeRectangle.MaxX) return false; // if a is right of b
            if (thisRectangle.MaxZ < nodeRectangle.MinZ) return false; // if a is above b
            if (thisRectangle.MinZ > nodeRectangle.MaxZ) return false; // if a is below b
            return true; // boxes overlap
        }

        /// <summary>
        /// Returns true if the line segment touches or crosses a line.
        /// </summary>
        /// <param name="lineVertex1">The vertex which marks the start of the first line.</param>
        /// <param name="lineVertex2">The vertex which marks the end of the first line.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LineSegmentTouchesOrCrossesLine(Vertex lineVertex1, Vertex lineVertex2, Vertex targetLineVertex1, Vertex targetLineVertex2)
        {
            // Check if either of the points we are checking of the second line are on the line (just in case).
            // Confirm if the two vertices are on the opposite end of the line.

            return IsPointOnLine(lineVertex1, lineVertex2, targetLineVertex1) ||
                   IsPointOnLine(lineVertex1, lineVertex2, targetLineVertex2) ||
                   (IsPointRightOfLine(lineVertex1, lineVertex2, targetLineVertex1) ^ IsPointRightOfLine(lineVertex1, lineVertex2, targetLineVertex2));
        }

        /// <summary>
        /// Checks if there the supplied point targetVertex is on the line defined by lineVertex1 & lineVertex2
        /// </summary>
        /// <param name="lineVertex1">The vertex which marks the start of the first line.</param>
        /// <param name="lineVertex2">The vertex which marks the end of the first line.</param>
        /// <param name="targetVertex">The vertex which we are checking against.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointOnLine(Vertex lineVertex1, Vertex lineVertex2, Vertex targetVertex)
        {
            // Move the line defined by lineVertex1, lineVertex2 such that the vertex defines the vector of a line that crosses the point 0,0.
            // More specifically: Move the end vertex to a location that is an offset of target vertex - source vertex. i.e. make it a vector relative to 0,0
            Vertex tempVector = new Vertex(lineVertex2.X - lineVertex1.X, 0, lineVertex2.Z - lineVertex1.Z);

            // Move the point we are comparing defined by targetVertex such that it is an offset/vector from 0,0 (another line)
            // Our assumption is that linevertex1 (original start point of first line segment) is located at 0,0 , and we want to define everything
            // relative to that specific point, thus both offsetting the end of the line segment and our target vertex.
            Vertex tempPoint = new Vertex(targetVertex.X - lineVertex1.X, 0, targetVertex.Z - lineVertex1.Z);

            // Obtain the Cross Product, if it is very close, within a certain range then the point is on the line.
            double crossProduct = CrossProduct(tempVector, tempPoint);

            // Extremely small margin of error.
            return Math.Abs(crossProduct) < 0.000001;
        }

        /// <summary>
        /// Checks if there the supplied point targetVertex is on the line defined by lineVertex1 & lineVertex2
        /// </summary>
        /// <param name="lineVertex1">The vertex which marks the start of the first line.</param>
        /// <param name="lineVertex2">The vertex which marks the end of the first line.</param>
        /// <param name="targetVertex">The vertex which we are checking against.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPointRightOfLine(Vertex lineVertex1, Vertex lineVertex2, Vertex targetVertex)
        {
            // Move the line defined by lineVertex1, lineVertex2 such that the vertex defines the vector of a line that crosses the point 0,0.
            // More specifically: Move the end vertex to a location that is an offset of target vertex - source vertex. i.e. make it a vector relative to 0,0
            Vertex tempVector = new Vertex(lineVertex2.X - lineVertex1.X, 0, lineVertex2.Z - lineVertex1.Z);

            // Move the point we are comparing defined by targetVertex such that it is an offset/vector from 0,0 (another line)
            // Our assumption is that linevertex1 (original start point of first line segment) is located at 0,0 , and we want to define everything
            // relative to that specific point, thus both offsetting the end of the line segment and our target vertex.
            Vertex tempPoint = new Vertex(targetVertex.X - lineVertex1.X, 0, targetVertex.Z - lineVertex1.Z);

            // Obtain the Cross Product, if it is very close, within a certain range then the point is on the line.
            double crossProduct = CrossProduct(tempVector, tempPoint);

            // Extremely small margin of error.
            return crossProduct < 0;
        }

        /// <summary>
        /// Calculates the Cross Product of Two Vertices/Points, using their X & Z Coordinates.
        /// Note: Cross product of two 2D vectors is ill defined, this is much rather the determinant.
        /// </summary>
        /// <param name="firstVertex"></param>
        /// <param name="secondVertex"></param>
        /// <returns></returns>
        public static double CrossProduct(Vertex firstVertex, Vertex secondVertex)
        {
            return firstVertex.X * secondVertex.Z - secondVertex.X * firstVertex.Z;
        }

        /// <summary>
        /// Returns true if a vertex is contained within the boundaries of a 2D Rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle (with the coordinates of the 4 edges) representing the box.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVertexInRectangle(Vertex vertex, Rectangle rectangle)
        {
            if
            (
                (vertex.X >= rectangle.MinX && vertex.X <= rectangle.MaxX) &&
                (vertex.Z >= rectangle.MinZ && vertex.Z <= rectangle.MaxZ)
            ) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Retrieves a bounding box for a set of two supplied vertices.
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetBoundingBox(Vertex lineVertex1, Vertex lineVertex2)
        {
            // Define bounding box
            Rectangle boundingBox = new Rectangle();

            // Determine Max X
            if (lineVertex1.X > lineVertex2.X) { boundingBox.MaxX = lineVertex1.X; boundingBox.MinX = lineVertex2.X; }
            else { boundingBox.MaxX = lineVertex2.X; boundingBox.MinX = lineVertex1.X; }

            // Determine Max Z
            if (lineVertex1.Z > lineVertex2.Z) { boundingBox.MaxZ = lineVertex1.Z; boundingBox.MinZ = lineVertex2.Z; }
            else { boundingBox.MaxZ = lineVertex2.Z; boundingBox.MinZ = lineVertex1.Z; }

            // Return bounding box.
            return boundingBox;
        }
    }
}
