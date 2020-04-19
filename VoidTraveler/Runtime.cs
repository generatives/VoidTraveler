using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace VoidTraveler
{
    public class Runtime
    {
        public Scene Scene { get; private set; }

        public Stopwatch Stopwatch { get; private set; }

        public Runtime(Scene scene)
        {
            Scene = scene;
            Stopwatch = new Stopwatch();
        }

        public virtual void Update()
        {
            var deltaSeconds = Stopwatch.Elapsed.TotalSeconds;

            Stopwatch.Restart();

            PreUpdate(deltaSeconds);
            Scene.Update(new LogicUpdate((float)deltaSeconds, null));
            PostUpdate(deltaSeconds);
        }

        protected virtual void PreUpdate(double deltaTime) { }

        protected virtual void PostUpdate(double deltaTime) { }
    }
}
