using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoidTraveler.Editor
{
    public class InfoViewer : IEditor
    {
        public static ConcurrentDictionary<string, string> Values { get; private set; }

        public string Name => "Info Viewer";

        public string Category => "Information";

        public bool Active { get; set; }

        static InfoViewer()
        {
            Values = new ConcurrentDictionary<string, string>();
        }

        public void Run(EditorRun runParam)
        {
            var open = Active;
            ImGui.Begin(Name, ref open);
            Active = open;
            foreach(var kvp in Values.OrderBy(kvp => kvp.Key))
            {
                ImGui.Text($"{kvp.Key}: {kvp.Value}");
            }
            Values.Clear();
            ImGui.End();
        }
    }
}
