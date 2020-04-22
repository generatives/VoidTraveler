using DefaultEcs;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veldrid;
using VoidTraveler.Game.Constructs;
using VoidTraveler.Game.Constructs.Components;
using VoidTraveler.Game.Physics;

namespace VoidTraveler.Editor
{
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

        public void Run(EditorRun runParam)
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
                var physicsSystem = runParam.Scene.LogicSystems.FirstOrDefault(s => s is PhysicsSystem) as PhysicsSystem;
                if (physicsSystem != null)
                {
                    var world = physicsSystem.World;

                    var fixture = world.TestPoint(runParam.CameraSpaceInput.MousePosition);
                    if (fixture != null &&
                        fixture.UserData is ConstructTileUserInfo userInfo &&
                        fixture.Body.UserData is Entity bodyEntity && bodyEntity.Has<Construct>())
                    {
                        var construct = bodyEntity.Get<Construct>();
                        construct[userInfo.X, userInfo.Y] = _tiles[_selectedTile].Item2;
                        bodyEntity.Set(construct);
                    }
                }
            }
        }
    }
}
