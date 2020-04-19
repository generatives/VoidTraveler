using DefaultEcs;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void Run(EditorRun runParam)
        {
            var open = Active;
            ImGui.Begin(Name, ref open);
            Active = open;
            ImGui.Text(Name);
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
                        construct[userInfo.X, userInfo.Y] = new ConstructTile();
                        bodyEntity.Set(construct);
                    }
                }
            }
        }
    }
}
