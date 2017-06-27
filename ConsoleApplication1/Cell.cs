using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    class Cell
    {
        int row;
        int col;
        int section;
        int val;

        public int Row
        {
            get { return row; }
            set { row = value; }
        }

        public int Col
        {
            get { return col; }
            set { col = value; }
        }

        public int Section
        {
            get { return section; }
            private set { section = value; }
        }

        public int Value
        {
            get { return val; }
            set { val = value; }
        }

        public List<int> PossibleValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public Cell(int x, int y)
        {
            Row = x;
            Col = y;
            Section = GetSection(x, y);
            Value = 0;
        }

        public Cell(int x, int y, char value)
        {
            Row = x;
            Col = y;
            Section = GetSection(x, y);

            //parse char into value
            if (!int.TryParse(value.ToString(), out val))
            {
                Value = 0;
            }
        }

        public int GetSection(int x, int y)
        {
            if (x < 4) //top 3 blocks
            {
                if(y < 4)
                {
                    return 1;
                }
                else if (y < 7)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
            else if (x < 7) //middle 3 blocks
            {
                if (y < 4)
                {
                    return 4;
                }
                else if (y < 7)
                {
                    return 5;
                }
                else
                {
                    return 6;
                }
            }
            else //bottom 3 blocks
            {
                if (y < 4)
                {
                    return 7;
                }
                else if(y < 7)
                {
                    return 8;
                }
                else
                {
                    return 9;
                }
            }
        }

        public void SetValueFromChar(char c)
        {
            //parse char into value
            if (!int.TryParse(c.ToString(), out val))
            {
                Value = 0;
            }
        }

    }
}
