using Beryllium.MonoInput.KeyboardInput;
using Beryllium.MonoInput.MouseInput;
using Beryllium.Primitives3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PBR.EffectManagers;
using PBR.Effects;
using PBR.Materials;
using PBR.Primitives3D;
using PBR.Utils;
using System;

namespace PBR;

public class PBRDemo : Game
{
    private bool _isLightOn;
    private bool _showOnlyBlurPart;
    private bool _applyBlur;
    private int _blurPasses = 10;

    private float _lightDirectionAngle;
    private float _meshRotationAngleX;
    private float _meshRotationAngleY;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    private RenderTarget2D _generalRenderTarget;
    private RenderTarget2D _brightOnlyRenderTarget;
    private RenderTarget2D _blurRenderTarget1;
    private RenderTarget2D _blurRenderTarget2;

    private DrawableSphere _drawableSphere;
    private DrawableTile _drawableTile;
    private DrawableFullScreenQuad _drawableFullScreenQuad;

    private DrawableBasePrimitive _currentDrawableMesh;

    private BasicEffect _baseEffect;
    private VertexPositionColor[] _lightVector;

    private FpsCounter _fpsCounter;
    
    private Material _material;

    private PbrEffectManager _pbrEffectManager;
    private BlurEffectManager _blurEffectManager;
    private MergeBlurEffectManager _mergeBlurEffectManager;

    private WireFrameManager _wireFrameManager;

    public PBRDemo()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        Window.AllowUserResizing = false;
        _graphics.PreferredBackBufferWidth = 1400;
        _graphics.PreferredBackBufferHeight = 780;
        _graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        _fpsCounter = new FpsCounter();

        _wireFrameManager = new WireFrameManager(GraphicsDevice);

        _isLightOn = true;

        _lightVector = new VertexPositionColor[2];
        _lightVector[0].Color = Color.Red;
        _lightVector[1].Color = Color.Yellow;
        _lightVector[1].Position = new Vector3(0, 2, 0);

        _drawableSphere = new DrawableSphere();
        _drawableSphere.Generate(2, 30, 30, 0.5f);
        _drawableTile = new DrawableTile();
        _drawableTile.Generate(2.0f);

        _drawableFullScreenQuad = new DrawableFullScreenQuad(GraphicsDevice);

        _generalRenderTarget = new RenderTarget2D(GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);

        _brightOnlyRenderTarget = new RenderTarget2D(GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);

        _blurRenderTarget1 = new RenderTarget2D(GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);

