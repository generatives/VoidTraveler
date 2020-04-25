using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DefaultEcs;
using DefaultEcs.System;
using Primitives2D;
using Ruffles.Configuration;
using Ruffles.Connections;
using Ruffles.Core;
using Tortuga.DesktopPlatform;
using Tortuga.Graphics;
using Veldrid;
using Veldrid.StartupUtilities;
using VoidTraveler.Editor;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Core.Ephemoral;
using VoidTraveler.Networking;
using VoidTraveler.Scenes;

namespace VoidTraveler
{
    public class Client : NetworkedRuntime
    {
        private DesktopWindow _window;
        private DrawDevice _drawDevice;
        private ViewportManager _viewport;

        private TransformedInputTracker _cameraSpaceInputTracker;
        private ActiveInputTracker _cameraSpaceGameInputTracker;

        private ImGuiRenderer _imGuiRenderer;

        private List<ISystem<ClientSystemUpdate>> _clientSystems;

        private EntitySet _camerasSet;

        private EditorMenu _editorMenu;

        private SocketConfig _clientConfig = new SocketConfig()
        {
            ChallengeDifficulty = 20, // Difficulty 20 is fairly hard
            DualListenPort = 0, // Port 0 means we get a port by the operating system
            SimulatorConfig = new Ruffles.Simulation.SimulatorConfig()
            {
                DropPercentage = 0.05f,
                MaxLatency = 10,
                MinLatency = 0
            },
            UseSimulator = false
        };

        private RuffleSocket _client;
        private Connection _server;
        private ulong _messagesSent;
        private Stopwatch _messageTimer;

        public Client(Scene scene, List<ISystem<ClientSystemUpdate>> clientSystems, Dictionary<int, Action<MemoryStream, World>> recievers, Dictionary<Type, Action<object, MemoryStream>> serializers) :
            base(scene, recievers, serializers)
        {
            _clientSystems = clientSystems;
            _camerasSet = scene.World.GetEntities().With<Transform>().With<Camera>().AsSet();

            _editorMenu = new EditorMenu();
            _editorMenu.Editors.Add(new ConstructEditor(scene.World));
            _editorMenu.Editors.Add(new InfoViewer());

            _client = new RuffleSocket(_clientConfig);
            _messageTimer = new Stopwatch();
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

            _window = platform.CreateWindow(wci, options, GraphicsBackend.Vulkan) as DesktopWindow;
            _window.GraphicsDeviceCreated += LoadResources;
            _window.Tick += Update;
            _window.Resized += _window_Resized;

            _viewport = new ViewportManager(1280, 720);

            _cameraSpaceInputTracker = new TransformedInputTracker(_window.InputTracker);
            _cameraSpaceGameInputTracker = new ActiveInputTracker(_cameraSpaceInputTracker);

            _client.Start();
            _client.Connect(new IPEndPoint(IPAddress.Loopback, 5674));

            _window.Run();
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

        public override void Update()
        {
            var deltaSeconds = Stopwatch.Elapsed.TotalSeconds;
            InfoViewer.Values["Client FPS"] = Math.Round(1f / deltaSeconds, 2).ToString();

            InfoViewer.Values["Roundtrip"] = _server?.Roundtrip.ToString();
            InfoViewer.Values["Roundtrip Varience"] = _server?.RoundtripVarience.ToString();

            Stopwatch.Restart();

            NetworkEvent networkEvent = _client.Poll();
            while (networkEvent.Type != NetworkEventType.Nothing)
            {
                switch (networkEvent.Type)
                {
                    case NetworkEventType.Connect:
                        _server = networkEvent.Connection;
                        break;
                    case NetworkEventType.Data:
                        var diff = _messageTimer.Elapsed.TotalMilliseconds;
                        //InfoViewer.Values["Message Time"] = Math.Round(diff, 4).ToString();
                        _messageTimer.Restart();
                        MessageRecieved(networkEvent.Data);
                        break;
                }
                networkEvent.Recycle();
                networkEvent = _client.Poll();
            }

            _imGuiRenderer.Update((float)deltaSeconds, _window.InputSnapshot);
            var imGuiWantsMouse = ImGuiNET.ImGui.GetIO().WantCaptureMouse;

            var cameraEntities = _camerasSet.GetEntities();
            var cameraTransform = cameraEntities.Length > 0 ? cameraEntities[0].Get<Transform>() : new Transform();

            var inputTrackerTransform = Matrix3x2.CreateTranslation(-_window.Width / 2f, -_window.Height / 2f) *
                Matrix3x2.CreateScale(1 / Settings.GRAPHICS_SCALE, -1 / Settings.GRAPHICS_SCALE) *
                cameraTransform.Matrix;

            _cameraSpaceInputTracker.SetTransform(inputTrackerTransform);
            _cameraSpaceGameInputTracker.SetActive(!imGuiWantsMouse);

            PreUpdate(deltaSeconds);
            Scene.Update(new LogicUpdate((float)deltaSeconds, _cameraSpaceGameInputTracker));
            PostUpdate(deltaSeconds);

            var serverMessages = new List<object>();

            _editorMenu.Run(new EditorUpdate()
            {
                CameraSpaceInput = _cameraSpaceInputTracker,
                CameraSpaceGameInput = _cameraSpaceGameInputTracker,
                Scene = Scene,
                ServerMessages = serverMessages
            });

            var clientUpdate = new ClientSystemUpdate()
            {
                Messages = serverMessages,
                Input = _cameraSpaceGameInputTracker
            };

            foreach (var system in _clientSystems)
            {
                system.Update(clientUpdate);
            }

            if(_server != null)
            {
                var messages = SerializeMessages(serverMessages);
                _server.Send(messages, 4, false, _messagesSent++);
            }

            cameraEntities = _camerasSet.GetEntities();
            cameraTransform = cameraEntities.Length > 0 ? cameraEntities[0].Get<Transform>() : new Transform();

            var cameraMatrix = cameraTransform.GetCameraMatrix(Settings.GRAPHICS_SCALE);

            var vp = _viewport.Viewport;
            _drawDevice.Begin(cameraMatrix * _viewport.GetScalingTransform(), vp);

            Scene.Render(_drawDevice);

            float gridSize = 20;
            var gridCenter = new Vector2((int)Math.Round(cameraTransform.WorldPosition.X / gridSize) * gridSize, (int)Math.Round(cameraTransform.WorldPosition.Y / gridSize) * gridSize);
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    SpriteBatchExtensions.DrawCircle(_drawDevice, (gridCenter + new Vector2(x, y) * gridSize) * Settings.GRAPHICS_SCALE, 0.2f * Settings.GRAPHICS_SCALE, 8, RgbaFloat.Red);
                }
            }

            _drawDevice.Draw();

            _imGuiRenderer.Render(_drawDevice.GraphicsDevice, _drawDevice.CommandList);

            _drawDevice.End();

            _window.GraphicsDevice.SwapBuffers(_window.MainSwapchain);
            _window.GraphicsDevice.WaitForIdle();
        }
    }
}