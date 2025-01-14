using System;
using System.Numerics;

namespace TinyTerrain
{
    /// <summary>
    /// Represents a 2D vector with unsigned integer components.
    /// </summary>
    public struct Vector2UInt
    {
        private const byte CHUNK_WORLD_SPACE_SIZE = 4;

        /// <summary>
        /// The x-coordinate of the vector.
        /// </summary>
        public uint x;
        /// <summary>
        /// The z-coordinate of the vector.
        /// </summary>
        public uint z;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2UInt"/> struct with the specified x and z coordinates.
        /// </summary>
        /// <param name="xCoordinate">The x-coordinate.</param>
        /// <param name="zCoordinate">The z-coordinate.</param>
        public Vector2UInt(uint xCoordinate, uint zCoordinate)
        {
            x = xCoordinate;
            z = zCoordinate;
        }

        /// <summary>
        /// Calculates the area represented by the vector (x * z).
        /// </summary>
        /// <param name="v">The vector.</param>
        /// <returns>The area.</returns>
        public static uint Area(Vector2UInt v)
        {
            return v.x * v.z;
        }

        private static uint TransformCoordinate(float coordinate)
        {
            return (uint)MathF.Floor(coordinate / CHUNK_WORLD_SPACE_SIZE);
        }

        internal static Vector2UInt WorldSpaceToChunkIndices(Vector2 worldSpacePosition)
        {
            return new Vector2UInt(TransformCoordinate(worldSpacePosition.X), TransformCoordinate(worldSpacePosition.Y));
        }

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector2UInt operator +(Vector2UInt left, Vector2UInt right)
        {
            return new Vector2UInt(left.x + right.x, left.z + right.x);
        }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>The difference between the two vectors.</returns>
        public static Vector2UInt operator -(Vector2UInt left, Vector2UInt right)
        {
            uint xDiff = left.x > right.x ? left.x - right.x : right.x - left.x;
            uint zDiff = left.z > right.z ? left.z - right.z : right.z - left.z;
            return new Vector2UInt(xDiff, zDiff);
        }

        /// <summary>
        /// Divides a vector by a scalar value.
        /// </summary>
        /// <param name="left">The vector to divide.</param>
        /// <param name="right">The scalar value to divide by.</param>
        /// <returns>The vector divided by the scalar value.</returns>
        public static Vector2UInt operator /(Vector2UInt left, uint right)
        {
            return new Vector2UInt(left.x / right, left.z / right);
        }

        /// <summary>
        /// Multiplies two vectors together.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>The product of the two vectors.</returns>
        public static Vector2UInt operator *(Vector2UInt left, Vector2UInt right)
        {
            return new Vector2UInt(left.x * right.x, left.z * right.z);
        }

        /// <summary>
        /// Determines whether two vectors are equal.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>A value indicating whether the two vectors are equal.</returns>
        public static bool operator ==(Vector2UInt left, Vector2UInt right)
        {
            return left.x == right.x && left.z == right.z;
        }

        /// <summary>
        /// Determines whether two vectors are not equal.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>A value indicating whether the two vectors are not equal.</returns>
        public static bool operator !=(Vector2UInt left, Vector2UInt right)
        {
            return left.x != right.x || left.z != right.z;
        }

        /// <summary>
        /// Determines whether the left vector is greater than or equal to the right vector.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>A value indicating whether the left vector is greater than or equal to the right vector.</returns>
        public static bool operator >=(Vector2UInt left, Vector2UInt right)
        {
            return left.x >= right.x && left.z >= right.z;
        }

        /// <summary>
        /// Determines whether the left vector is less than or equal to the right vector.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>A value indicating whether the left vector is less than or equal to the right vector.</returns>
        public static bool operator <=(Vector2UInt left, Vector2UInt right)
        {
            return left.x <= right.x && left.z <= right.z;
        }

        /// <summary>
        /// Determines whether the left vector is greater than the right vector.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>A value indicating whether the left vector is greater than the right vector.</returns>
        public static bool operator >(Vector2UInt left, Vector2UInt right)
        {
            return left.x > right.x && left.z > right.z;
        }

        /// <summary>
        /// Determines whether the left vector is less than the right vector.
        /// </summary>
        /// <param name="left">The left vector.</param>
        /// <param name="right">The right vector.</param>
        /// <returns>A value indicating whether the left vector is less than the right vector.</returns>
        public static bool operator <(Vector2UInt left, Vector2UInt right)
        {
            return left.x < right.x && left.z < right.z;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            if (obj is Vector2UInt vector2UInt)
            {
                return this == vector2UInt;
            }

            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(x, z);
        }
    }
}
