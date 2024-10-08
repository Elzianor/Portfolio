using Beryllium.MonoInput.KeyboardInput;
using Beryllium.MonoInput.MouseInput;
using Beryllium.Primitives3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PBR.EffectManagers;
using PBR.Effects;
using PBR.Materials;
using PBR.Utils;
using System;

namespace PBR;

public class PBRDemo : Game
{
    private bool _showOnlyBlurPart;
    private bool _applyBlur;
    private int _blurPasses = 10;

    private float _lightDirectionAngle;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    private RenderTarget2D _generalRenderTarget;
    private RenderTarget2D _brightOnlyRenderTarget;
    private RenderTarget2D _blurRenderTarget1;
    private RenderTarget2D _blurRenderTarget2;

    private DrawableSphere _drawableSphere;
    private DrawableFullScreenQuad _drawableFullScreenQuad;

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

        _drawableSphere = new DrawableSphere(GraphicsDevice);
        _drawableSphere.Generate(2, 30, 30, 0.5f);

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
        const string folderName = "LavaRock";

        _material = new Material(Content,
            $"Material/{folderName}/Diffuse",
            $"Material/{folderName}/Normal",
            $"Material/{folderName}/Roughness",
            $"Material/{folderName}/Metallic",
            $"Material/{folderName}/AO",
            $"Material/{folderName}/Emissive",
            0.04f,
            true);

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
        _pbrEffectManager.LightIntensity = 6.5f;

        _blurEffectManager.TexelSize = new Vector2(1.0f / _graphics.PreferredBackBufferWidth,
            1.0f / _graphics.PreferredBackBufferHeight);
        _blurEffectManager.HorizontalPass = true;
        _blurEffectManager.GaussianWeights = new[] { 0.227027f, 0.1945946f, 0.1216216f, 0.054054f, 0.016216f };
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

        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, $"FPS: {_fpsCounter.Fps:n2}", new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_font, $"Base reflectivity: {_pbrEffectManager.BaseReflectivity:n2}", new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_font, $"Blur: {(_applyBlur ? "ON" : "OFF")}", new Vector2(10, 50), Color.White);

        if (_applyBlur)
        {
            _spriteBatch.DrawString(_font, $"Blur passes: {_blurPasses}", new Vector2(10, 70), Color.White);
        }

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
            _drawableSphere.Vertices,
            0,
            _drawableSphere.Vertices.Length,
            _drawableSphere.Indices,
            0,
            _drawableSphere.Indices.Length / 3);

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

    private void HandleInputs()
    {
        KeyboardManager.Update();
        MouseManager.Update();

        // Wireframe mode
        if (KeyboardManager.IsKeyPressed(Keys.W)) _wireFrameManager.ToggleWireFrame();

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
        if (KeyboardManager.IsKeyDown(Keys.Left))
        {
            _lightDirectionAngle += (float)(1.0 / _fpsCounter.Fps);

            _pbrEffectManager.LightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
                -1,
                (float)Math.Sin(_lightDirectionAngle));
        }

        if (KeyboardManager.IsKeyDown(Keys.Right))
        {
            _lightDirectionAngle -= (float)(1.0 / _fpsCounter.Fps);

            _pbrEffectManager.LightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
                -1,
                (float)Math.Sin(_lightDirectionAngle));
        }
    }
}