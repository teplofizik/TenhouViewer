using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TenhouViewer.Render
{
    public enum HandPosition
    {
        Bottom, Right, Top, Left
    };

    class HandRender
    {
        HandPosition Position;
        Form  TargetForm;
        Panel TargetPanel;

        Label ShantenCount;

        Mahjong.Hand TargetHand;
        TileHighlight Highlight;

        PictureBox[] ClosedPB = new PictureBox[14];

        int TileWidth;
        int TileHeight;
        int PanelWidth;
        int PanelHeight;

        int AdditionalHeight = 30;

        public HandRender(Form TargetForm, HandPosition Position)
        {
            this.TargetForm = TargetForm;
            this.Position = Position;

            MeasureTiles();
            CreateElements();
            
            PlacePanel();
        }

        public void SetHighlight(TileHighlight Highlight)
        {
            this.Highlight = Highlight;
        }

        public void Update()
        {
            PlacePanel();
        }

        public void SetHand(Mahjong.Hand Hand)
        {
            TargetHand = Hand;
            DrawTiles(Hand);

            ShantenCount.Text = "Shanten: " + Convert.ToString(Hand.Shanten);
        }

        private void MeasureTiles()
        {
            string FileName = ".//tiles//0z.gif";
            Image img = new Bitmap(FileName);

            TileWidth = img.Width;
            TileHeight = img.Height;

            PanelWidth = TileWidth * 14 + 5;
            PanelHeight = TileHeight + AdditionalHeight;
        }

        private void onTileClick(Object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            int Index = Convert.ToInt32(pb.Tag);

            Mahjong.Tile Tile = TargetHand.GetTile(Index);

            int TileIndex = Tile.TileIndex;
            string TileName = Tile.GetName();
        }

        private void ColorizeTile(int Index, Color Target, double Percent)
        {
            PictureBox pb = ClosedPB[Index];
            if (pb.Image == null) return;

            ShowTile(Index, TargetHand.GetTile(Index).GetImagePath());

            Bitmap bmp = new Bitmap(pb.Image);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color S = bmp.GetPixel(x, y);

                    int R = Convert.ToInt32(S.R + (1 - S.R / 255.0) * Target.R);
                    int G = Convert.ToInt32(S.G + (1 - S.G / 255.0) * Target.G);
                    int B = Convert.ToInt32(S.B + (1 - S.B / 255.0) * Target.B);

                    R = Convert.ToInt32(S.R * (1 - Percent) + R * Percent);
                    G = Convert.ToInt32(S.G * (1 - Percent) + G * Percent);
                    B = Convert.ToInt32(S.B * (1 - Percent) + B * Percent);

                    bmp.SetPixel(x, y, Color.FromArgb(R, G, B));
                }
            }

            pb.Image = bmp;
        }

        private void CreateTile(int Index)
        {
            PictureBox pb = new PictureBox();

            pb.Tag = Index;
            pb.SizeMode = PictureBoxSizeMode.StretchImage;
            pb.BorderStyle = BorderStyle.None;
            pb.ClientSize = new Size(TileWidth, TileHeight);
            pb.Click += new EventHandler(onTileClick);

            pb.Image = null;

            switch (Position)
            {
                case HandPosition.Bottom:
                    pb.Top = 0;
                    pb.Left = Index * TileWidth;
                    if (Index == 13) pb.Left += 5;
                    break;
                case HandPosition.Left:
                    pb.Top = Index * TileWidth;
                    pb.Left = 0;
                    if (Index == 13) pb.Top += 5;
                    break;
                case HandPosition.Top:
                    pb.Top = 0;
                    pb.Left = (13 - Index) * TileWidth;
                    if (Index < 13) pb.Left += 5;
                    break;
                case HandPosition.Right:
                    pb.Top = (13 - Index) * TileWidth;
                    pb.Left = 0;
                    if (Index < 13) pb.Top += 5;
                    break;
            }

            TargetPanel.Controls.Add(pb);

            ClosedPB[Index] = pb;
        }

        private void CreateElements()
        {
            TargetPanel = new Panel();
            TargetPanel.BackColor = Color.AliceBlue;

            TargetForm.Controls.Add(TargetPanel);

            // Тайлы
            for (int i = 0; i < ClosedPB.Length; i++)
            {
                CreateTile(i);
            }

            // Показометр шантена
            switch (Position)
            {
                case HandPosition.Bottom:
                    ShantenCount = new Label();
                    ShantenCount.AutoSize = true;
                    ShantenCount.Top = TileHeight;
                    ShantenCount.Left = 10;
                    ShantenCount.Font = new Font("Arial", 15);
                    TargetPanel.Controls.Add(ShantenCount);
                    break;
            }
        }

        private void PlacePanel()
        {
            switch (Position)
            {
                case HandPosition.Bottom:
                    TargetPanel.Width = PanelWidth;
                    TargetPanel.Height = PanelHeight;

                    TargetPanel.Left = (TargetForm.ClientSize.Width - PanelWidth) / 2;
                    TargetPanel.Top = TargetForm.ClientSize.Height - PanelHeight - 10;
                    break;
                case HandPosition.Top:
                    TargetPanel.Width = PanelWidth;
                    TargetPanel.Height = PanelHeight;

                    TargetPanel.Left = (TargetForm.ClientSize.Width - PanelWidth) / 2;
                    TargetPanel.Top = 10;
                    break;
                case HandPosition.Right:
                    TargetPanel.Width = PanelHeight;
                    TargetPanel.Height = PanelWidth;

                    TargetPanel.Left = TargetForm.ClientSize.Width - PanelHeight - 10;
                    TargetPanel.Top = (TargetForm.ClientSize.Height - PanelWidth) / 2;
                    break;
                case HandPosition.Left:
                    TargetPanel.Width = PanelHeight;
                    TargetPanel.Height = PanelWidth;

                    TargetPanel.Left = 10;
                    TargetPanel.Top = (TargetForm.ClientSize.Height - PanelWidth) / 2;
                    break;
            }
        }

        private void HideTile(int Index)
        {
            PictureBox pb = ClosedPB[Index];

            pb.Image = null;
        }

        private void ShowTile(int Index, string FileName)
        {
            Image img = new Bitmap(FileName);

            switch (Position)
            {
                case HandPosition.Bottom: break;
                case HandPosition.Left: img.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                case HandPosition.Top: img.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                case HandPosition.Right: img.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
            }

            PictureBox pb = ClosedPB[Index];

            pb.ClientSize = new Size(img.Width, img.Height);
            pb.Image = (Image)img;
        }

        private void ClearTiles()
        {
            for (int i = 0; i < ClosedPB.Length; i++) HideTile(i);
        }

        private void DrawTiles(Mahjong.Hand Hand)
        {
            for (int i = 0; i < ClosedPB.Length; i++)
            {
                Mahjong.Tile Tile = Hand.GetTile(i);
                if (Tile != null)
                {
                    ShowTile(i, Tile.GetImagePath());
                    if (Highlight != null)
                    {
                        Color TileColor = Highlight.GetTileColor(Tile);

                        if(TileColor != Color.White) ColorizeTile(i, TileColor, 0.4);
                    }
                }
                else
                {
                    HideTile(i);
                }
            }
        }
    }
}
