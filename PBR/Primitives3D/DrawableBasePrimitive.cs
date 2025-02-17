using Beryllium.VertexTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beryllium.Primitives3D
{
    public abstract class DrawableBasePrimitive(GraphicsDevice graphicsDevice)
    {
        public VertexPositionNormalTangentTexture[] Vertices { get; protected set; }
        public int[] Indices { get; protected set; }
        public Vector3 Position {get; set; }

        public virtual void Draw(Effect effect)
        {
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    Vertices,
                    0,
                    Vertices.Length,
                    Indices,
                    0,
                    Indices.Length / 3
                );
            }
        }
    }
}
