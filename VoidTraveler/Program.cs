using DefaultEcs.System;
using Primitives2D;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Tortuga.DesktopPlatform;
using Tortuga.Graphics;
using Veldrid;
using Veldrid.StartupUtilities;
using VoidTraveler.Game.Constructs;
using VoidTraveler.Game.Player;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Projectiles;
using DefaultEcs;
using VoidTraveler.Game.Physics;
using VoidTraveler.Physics;
using VoidTraveler.Core;
using System.Collections.Generic;
using VoidTraveler.Game.Constructs.Components;
using System.Linq;
using System.Diagnostics;
using VoidTraveler.Editor;

namespace VoidTraveler
{
    class Program
    {
        static void Main(string[] args)
        {
            var scene = new Scene();

            scene.RenderingSystems.Add(new ConstructRenderer(scene.World));
            scene.RenderingSystems.Add(new PlayerRenderer(scene.World));
            scene.RenderingSystems.Add(new ProjectileRenderer(scene.World));

            var physicsSystem = new PhysicsSystem();
            scene.LogicSystems.Add(new ConstructBodyGenerator(physicsSystem, scene.World));
            scene.LogicSystems.Add(new PhysicsBodyMover(scene.World));
            scene.LogicSystems.Add(physicsSystem);
            scene.LogicSystems.Add(new PhysicsBodySync(scene.World));

            scene.LogicSystems.Add(new PlayerInputSystem(scene.World));
            scene.LogicSystems.Add(new PlayerMovementSystem(physicsSystem, scene.World));

            scene.LogicSystems.Add(new ProjectileMovementSystem(physicsSystem, scene.World));

            var construct = scene.World.CreateEntity();
            var constructTransform = new Transform() { Position = new Vector2(0, 0) };
            construct.Set(constructTransform);
            construct.Set(new Construct() { Components = CreateBoundedComponents(20, 10, 20).ToList() });
            construct.Set(new PhysicsBody());
            construct.Set(new ControlledPhysicsBody());
            construct.Set(new Camera());

            var player = scene.World.CreateEntity();
            var playerTransform = new Transform() { Position = new Vector2(0, 0) };
            playerTransform.SetParent(constructTransform);
            player.Set(playerTransform);
            player.Set(new Player() { Radius = 10, Colour = RgbaFloat.Blue, MoveSpeed = 100 });

            var game = new Game(scene);
            game.Run();
        }

