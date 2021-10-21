using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TenhouViewer.Mahjong
{
    enum TenhouFormType
    {
        Toitsu,  // Пара
        Ankou,   // Сет из трёх одинаковых тайлов
        Mentsu,  // Сет из трёх последовательных тайлов
        Kanchan, // Форма из двух тайлов через один: 1-3, 2-4, 3-5 и т.д.
        Ryanmen, // форма из двух последовательных тайлов (сюда же отнесётся и пентян): 12, 23, 34, 45 и т.д.
        ToitsuWait, // форма из двух одинаковых тайлов (ожидающая завершения дло анко)
    }

    class Hand
    {
        private Tile[] Tiles = new Tile[13];
        private Tile TsumoTile;

        private int NakiCount = 0;

        private int[] Tehai = new int[38];
        private int[] Waitings = new int[38];
        private int[] UkeIreList = new int[38];

        private int ShantenNumber;
        private bool Riichi;
        private bool CanAgari; // можно ли с этой рукой объявить цумо?
        private bool Furiten;

        private Stack Forms = new Stack();

        private int Pair; // Количество пар (читой)

        private int Mentsu; // Количество сетов
        private int Kouho;  //
        private int Toitsu; // Количество пар

        private int ShantenChiitoi; // Шантен до 7 пар
        private int ShantenKokushi; // Шантен до Кокуши
        private int ShantenNormal;  // Шантен до обычной руки

        private int TempShantenNormal;

        private int UkeIre;

        private int WaitingCount; // количество сторон в выигрышном ожидании

        private List<uint> AllTiles = new List<uint>(); // Все тайлы из руки
        private List<uint> Discard = new List<uint>();  // Дискард

        int[] TileList = new int[14];

        public Hand(int[] List)
        {
            if (List.Length != 13) return;

            TileList = List;

            SortHand();
            AnalyzeHand();
        }

        // Создадим массив для анализа руки
        private void CreateTehai()
        {
            ShantenNumber = 8;

            for (int i = 0; i < 38; i++) Tehai[i] = 0;

            for (int i = 0; i < 13; i++) if (Tiles[i] != null) Tehai[Tiles[i].TileIndex]++;
            if (TsumoTile != null) Tehai[TsumoTile.TileIndex]++;
        }

        public Tile GetTile(int Index)
        {
            if (Index < 13)
                return Tiles[Index];
            else
                return TsumoTile;
        }
        
        public void DrawTile(int TileIndex)
        {
            TsumoTile = new Tile(TileIndex);

            TileList[13] = TileIndex;
        }

        public void DeclareRiichi()
        {
            Riichi = true;
        }

        public bool IsConcealed
        {
            get { return (NakiCount == 0); }
        }

        public bool IsFuriten
        {
            get { return Furiten; }
        }

        public int Shanten
        {
            get { return ShantenNumber; }
        }

        private void SortHand()
        {
            Array.Sort(TileList);

            for (int i = 0; i < TileList.Length; i++)
            {
                Tiles[i] = new Tile(TileList[i]);
            }
        }

        // Даматен?
        public bool IsDamaten
        {
            get { return (IsConcealed && (ShantenNumber == 0) && (!Riichi)); }
        }

        private UInt32 EncodeForm(TenhouFormType Type, int Tile1, int Tile2, int Tile3)
        {
            return (Convert.ToUInt32(Type) << 24) | (((uint)Tile1 & 0xFF) << 16) | (((uint)Tile2 & 0xFF) << 8) | ((uint)Tile3 & 0xFF);
        }

        //Проанализировать руку
        private void AnalyzeHand()
        {
            CreateTehai();

            ShantenNumber = GetShanten();

            if (WaitingCount > 0)
            {
                // проверим на фуритен
                for (uint i = 0; i < 38; i++)
                {
                    if (Waitings[i] == 0) continue;

                    if (Discard.Contains(i))
                        Furiten = true;
                }
            }

            // Посчитаем количество тайлов, улучшающих руку, и составим список.
            CalcUkeIre();
        }

        private void CalcWaitings()
        {
            int i;

            Stack Temp = (Stack)Forms.Clone();
            bool IsSyanpon = false;

            for (i = 0; i < Forms.Count; i++)
            {
                UInt32 Form = Convert.ToUInt32(Temp.Pop());

                TenhouFormType FormType = (TenhouFormType)((Form >> 24) & 0xFF);
                int Tile1 = Convert.ToInt32((Form >> 16) & 0xFF);
                int Tile2 = Convert.ToInt32((Form >> 8) & 0xFF);
                int Tile3 = Convert.ToInt32((Form >> 0) & 0xFF);

                switch (FormType)
                {
                    case TenhouFormType.Toitsu:
                        if (!IsSyanpon) break;
                        if (Waitings[Tile1] == 0) { Waitings[Tile1] = 1; WaitingCount++; }

                        break;
                    case TenhouFormType.ToitsuWait:
                        IsSyanpon = true;
                        if (Waitings[Tile1] == 0) { Waitings[Tile1] = 1; WaitingCount++; }
                        break;
                    case TenhouFormType.Ryanmen:
                        // Не пентян нижний
                        if (Tile1 % 10 != 1)
                        {
                            if (Waitings[Tile1 - 1] == 0) { Waitings[Tile1 - 1] = 1; WaitingCount++; }
                        }
                        // Не пентян верхний
                        if (Tile2 % 10 != 9)
                        {
                            if (Waitings[Tile2 + 1] == 0) { Waitings[Tile2 + 1] = 1; WaitingCount++; }
                        }
                        break;
                    case TenhouFormType.Kanchan:
                        // Средний тайл - ожидание.
                        if (Waitings[Tile1 + 1] == 0) { Waitings[Tile1 + 1] = 1; WaitingCount++; }
                        break;
                }
            }
        }

        // Посчитаем уке-ире
        private void CalcUkeIre()
        {
            int i;

            UkeIre = 0;

            if (ShantenNumber > 0)
            {
                for (i = 0; i < 38; i++)
                {
                    UkeIreList[i] = 0;

                    if (i % 10 == 0) continue; // 10, 20, 30 пропускаем

                    // Добавим 15й тайл к руке
                    Tehai[i]++;

                    int SChiitoi = CalcShantenChiitoi();
                    int SKokushi = CalcShantenChiitoi();
                    int SNormal = CalcShantenNormal();

                    // Уберём его
                    Tehai[i]--;

                    // Посчитаем шантен
                    if ((SChiitoi < ShantenNumber) ||
                        (SKokushi < ShantenNumber) ||
                        (SNormal < ShantenNumber))
                    {
                        // Сколько тайлов осталось?
                        int UkeIreCount = 4 - Tehai[i];
                        UkeIreList[i] = UkeIreCount;
                        UkeIre += UkeIreCount;
                    }
                }

                if (UkeIre == 0)
                {

                }
            }
        }

        private int GetShanten()
        {
            // Считаем параметры рук:

            // Шантен до обычной руки
            ShantenNormal = CalcShantenNormal();
            // Шантен до 7 пар
            ShantenChiitoi = CalcShantenChiitoi();
            // Шантен до кокуши
            ShantenKokushi = CalcShantenKokushi();

            return Math.Min(ShantenChiitoi, Math.Min(ShantenKokushi, ShantenNormal));
        }

        // Шантен до читойцу
        private int CalcShantenChiitoi()
        {
            int i;
            int Types = 0;

            // В открытых руках нет читоя
            // if (NakiList.Count > 0) return 6;

            // Считаем пары
            Pair = 0;
            for (i = 0; i < 38; i++)
            {
                if (Tehai[i] >= 2) Pair++;
            }

            // Посчитаем количество типов тайлов
            for (i = 0; i < 38; i++)
            {
                if (Tehai[i] > 0) Types++;
            }

            // Шантен равен 6 - количество имеющихся пар
            int shanten = 6 - Pair;

            // Единственное ограничение: пары не должны повторяться, повторы (4 одинаковых тайла) увеличивают шантен
            if (Types < 7) shanten += 7 - Types;

            // Ожидания только для темпая
            if (shanten == 0)
            {
                for (i = 0; i < 38; i++)
                {
                    if (Tehai[i] == 1)
                    {
                        if (Waitings[i] == 0) { Waitings[i] = 1; WaitingCount++; }
                    }
                }
            }

            return shanten;
        }

        private int CalcShantenKokushi()
        {
            int KokushiPair = 0;
            int shanten = 13;
            int i;

            // В открытых руках нет кокуши
            // if (NakiList.Count > 0) return 13;

            // посмотрим терминалы
            // Для кокуши нужны все единицы и девятки
            for (i = 1; i < 30; i++)
            {
                if ((i % 10 == 1) || (i % 10 == 9))
                {
                    if (Tehai[i] > 0) shanten--;
                    if ((Tehai[i] >= 2) && (KokushiPair == 0)) KokushiPair = 1;
                }
            }

            // Посмотрим благородные тайлы
            for (i = 31; i < 38; i++)
            {
                if (Tehai[i] > 0)
                {
                    shanten--;
                    if ((Tehai[i] >= 2) && (KokushiPair == 0)) KokushiPair = 1;
                }
            }
            // Если есть пара - уменьшим шантен
            shanten -= KokushiPair;

            // Ожидания только для темпая
            if (shanten == 0)
            {
                for (i = 1; i < 30; i++)
                {
                    if ((i % 10 == 1) || (i % 10 == 9))
                    {
                        if (Tehai[i] == 0)
                        {
                            if (Waitings[i] == 0) { Waitings[i] = 1; WaitingCount++; }
                        }
                    }
                }

                // Посмотрим благородные тайлы
                for (i = 31; i < 38; i++)
                {
                    if (Tehai[i] == 0)
                    {
                        if (Waitings[i] == 0) { Waitings[i] = 1; WaitingCount++; }
                    }
                }
            }

            return shanten;
        }


        private int CalcShantenNormal()
        {
            int i;

            // Шантен максимален
            TempShantenNormal = 8;

            // Очистим список форм
            Forms.Clear();

            // Сетов пока не считали
            Mentsu = 0;// (uint)NakiList.Count; // Открытые уже есть если, добавим
            Toitsu = 0;
            Kouho = 0;

            for (i = 1; i < 38; i++)
            {
                // Если тайлов этой масти более одного, то от него можно попробовать отчекрыжить пару и посмотреть,
                // что из этого выйдет
                if (Tehai[i] >= 2)
                {
                    Toitsu++;
                    Tehai[i] -= 2;

                    Forms.Push(EncodeForm(TenhouFormType.Toitsu, i, i, 0));
                    CutMentsu(1);
                    Forms.Pop();

                    Tehai[i] += 2;
                    Toitsu--;
                }
            }
            CutMentsu(1);

            return TempShantenNormal;
        }

        // По очереди вырезаем сеты из руки и смотрим, что там остаётся
        private void CutMentsu(int StartTile)
        {
            int i = StartTile;

            // Ищем первый тайл, имеющийся в руке
            if (i < 38) for (; (i < 38) && (Tehai[i] == 0); i++) ;

            // Если число слишком большое, смотрим рянмены и выходим
            if (i >= 38) { CutTaatsu(1); return; }

            // если таких тайлов 3, то можно их рассмотреть как пон
            if (Tehai[i] >= 3)
            {
                Mentsu++;
                Tehai[i] -= 3;

                Forms.Push(EncodeForm(TenhouFormType.Ankou, i, i, i));
                CutMentsu(i);
                Forms.Pop();

                Tehai[i] += 3;
                Mentsu--;
            }

            // Если есть следующий и ещё послеследующий тайл, то это можно рассмотреть как чи
            if ((i < 30) && (Tehai[i + 1] > 0) && (Tehai[i + 2] > 0))
            {
                Mentsu++;
                Tehai[i]--;
                Tehai[i + 1]--;
                Tehai[i + 2]--;

                Forms.Push(EncodeForm(TenhouFormType.Mentsu, i, i + 1, i + 2));
                CutMentsu(i);
                Forms.Pop();

                Tehai[i]++;
                Tehai[i + 1]++;
                Tehai[i + 2]++;
                Mentsu--;
            }

            // Отрезали, смотрим дальше
            CutMentsu(i + 1);
        }

        private void CutTaatsu(int Start)
        {
            int i = Start;

            if (i < 38) for (; (i < 38) && (Tehai[i] == 0); i++) ;

            // Расчёт результата
            if (i >= 38)
            {
                int Temp = 8 - Mentsu * 2 - Kouho - Toitsu;
                if (Temp <= TempShantenNormal)
                {
                    if (Temp == 0)
                    {
                        if ((Kouho == 0) && (Toitsu == 0))
                        {
                            // Ожидание танки!
                            for (int j = 0; j < Tehai.Length; j++)
                            {
                                if (Tehai[j] > 0)
                                {
                                    if (Waitings[j] == 0) { Waitings[j] = 1; WaitingCount++; }
                                }
                            }
                        }
                        if ((Kouho == 0) && (Toitsu == 1))
                        {
                            // агари!
                            CanAgari = true;
                        }
                        else
                        {
                            // Темпай! Можно посмотреть ожиданя в руке
                            CalcWaitings();
                        }
                    }
                    TempShantenNormal = Temp;
                }
                return;
            }

            if (Mentsu + Kouho < 4)
            {
                // Пары
                if (Tehai[i] == 2)
                {
                    Kouho++;
                    Tehai[i] -= 2;

                    Forms.Push(EncodeForm(TenhouFormType.ToitsuWait, i, i, 0));
                    CutTaatsu(i);
                    Forms.Pop();

                    Tehai[i] += 2;
                    Kouho--;
                }

                // Пентян и рянмен
                if ((i < 30) && (Tehai[i + 1] > 0))
                {
                    Kouho++;
                    Tehai[i]--; Tehai[i + 1]--;

                    Forms.Push(EncodeForm(TenhouFormType.Ryanmen, i, i + 1, 0));
                    CutTaatsu(i);
                    Forms.Pop();

                    Tehai[i]++; Tehai[i + 1]++;
                    Kouho--;
                }

                // Кантян
                if ((i < 30) && ((i % 10) <= 8) && (Tehai[i + 2] > 0))
                {
                    Kouho++;
                    Tehai[i]--; Tehai[i + 2]--;

                    Forms.Push(EncodeForm(TenhouFormType.Kanchan, i, i + 2, 0));
                    CutTaatsu(i);
                    Forms.Pop();

                    Tehai[i]++; Tehai[i + 2]++;
                    Kouho--;
                }
            }
            CutTaatsu(i + 1);
        }
    }
}
