using Beryllium.VertexTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBR.Primitives3D
{
    public abstract class DrawableBasePrimitive
    {
        public VertexPositionNormalTangentTexture[] Vertices { get; protected set; }
        public int[] Indices { get; protected set; }
    }
}
