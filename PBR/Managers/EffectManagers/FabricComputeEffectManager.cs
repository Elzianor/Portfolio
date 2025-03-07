using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PBR.EffectManagers;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;

namespace PBR.Managers.EffectManagers;

struct FabricParticle
{
    public Vector3 PrevPosition;
    public Vector3 Position;
    public Vector3 TotalForce;
    public Vector3 Velocity;
    public Vector3 Acceleration;
    public Vector3 Normal;
    public Vector2 TextureCoord;
    public bool IsPinned;
};

internal class FabricComputeEffectManager(GraphicsDevice graphicsDevice, ContentManager contentManager, string effectPath)
    : EffectManagerBase(contentManager, effectPath)
{
    #region Compute group size
    // has to be the same as the GroupSize define in the compute shader
    private int _computeGroupSize;
    public int ComputeGroupSize
    {
        get => _computeGroupSize;
        set
        {
            _computeGroupSize = value;

            FabricParticlesGroupCount = (int)Math.Ceiling((double)FabricParticlesCount / _computeGroupSize);
        }
    }

    public int FabricParticlesGroupCount { get; private set; }
    #endregion

    #region Buffers
    // stores all the particle information, will be updated by the compute shader
    private StructuredBuffer _fabricParticlesInputBuffer;
    public StructuredBuffer FabricParticlesInputBuffer
    {
        get => _fabricParticlesInputBuffer;
        set
        {
            _fabricParticlesInputBuffer = value;
            Effect.Parameters["FabricParticlesInput"].SetValue(_fabricParticlesInputBuffer);
        }
    }

    private StructuredBuffer _fabricParticlesOutputBuffer;
    public StructuredBuffer FabricParticlesOutputBuffer
    {
        get => _fabricParticlesOutputBuffer;
        set
        {
            _fabricParticlesOutputBuffer = value;
            Effect.Parameters["FabricParticlesOutput"].SetValue(_fabricParticlesOutputBuffer);
        }
    }
    #endregion

    #region Fabric size
    private int _fabricWidthInParticles;
    public int FabricWidthInParticles
    {
        get => _fabricWidthInParticles;
        set
        {
            _fabricWidthInParticles = value;
            Effect.Parameters["FabricWidthInParticles"].SetValue(_fabricWidthInParticles);

            FabricParticlesCount = _fabricWidthInParticles * FabricHeightInParticles;
        }
    }

    private int _fabricHeightInParticles;
    public int FabricHeightInParticles
    {
        get => _fabricHeightInParticles;
        set
        {
            _fabricHeightInParticles = value;
            Effect.Parameters["FabricHeightInParticles"].SetValue(_fabricHeightInParticles);

            FabricParticlesCount = FabricWidthInParticles * _fabricHeightInParticles;
        }
    }

    private int _fabricParticlesCount;
    public int FabricParticlesCount
    {
        get => _fabricParticlesCount;
        private set
        {
            _fabricParticlesCount = value;
            Effect.Parameters["FabricParticlesCount"].SetValue(_fabricParticlesCount);

            FabricParticlesGroupCount = (int)Math.Ceiling((double)_fabricParticlesCount / ComputeGroupSize);
        }
    }
    #endregion

    #region Constraints
    private float _fabricStructuralRestLength;
    public float FabricStructuralRestLength
    {
        get => _fabricStructuralRestLength;
        set
        {
            _fabricStructuralRestLength = value;
            Effect.Parameters["FabricStructuralRestLength"].SetValue(_fabricStructuralRestLength);

            FabricShearingRestLength = (float)Math.Sqrt(2 * _fabricStructuralRestLength * _fabricStructuralRestLength);
            FabricParticleProjectedArea = _fabricStructuralRestLength * _fabricStructuralRestLength;
        }
    }

    private float _fabricShearingRestLength;
    public float FabricShearingRestLength
    {
        get => _fabricShearingRestLength;
        private set
        {
            _fabricShearingRestLength = value;
            Effect.Parameters["FabricShearingRestLength"].SetValue(_fabricShearingRestLength);
        }
    }

    private bool _useShearingConstraints;
    public bool UseShearingConstraints
    {
        get => _useShearingConstraints;
        set
        {
            _useShearingConstraints = value;
            Effect.Parameters["UseShearingConstraints"].SetValue(_useShearingConstraints);
        }
    }

    public int IterativeRelaxationStepCount { get; set; }
    #endregion

    #region Physics
    private float _currentElapsedTimeSeconds;

    private float _physicsUpdateTimeStep;
    public float PhysicsUpdateTimeStep
    {
        get => _physicsUpdateTimeStep;
        set
        {
            _physicsUpdateTimeStep = value;
            Effect.Parameters["PhysicsUpdateTimeStep"].SetValue(_physicsUpdateTimeStep);
        }
    }

    private Vector3 _gravitationalAcceleration;
    public Vector3 GravitationalAcceleration
    {
        get => _gravitationalAcceleration;
        set
        {
            _gravitationalAcceleration = value;
            Effect.Parameters["GravitationalAcceleration"].SetValue(_gravitationalAcceleration);
        }
    }

    private float _fabricParticleMass;
    public float FabricParticleMass
    {
        get => _fabricParticleMass;
        set
        {
            _fabricParticleMass = value;
            Effect.Parameters["FabricParticleMass"].SetValue(_fabricParticleMass);
        }
    }

    private float _fabricDamping;
    public float FabricDamping
    {
        get => _fabricDamping;
        set
        {
            _fabricDamping = value;
            Effect.Parameters["FabricDamping"].SetValue(_fabricDamping);
        }
    }

    private float _fabricStiffness;
    public float FabricStiffness
    {
        get => _fabricStiffness;
        set
        {
            _fabricStiffness = value;
            Effect.Parameters["FabricStiffness"].SetValue(_fabricStiffness);
        }
    }

    private float _fabricParticleDragCoefficient;
    public float FabricParticleDragCoefficient
    {
        get => _fabricParticleDragCoefficient;
        set
        {
            _fabricParticleDragCoefficient = value;
            Effect.Parameters["FabricParticleDragCoefficient"].SetValue(_fabricParticleDragCoefficient);
        }
    }

    private float _fabricParticleProjectedArea;
    public float FabricParticleProjectedArea
    {
        get => _fabricParticleProjectedArea;
        private set
        {
            _fabricParticleProjectedArea = value;
            Effect.Parameters["FabricParticleProjectedArea"].SetValue(_fabricParticleProjectedArea);
        }
    }
    #endregion

    #region Air properties
    private float _airDensity;
    public float AirDensity
    {
        get => _airDensity;
        set
        {
            _airDensity = value;
            Effect.Parameters["AirDensity"].SetValue(_airDensity);
        }
    }

    private Vector3 _wind;
    public Vector3 Wind
    {
        get => _wind;
        set
        {
            _wind = value;
            Effect.Parameters["Wind"].SetValue(_wind);
        }
    }
    #endregion

    public void SetupBuffers(List<FabricParticle> particles)
    {
        for (var i = 0; i < particles.Count; i++)
        {
            var particle = particles[i];
            particle.Acceleration = GravitationalAcceleration;
            particles[i] = particle;
        }

        FabricParticlesInputBuffer = new StructuredBuffer(graphicsDevice,
            typeof(FabricParticle),
            particles.Count,
            BufferUsage.None,
            ShaderAccess.ReadWrite);

        FabricParticlesOutputBuffer = new StructuredBuffer(graphicsDevice,
            typeof(FabricParticle),
            particles.Count,
            BufferUsage.None,
            ShaderAccess.ReadWrite);

        FabricParticlesInputBuffer.SetData(particles.ToArray());
        Effect.Parameters["FabricParticlesReadOnly"].SetValue(FabricParticlesOutputBuffer);
    }

    public void ComputeFabricParticles(GameTime gameTime)
    {
        _currentElapsedTimeSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_currentElapsedTimeSeconds < PhysicsUpdateTimeStep) return;

        // Verlet pass
        Effect.CurrentTechnique.Passes["VerletPass"].ApplyCompute();
        graphicsDevice.DispatchCompute(FabricParticlesGroupCount, 1, 1);

        // Constraints pass
        for (var i = 0; i < IterativeRelaxationStepCount; i++)
        {
            Effect.CurrentTechnique.Passes["ConstraintsPass"].ApplyCompute();
            graphicsDevice.DispatchCompute(FabricParticlesGroupCount, 1, 1);

            Effect.CurrentTechnique.Passes["UpdateInputBufferPass"].ApplyCompute();
            graphicsDevice.DispatchCompute(FabricParticlesGroupCount, 1, 1);
        }

        // Air calculations pass
        Effect.CurrentTechnique.Passes["AirCalculationsPass"].ApplyCompute();
        graphicsDevice.DispatchCompute(FabricParticlesGroupCount, 1, 1);

        Effect.CurrentTechnique.Passes["UpdateInputBufferPass"].ApplyCompute();
        graphicsDevice.DispatchCompute(FabricParticlesGroupCount, 1, 1);

        _currentElapsedTimeSeconds -= PhysicsUpdateTimeStep;
    }
}
