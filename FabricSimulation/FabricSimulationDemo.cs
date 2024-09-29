using Beryllium.MonoInput.KeyboardInput;
using Beryllium.MonoInput.MouseInput;
using Beryllium.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace FabricSimulation;

public class FabricSimulationDemo : Game
{
    private const int Width = 50;
    private const int Height = 50;
    private const float Step = 0.1f;

    private const float Mass = 0.01f;

    private const float AnimationSpeedMultiplier = 1.0f / 1.5f;

    private readonly Vector3 _sphereCenter = new(3.0f, -1.5f, -3.0f);
    private const float SphereRadius = 1.0f;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    private double _fps;
    private double _frameCnt;
    private double _elapsedFramesTimeSec;

    private Matrix _worldMatrix;
    private Matrix _viewMatrix;
    private Matrix _projectionMatrix;

    private BasicEffect _effect;

    private VertexPositionColor[] _vertices;
    private VertexBuffer _vertexBuffer;

    private Fabric _fabric;

    private bool _simulationRunning;

    private Random _random;

    private MassParticle _leftBottom;
    private MassParticle _leftTop;
    private MassParticle _rightBottom;
    private MassParticle _rightTop;

    public FabricSimulationDemo()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        //_graphics.IsFullScreen = true;
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

        _random = new Random();
        _fabric = new Fabric();

        SetupMassParticles();
        SetupFabricThreads();

        InitializeVertices();

        SetupMatrices();

        _effect = new BasicEffect(GraphicsDevice)
        {
            World = _worldMatrix,
            View = _viewMatrix,
            Projection = _projectionMatrix,
            VertexColorEnabled = true
        };

        /*_graphics.PreferredBackBufferWidth = _graphics.GraphicsDevice.DisplayMode.Width;
        _graphics.PreferredBackBufferHeight = _graphics.GraphicsDevice.DisplayMode.Height;
        _graphics.ApplyChanges();*/

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("defaultFont");

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here
        CalculateFps(gameTime);

        HandleInputs();

