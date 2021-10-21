using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TenhouViewer
{
    public partial class fMain : Form
    {
        Render.HandRender[] Hands = new Render.HandRender[4];
        Render.TileHighlight[] Highlight = new Render.TileHighlight[4];

        public fMain()
        {
            InitializeComponent();
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            Mahjong.Hand Hand0 = new Mahjong.Hand(new int[13] { 21, 22, 12, 44, 73, 75, 79, 124, 83, 32, 103, 104, 8 });

            Highlight[0] = new Render.TileHighlight();
            Highlight[1] = new Render.TileHighlight();
            Highlight[2] = new Render.TileHighlight();
            Highlight[3] = new Render.TileHighlight();

            Hands[0] = new Render.HandRender(this, Render.HandPosition.Bottom);
            Hands[1] = new Render.HandRender(this, Render.HandPosition.Right);
            Hands[2] = new Render.HandRender(this, Render.HandPosition.Top);
            Hands[3] = new Render.HandRender(this, Render.HandPosition.Left);

            Highlight[0].SetTileDanger(3, 5);
            Highlight[0].SetTileDanger(6, 4);
            Highlight[0].SetTileDanger(4, 3);
            Highlight[0].SetTileDanger(9, 2);
            Highlight[0].SetTileDanger(13, 1);

            Hands[0].SetHighlight(Highlight[0]);
            Hands[0].SetHand(Hand0);

            PlaceComponents();
        }

        private void fMain_Resize(object sender, EventArgs e)
        {
            PlaceComponents();
        }

        private void PlaceComponents()
        {
            int i;

            for(i = 0; i < Hands.Length; i++) Hands[i].Update();
        }
    }
}
