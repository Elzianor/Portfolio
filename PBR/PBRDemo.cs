﻿using Beryllium.Camera;
using Beryllium.FrameRateCounter;
using Beryllium.Materials;
using Beryllium.MonoInput.KeyboardInput;
using Beryllium.MonoInput.MouseInput;
using Beryllium.Primitives3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PBR.EffectManagers;
using PBR.Managers;
using PBR.Utils;
using System;
using PBR.Managers.EffectManagers;
using PBR.Primitives3D;

namespace PBR;

public class PBRDemo : Game
{
    //private readonly Color _background = new(50, 50, 50);
    private readonly Color _background = new(15, 15, 15);

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont _font;
    private int _nextStringPosition;

    private Camera _camera;

    private PbrEffectManager _pbrEffectManager;
    private LightSourceEffectManager _lightSourceEffectManager;
    private TexturedXZPlane _texturedXZPlane;
    private LightManager _lightManager;

    private DrawableSphere _forceFieldSphere;

    private CoordinateAxes _coordinateAxes;

    public PBRDemo()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        Window.AllowUserResizing = true;
        IsMouseVisible = true;
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        _camera = new Camera(new Vector3(0, 10, 10),
            new Vector3(0, 0, 0),
            Vector3.Up,
            _graphics.PreferredBackBufferWidth,
            _graphics.PreferredBackBufferHeight,
            movementVelocity: 20,
            rotationVelocity: 100,
            zNear: 0.1f,
            zFar: 1000f);

        var materialFolder = "WoodFloor";

        _pbrEffectManager = new PbrEffectManager(Content, @"Effects\PBR")
        {
            Material = new Material(materialFolder)
            {
                TexturedProperties = new TexturedProperties
                {
                    DiffuseTexturePath = @$"Materials\PBR\{materialFolder}\Diffuse",
                    NormalTexturePath = @$"Materials\PBR\{materialFolder}\Normal",
                    HeightTexturePath = @$"Materials\PBR\{materialFolder}\Height",
                    RoughnessTexturePath = @$"Materials\PBR\{materialFolder}\Roughness",
                    MetallicTexturePath = @$"Materials\PBR\{materialFolder}\Metallic",
                    AmbientOcclusionTexturePath = @$"Materials\PBR\{materialFolder}\AO",
                    InvertNormalYAxis = true,
                    IsDepthMap = false,
                    ParallaxMinSteps = 0,
                    ParallaxMaxSteps = 0,
                    ParallaxHeightScale = 0.0f,
                },
                BaseReflectivity = 0.04f
            },
            //Material = new Material("Solid")
            //{
            //    SolidColorProperties = new SolidColorProperties
            //    {
            //        DiffuseColor = Color.Coral.ToVector3(),
            //        Metallic = 0.1f,
            //        Roughness = 0.8f
            //    },
            //    BaseReflectivity = 0.04f
            //},
            Gamma = 2.2f,
            ApplyGammaCorrection = true
        };

        _lightSourceEffectManager = new LightSourceEffectManager(Content, @"Effects\LightSource");

        _lightManager = new LightManager(_pbrEffectManager,
            _lightSourceEffectManager,
            new LightSourceRepresentation(GraphicsDevice,
                new DrawableSphere(GraphicsDevice,
                    0.05f, 8, 8, 0)))
        {
            LightDirection = new Vector3(1, -0.2f, -0.5f),
            LightPosition = new Vector3(-3.5f, 1, -1.5f),
            LightColor = Color.White.ToVector3(),
            LightIntensity = 1.0f,
            AmbientColor = Color.White.ToVector3() * 0.15f,
            Constant = 1.0f,
            Linear = 0.09f,
            Quadratic = 0.032f,
            CutOffInnerDegrees = 25,
            CutOffOuterDegrees = 35,
            LightType = LightType.Spot
        };

        _texturedXZPlane = new TexturedXZPlane(GraphicsDevice, new Point(10, 10), 4.0f);
        _texturedXZPlane.Position = new Vector3(-_texturedXZPlane.SizeX / 2.0f, 0, _texturedXZPlane.SizeZ / 2.0f);

        //_coordinateAxes = new CoordinateAxes(GraphicsDevice, 2.0f);

        // --- force field ---

        //_forceFieldSphere = new DrawableSphere(GraphicsDevice,
        //    5, 64, 64, 0.15f);
        //_ffEffect = Content.Load<Effect>(@"Effects\ForceFieldEffect");

        //_ffEffect.Parameters["GridTexture"].SetValue(Content.Load<Texture2D>(@"Noise\Grid"));
        //_ffEffect.Parameters["NoiseTexture"].SetValue(Content.Load<Texture2D>(@"Noise\Noise"));
        //_ffEffect.Parameters["FieldColor"].SetValue(new Vector3(0.745f, 0.823f, 0.96f));
        //_ffEffect.Parameters["GridLinesColor"].SetValue(new Vector3(0.15f, 0.0f, 0.0f));
        //_ffEffect.Parameters["GlowIntensity"].SetValue(1.0f);

        //_ffEffect.Parameters["WaveCenter"].SetValue(new Vector2(1.5f, 2.0f));

        //_rand = new Random();

