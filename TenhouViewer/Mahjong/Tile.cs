using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TenhouViewer.Mahjong
{
    class Tile
    {
        string TileName;
        int Value;
        int Index;
        string TileType;

        const string Path = "tiles/";

        //1  - 1 ман
        //↓
        //9  - 9 ман
        //10 - не определено
        //11 - 1 пин
        //↓
        //19 - 9 пин
        //20 - не определено
        //21 - 1 соу
        //↓
        //29 - 9 соу
        //30 - не определено
        //31 - 東 (восток)
        //32 - 南 (юг)
        //33 - 西 (запад)
        //34 - 北 (север)
        //35 - 白 (белый дракон)
        //36 - 発 (зелёный дракон)
        //37 - 中 (красный дракон)

        // Согласно индекса
        public Tile(int Index)
        {
            this.Index = Index;

            int Pos = TileIndex;

            int Value = (Pos % 10); // Позиция в масти
            bool Red = ((Value == 5) && ((Index & 0x03) == 0) && (Index < 36 * 3));

            if (Index < 36 * 1) // Ман
            {
                TileType = "m";
            }
            else if (Index < 36 * 2) // Пин
            {
                TileType = "p";
            }
            else if (Index < 36 * 3) // Соу
            {
                TileType = "s";
            }
            else // Благородные
            {
                TileType = "z";
            }

            if (Red)
            {
                TileName = "0" + TileType;
            }
            else
            {
                TileName = Convert.ToString(Value) + TileType;
            }

            this.Value = Value;
        }

        public int TileIndex
        {
            get
            {
                int Pos = (Index / 4) + 1; // Номер тайла в принципе

                if (Pos > 9) Pos++;
                if (Pos > 19) Pos++;
                if (Pos > 29) Pos++;

                return Pos;
            }
        }

        public string GetName()
        {
            return TileName;
        }

        public Image GetImage()
        {
            return new Bitmap(GetImagePath());
        }

        public string GetImagePath()
        {
            return Path + TileName + ".gif";
        }

        public string GetTileType()
        {
            return TileType;
        }

        public int GetValue()
        {
            return Value;
        }

        public int GetTile()
        {
            return TileIndex;
        }
    }
}
