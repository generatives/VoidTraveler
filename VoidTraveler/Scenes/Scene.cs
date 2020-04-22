using DefaultEcs;
using DefaultEcs.System;
using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Graphics;

namespace VoidTraveler.Scenes
{
    public class Scene
    {
        public World World { get; private set; }
        public IEnumerable<ISystem<LogicUpdate>> LogicSystems { get; private set; }
        public IEnumerable<ISystem<DrawDevice>> RenderingSystems { get; private set; }

        public Scene(World world, IEnumerable<ISystem<LogicUpdate>> logicSystems, IEnumerable<ISystem<DrawDevice>> drawSystems)
        {
            World = world;
            LogicSystems = logicSystems;
            RenderingSystems = drawSystems;
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