        _blurRenderTarget2 = new RenderTarget2D(GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("defaultFont");

        _pbrEffectManager = new PbrEffectManager(Content, "Effects/PBR");
        _blurEffectManager = new BlurEffectManager(Content, "Effects/Blur");
        _mergeBlurEffectManager = new MergeBlurEffectManager(Content, "Effects/MergeBlur");

        //const string folderName = "Bricks";
        //const string folderName = "RustyMetal";
        //const string folderName = "PolishedWood";
        //const string folderName = "LavaRock";
        //const string folderName = "SpaceShipMonitors";
        //const string folderName = "TreeBark";
        //const string folderName = "WallScales";
        const string folderName = "ColorTiles";
        //const string folderName = "WoodToy";

        _currentDrawableMesh = _drawableTile;
        //_currentDrawableMesh = _drawableSphere;

        _material = new Material(Content,
            $"Material/{folderName}/Diffuse",
            $"Material/{folderName}/Normal",
            $"Material/{folderName}/Height",
            $"Material/{folderName}/Roughness",
            $"Material/{folderName}/Metallic",
            $"Material/{folderName}/AO",
            $"Material/{folderName}/Emissive",
            0.04f,
            true,
            false,
            0.025f,
            10,
            30);

        var cameraPosition = new Vector3(0, 5, 5);
        var cameraLookAt = Vector3.Zero;

        _pbrEffectManager.Material = _material;
        _pbrEffectManager.WorldMatrix = Matrix.Identity;
        _pbrEffectManager.ViewMatrix = Matrix.CreateLookAt(cameraPosition,
            cameraLookAt,
            Vector3.Up);
        _pbrEffectManager.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            (float)Window.ClientBounds.Width /
            (float)Window.ClientBounds.Height,
            1, 100000);
        _pbrEffectManager.LightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
            -1,
            (float)Math.Sin(_lightDirectionAngle));
        _pbrEffectManager.LightColor = new Color(255, 251, 215).ToVector3();
        _pbrEffectManager.AmbientColor = new Vector3(0.02f, 0.02f, 0.02f);
        _pbrEffectManager.EmissiveColor = Vector3.Zero;
        _pbrEffectManager.LightIntensity = 3.5f;

        _blurEffectManager.TexelSize = new Vector2(1.0f / _graphics.PreferredBackBufferWidth,
            1.0f / _graphics.PreferredBackBufferHeight);
        _blurEffectManager.HorizontalPass = true;
        _blurEffectManager.GaussianWeights = new[] { 0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f };

        _baseEffect = new BasicEffect(GraphicsDevice)
        {
            VertexColorEnabled = true,
            World = _pbrEffectManager.WorldMatrix,
            View = _pbrEffectManager.ViewMatrix,
            Projection = _pbrEffectManager.ProjectionMatrix
        };

        UpdateLightVector();
        UpdateMeshRotation();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _fpsCounter.Update(gameTime);
        HandleInputs();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        PbrPass();

        if (_applyBlur)
        {
            BlurPass();
            if (!_showOnlyBlurPart) MergePass();
        }
        else
        {
            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_generalRenderTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        if (_isLightOn)
        {
            _baseEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList,
                _lightVector,
                0,
                1);
        }

        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, $"FPS: {_fpsCounter.Fps:n2}", new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_font, $"Wireframe: {_wireFrameManager.IsWireFrame}", new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_font, $"Base reflectivity (I) - (O): {_pbrEffectManager.BaseReflectivity:n2}",
            new Vector2(10, 50), Color.White);
        _spriteBatch.DrawString(_font, $"Blur (B): {(_applyBlur ? "ON" : "OFF")}", new Vector2(10, 70), Color.White);
        if (_applyBlur)
            _spriteBatch.DrawString(_font, $"Blur passes (V) - (N): {_blurPasses}", new Vector2(10, 90), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void PbrPass()
    {
        GraphicsDevice.SetRenderTargets(_generalRenderTarget, _brightOnlyRenderTarget);
        GraphicsDevice.Clear(Color.Black);

        _pbrEffectManager.ApplyPass();

        _wireFrameManager.ApplyWireFrame();

        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
            _currentDrawableMesh.Vertices,
            0,
            _currentDrawableMesh.Vertices.Length,
            _currentDrawableMesh.Indices,
            0,
            _currentDrawableMesh.Indices.Length / 3);

        _wireFrameManager.RestoreDefault();
    }

    private void BlurPass()
    {
        var sourceRenderTarget = _brightOnlyRenderTarget;
        var destinationRenderTarget = _blurRenderTarget1;

        for (var i = 0; i < _blurPasses; i++)
        {
            if (i > 0)
            {
                sourceRenderTarget = i % 2 == 0 ? _blurRenderTarget2 : _blurRenderTarget1;
                destinationRenderTarget = i % 2 == 0 ? _blurRenderTarget1 : _blurRenderTarget2;
            }

            GraphicsDevice.SetRenderTarget(destinationRenderTarget);

            _blurEffectManager.HorizontalPass = i % 2 == 0;

            _blurEffectManager.ApplyPass();

            _spriteBatch.Begin(effect: _blurEffectManager.Effect);
            _spriteBatch.Draw(sourceRenderTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        _blurRenderTarget2 = destinationRenderTarget;

        if (_showOnlyBlurPart)
        {
            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_blurRenderTarget2, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }

    private void MergePass()
    {
        GraphicsDevice.SetRenderTarget(null);

        _mergeBlurEffectManager.MainScene = _generalRenderTarget;
        _mergeBlurEffectManager.Blur = _blurRenderTarget2;

        _mergeBlurEffectManager.ApplyPass();

        DrawFullScreenQuad();
    }

    private void DrawFullScreenQuad()
    {
        GraphicsDevice.SetVertexBuffer(_drawableFullScreenQuad.VertexBuffer);
        GraphicsDevice.Indices = _drawableFullScreenQuad.IndexBuffer;

        // Draw the quad
        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
            0,
            0,
            2);
    }

    private void UpdateLightVector()
    {
        var ldn = _pbrEffectManager.LightDirection;
        ldn.Normalize();
        _lightVector[0].Position = _lightVector[1].Position - ldn * 0.3f;
    }

    private void UpdateMeshRotation()
    {
        _pbrEffectManager.WorldMatrix = Matrix.CreateRotationZ(_meshRotationAngleY) *
                                        Matrix.CreateRotationX(_meshRotationAngleX);
    }

    private void HandleInputs()
    {
        KeyboardManager.Update();
        MouseManager.Update();

        // Wireframe mode
        if (KeyboardManager.IsKeyPressed(Keys.W)) _wireFrameManager.ToggleWireFrame();

        // Light on/off
        if (KeyboardManager.IsKeyPressed(Keys.L))
        {
            _isLightOn = !_isLightOn;
            _pbrEffectManager.LightIntensity = _isLightOn ? 3.5f : 0.0f;
        }

        // Blur
        if (KeyboardManager.IsKeyPressed(Keys.Q)) _showOnlyBlurPart = !_showOnlyBlurPart;

        if (KeyboardManager.IsKeyPressed(Keys.B)) _applyBlur = !_applyBlur;

        if (KeyboardManager.IsKeyPressed(Keys.V))
        {
            if (_blurPasses == 2) return;

            _blurPasses -= 2;
        }

        if (KeyboardManager.IsKeyPressed(Keys.N))
        {
            if (_blurPasses == 50) return;

            _blurPasses += 2;
        }

        // Base reflectivity
        if (KeyboardManager.IsKeyPressed(Keys.I))
        {
            var br = _pbrEffectManager.BaseReflectivity - 0.05f;

            _pbrEffectManager.BaseReflectivity = br <= 0.0f ? 0.0f : br;
        }

        if (KeyboardManager.IsKeyPressed(Keys.O))
        {
            var br = _pbrEffectManager.BaseReflectivity + 0.05f;

            _pbrEffectManager.BaseReflectivity = br >= 1.0f ? 1.0f : br;
        }

        // Light direction
        if (KeyboardManager.IsKeyDown(Keys.OemComma))
        {
            _lightDirectionAngle += (float)(1.0 / _fpsCounter.Fps);

            _pbrEffectManager.LightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
                -1,
                (float)Math.Sin(_lightDirectionAngle));

            UpdateLightVector();
        }

        if (KeyboardManager.IsKeyDown(Keys.OemPeriod))
        {
            _lightDirectionAngle -= (float)(1.0 / _fpsCounter.Fps);

            _pbrEffectManager.LightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
                -1,
                (float)Math.Sin(_lightDirectionAngle));

            UpdateLightVector();
        }

        // Mesh rotation
        if (KeyboardManager.IsKeyDown(Keys.Up))
        {
            _meshRotationAngleX -= (float)(1.0 / _fpsCounter.Fps);

            UpdateMeshRotation();
        }

        if (KeyboardManager.IsKeyDown(Keys.Down))
        {
            _meshRotationAngleX += (float)(1.0 / _fpsCounter.Fps);

            UpdateMeshRotation();
        }

        if (KeyboardManager.IsKeyDown(Keys.Left))
        {
            _meshRotationAngleY += (float)(1.0 / _fpsCounter.Fps);

            UpdateMeshRotation();
        }

        if (KeyboardManager.IsKeyDown(Keys.Right))
        {
            _meshRotationAngleY -= (float)(1.0 / _fpsCounter.Fps);

            UpdateMeshRotation();
        }
    }
}