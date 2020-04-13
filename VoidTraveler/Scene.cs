using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Graphics;

namespace VoidTraveler
{
    public class Scene
    {
        public World World { get; private set; }
        public List<ISystem<LogicUpdate>> LogicSystems { get; private set; }
        public List<ISystem<DrawDevice>> RenderingSystems { get; private set; }

        public Scene()
        {
            World = new World();
            LogicSystems = new List<ISystem<LogicUpdate>>();
            RenderingSystems = new List<ISystem<DrawDevice>>();
        }

        public void Update(LogicUpdate update)
        {
            foreach(var system in LogicSystems)
            {
                system.Update(update);
            }
        }

        public void Render(DrawDevice drawDevice)
        {
            foreach (var system in RenderingSystems)
            {
                system.Update(drawDevice);
            }
        }
    }
}
