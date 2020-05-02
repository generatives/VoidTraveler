using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Game.Constructs
{
    public struct ConstructPilotable
    {
        public List<Entity> All;

        public List<Entity> PosX;
        public List<Entity> NegX;

        public List<Entity> PosY;
        public List<Entity> NegY;

        public List<Entity> PosRot;
        public List<Entity> NegRot;

        public static ConstructPilotable New()
        {
            var pilotable = new ConstructPilotable();
            pilotable.All = new List<Entity>();

            pilotable.PosX = new List<Entity>();
            pilotable.NegX = new List<Entity>();

            pilotable.PosY = new List<Entity>();
            pilotable.NegY = new List<Entity>();

            pilotable.PosRot = new List<Entity>();
            pilotable.NegRot = new List<Entity>();

            return pilotable;
        }
    }
}