        base.Initialize();
    }

    private Effect _ffEffect;
    private float _ffTime;
    private float _ffShockwaveTime;
    private Random _rand;

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>(@"Fonts\defaultFont");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        FrameRateCounter.Update(gameTime.ElapsedGameTime.TotalSeconds);
        KeyboardManager.Update();
        MouseManager.Update();

        HandleCameraInput();
        _camera.Update(gameTime);

        HandleInput();

        //_ffTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        //_ffEffect.Parameters["Time"].SetValue(_ffTime);

        //_ffEffect.Parameters["ShockwaveTime"].SetValue(_ffShockwaveTime);
        //_ffShockwaveTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        _pbrEffectManager.Update(_camera);
        _lightManager.Update(_camera);

        //_coordinateAxes.Update(_camera.OffsetWorldMatrix,
        //    _camera.ViewMatrix,
        //    _camera.ProjectionMatrix);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_background);

        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        _pbrEffectManager.ApplyTechnique("Textured").ApplyPass();
        _texturedXZPlane.Draw(_pbrEffectManager.Effect);
        _lightManager.Draw();

        //_coordinateAxes.Draw();

        //var wvp = _camera.OffsetWorldMatrix * _camera.ViewMatrix * _camera.ProjectionMatrix;
        //var wv = _camera.OffsetWorldMatrix * _camera.ViewMatrix;
        //var wvit = Matrix.Transpose(Matrix.Invert(wv));

        //_ffEffect.Parameters["WorldViewProjection"].SetValue(wvp);
        //_ffEffect.Parameters["WorldView"].SetValue(wv);
        //_ffEffect.Parameters["WorldViewInverseTranspose"].SetValue(wvit);

        //GraphicsDevice.RasterizerState = _camera.Offset.LengthSquared() > 25 ?
        //    RasterizerState.CullCounterClockwise :
        //    RasterizerState.CullClockwise;

        //_forceFieldSphere.Draw(_ffEffect);

        _spriteBatch.Begin();
        _nextStringPosition = 10;
        DrawNextString($"FPS: {FrameRateCounter.FrameRate:n2}");
        DrawNextString($"FOV: {_camera.FovDegrees} degrees");
        DrawNextString($"Light type: {_lightManager.LightType}");
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void HandleCameraInput()
    {
        if (KeyboardManager.IsKeyDown(Keys.W)) _camera.MoveForward();
        if (KeyboardManager.IsKeyDown(Keys.S)) _camera.MoveBackwards();
        if (KeyboardManager.IsKeyDown(Keys.A)) _camera.MoveLeft();
        if (KeyboardManager.IsKeyDown(Keys.D)) _camera.MoveRight();

        if (KeyboardManager.IsKeyDown(Keys.Q)) _camera.TiltLeft();
        if (KeyboardManager.IsKeyDown(Keys.E)) _camera.TiltRight();

        if (MouseManager.MouseStatus.XButton1.Down) _camera.MoveDown();
        if (MouseManager.MouseStatus.XButton2.Down) _camera.MoveUp();

        if (MouseManager.MouseStatus.RightButton.Down)
        {
            _camera.RotateRelativeX(MouseManager.MouseStatus.DeltaY * 0.2f);
            _camera.RotateRelativeY(MouseManager.MouseStatus.DeltaX * 0.2f);
        }
    }

    private void HandleInput()
    {
        if (KeyboardManager.IsKeyPressed(Keys.K))
        {
            var nextU = (float)_rand.NextDouble() - 0.5f;
            var nextV = (float)_rand.NextDouble() - 0.5f;
            _ffEffect.Parameters["WaveCenter"].SetValue(new Vector2(1.5f + nextU, 2.0f + nextV));

            var nextY = 0.3f + (float)_rand.NextDouble() * 2.7f;
            _ffEffect.Parameters["WaveParams"].SetValue(new Vector3(10.0f, nextY, 0.1f));

            _ffShockwaveTime = 0;
        }

        if (KeyboardManager.IsKeyPressed(Keys.D1)) _lightManager.LightType = LightType.Directional;
        if (KeyboardManager.IsKeyPressed(Keys.D2)) _lightManager.LightType = LightType.Point;
        if (KeyboardManager.IsKeyPressed(Keys.D3)) _lightManager.LightType = LightType.Spot;

        if (KeyboardManager.IsKeyUp(Keys.Up) || KeyboardManager.IsKeyUp(Keys.Down))
        {
            _pbrEffectManager.Effect.Parameters["PinnedYShift"].SetValue(0.0f);
        }

        if (MouseManager.MouseStatus.WheelDelta > 0)
        {
            if (_camera.FovDegrees > 1)
                _camera.FovDegrees--;
        }

        if (MouseManager.MouseStatus.WheelDelta < 0)
        {
            if (_camera.FovDegrees < 120)
                _camera.FovDegrees++;
        }

        if (MouseManager.MouseStatus.MiddleButton.Down)
        {
            var rayPlaneIntersectionPoint = RayCalculations.GetRayPlaneIntersectionPoint(
                RayCalculations.CalculateRay(GraphicsDevice.Viewport, _camera), new Plane(Vector3.Up, 0));

            if (rayPlaneIntersectionPoint != null)
            {
                _lightManager.LightDirection = rayPlaneIntersectionPoint.Value - _lightManager.LightPosition;
            }
        }

        if (MouseManager.MouseStatus.LeftButton.Down)
        {
            var rayPlaneIntersectionPoint = RayCalculations.GetRayPlaneIntersectionPoint(
                RayCalculations.CalculateRay(GraphicsDevice.Viewport, _camera), new Plane(Vector3.Up, 0));

            if (rayPlaneIntersectionPoint != null)
            {
                _lightManager.LightPosition = new Vector3(rayPlaneIntersectionPoint.Value.X,
                    1,
                    rayPlaneIntersectionPoint.Value.Z);
            }
        }
    }

    private void DrawNextString(string str)
    {
        _spriteBatch.DrawString(_font,
            str,
            new Vector2(10, _nextStringPosition),
            Color.Green);
        _nextStringPosition += 20;
    }
}