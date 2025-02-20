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

namespace PBR;

public class PBRDemo : Game
{
    private readonly Color _background = new(50, 50, 50);

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private SpriteFont _font;
    private int _nextStringPosition;

    private Camera _camera;
    private PbrEffectManager _pbrEffectManager;
    private LightSourceEffectManager _lightSourceEffectManager;
    private TexturedXZPlane _texturedXZPlane;
    private LightManager _lightManager;

    //private DrawableTile _drawableTile;
    //private DrawableSphere _drawableSphere;
    //private CoordinateAxes _coordinateAxes;

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

        _pbrEffectManager = new PbrEffectManager(Content, @"Effects\MainEffect")
        {
            /*Material = new Material(materialFolder)
            {
                TexturedProperties = new TexturedProperties
                {
                    DiffuseTexturePath = @$"Material\{materialFolder}\Diffuse",
                    NormalTexturePath = @$"Material\{materialFolder}\Normal",
                    HeightTexturePath = @$"Material\{materialFolder}\Height",
                    RoughnessTexturePath = @$"Material\{materialFolder}\Roughness",
                    MetallicTexturePath = @$"Material\{materialFolder}\Metallic",
                    AmbientOcclusionTexturePath = @$"Material\{materialFolder}\AO",
                    InvertNormalYAxis = true,
                    IsDepthMap = false,
                    ParallaxMinSteps = 0,
                    ParallaxMaxSteps = 0,
                    ParallaxHeightScale = 0.0f,
                },
                BaseReflectivity = 0.0f
            },*/
            Material = new Material("Solid")
            {
                SolidColorProperties = new SolidColorProperties
                {
                    DiffuseColor = Color.Coral.ToVector3(),
                    Metallic = 0.01f,
                    Roughness = 0.5f
                },
                BaseReflectivity = 0.04f
            },
            Gamma = 2.2f,
            ApplyGammaCorrection = true
        };

        _lightSourceEffectManager = new LightSourceEffectManager(Content, @"Effects\LightSourceEffect")
        {
            LightColor = _pbrEffectManager.LightColor
        };

        _texturedXZPlane = new TexturedXZPlane(GraphicsDevice, new Point(10, 10), 4.0f);
        _texturedXZPlane.Position = new Vector3(-_texturedXZPlane.SizeX / 2.0f, 0, _texturedXZPlane.SizeZ / 2.0f);

        _lightManager = new LightManager(_pbrEffectManager,
            _lightSourceEffectManager,
            new LightSourceRepresentation(GraphicsDevice,
                new DrawableSphere(GraphicsDevice,
                    0.05f, 8, 8, 0)))
        {
            LightDirection = new Vector3(1, -0.5f, 0),
            LightPosition = new Vector3(0, 1, 0),
            LightColor = Color.White.ToVector3(),
            LightIntensity = 1.0f,
            AmbientColor = Color.White.ToVector3() * 0.03f,
            Constant = 1.0f,
            Linear = 0.09f,
            Quadratic = 0.032f,
            CutOffInnerDegrees = 25,
            CutOffOuterDegrees = 35,
            LightType = LightType.Directional
        };

        //_coordinateAxes = new CoordinateAxes(GraphicsDevice, 2.0f);

        base.Initialize();
    }

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

        _lightManager.Update(_camera);

        /*_coordinateAxes.Update(_camera.OffsetWorldMatrix,
            _camera.ViewMatrix,
            _camera.ProjectionMatrix);*/

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_background);

        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _pbrEffectManager.ApplyTechnique("Solid").ApplyPass();
        _texturedXZPlane.Draw(_pbrEffectManager.Effect);

        _lightManager.Draw();

        //_coordinateAxes.Draw();

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
        if (KeyboardManager.IsKeyPressed(Keys.D1))
        {
            _lightManager.LightType = LightType.Directional;
        }

        if (KeyboardManager.IsKeyPressed(Keys.D2))
        {
            _lightManager.LightType = LightType.Point;
        }

        if (KeyboardManager.IsKeyPressed(Keys.D3))
        {
            _lightManager.LightType = LightType.Spot;
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