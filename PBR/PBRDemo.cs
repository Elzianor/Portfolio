using Beryllium.Camera;
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
using System.Linq;

namespace PBR;

public class PBRDemo : Game
{
    private readonly Color _background = new(15, 15, 15);

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont _defaultFont;
    private SpriteFont _bigFont;
    private int _nextStringPosition;

    private Camera _camera;

    private PbrEffectManager _pbrEffectManager;
    private LightSourceEffectManager _lightSourceEffectManager;
    private TexturedXZPlane _texturedXZPlane;
    private LightManager _lightManager;

    private ForceField _forceField;

    private RenderTarget2D _sceneRenderTarget;
    private DrawableCube _cube;

    private CoordinateAxes _coordinateAxes;

    public PBRDemo()
    {
        var screenBounds = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

        _graphics = new GraphicsDeviceManager(this);
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.PreferredBackBufferWidth = /*screenBounds.Width*/1600;
        _graphics.PreferredBackBufferHeight = /*screenBounds.Height*/1200;
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        Window.AllowUserResizing = true;
        Window.IsBorderless = false;
        Window.Position = Point.Zero;
        IsMouseVisible = true;
        IsFixedTimeStep = false;
        Window.ClientSizeChanged += WindowClientSizeChangedHandler;
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
            zFar: 1000f,
            fovDegrees: 65);

        CreateLightManager();
        CreateForceField();
        CreateTexturedXZPlane();
        CreateCoordinateAxes();

        //TextureCube cubeMap = CreateCubMapTexture();
        //_cubeMapEffect.Parameters["CubeSampler"].SetValue(cubeMap);

        _cube = new DrawableCube(GraphicsDevice, 1.5f)
        {
            Position = new Vector3(0.0f, 2.0f, 0.0f)
        };

        var pt = Perlin3D.PermutationTable.Select(ptEntry => (int)ptEntry).ToArray();
        var gs = Perlin3D.GradientSet.Select(vec => new Vector3(vec.X, vec.Y, vec.Z)).ToArray();

        _pbrEffectManager.Effect.Parameters["mX"].SetValue(Perlin3D.MX);
        _pbrEffectManager.Effect.Parameters["mY"].SetValue(Perlin3D.MY);
        _pbrEffectManager.Effect.Parameters["mZ"].SetValue(Perlin3D.MZ);
        _pbrEffectManager.Effect.Parameters["permutationTable"].SetValue(pt);
        _pbrEffectManager.Effect.Parameters["gradientSet"].SetValue(gs);

        _sceneRenderTarget = new RenderTarget2D(GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _defaultFont = Content.Load<SpriteFont>(@"Fonts\defaultFont");
        _bigFont = Content.Load<SpriteFont>(@"Fonts\bigFont");
    }

