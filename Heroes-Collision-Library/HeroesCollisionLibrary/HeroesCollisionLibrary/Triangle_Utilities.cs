using System;

namespace HeroesCollisionLibrary
{
    /// <summary>
    /// Defines various utility classes for performing mathematics on triangles.
    /// </summary>
    public static class Triangle_Utilities
    {
        /// <summary>
        /// Calculates the normal unit vector perpendicular to the vertices of a triangle.
        /// </summary>
        public static Geometry_Properties.Vertex CalculateNormal(Geometry_Properties.Vertex vertex1, Geometry_Properties.Vertex vertex2, Geometry_Properties.Vertex vertex3)
        {
            // Calculate the delta/difference in the XYZ coordinates of Vectors 2 - 1 and Vectors 3 - 1
            Geometry_Properties.Vertex vertexOne = CalculateVectorDifference(vertex1, vertex2);
            Geometry_Properties.Vertex vertexTwo = CalculateVectorDifference(vertex1, vertex3);

            // Calculate Vertex Normal - Cross Product 
            Geometry_Properties.Vertex vertexNormal = CalculateCrossProduct(vertexOne, vertexTwo);

            // Scale it to a unit vector to obtain Unit Normal.
            Geometry_Properties.Vertex unitNormal = NormalizeVector(vertexNormal);

            // Returns the unit normal.
            return unitNormal;
        }

        /// <summary>
        /// Returns true if the supplied vertex index is shared with another vertex in the passed in Triangle parameter.
        /// </summary>
        public static bool HasSharedVertex(ushort index, Geometry_Properties.Triangle triangle)
        {
            if ( index == triangle.vertexOne || index == triangle.vertexTwo || index == triangle.vertexThree ) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Calculates the delta/difference in the XYZ coordinates of Vectors 2 & 1. 2 minus 1. 
        /// </summary>
        private static Geometry_Properties.Vertex CalculateVectorDifference(Geometry_Properties.Vertex vertex1, Geometry_Properties.Vertex vertex2)
        {
            // Defines a unit vector normal to the three vertices.
            Geometry_Properties.Vertex vectorAverage = new Geometry_Properties.Vertex();

            // Calculates the vector averages.
            vectorAverage.X = vertex2.X - vertex1.X;
            vectorAverage.Y = vertex2.Y - vertex1.Y;
            vectorAverage.Z = vertex2.Z - vertex1.Z;

            // Return the vector average3
            return vectorAverage;
        }

        /// <summary>
        /// Calculates the Cross Product of two vectors, returning a normal vector to the two vectors. 
        /// </summary>
        private static Geometry_Properties.Vertex CalculateCrossProduct(Geometry_Properties.Vertex vertex1, Geometry_Properties.Vertex vertex2)
        {
            // Defines a unit vector normal to the three vertices.
            Geometry_Properties.Vertex crossProduct = new Geometry_Properties.Vertex();

            /// Example:
            /// [ i, j, k ] - i,j,k componentsa
            /// | 1, 2, 3 | - vertexOne
            /// [ 4, 5, 6 ] - vertexTwo
            
            crossProduct.X = (vertex1.Y * vertex2.Z) - (vertex1.Z * vertex2.Y);     //  [2(6) - 3(5)]i
            crossProduct.Y = -((vertex1.X * vertex2.Z) - (vertex1.Z * vertex2.X));  // -[1(6) - 3(4)]j
            crossProduct.Z = (vertex1.X * vertex2.Y) - (vertex1.Y * vertex2.X);     //  [1(5) - 2(4)]k

            // Return the vector average3
            return crossProduct;
        }

        /// <summary>
        /// Normalizes a vector, converting the vector into an equivalent vector of length 1.
        /// </summary>
        private static Geometry_Properties.Vertex NormalizeVector(Geometry_Properties.Vertex vertex)
        {
            // Defines a unit vector normal to the three vertices.
            Geometry_Properties.Vertex unitVector = new Geometry_Properties.Vertex();

            // Calculates the vector magnitude.
            double vectorMagnitude = Math.Pow((vertex.X * vertex.X) + (vertex.Y * vertex.Y) + (vertex.Z * vertex.Z), 0.5F);
            
            // Calculate all of the unit vector components.
            unitVector.X = (float)(vertex.X / vectorMagnitude);
            unitVector.Y = (float)(vertex.Y / vectorMagnitude);
            unitVector.Z = (float)(vertex.Z / vectorMagnitude);

            // Return the vector average3
            return unitVector;
        }
    }
}