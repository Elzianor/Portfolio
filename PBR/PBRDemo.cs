/*using Beryllium.MonoInput.KeyboardInput;
using Beryllium.MonoInput.MouseInput;*/

using System;
using System.Collections.Generic;
using Beryllium.Primitives3D;
using Beryllium.VertexTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PBR;

public class PBRDemo : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    private double _fps;
    private double _frameCnt;
    private double _elapsedFramesTimeSec;

    private Matrix _worldMatrix;
    private Matrix _viewMatrix;
    private Matrix _projectionMatrix;

    private Effect _pbrEffect;

    private Sphere _sphere;

    private VertexPositionNormalTangentTexture[] _vertices;
    private VertexBuffer _vertexBuffer;

    private float _lightDirectionAngle;

    private Texture2D _diffuseTexture;
    private Texture2D _normalTexture;
    private Texture2D _roughnessTexture;
    private Texture2D _metallicTexture;
    private Texture2D _aoTexture;

    public PBRDemo()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        Window.AllowUserResizing = true;
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 950;
        _graphics.SynchronizeWithVerticalRetrace = false;
        IsFixedTimeStep = false;
        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        _sphere = new Sphere(1, 30, 30);

        InitializeVertices();

        SetupMatrices();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("defaultFont");
        _pbrEffect = Content.Load<Effect>("PBR");

        _diffuseTexture = Content.Load<Texture2D>("Material/Diffuse");
        _normalTexture = Content.Load<Texture2D>("Material/Normal");
        _roughnessTexture = Content.Load<Texture2D>("Material/Roughness");
        _metallicTexture = Content.Load<Texture2D>("Material/Metallic");
        _aoTexture = Content.Load<Texture2D>("Material/AO");

        SetupMatrices();

        var wvp = _worldMatrix * _viewMatrix * _projectionMatrix;
        var wv = _worldMatrix * _viewMatrix;
        var wvit = Matrix.Transpose(Matrix.Invert(wv));

        var ambientColor = new Color(0.03f, 0.03f, 0.03f);
        var emissiveColor = Color.Black;
        var lightColor = Color.White;

        //_lightDirectionAngle = 2;

        _pbrEffect.Parameters["WorldViewProjection"].SetValue(wvp);
        _pbrEffect.Parameters["WorldView"].SetValue(wv);
        _pbrEffect.Parameters["WorldViewInverseTranspose"].SetValue(wvit);

        _pbrEffect.Parameters["AmbientColor"].SetValue(ambientColor.ToVector3());
        _pbrEffect.Parameters["EmissiveColor"].SetValue(emissiveColor.ToVector3());

        _pbrEffect.Parameters["LightColor"].SetValue(lightColor.ToVector3());
        _pbrEffect.Parameters["LightDirection"].SetValue(new Vector3((float)Math.Cos(_lightDirectionAngle),
            -1,
            (float)Math.Sin(_lightDirectionAngle)));

        _pbrEffect.Parameters["DiffuseMapTexture"].SetValue(_diffuseTexture);
        _pbrEffect.Parameters["NormalMapTexture"].SetValue(_normalTexture);
        _pbrEffect.Parameters["RoughnessMapTexture"].SetValue(_roughnessTexture);
        _pbrEffect.Parameters["MetallicMapTexture"].SetValue(_metallicTexture);
        _pbrEffect.Parameters["AoMapTexture"].SetValue(_aoTexture);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        CalculateFps(gameTime);

        HandleInputs();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10, 10, 10));

        // TODO: Add your drawing code here
        _pbrEffect.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
            _vertices,
            0,
            _vertices.Length,
            _sphere.Indices,
            0,
            _sphere.Indices.Length / 3);

        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, $"FPS: {_fps:n2}", new Vector2(10, 10), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void InitializeVertices()
    {
        _vertices = _sphere.Vertices;

        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTangentTexture),
            _vertices.Length, BufferUsage.None);

        _vertexBuffer.SetData(_vertices);
    }

    private void CalculateFps(GameTime gameTime)
    {
        _frameCnt++;
        _elapsedFramesTimeSec += gameTime.ElapsedGameTime.TotalSeconds;

        if (_elapsedFramesTimeSec < 0.5) return;

        _fps = _frameCnt / _elapsedFramesTimeSec;
        _frameCnt = 0;
        _elapsedFramesTimeSec = 0;
    }

    private void SetupMatrices()
    {
        _worldMatrix = Matrix.Identity;

        var position = new Vector3(0, 5, 5);
        var lookAt = Vector3.Zero;

        _viewMatrix = Matrix.CreateLookAt(position, lookAt, Vector3.Up);

        _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            (float)Window.ClientBounds.Width /
            (float)Window.ClientBounds.Height,
            1, 100000);
    }

    private void HandleInputs()
    {
        /*KeyboardManager.Update();
        MouseManager.Update();*/

        // Light direction
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            _lightDirectionAngle += (float)(1.0 / _fps);

            var lightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
                -1,
                (float)Math.Sin(_lightDirectionAngle));

            _pbrEffect.Parameters["LightDirection"].SetValue(lightDirection);
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            _lightDirectionAngle -= (float)(1.0 / _fps);

            var lightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
                -1,
                (float)Math.Sin(_lightDirectionAngle));

            _pbrEffect.Parameters["LightDirection"].SetValue(lightDirection);
           
        }
    }
}