using DefaultEcs;
using ImGuiNET;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veldrid;
using VoidTraveler.Game.Constructs;
using VoidTraveler.Game.Constructs.Components;
using VoidTraveler.Game.Core;
using VoidTraveler.Game.Physics;
using VoidTraveler.Networking;

namespace VoidTraveler.Editor
{
    [MessagePackObject]
    public struct ConstructEditorAction
    {
        [Key(0)]
        public ConstructTile Tile;
        [Key(1)]
        public int X;
        [Key(2)]
        public int Y;
    }

    public class ConstructEditor : IEditor
    {
        public string Name => "Construct Editor";

        public string Category => "Construct";

        public bool Active { get; set; }

        public (string, ConstructTile)[] _tiles = new (string, ConstructTile)[]
        {
            ("Red Wall", new ConstructTile() { Exists = true, Collides = true, Colour = RgbaFloat.Red }),
            ("Black Wall", new ConstructTile() { Exists = true, Collides = true, Colour = RgbaFloat.Black }),
            ("White Floor", new ConstructTile() { Exists = true, Collides = false, Colour = RgbaFloat.White }),
            ("None", new ConstructTile() { Exists = false })
        };
        private int _selectedTile;

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

            if (ImGui.BeginCombo("Tile Combo", _tiles[_selectedTile].Item1)) // The second parameter is the label previewed before opening the combo.
            {
                for (int n = 0; n < _tiles.Length; n++)
                {
                    bool is_selected = _selectedTile == n; // You can store your selection however you want, outside or inside your objects
                    if (ImGui.Selectable(_tiles[n].Item1, is_selected))
                        _selectedTile = n;
                    if (is_selected)
                        ImGui.SetItemDefaultFocus();   // You may set the initial focus when opening the combo (scrolling + for keyboard navigation support)
                }
                ImGui.EndCombo();
            }

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
                        var action = new ConstructEditorAction() { Tile = _tiles[_selectedTile].Item2, X = xIndex, Y = yIndex };
                        runParam.ServerMessages.Add(new EntityMessage<ConstructEditorAction>(netEntity.Id, action));
                        break;
                    }
                }
            }
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
            construct[messageData.X, messageData.Y] = messageData.Tile;
            entity.Set(construct);
        }
    }
}
