using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PBR.EffectManagers;
using PBR.Managers.EffectManagers;
using System.Collections.Generic;

namespace PBR.Primitives3D;

internal class DrawableFabric
{
    private GraphicsDevice _graphicsDevice;
    private FabricComputeEffectManager _fabricComputeEffectManager;

    public int FabricWidth { get; }
    public int FabricHeight { get; }
    public float FabricStep { get; }

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private short[] _indices;

    public DrawableFabric(GraphicsDevice graphicsDevice,
        int fabricWidth,
        int fabricHeight,
        float fabricStep,
        FabricComputeEffectManager fabricComputeEffectManager)
    {
        _graphicsDevice = graphicsDevice;
        _fabricComputeEffectManager = fabricComputeEffectManager;

        FabricWidth = fabricWidth;
        FabricHeight = fabricHeight;
        FabricStep = fabricStep;

        _fabricComputeEffectManager.FabricWidthInParticles = fabricWidth;
        _fabricComputeEffectManager.FabricHeightInParticles = fabricHeight;
        _fabricComputeEffectManager.FabricStructuralRestLength = fabricStep;

        var particles = SetupFabricParticles();
        SetupBuffers(particles);
    }

    private List<FabricParticle> SetupFabricParticles()
    {
        var particles = new List<FabricParticle>();
        var indices = new List<short>();

        var y = FabricHeight * FabricStep / 2.0f;

        var currentIndex = 0;

        for (var h = 0; h < FabricHeight; h++)
        {
            for (var w = 0; w < FabricWidth; w++)
            {
                var newParticle = new FabricParticle
                {
                    Position = new Vector3(w * FabricStep, h * FabricStep, -h * FabricStep * 0.01f),
                    //Position = new Vector3(w * FabricStep, y, -h * FabricStep),
                    Normal = Vector3.UnitY,
                    TextureCoord = new Vector2((float)w / FabricWidth, FabricHeight - (float)h / FabricHeight),
                    //IsPinned = w == 0 && h == FabricHeight - 1 ||
                    //           w == FabricWidth / 2 && h == FabricHeight - 1 ||
                    //         w == FabricWidth - 1 && h == FabricHeight - 1

                    IsPinned = w == 0 && h == 0 ||
                               w == 0 && h == FabricHeight / 2 ||
                               w == 0 && h == FabricHeight - 1
                };

                newParticle.PrevPosition = newParticle.Position;

                if (w < FabricWidth - 1 && h < FabricHeight - 1)
                {
                    var upIndex = currentIndex + FabricWidth;
                    var rightIndex = currentIndex + 1;
                    var upRightIndex = upIndex + 1;

                    indices.Add((short)currentIndex);
                    indices.Add((short)upIndex);
                    indices.Add((short)rightIndex);

                    indices.Add((short)rightIndex);
                    indices.Add((short)upIndex);
                    indices.Add((short)upRightIndex);
                }

                currentIndex++;

                particles.Add(newParticle);
            }
        }

        _indices = indices.ToArray();

        return particles;
    }

    private void SetupBuffers(List<FabricParticle> particles)
    {
        _fabricComputeEffectManager.SetupBuffers(particles);

        // no need to initialize, all the data for drawing the particles is coming from the structured buffer
        _vertexBuffer = new VertexBuffer(_graphicsDevice,
            typeof(VertexPositionNormalTexture),
            particles.Count,
            BufferUsage.WriteOnly);

        _indexBuffer = new IndexBuffer(_graphicsDevice,
            typeof(short),
            _indices.Length,
            BufferUsage.WriteOnly);

        _indexBuffer.SetData(_indices);
    }

    public void Update(GameTime gameTime)
    {
        _fabricComputeEffectManager.ComputeFabricParticles(gameTime);
    }

    public void Draw()
    {
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

        _fabricComputeEffectManager.ApplyPass("RenderPass");

        _graphicsDevice.Indices = _indexBuffer;
        _graphicsDevice.SetVertexBuffer(_vertexBuffer);
        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
            0,
            0,
            _indices.Length / 3);
    }
}
