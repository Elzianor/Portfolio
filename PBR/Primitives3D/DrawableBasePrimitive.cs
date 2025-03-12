using Beryllium.VertexTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Beryllium.Primitives3D
{
    public abstract class DrawableBasePrimitive(GraphicsDevice graphicsDevice)
    {
        public VertexPositionNormalTangentTexture[] Vertices { get; protected set; }
        public int[] Indices { get; protected set; }

        private Vector3 _position;
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                WorldMatrix = Matrix.CreateTranslation(_position);
            }
        }

        public Matrix WorldMatrix { get; private set; } = Matrix.Identity;

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
