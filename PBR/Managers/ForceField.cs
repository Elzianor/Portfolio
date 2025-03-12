using Beryllium.Camera;
using Beryllium.Primitives3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PBR.Managers.EffectManagers;
using System;

namespace PBR.Managers;

public class ForceField
{
    private DrawableSphere _representation;
    private ForceFieldEffectManager _effectManager;

    public bool IsOn { get; set; }
    public Vector3 Position { get => _representation.Position; set => _representation.Position = value; }

    public float MaxCapacity { get; set; }

    private float _capacity;
    public float Capacity
    {
        get => _capacity;
        set => _capacity = Math.Min(Math.Max(value, 0.0f), MaxCapacity);
    }

    public bool IsLowCapacity => Capacity <= MaxCapacity * 0.15f;

    public ForceField(DrawableSphere representation,
        ForceFieldEffectManager effectManager,
        float maxCapacity)
    {
        _representation = representation;
        _effectManager = effectManager;
        MaxCapacity = maxCapacity;
        Capacity = maxCapacity;
    }

    public void Update(GameTime gameTime, Camera camera)
    {
        _effectManager.WorldMatrix = _representation.WorldMatrix;
        _effectManager.CameraOffsetMatrix = camera.OffsetWorldMatrix;
        _effectManager.ViewMatrix = camera.ViewMatrix;
        _effectManager.ProjectionMatrix = camera.ProjectionMatrix;

        var elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _effectManager.Time += elapsedSeconds;

        HandleIsOn(elapsedSeconds);
        HandleCapacity(elapsedSeconds);
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.RasterizerState = RasterizerState.CullClockwise;

        _representation.Draw(_effectManager.Effect);

        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        _representation.Draw(_effectManager.Effect);
    }

    private void HandleIsOn(float elapsedSeconds)
    {
        if (IsOn)
        {
            if (_effectManager.Height >= _representation.Radius) return;

            _effectManager.Height = Math.Min(_effectManager.Height + elapsedSeconds * 1.5f, _representation.Radius);
        }
        else
        {
            if (_effectManager.Height <= 0.0) return;

            _effectManager.Height = Math.Max(_effectManager.Height - elapsedSeconds * 1.5f, 0.0f);
        }
    }

    private void HandleCapacity(float elapsedSeconds)
    {
        _effectManager.IsLowCapacity = IsLowCapacity;

        if (Capacity > 0.0f)
        {
            _effectManager.DissolveThreshold = IsLowCapacity ? 0.75f : 1.0f;
            return;
        }

        if (_effectManager.DissolveThreshold <= 0.0f)
        {
            _effectManager.Height = 0.0f;
            return;
        }

        _effectManager.DissolveThreshold = Math.Max(_effectManager.DissolveThreshold - elapsedSeconds * 0.7f, 0.0f);
    }
}