        if (_simulationRunning)
        {
            _fabric.Update((float)gameTime.ElapsedGameTime.TotalSeconds * AnimationSpeedMultiplier);
            UpdateVertices();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList,
                _vertices,
                0,
                _fabric.FabricThreads.Count);
        }

        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, $"FPS: {_fps:n2}", new Vector2(10, 10), Color.White);
        _spriteBatch.DrawString(_font, $"Simulation running: {_simulationRunning}", new Vector2(10, 30), Color.White);
        _spriteBatch.DrawString(_font, "Run/stop simulation: R", new Vector2(10, 50), Color.White);
        _spriteBatch.DrawString(_font, "Reset simulation: P", new Vector2(10, 70), Color.White);
        _spriteBatch.DrawString(_font, "Destroy some springs: O", new Vector2(10, 90), Color.White);
        _spriteBatch.DrawString(_font, "Unpin left bottom corner: H", new Vector2(10, 110), Color.White);
        _spriteBatch.DrawString(_font, "Unpin left top corner: Y", new Vector2(10, 130), Color.White);
        _spriteBatch.DrawString(_font, "Unpin right bottom corner: J", new Vector2(10, 150), Color.White);
        _spriteBatch.DrawString(_font, "Unpin right top corner: U", new Vector2(10, 170), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void SetupMassParticles()
    {
        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                var newParticle = new MassParticle
                {
                    Position = new Vector3(i * Step, 0, -j * Step),
                    PrevPosition = new Vector3(i * Step, 0, -j * Step),
                    Mass = Mass,
                    Pinned = i == 0 && j == 0 ||
                             i == 0 && j == Height - 1 ||
                             i == Width - 1 && j == 0 ||
                             i == Width - 1 && j == Height - 1,
                    AdditionalConstraints = SphereConstraints
                };

                //if (i == 0 && j % 7 == 0) newParticle.Pinned = true;

                if (i == 0 && j == 0) _leftBottom = newParticle;
                if (i == 0 && j == Height - 1) _leftTop = newParticle;
                if (i == Width - 1 && j == 0) _rightBottom = newParticle;
                if (i == Width - 1 && j == Height - 1) _rightTop = newParticle;

                _fabric.MassParticles.Add(newParticle);
            }
        }
    }

    private void SetupFabricThreads()
    {
        for (var i = 0; i < Width - 1; i++)
        {
            for (var j = 0; j < Height - 1; j++)
            {
                var mass1 = _fabric.MassParticles[i * Width + j];
                var mass2 = _fabric.MassParticles[i * Width + j + 1];
                var mass3 = _fabric.MassParticles[(i + 1) * Width + j];
                var mass4 = _fabric.MassParticles[(i + 1) * Width + j + 1];

                _fabric.FabricThreads.Add(new FabricThread(mass1, mass2)
                {
                    Length = (mass2.Position - mass1.Position).Length()
                });

                _fabric.FabricThreads.Add(new FabricThread(mass1, mass3)
                {
                    Length = (mass3.Position - mass1.Position).Length()
                });

                //_fabric.FabricThreads.Add(new FabricThread(mass1, mass4)
                //{
                //    Length = (mass4.Position - mass1.Position).Length()
                //});

                //_fabric.FabricThreads.Add(new FabricThread(mass2, mass3)
                //{
                //    Length = (mass3.Position - mass2.Position).Length()
                //});

                if (i == Width - 2)
                {
                    _fabric.FabricThreads.Add(new FabricThread(mass3, mass4)
                    {
                        Length = (mass4.Position - mass3.Position).Length()
                    });
                }

                if (j == Height - 2)
                {
                    _fabric.FabricThreads.Add(new FabricThread(mass2, mass4)
                    {
                        Length = (mass4.Position - mass2.Position).Length()
                    });
                }
            }
        }
    }

    private void InitializeVertices()
    {
        _vertices = new VertexPositionColor[_fabric.FabricThreads.Count * 2];

        var index = 0;

        foreach (var fabricThread in _fabric.FabricThreads)
        {
            var r = (float)_random.NextDouble();
            var g = (float)_random.NextDouble();
            var b = (float)_random.NextDouble();

            var color = new Color(r, g, b);

            _vertices[index++] = new VertexPositionColor(fabricThread.Mass1.Position, color);

            r = (float)_random.NextDouble();
            g = (float)_random.NextDouble();
            b = (float)_random.NextDouble();

            color = new Color(r, g, b);

            _vertices[index++] = new VertexPositionColor(fabricThread.Mass2.Position, color);
        }

        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor),
            _vertices.Length, BufferUsage.None);

        _vertexBuffer.SetData(_vertices);
    }

    private void UpdateVertices()
    {
        var index = 0;

        foreach (var fabricThread in _fabric.FabricThreads)
        {
            _vertices[index++].Position = fabricThread.Mass1.Position;
            _vertices[index++].Position = fabricThread.Mass2.Position;
        }
    }

    private void SphereConstraints(Vector3 oldPosition,
        ref Vector3 newPosition,
        Vector3 velocity,
        float timeStep)
    {
        var sphereCenterDirection = newPosition - _sphereCenter;

        if (sphereCenterDirection.Length() > SphereRadius) return;

        sphereCenterDirection.Normalize();

        newPosition = _sphereCenter + SphereRadius * sphereCenterDirection;

        var vDotN = Vector3.Dot(velocity, sphereCenterDirection);

        if (vDotN > 0) return;

        var vTangent = velocity - vDotN * sphereCenterDirection;
        var newVelocity = velocity - vTangent;
        newPosition += newVelocity * timeStep;
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

        var position = new Vector3((Width - 1) * Step / 2.0f, (Height - 1) * Step, Height * Step);
        var lookAt = new Vector3((Width - 1) * Step / 2.0f, 0, -(Height - 1) * Step / 2.0f);

        _viewMatrix = Matrix.CreateLookAt(position, lookAt, Vector3.Up);

        _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            (float)Window.ClientBounds.Width /
            (float)Window.ClientBounds.Height,
            1, 100000);
    }

    private void HandleInputs()
    {
        KeyboardManager.Update();
        MouseManager.Update();

        if (KeyboardManager.IsKeyPressed(Keys.P))
        {
            _simulationRunning = false;

            _fabric = new Fabric();

            SetupMassParticles();
            SetupFabricThreads();

            InitializeVertices();
        }

        if (KeyboardManager.IsKeyPressed(Keys.O))
        {
            // tbd
        }

        if (KeyboardManager.IsKeyPressed(Keys.R)) _simulationRunning = !_simulationRunning;

        if (Keyboard.GetState().IsKeyDown(Keys.H)) _leftBottom.Pinned = false;
        if (Keyboard.GetState().IsKeyDown(Keys.Y)) _leftTop.Pinned = false;
        if (Keyboard.GetState().IsKeyDown(Keys.J)) _rightBottom.Pinned = false;
        if (Keyboard.GetState().IsKeyDown(Keys.U)) _rightTop.Pinned = false;
    }
}