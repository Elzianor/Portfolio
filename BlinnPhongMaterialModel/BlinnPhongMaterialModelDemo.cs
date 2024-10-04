/*using Beryllium.MonoInput.KeyboardInput;
using Beryllium.MonoInput.MouseInput;*/

using System;
using Beryllium.Primitives3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BlinnPhongMaterialModel;

public class BlinnPhongMaterialModelDemo : Game
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

    private Effect _blinnPhongEffect;

    private Sphere _sphere;
    private Texture2D _sphereTexture;

    private VertexPositionNormalTexture[] _vertices;
    private VertexBuffer _vertexBuffer;

    private float _lightDirectionAngle;
    private float _roughness;
    private float _metalness;

    public BlinnPhongMaterialModelDemo()
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
        _roughness = 500.0f;
        _metalness = 0.5f;

        InitializeVertices();

        SetupMatrices();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("defaultFont");
        _blinnPhongEffect = Content.Load<Effect>("BlinnPhong");

        SetupMatrices();

        var wvp = _worldMatrix * _viewMatrix * _projectionMatrix;
        var wv = _worldMatrix * _viewMatrix;
        var wvit = Matrix.Transpose(Matrix.Invert(wv));

        var diffuseColor = Color.Red;
        var ambientColor = Color.White;
        var emissiveColor = Color.Black;
        var lightColor = Color.White;

        _roughness = 0.5f;
        _metalness = 0.5f;

        _blinnPhongEffect.Parameters["World"].SetValue(_worldMatrix);
        _blinnPhongEffect.Parameters["View"].SetValue(_viewMatrix);
        _blinnPhongEffect.Parameters["Projection"].SetValue(_projectionMatrix);

        _blinnPhongEffect.Parameters["ViewPos"].SetValue(new Vector3(0, 5, 5));

        _blinnPhongEffect.Parameters["DiffuseColor"].SetValue(diffuseColor.ToVector3());
        _blinnPhongEffect.Parameters["AmbientColor"].SetValue(ambientColor.ToVector3());
        _blinnPhongEffect.Parameters["EmissiveColor"].SetValue(emissiveColor.ToVector3());

        _blinnPhongEffect.Parameters["LightColor"].SetValue(lightColor.ToVector4());
        _blinnPhongEffect.Parameters["LightDirection"].SetValue(new Vector3(1.0f, -1.0f, 0.0f));

        _blinnPhongEffect.Parameters["Roughness"].SetValue(_roughness);
        _blinnPhongEffect.Parameters["Metallic"].SetValue(_metalness);

        /*_blinnPhongEffect.Parameters["LightDirection"].SetValue(new Vector3((float)Math.Cos(_lightDirectionAngle),
            -1,
            (float)Math.Sin(_lightDirectionAngle)));
        _blinnPhongEffect.Parameters["LightColor"].SetValue(lightColor.ToVector4());

        _blinnPhongEffect.Parameters["DiffuseColor"].SetValue(diffuseColor.ToVector4());
        _blinnPhongEffect.Parameters["EmissiveColor"].SetValue(emissiveColor.ToVector4());
        _blinnPhongEffect.Parameters["AmbientColor"].SetValue(ambientColor.ToVector4());

        _blinnPhongEffect.Parameters["Roughness"].SetValue(_roughness);
        _blinnPhongEffect.Parameters["BaseReflectance"].SetValue(_baseReflectance);
        _blinnPhongEffect.Parameters["Metallic"].SetValue(_metalness);*/
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
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        _blinnPhongEffect.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
            _vertices,
            0,
            _vertices.Length,
            _sphere.Indices,
            0,
            _sphere.Indices.Length / 3);

        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, $"FPS: {_fps:n2}", new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_font, $"Roughness (U/J): {_roughness}", new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_font, $"Metalness (O/L): {_metalness}", new Vector2(10, 70), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void InitializeVertices()
    {
        _vertices = new VertexPositionNormalTexture[_sphere.Vertices.Count];

        for (var i = 0; i < _sphere.Vertices.Count; i++)
        {
            _vertices[i] = new VertexPositionNormalTexture(_sphere.Vertices[i].Position,
                _sphere.Vertices[i].Normal, _sphere.Vertices[i].TextureCoordinate);
        }

        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture),
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

        // Roughness
        if (Keyboard.GetState().IsKeyDown(Keys.U))
        {
            if (_roughness >= 1) _roughness = 1;
            else _roughness += 0.0001f;

            _blinnPhongEffect.Parameters["Roughness"].SetValue(_roughness);
        }

        if (Keyboard.GetState().IsKeyDown(Keys.J))
        {
            if (_roughness <= 0) _roughness = 0;
            else _roughness -= 0.0001f;

            _blinnPhongEffect.Parameters["Roughness"].SetValue(_roughness);
        }

        // Metalness
        if (Keyboard.GetState().IsKeyDown(Keys.O))
        {
            if (_metalness >= 1) _metalness = 1;
            else _metalness += 0.0001f;

            _blinnPhongEffect.Parameters["Metallic"].SetValue(_metalness);
        }

        if (Keyboard.GetState().IsKeyDown(Keys.L))
        {
            if (_metalness <= 0) _metalness = 0;
            else _metalness -= 0.0001f;

            _blinnPhongEffect.Parameters["Metallic"].SetValue(_metalness);
        }

        // Light direction
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            _lightDirectionAngle -= 0.0001f;

            var lightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
                -1,
                (float)Math.Sin(_lightDirectionAngle));

            _blinnPhongEffect.Parameters["LightDirection"].SetValue(lightDirection);
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            _lightDirectionAngle += 0.0001f;

            var lightDirection = new Vector3((float)Math.Cos(_lightDirectionAngle),
                -1,
                (float)Math.Sin(_lightDirectionAngle));

            _blinnPhongEffect.Parameters["LightDirection"].SetValue(lightDirection);
        }
    }
}