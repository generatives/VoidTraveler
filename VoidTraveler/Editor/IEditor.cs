using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Editor
{
    public interface IEditor
    {
        string Name { get; }
        string Category { get; }
        bool Active { get; set; }

        void Run(EditorRun runParam);
    }
}
