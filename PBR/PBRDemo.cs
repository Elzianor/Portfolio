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
            movementVelocity: 3,
            rotationVelocity: 100,
            zNear: 0.1f,
            zFar: 1000f);

        var materialFolder = "WoodFloor";

        _pbrEffectManager = new PbrEffectManager(Content, @"Effects\MainEffect")
        {
            Material = new Material(materialFolder)
            {
                TexturedProperties = new TexturedProperties
                {
                    DiffuseTexturePath = @$"Textures\{materialFolder}\Diffuse",
                    NormalTexturePath = @$"Textures\{materialFolder}\Normal",
                    HeightTexturePath = @$"Textures\{materialFolder}\Height",
                    RoughnessTexturePath = @$"Textures\{materialFolder}\Roughness",
                    MetallicTexturePath = @$"Textures\{materialFolder}\Metallic",
                    AmbientOcclusionTexturePath = @$"Textures\{materialFolder}\AO",
                    InvertNormalYAxis = true,
                    IsDepthMap = false,
                    ParallaxMinSteps = 0,
                    ParallaxMaxSteps = 0,
                    ParallaxHeightScale = 0.0f,
                },
                BaseReflectivity = 0.0f
            },
            /*Material = new Material("Solid")
            {
                SolidColorProperties = new SolidColorProperties
                {
                    DiffuseColor = Color.Coral.ToVector3(),
                    Metallic = 0.01f,
                    Roughness = 0.5f
                },
                BaseReflectivity = 0.04f
            },*/
            LightDirection = new Vector3(1, -0.5f, 0),
            LightColor = Color.White.ToVector3(),
            AmbientColor = Color.White.ToVector3() * 0.03f,
            LightIntensity = 1.0f,
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
                    0.05f, 8, 8, 0)
                {
                    Position = new Vector3(0, 1, 0)
                }));

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

        _pbrEffectManager.WorldMatrix = Matrix.CreateTranslation(_texturedXZPlane.Position - _camera.Offset);
        _pbrEffectManager.ViewMatrix = _camera.ViewMatrix;
        _pbrEffectManager.ProjectionMatrix = _camera.ProjectionMatrix;
        _pbrEffectManager.LightSourceWorldMatrix = Matrix.CreateTranslation(
            _lightSourceRepresentation.Position - _camera.Offset);

        _basicEffect.World = _pbrEffectManager.LightSourceWorldMatrix;
        _basicEffect.View = _camera.ViewMatrix;
        _basicEffect.Projection = _camera.ProjectionMatrix;

        /*_coordinateAxes.Update(_camera.OffsetWorldMatrix,
            _camera.ViewMatrix,
            _camera.ProjectionMatrix);*/

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_background);

        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _pbrEffectManager.ApplyTechnique("Textured");
        _pbrEffectManager.ApplyPass();

        _texturedXZPlane.Draw(_pbrEffectManager.Effect);
        _lightSourceRepresentation.Draw(_pbrEffectManager.LightSourceEffectManager.Effect);

        _basicEffect.CurrentTechnique.Passes[0].Apply();
        GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _lightPositionProjection, 0, 1);

        //_coordinateAxes.Draw();

        _spriteBatch.Begin();
        _nextStringPosition = 10;
        DrawNextString($"FPS: {FrameRateCounter.FrameRate:n2}");
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
        if (MouseManager.MouseStatus.MiddleButton.Down)
        {
            var rayPlaneIntersectionPoint = RayCalculations.GetRayPlaneIntersectionPoint(
                RayCalculations.CalculateRay(GraphicsDevice.Viewport, _camera), new Plane(Vector3.Up, 0));

            if (rayPlaneIntersectionPoint != null)
            {
                _lightManager.LightDirection = rayPlaneIntersectionPoint.Value;
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