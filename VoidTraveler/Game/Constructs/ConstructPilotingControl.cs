using System;
using System.Collections.Generic;
using System.Text;

namespace VoidTraveler.Game.Constructs
{
    public struct ConstructPilotingControl
    {
        public bool Forward { get; set; }
        public bool Backward { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
        public bool RotateLeft { get; set; }
        public bool RotateRight { get; set; }
        public bool Cheat { get; set; }
    }
}
