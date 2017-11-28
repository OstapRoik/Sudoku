using System;
using System.Linq;
using System.Collections.Generic;

namespace Game
{
    class GridForSudoku
    {
        private int[][] array;
        private readonly int LevelSize;// 9 or 16
        private readonly int SqrtLevelSize;
        private const int AmountMixing = 200;
        private readonly int Complexity;// 50 or 150       LevelSize*LevelSize*0.65
        private bool[] CheckRow;
        private bool[] CheckCol;
        private bool[] CheckSquare;
        private bool Correct;
        private bool[,] ConstElement;

        private delegate void MethodMixing();

        public GridForSudoku(int _LevelSize, int _Complexity)
        {
            LevelSize = _LevelSize;
            Complexity = _Complexity;
            SqrtLevelSize = (int)Math.Sqrt(LevelSize);
            CheckRow = new bool[LevelSize];
            CheckCol = new bool[LevelSize];
            CheckSquare = new bool[LevelSize];
            Correct = false;
            Random RandomValue;

            //Generate
            array = new int[LevelSize][];
            for (int i = 0; i < LevelSize; i++)
            {
                array[i] = new int[LevelSize];
                for (int j = 1; j <= LevelSize; j++)
                {
                    array[i][j-1] = j;
                }
                array[i] = ShiftLeft(array[i], SqrtLevelSize * (i%SqrtLevelSize));
                array[i] = ShiftLeft(array[i], (int)(i / SqrtLevelSize));
            }

            int[][] OldArray = array;
            double Uniqueness;
            do
            {
                List<MethodMixing> mixing = new List<MethodMixing>();
                mixing.Add(Transposing);
                mixing.Add(SwapCol);
                mixing.Add(SwapCols);
                mixing.Add(SwapRow);
                mixing.Add(SwapRows);
                RandomValue = new Random();
                for (int i = 0; i < AmountMixing; i++)
                {
                    mixing[RandomValue.Next(0, mixing.Count)]();
                }

                int value = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (OldArray[i][j] == array[i][j])
                            value++;
                    }
                }

                Uniqueness = (double)value / (LevelSize * LevelSize);
            } while (Uniqueness > 0.25);

            //Erase
            RandomValue = new Random();
            HashSet<int> ArrayRandomValue = new HashSet<int>();
            for (int i = 0; i < Complexity; )
            {
                if (ArrayRandomValue.Add(RandomValue.Next(0, LevelSize * LevelSize)))
                    i++;
            }
            foreach (var item in ArrayRandomValue)
            {
                array[(int)(item / LevelSize)][item % LevelSize] = 0;
            }

