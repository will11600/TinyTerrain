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
        /// Initializes a new instance of the <see cref="Vector2UInt"/> struct with both components set to 0.
        /// </summary>
        public Vector2UInt()
        {
            x = 0;
            z = 0;
        }

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
            return new(TransformCoordinate(worldSpacePosition.X), TransformCoordinate(worldSpacePosition.Y));
        }

        public static Vector2UInt operator +(Vector2UInt left, Vector2UInt right)
        {
            return new(left.x + right.x, left.z + right.x);
        }

        public static Vector2UInt operator -(Vector2UInt left, Vector2UInt right)
        {
            uint xDiff = left.x > right.x ? left.x - right.x : right.x - left.x;
            uint zDiff = left.z > right.z ? left.z - right.z : right.z - left.z;
            return new(xDiff, zDiff);
        }

        public static Vector2UInt operator /(Vector2UInt left, uint right)
        {
            return new(left.x / right, left.z / right);
        }

        public static Vector2UInt operator *(Vector2UInt left, Vector2UInt right)
        {
            return new(left.x * right.x, left.z * right.z);
        }

        public static bool operator ==(Vector2UInt left, Vector2UInt right)
        {
            return left.x == right.x && left.z == right.z;
        }

        public static bool operator !=(Vector2UInt left, Vector2UInt right)
        {
            return left.x != right.x || left.z != right.z;
        }

        public static bool operator >=(Vector2UInt left, Vector2UInt right)
        {
            return left.x >= right.x && left.z >= right.z;
        }

        public static bool operator <=(Vector2UInt left, Vector2UInt right)
        {
            return left.x <= right.x && left.z <= right.z;
        }

        public static bool operator >(Vector2UInt left, Vector2UInt right)
        {
            return left.x > right.x && left.z > right.z;
        }

        public static bool operator <(Vector2UInt left, Vector2UInt right)
        {
            return left.x < right.x && left.z < right.z;
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is Vector2UInt vector2UInt)
            {
                return this == vector2UInt;
            }

            return base.Equals(obj);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(x, z);
        }
    }
}
