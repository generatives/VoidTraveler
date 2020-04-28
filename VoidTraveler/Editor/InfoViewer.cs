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
        private static ConcurrentBag<string> _logs;

        public string Name => "Info Viewer";

        public string Category => "Information";

        public bool Active { get; set; }

        static InfoViewer()
        {
            _logs = new ConcurrentBag<string>();
        }

        public static void Log(string name, string value)
        {
            _logs.Add($"{name}: {value}");
        }

        public void Run(EditorUpdate runParam)
        {
            var open = Active;
            ImGui.Begin(Name, ref open);
            Active = open;
            var values = _logs.ToList();
            foreach(var log in values.OrderBy(log => log))
            {
                ImGui.Text(log);
            }
            _logs.Clear();
            ImGui.End();
        }
    }
}
