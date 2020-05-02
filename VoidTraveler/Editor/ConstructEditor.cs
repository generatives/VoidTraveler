using DefaultEcs;
using ImGuiNET;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veldrid;
using VoidTraveler.Game.Constructs;
using VoidTraveler.Game.Constructs.Component;
using VoidTraveler.Game.Core;
using VoidTraveler.Networking;

namespace VoidTraveler.Editor
{
    [MessagePackObject]
    public struct ConstructEditorAction
    {
        [Key(0)]
        public int Action;
        [Key(1)]
        public int X;
        [Key(2)]
        public int Y;
        [Key(3)]
        public ConstructTileOrientation Orientation;
    }

    public class ConstructEditor : IEditor
    {
        public string Name => "Construct Editor";

        public string Category => "Construct";

        public bool Active { get; set; }

        public static (string, Action<Entity, Construct, int, int, ConstructTileOrientation>)[] TileActions = new (string, Action<Entity, Construct, int, int, ConstructTileOrientation>)[]
        {
            ("Blue Wall", Tile(new ConstructTile() { Exists = true, Collides = true, Colour = RgbaFloat.Blue })),
            ("Black Wall", Tile(new ConstructTile() { Exists = true, Collides = true, Colour = RgbaFloat.Black })),
            ("White Floor", Tile(new ConstructTile() { Exists = true, Collides = false, Colour = RgbaFloat.White })),
            ("Thruster", (Entity e, Construct c, int x, int y, ConstructTileOrientation orientation) =>
            {
                c[x, y] = new ConstructTile() { Exists = true, Collides = true, Orientation = orientation, Colour = RgbaFloat.Red };
                var thrusterEntity = e.World.CreateEntity();
                thrusterEntity.Set(new Thruster() { Thrust = 5f });
                c.SetComponent(x, y, thrusterEntity, e);
            }),
            ("None", Tile(new ConstructTile() { Exists = false }))
        };
        private int _selectedTile;
        private ConstructTileOrientation _selectedOrientation;

        private EntitySet _constructEntities;

        public ConstructEditor(World world)
        {
            _constructEntities = world.GetEntities().With<NetworkedEntity>().With<Transform>().With<Construct>().AsSet();
        }

        public void Run(EditorUpdate runParam)
        {
            var open = Active;
            ImGui.Begin(Name, ref open);
            Active = open;

            if (ImGui.BeginCombo("Tile Combo", TileActions[_selectedTile].Item1)) // The second parameter is the label previewed before opening the combo.
            {
                for (int n = 0; n < TileActions.Length; n++)
                {
                    bool is_selected = _selectedTile == n; // You can store your selection however you want, outside or inside your objects
                    if (ImGui.Selectable(TileActions[n].Item1, is_selected))
                        _selectedTile = n;
                    if (is_selected)
                        ImGui.SetItemDefaultFocus();   // You may set the initial focus when opening the combo (scrolling + for keyboard navigation support)
                }
                ImGui.EndCombo();
            }

            var sides = Enum.GetNames(typeof(ConstructTileOrientation));
            var selectedOrientation = (int)_selectedOrientation;
            ImGui.Combo("Orientation", ref selectedOrientation, sides, sides.Length);
            _selectedOrientation = (ConstructTileOrientation)selectedOrientation;

            ImGui.End();

            if(runParam.CameraSpaceGameInput.GetMouseButtonPressed(Tortuga.Platform.TMouseButton.Left))
            {
                var mousePosition = runParam.CameraSpaceGameInput.MousePosition;
                foreach (var entity in _constructEntities.GetEntities())
                {
                    var transform = entity.Get<Transform>();
                    var construct = entity.Get<Construct>();

                    var local = transform.GetLocal(mousePosition);
                    var (xIndex, yIndex) = construct.GetIndex(local);

                    if(construct.Contains(xIndex, yIndex))
                    {
                        var netEntity = entity.Get<NetworkedEntity>();
                        var action = new ConstructEditorAction() { Action = _selectedTile, X = xIndex, Y = yIndex, Orientation = _selectedOrientation };
                        runParam.ServerMessages.Add(new EntityMessage<ConstructEditorAction>(netEntity.Id, action));
                        break;
                    }
                }
            }
        }

        private static Action<Entity, Construct, int, int, ConstructTileOrientation> Tile(ConstructTile tile)
        {
            return (Entity entity, Construct construct, int x, int y, ConstructTileOrientation orientation) =>
            {
                tile.Orientation = orientation;
                construct[x, y] = tile;
            };
        }
    }

    public class ConstructEditorActionReceiver : EntityMessageApplier<ConstructEditorAction>
    {
        public ConstructEditorActionReceiver(NetworkedEntities entities) : base(entities)
        {
        }

        protected override void On(in ConstructEditorAction messageData, in Entity entity)
        {
            var construct = entity.Get<Construct>();
            ConstructEditor.TileActions[messageData.Action].Item2(entity, construct, messageData.X, messageData.Y, messageData.Orientation);
            entity.Set(construct);
        }
    }
}
