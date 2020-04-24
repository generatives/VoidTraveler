using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tortuga.Platform;
using VoidTraveler.Scenes;

namespace VoidTraveler.Editor
{
    public struct EditorUpdate
    {
        public Scene Scene;
        public IInputTracker CameraSpaceInput;
        public IInputTracker CameraSpaceGameInput;
        public List<object> ServerMessages;
    }

    public class EditorMenu
    {
        public List<IEditor> Editors { get; private set; }

        public EditorMenu()
        {
            Editors = new List<IEditor>();
        }

        public void Run(EditorUpdate runParam)
        {
            var groupedEditors = Editors.GroupBy(e => e.Category);

            if(ImGui.BeginMainMenuBar())
            {
                foreach(var group in groupedEditors)
                {
                    if (ImGui.BeginMenu(group.Key))
                    {
                        foreach(var editor in group)
                        {
                            bool active = editor.Active;
                            ImGui.MenuItem(editor.Name, "", ref active, true);
                            editor.Active = active;
                        }
                        ImGui.EndMenu();
                    }
                }
                ImGui.EndMainMenuBar();
            }

            foreach(var editor in Editors.Where(e => e.Active))
            {
                editor.Run(runParam);
            }
        }
    }
}
