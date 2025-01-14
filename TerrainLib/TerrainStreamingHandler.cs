using System.Numerics;

namespace TinyTerrain
{
    /// <summary>
    /// Handles streaming of terrain chunks around a specific position.
    /// </summary>
    public struct TerrainStreamingHandler
    {
        internal bool IsDirty { get; set; }

        private byte radius;

        /// <summary>
        /// Gets or sets the radius of chunks to stream around the position.
        /// Setting this value will mark the handler as dirty if the new radius is larger than the current one.
        /// </summary>
        public byte Radius
        {
            readonly get => radius;
            set
            {
                IsDirty |= value > radius;
                radius = value;
            }
        }

        private Vector2 position;

        /// <summary>
        /// Gets or sets the position around which to stream chunks.
        /// Setting this value will mark the handler as dirty.
        /// </summary>
        public Vector2 Position
        {
            readonly get => position;
            set
            {
                IsDirty |= value != position;
                position = value;
            }
        }

        internal TerrainStreamingHandler(byte radius)
        {
            position = Vector2.Zero;
            IsDirty = true;
            this.radius = radius;
        }

        internal readonly Vector2UInt GetTopLeftOfRange()
        {
            Vector2 topLeft = new Vector2(position.X - radius, position.Y - radius);
            return Vector2UInt.WorldSpaceToChunkIndices(topLeft);
        }

        internal readonly Vector2UInt GetBottomRightOfRange()
        {
            Vector2 bottomRight = new Vector2(Position.X + radius, Position.Y + radius);
            return Vector2UInt.WorldSpaceToChunkIndices(bottomRight);
        }
    }
}
