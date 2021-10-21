using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TenhouViewer.Render
{
    class TileHighlight
    {
        private int[] Danger = new int[38];

        private Color[] ColorTable = new Color[] {
            Color.White,
            Color.FromArgb(29, 219, 0), 
            Color.FromArgb(125, 214, 0),
            Color.FromArgb(214, 214, 0),
            Color.FromArgb(214, 125, 0),
            Color.FromArgb(214, 29, 0),
        };

        public TileHighlight()
        {
            for (int i = 0; i < Danger.Length; i++)
            {
                Danger[i] = 0;
            }
        }

        public Color GetTileColor(Mahjong.Tile Tile)
        {
            int Index = Tile.TileIndex;

            return ColorTable[Danger[Index]];
        }

        public void SetTileDanger(int Index, int Danger)
        {
            this.Danger[Index] = Danger;
        }
    }
}
