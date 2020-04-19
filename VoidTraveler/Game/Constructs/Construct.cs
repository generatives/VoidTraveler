using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using VoidTraveler.Game.Constructs.Components;

namespace VoidTraveler.Game.Constructs
{
    public struct Construct
    {
        public int XLength { get; private set; }
        public int YLength { get; private set; }
        public float TileSize { get; set; }
        public ConstructTile[] Tiles { get; private set; }

        public ConstructTile this[int x, int y]
        {
            get => Tiles[y * XLength + x];
            set => Tiles[y * XLength + x] = value;
        }

        public Construct(int xLength, int yLength, float tileSize)
        {
            XLength = xLength;
            YLength = yLength;
            TileSize = tileSize;
            Tiles = new ConstructTile[XLength * YLength];
        }

        public IEnumerable<(int, int, ConstructTile)> GetTiles()
        {
            for(int x = 0; x < XLength; x++)
            {
                for(int y = 0; y < YLength; y++)
                {
                    yield return (x, y, this[x, y]);
                }
            }
        }
    }
}
