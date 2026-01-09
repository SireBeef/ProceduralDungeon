using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProceduralDungeon;

public class Game1 : Game
{
    private static float MOVEMENT_SPEED = 0.1f;
    private static float MOUSE_SENSITIVITY = 0.25f;

    private GraphicsDeviceManager _graphics;

    private int virtualResolutionHeight = 1280;
    private int virtualResolutionWidth = 720;

    Vector3 camPosition;
    float yaw = 0f;   // Y-axis rotation (left/right)
    float pitch = 0f; // X-axis rotation (up/down)

    Vector3 pillarOnePositionOriginBottom;

    // Responsible for taking 3d and turning it into 2D
    Matrix projectionMatrix;
    // Camera's physical position location & orientation
    Matrix viewMatrix;

    // Projects object into the world
    Matrix pillarOneWorldMatrix;
    Matrix pillarTwoWorldMatrix;
    Matrix floorMatrix;
    Matrix wallTorchMatrix;

    Model pillarOneOriginMiddle;
    Model pillarTwoOriginBottom;
    Model floor;
    Model walltorch;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Set up Camera
        camPosition = new Vector3(0f, .7f, 6f);
        yaw = 0f;
        pitch = 0f;

        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GraphicsDevice.DisplayMode.AspectRatio, 0.1f, 1000f);
        viewMatrix = Matrix.CreateLookAt(camPosition, GetCamTarget(), Vector3.Up);

        Mouse.SetPosition((int)GraphicsDeviceManager.DefaultBackBufferWidth / 2, (int)GraphicsDeviceManager.DefaultBackBufferHeight / 2);

        BuildMap();
    }

    protected override void LoadContent()
    {
        pillarOneOriginMiddle = Content.Load<Model>("models/pillar");
        pillarTwoOriginBottom = Content.Load<Model>("models/pillar_origin_bottom_blue");
        floor = Content.Load<Model>("models/floor");
        walltorch = Content.Load<Model>("models/wall_torch");
    }

    protected void BuildMap()
    {
        Vector3 pillarOnePosition = Vector3.Zero;
        pillarOneWorldMatrix = Matrix.CreateWorld(pillarOnePosition, Vector3.Forward, Vector3.Up);
        Vector3 pillarTwoPosition = new Vector3(1f, 0f, 0f);
        pillarTwoWorldMatrix = Matrix.CreateWorld(pillarTwoPosition, Vector3.Forward, Vector3.Up);
        floorMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
        wallTorchMatrix = Matrix.CreateWorld(new Vector3(0, 0, 1f), Vector3.Forward, Vector3.Up);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Create rotation matrices
        Matrix yawRotation = Matrix.CreateRotationY(yaw);
        Matrix fullRotation = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);

        // Strafing uses only yaw (horizontal rotation) so we don't move up/down
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            camPosition += Vector3.Transform(Vector3.Left, yawRotation) * MOVEMENT_SPEED;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            camPosition += Vector3.Transform(Vector3.Right, yawRotation) * MOVEMENT_SPEED;
        }

        // Forward/backward uses full rotation (pitch + yaw)
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            camPosition += Vector3.Transform(Vector3.Forward, fullRotation) * MOVEMENT_SPEED;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            camPosition += Vector3.Transform(Vector3.Backward, fullRotation) * MOVEMENT_SPEED;
        }
        float millis = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;

        if (Mouse.GetState().X != GraphicsDeviceManager.DefaultBackBufferWidth / 2 || Mouse.GetState().Y != GraphicsDeviceManager.DefaultBackBufferHeight / 2)
        {
            int mouseCenterDeltaY = (GraphicsDeviceManager.DefaultBackBufferHeight / 2) - Mouse.GetState().Y;
            int mouseCenterDeltaX = (GraphicsDeviceManager.DefaultBackBufferWidth / 2) - Mouse.GetState().X;

            // Ignore huge deltas (likely from window focus)
            if (Math.Abs(mouseCenterDeltaX) > 100 || Math.Abs(mouseCenterDeltaY) > 100)
            {
                Mouse.SetPosition((int)GraphicsDeviceManager.DefaultBackBufferWidth / 2, (int)GraphicsDeviceManager.DefaultBackBufferHeight / 2);
            }
            else
            {
                // Update yaw (left/right) and pitch (up/down)
                yaw += mouseCenterDeltaX * millis * MOUSE_SENSITIVITY;
                pitch += mouseCenterDeltaY * millis * MOUSE_SENSITIVITY;

                // Clamp pitch to prevent camera flipping upside down
                float maxPitch = MathHelper.ToRadians(89);
                pitch = MathHelper.Clamp(pitch, -maxPitch, maxPitch);

                Mouse.SetPosition((int)GraphicsDeviceManager.DefaultBackBufferWidth / 2, (int)GraphicsDeviceManager.DefaultBackBufferHeight / 2);
            }
        }

        viewMatrix = Matrix.CreateLookAt(camPosition, GetCamTarget(), Vector3.Up);

        base.Update(gameTime);
    }

    protected Vector3 GetCamTarget()
    {
        Matrix rotation = Matrix.CreateRotationX(pitch) * Matrix.CreateRotationY(yaw);
        return camPosition + Vector3.Transform(Vector3.Forward, rotation);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        pillarOneOriginMiddle.Draw(pillarOneWorldMatrix, viewMatrix, projectionMatrix);
        // pillarTwoOriginBottom.Draw(pillarTwoWorldMatrix, viewMatrix, projectionMatrix);
        floor.Draw(floorMatrix, viewMatrix, projectionMatrix);
        walltorch.Draw(wallTorchMatrix, viewMatrix, projectionMatrix);
        base.Draw(gameTime);
    }
}