        private static IEnumerable<ConstructComponent> CreateBoundedComponents(float tileSize, int width, int height)
        {
            var halfTotalWidth = (tileSize * width) / 2f;
            var halfTotalHeight = (tileSize * height) / 2f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var edge = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    yield return new ConstructComponent()
                    {
                        Position = new Vector2(x * tileSize - halfTotalWidth, y * tileSize - halfTotalHeight),
                        Size = new Vector2(tileSize, tileSize),
                        Colour = edge ? RgbaFloat.Grey : RgbaFloat.White,
                        Collides = edge
                    };
                }
            }
        }

        public class Game
        {
            private DesktopWindow _window;
            private DrawDevice _drawDevice;
            private ViewportManager _viewport;

            private TransformedInputTracker _cameraSpaceInputTracker;
            private ActiveInputTracker _cameraSpaceGameInputTracker;

            private ImGuiRenderer _imGuiRenderer;

            private Scene _scene;

            private EntitySet _camerasSet;

            private Stopwatch _stopwatch;

            private EditorMenu _editorMenu;

            public Game(Scene scene)
            {
                _scene = scene;
                _camerasSet = _scene.World.GetEntities().With<Transform>().With<Camera>().AsSet();
                _stopwatch = new Stopwatch();

                _editorMenu = new EditorMenu();
                _editorMenu.Editors.Add(new ConstructEditor());
            }

            public void Run()
            {
                var platform = new DesktopPlatform();

                WindowCreateInfo wci = new WindowCreateInfo
                {
                    X = 100,
                    Y = 100,
                    WindowWidth = 1280,
                    WindowHeight = 720,
                    WindowTitle = "Tortuga Demo"
                };

                GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                    debug: false,
                    swapchainDepthFormat: PixelFormat.R16_UNorm,
                    syncToVerticalBlank: true,
                    resourceBindingModel: ResourceBindingModel.Improved,
                    preferDepthRangeZeroToOne: true,
                    preferStandardClipSpaceYDirection: true);
#if DEBUG
                options.Debug = true;
#endif

                _window = platform.CreateWindow(wci, options) as DesktopWindow;
                _window.GraphicsDeviceCreated += LoadResources;
                _window.Tick += Update;
                _window.Resized += _window_Resized;

                _viewport = new ViewportManager(1280, 720);

                _cameraSpaceInputTracker = new TransformedInputTracker(_window.InputTracker);
                _cameraSpaceGameInputTracker = new ActiveInputTracker(_cameraSpaceInputTracker);

                var task = _window.Run();
                Task.WaitAll(task);
            }

            private void _window_Resized()
            {
                _viewport.WindowChanged(_window.Width, _window.Height);
                _imGuiRenderer.WindowResized(_window.Width, _window.Height);
            }

            public void LoadResources()
            {
                _drawDevice = new DrawDevice(_window.GraphicsDevice, _window.MainSwapchain);

                _imGuiRenderer = new ImGuiRenderer(_window.GraphicsDevice, _window.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);
            }

            public void Update()
            {
                var deltaSeconds = _stopwatch.Elapsed.TotalSeconds;
                _stopwatch.Restart();
                _imGuiRenderer.Update((float)deltaSeconds, _window.InputSnapshot);
                var imGuiWantsMouse = ImGuiNET.ImGui.GetIO().WantCaptureMouse;

                var cameraEntities = _camerasSet.GetEntities();
                var cameraTransform = cameraEntities.Length > 0 ? cameraEntities[0].Get<Transform>() : default;

                var inputTrackerTransform = Matrix3x2.CreateTranslation(-_window.Width / 2f, -_window.Height / 2f) *
                    Matrix3x2.CreateScale(1, -1) *
                    cameraTransform.Matrix * 
                    Matrix3x2.CreateScale(1 / 0.8f);

                _cameraSpaceInputTracker.SetTransform(inputTrackerTransform);
                _cameraSpaceGameInputTracker.SetActive(!imGuiWantsMouse);
                _scene.Update(new LogicUpdate((float)deltaSeconds, _cameraSpaceGameInputTracker));

                cameraEntities = _camerasSet.GetEntities();
                cameraTransform = cameraEntities.Length > 0 ? cameraEntities[0].Get<Transform>() : default;

                var cameraMatrix = cameraTransform.GetCameraMatrix() * Matrix4x4.CreateScale(0.8f);

                var vp = _viewport.Viewport;
                _drawDevice.Begin(cameraMatrix * _viewport.GetScalingTransform(), vp);

                _scene.Render(_drawDevice);
                SpriteBatchExtensions.DrawCircle(_drawDevice, Vector2.Zero, 5, 8, RgbaFloat.Red);
                SpriteBatchExtensions.DrawCircle(_drawDevice, new Vector2(300, 300), 5, 8, RgbaFloat.Red);
                SpriteBatchExtensions.DrawCircle(_drawDevice, new Vector2(300, -300), 5, 8, RgbaFloat.Red);
                SpriteBatchExtensions.DrawCircle(_drawDevice, new Vector2(-300, -300), 5, 8, RgbaFloat.Red);
                SpriteBatchExtensions.DrawCircle(_drawDevice, new Vector2(-300, 300), 5, 8, RgbaFloat.Red);

                _editorMenu.Run(new EditorRun() { CameraSpaceInput = _cameraSpaceInputTracker, CameraSpaceGameInput = _cameraSpaceGameInputTracker , Scene = _scene });

                _drawDevice.Draw();

                _imGuiRenderer.Render(_drawDevice.GraphicsDevice, _drawDevice.CommandList);

                _drawDevice.End();

                _window.GraphicsDevice.SwapBuffers(_window.MainSwapchain);
                _window.GraphicsDevice.WaitForIdle();
            }
        }
    }
}