    protected override void UnloadContent()
    {
        Window.ClientSizeChanged -= WindowClientSizeChangedHandler;
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

        _forceField.Update(gameTime, _camera);

        _pbrEffectManager.Effect.Parameters["ForceFieldPosition"].SetValue(_forceField.Position);
        _pbrEffectManager.Effect.Parameters["ForceFieldRadius"].SetValue(_forceField.Radius);
        _pbrEffectManager.Effect.Parameters["ForceFieldHighlightColor"].SetValue(_forceField.HighlightColor);
        _pbrEffectManager.Effect.Parameters["ForceFieldHeight"].SetValue(_forceField.Height);
        _pbrEffectManager.Effect.Parameters["Time"].SetValue(_forceField.Time);

        _coordinateAxes.Update(_camera);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
        GraphicsDevice.Clear(_background);

        RenderScene();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(_background);

        RenderScene();

        _forceField.Effect.Parameters["SceneTextureSampler"].SetValue(_sceneRenderTarget);
        _forceField.Draw(GraphicsDevice);

        //_coordinateAxes.Draw();

        _spriteBatch.Begin();
        _nextStringPosition = 10;
        DrawNextString($"FPS: {FrameRateCounter.FrameRate:n2}");
        DrawNextString($"FOV: {_camera.FovDegrees} degrees");
        DrawNextString($"Light type: {_lightManager.LightType}");

        DrawForceFieldInfo();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void RenderScene()
    {
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.BlendState = BlendState.NonPremultiplied;

        _lightManager.Update(_camera, _texturedXZPlane.WorldMatrix);
        _pbrEffectManager.Effect.Parameters["World"].SetValue(_texturedXZPlane.WorldMatrix);
        _pbrEffectManager.ApplyTechnique("TexturedFFInteraction").ApplyPass();
        _texturedXZPlane.Draw(_pbrEffectManager.Effect);
        _lightManager.Draw();

        _lightManager.Update(_camera, _cube.WorldMatrix);
        _pbrEffectManager.Effect.Parameters["World"].SetValue(_cube.WorldMatrix);
        _pbrEffectManager.ApplyTechnique("SolidFFInteraction").ApplyPass();
        _cube.Draw(_pbrEffectManager.Effect);
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
        if (KeyboardManager.IsKeyPressed(Keys.B)) _forceField.IsOn = !_forceField.IsOn;
        if (KeyboardManager.IsKeyDown(Keys.N)) _forceField.Capacity -= 1.0f;
        if (KeyboardManager.IsKeyDown(Keys.M)) _forceField.Capacity += 1.0f;

        if (KeyboardManager.IsKeyPressed(Keys.D1)) _lightManager.LightType = LightType.Directional;
        if (KeyboardManager.IsKeyPressed(Keys.D2)) _lightManager.LightType = LightType.Point;
        if (KeyboardManager.IsKeyPressed(Keys.D3)) _lightManager.LightType = LightType.Spot;

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
                if (KeyboardManager.IsKeyDown(Keys.J))
                {
                    _forceField.Position = rayPlaneIntersectionPoint.Value;
                }
                else if (KeyboardManager.IsKeyDown(Keys.K))
                {
                    _cube.Position = new Vector3(rayPlaneIntersectionPoint.Value.X,
                        2,
                        rayPlaneIntersectionPoint.Value.Z);
                }
                else
                {
                    _lightManager.LightPosition = new Vector3(rayPlaneIntersectionPoint.Value.X,
                        1,
                        rayPlaneIntersectionPoint.Value.Z);
                }
            }
        }
    }

    #region Text drawers
    private void DrawNextString(string str)
    {
        _spriteBatch.DrawString(_defaultFont,
            str,
            new Vector2(10, _nextStringPosition),
            Color.Green);
        _nextStringPosition += 20;
    }

    private void DrawForceFieldInfo()
    {
        string state;
        Color color;

        if (_forceField.Capacity <= 0.0f)
        {
            state = "DEPLETED";
            color = Color.Red;
        }
        else if (_forceField.IsLowCapacity)
        {
            state = "LOW CAPACITY";
            color = Color.Orange;
        }
        else
        {
            state = "NORMAL";
            color = Color.Green;
        }

        if (!_forceField.IsOn) color = Color.Gray;

        var info = $"Switched: {(_forceField.IsOn ? "ON" : "OFF")} " +
                   $"State: {state} " +
                   $"Capacity: {_forceField.Capacity} / {_forceField.MaxCapacity}";

        var position = new Vector2(
            GraphicsDevice.PresentationParameters.BackBufferWidth / 2.0f - _bigFont.MeasureString(info).X / 2.0f,
            10.0f);

        _spriteBatch.DrawString(_bigFont,
            info,
            position,
            color);
    }
    #endregion

    #region Creators
    private void CreateLightManager()
    {
        var materialFolder = "WoodFloor";

        _pbrEffectManager = new PbrEffectManager(Content, @"Effects\ForceFieldInteraction")
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
                SolidColorProperties = new SolidColorProperties
                {
                    DiffuseColor = Color.Coral.ToVector3(),
                    Metallic = 0.1f,
                    Roughness = 0.8f
                },
                BaseReflectivity = 0.04f
            },
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
            LightIntensity = 2.0f,
            AmbientColor = Color.White.ToVector3() * 0.15f,
            Constant = 1.0f,
            Linear = 0.09f,
            Quadratic = 0.032f,
            CutOffInnerDegrees = 25,
            CutOffOuterDegrees = 35,
            LightType = LightType.Spot
        };
    }

    private void CreateForceField()
    {
        var forceFieldEffectManager = new ForceFieldEffectManager(Content, @"Effects\ForceField")
        {
            DissolveThreshold = 1.0f,
            GlowIntensity = 4.0f,
            Height = 0.0f,
            HighlightThickness = 0.015f,
            MainColor = new Vector3(0.5f, 0.3f, 0.0f),
            HighlightColor = new Vector3(1.0f, 0.9f, 0.0f),
            LowCapacityColor = new Vector3(0.9f, 0.25f, 0.0f)
        };

        var forceFieldSphere = new DrawableSphere(GraphicsDevice,
            5,
            64,
            64,
            1.0f);

        _forceField = new ForceField(forceFieldSphere, forceFieldEffectManager, 100.0f)
        {
            IsOn = true
        };
    }

    private void CreateTexturedXZPlane()
    {
        _texturedXZPlane = new TexturedXZPlane(GraphicsDevice, new Point(10, 10), 4.0f);
        _texturedXZPlane.Position = new Vector3(-_texturedXZPlane.SizeX / 2.0f, 0, _texturedXZPlane.SizeZ / 2.0f);
    }

    private void CreateCoordinateAxes()
    {
        _coordinateAxes = new CoordinateAxes(GraphicsDevice, 2.0f);
    }
    #endregion

    #region Misc
    private void WindowClientSizeChangedHandler(object sender, EventArgs e)
    {
        var newWidth = Window.ClientBounds.Width;
        var newHeight = Window.ClientBounds.Height;

        _graphics.PreferredBackBufferWidth = newWidth;
        _graphics.PreferredBackBufferHeight = newHeight;
        _graphics.ApplyChanges();

        _camera.ViewPortWidth = newWidth;
        _camera.ViewPortHeight = newHeight;
    }

    private TextureCube CreateCubMapTexture()
    {
        //var posX = Content.Load<Texture2D>(@"SkyBox\Right");
        //var negX = Content.Load<Texture2D>(@"SkyBox\Left");
        //var posY = Content.Load<Texture2D>(@"SkyBox\Up");
        //var negY = Content.Load<Texture2D>(@"SkyBox\Down");
        //var posZ = Content.Load<Texture2D>(@"SkyBox\Front");
        //var negZ = Content.Load<Texture2D>(@"SkyBox\Back");

        var posX = Content.Load<Texture2D>(@"Materials\ForceField\Grid");
        var negX = Content.Load<Texture2D>(@"Materials\ForceField\Grid");
        var posY = Content.Load<Texture2D>(@"Materials\ForceField\Grid");
        var negY = Content.Load<Texture2D>(@"Materials\ForceField\Grid");
        var posZ = Content.Load<Texture2D>(@"Materials\ForceField\Grid");
        var negZ = Content.Load<Texture2D>(@"Materials\ForceField\Grid");

        var cubeMap = new TextureCube(GraphicsDevice, posX.Width, false, SurfaceFormat.Color);

        cubeMap.SetData(CubeMapFace.PositiveX, GetTextureData(posX));
        cubeMap.SetData(CubeMapFace.NegativeX, GetTextureData(negX));
        cubeMap.SetData(CubeMapFace.PositiveY, GetTextureData(posY));
        cubeMap.SetData(CubeMapFace.NegativeY, GetTextureData(negY));
        cubeMap.SetData(CubeMapFace.PositiveZ, GetTextureData(posZ));
        cubeMap.SetData(CubeMapFace.NegativeZ, GetTextureData(negZ));

        return cubeMap;
    }

    private static Color[] GetTextureData(Texture2D texture)
    {
        var data = new Color[texture.Width * texture.Height];
        texture.GetData(data);
        return data;
    }
    #endregion
}