            ConstElement = new bool[LevelSize, LevelSize];
            for (int i = 0; i < LevelSize; i++)
            {
                for (int j = 0; j < LevelSize; j++)
                {
                    if (array[i][j] == 0)
                        ConstElement[i, j] = false;
                    else
                        ConstElement[i, j] = true;
                }
            }
        }

        public int[][] Value
        {
            get
            {
                return array;
            }
        }
        public int SizeLevel
        {
            get
            {
                return LevelSize;
            }
        }
        public bool IsCorrect
        {
            get
            {
                return Correct;
            }
        }
        public bool[,] ArrayConstElement
        {
            get
            {
                return ConstElement;
            }
        }
        public bool[] CheckArrayRow
        {
            get
            {
                return CheckRow;
            }
        }
        public bool[] CheckArrayCol
        {
            get
            {
                return CheckCol;
            }
        }
        public bool[] CheckArraySquare
        {
            get
            {
                return CheckSquare;
            }
        }

        public void ChangeElement(int NumberElement, int Value)
        {
            // NumberElement 0 ... (LevelSize * LevelSize) - 1
            // Value 1 ... LevelSize

            if (NumberElement >= LevelSize * LevelSize || NumberElement < 0)
                return;
            if (Value < 0 || Value > LevelSize)
                return;
            if (ConstElement[(int)(NumberElement / LevelSize), NumberElement % LevelSize] == true)
                return;

            array[(int)(NumberElement / LevelSize)][NumberElement % LevelSize] = Value;

            for (int i = 0; i < LevelSize; i++)
            {
                CheckRow[i] = CheckArray(array[i]);
                CheckCol[i] = CheckArray(GetCol(i));
                CheckSquare[i] = CheckArray(GetSquare(i));
            }

            if (!CheckRow.Contains(false))
            {
                if(!CheckCol.Contains(false))
                {
                    if(!CheckSquare.Contains(false))
                    {
                        Correct = true;
                    }
                }
            }
        }
        private bool CheckArray(int[] array)
        {
            bool check = true;
            for (int i = 1; i <= array.Length; i++)
            {
                if (!array.Contains(i))
                {
                    check = false;
                    break;
                }
            }
            return check;
        }
        private int[] ShiftLeft(int[] array, int Count)
        {
            if (Count <= 0)
                return array;
            array.CopyTo(array, 0);
            for (int i = 0; i < Count; i++)
            {
                int temp = array[0];
                for (int j = 0; j < array.Length - 1; j++)
                {
                    int SwapValue = array[j];
                    array[j] = array[j + 1];
                    array[j + 1] = SwapValue;
                }
                array[array.Length - 1] = temp;
            }
            return array;
        }
        private int[] GetCol(int Count)
        {
            // Count 0 ... (LevelSize - 1)

            int[] result = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i][Count];
            }
            return result;
        }
        private int[] GetSquare(int Count)
        {
            // Count 0 ... (LevelSize - 1)

            int[] result = new int[array.Length];
            int SquareNumber = Count + 1;
            int TempNumber = (int)(Count / SqrtLevelSize);
            for (int i = TempNumber * SqrtLevelSize, k = 0; i < (TempNumber + 1) * SqrtLevelSize; i++)
            {
                for (int j = SqrtLevelSize * (SquareNumber - TempNumber * SqrtLevelSize) - SqrtLevelSize; j < SqrtLevelSize * (SquareNumber - TempNumber * SqrtLevelSize); j++, k++)
                {
                    result[k] = array[i][j];
                }
            }
            return result;
        }
        private void Transposing()
        {
            int[][] tempArray = new int[array.Length][];
            for (int i = 0; i < array.Length; i++)
            {
                tempArray[i] = GetCol(i);
            }
            array = tempArray;
        }
        private void SwapRow()
        {
            Random RandomValue = new Random();
            int RandomRows = RandomValue.Next(0, SqrtLevelSize) * SqrtLevelSize;
            int RandomRow1 = RandomValue.Next(0, SqrtLevelSize) + RandomRows;
            int RandomRow2;
            do
            {
                RandomRow2 = RandomValue.Next(0, SqrtLevelSize) + RandomRows;
            } while (RandomRow1 == RandomRow2);

            Swap(ref array[RandomRow1], ref array[RandomRow2]);
        }
        private void SwapCol()
        {
            Transposing();
            SwapRow();
            Transposing();
        }
        private void SwapRows()
        {
            Random RandomValue = new Random();
            int RandomRows1 = RandomValue.Next(0, SqrtLevelSize) * SqrtLevelSize;
            int RandomRows2;
            do
            {
                RandomRows2 = RandomValue.Next(0, SqrtLevelSize) * SqrtLevelSize;
            } while (RandomRows1 == RandomRows2);

            for (int i = 0; i < SqrtLevelSize; i++)
            {
                Swap(ref array[i + RandomRows1], ref array[i + RandomRows2]);
            }
        }
        private void SwapCols()
        {
            Transposing();
            SwapRows();
            Transposing();
        }
        private void Swap(ref int[] array1, ref int[] array2)
        {
            int[] tempArray = array1;
            array1 = array2;
            array2 = tempArray;
        }

        public override string ToString()
        {
            string[] preResult = new string[array.Length];
            for (int i = 0; i < preResult.Length; i++)
            {
                preResult[i] = string.Join(" ", array[i]);
            }
            string result = string.Empty;
            for (int i = 0; i < preResult.Length; i++)
            {
                result = string.Join("\r\n", preResult);
            }
            return result;
        }
    }
}