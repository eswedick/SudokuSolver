using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    class Board
    {
        List<Cell> Cells = new List<Cell>();

        public Board()
        {
            Cells = new List<Cell>();

            for (int r = 1; r < 10; r++)
            {
                for (int c = 1; c < 10; c++)
                {
                    Cells.Add(new Cell(r, c));
                }
            }
        }

        /// <summary>
        /// Parse the board starting state from a text file and init the board object
        /// </summary>
        /// <param name="FilePath">The file containing the puzzle</param>
        public Boolean Load(string FilePath)
        {
            //ensure file exists
            try
            {
                if (File.Exists(FilePath))
                {
                    //open file
                    using (StreamReader sr = File.OpenText(FilePath))
                    {
                        string text = "";
                        int r = 1, c = 1;

                        //while the first 9 rows have data
                        while ((text = sr.ReadLine()) != null && r < 10)
                        {
                            //process the first 9 chars
                            while (c < 10)
                            {
                                foreach (char value in text)
                                {
                                    //for non 0 values
                                    if (value != 'X')
                                    {
                                        //get cell and set value
                                        Cell cell = Cells.Single(x => x.Row == r && x.Col == c);
                                        cell.SetValueFromChar(value);

                                        //Remove value possibility from other cells
                                        PropagateValue(cell);

                                    }

                                    c++;
                                }
                            }

                            c = 1; //reset column number and go to next row
                            r++;
                        }
                    }
                }
                else
                {
                    Console.Write("The input file " + FilePath + " was not found.");
                    return false;
                }

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occurred loading the board: " + e.InnerException + ". Please try a different puzzle.");
                return false;
            }
        }

        /// <summary>
        /// Remove new value from the possible values list of other cells in the row, col, block groups.
        /// </summary>
        /// <param name="row">The current cell row</param>
        /// <param name="col">The current cell column</param>
        /// <param name="val">The value to propagate</param>
        public void PropagateValue(Cell c)
        {
            //Remove from cells in same row
            foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Row == c.Row)))
            {
                cell.PossibleValues.Remove(c.Value);
            }
            //Remove from cells in same col
            foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Col == c.Col)))
            {
                cell.PossibleValues.Remove(c.Value);
            }

            //Remove from cells in same 3x3 block
            foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Section == c.Section)))
            {
                cell.PossibleValues.Remove(c.Value);
            }

            //check for pointing pair
            CheckForPointingPairRowCol();

            //check for pointing pair in box
            CheckForPointingPairBox();

            //check for naked triple
            CheckForNakedTriple();

            //check for hidden singles
            CheckForHiddenSingle();

            //Fill any values we can
            CheckForSinglePossibleValue();
        }

        /// <summary>
        /// Set value where only one possibility remains.
        /// </summary>
        public void CheckForSinglePossibleValue()
        {
            //check for single possible value in cell
            foreach(Cell cell in Cells.Where(x => (x.Value == 0) && (x.PossibleValues.Count == 1)))
            {
                cell.Value = cell.PossibleValues[0];

                //propagate new value
                PropagateValue(cell);
            }
        }

        public void CheckForHiddenSingle()
        {
            for (int i = 1; i < 10; i++)
            {
                foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Section == i)))
                {
                    List<int> possibleValues = GetRemainingPossibilitiesForSection(cell);
                    if (possibleValues.Count() == 1)
                    {
                        cell.Value = possibleValues[0];

                        //propagate new value
                        PropagateValue(cell);
                    }
                }
            }
        }

        /// <summary>
        /// Removes possibilities from cells in same row or column, but different section
        /// </summary>
        public void CheckForPointingPairRowCol()
        {
            List<int> possibleValues;
            List<int> outsideValues;
            List<Cell> cellToCheckGroup;

            foreach (Cell cell in Cells.Where(x => (x.Value == 0)))
            {
                //get cells in row
                cellToCheckGroup = GetCellsInRowAndSection(cell);
                //intersected values in row 
                possibleValues = GetIntersectedPossibilitiesForRow(cell);

                //distinct values in row
                outsideValues = GetRemainingPossibilitiesForSection(cellToCheckGroup);

                //values in section that arent outside the section can be removed
                foreach (int value in possibleValues.Except(outsideValues))
                {
                    foreach (Cell c in Cells.Where(x => (x.Value == 0) && (x.Row == cell.Row) && (x.Section != cell.Section)))
                    {
                        c.PossibleValues.Remove(value);
                    }
                }

                //get cells in column
                cellToCheckGroup = GetCellsInColAndSection(cell);
                //intersected values in column
                possibleValues = GetIntersectedPossibilitiesForColumn(cell);

                //distinct values in column 
                outsideValues = GetRemainingPossibilitiesForSection(cellToCheckGroup);

                //values in section that arent outside the section can be removed
                foreach (int value in possibleValues.Except(outsideValues))
                {
                    foreach (Cell c in Cells.Where(x => (x.Value == 0) && (x.Col == cell.Col) && (x.Section != cell.Section)))
                    {
                        c.PossibleValues.Remove(value);
                    }
                }
            }
        }

        /// <summary>
        /// Removes possibilities from cells in different row or column, but same section
        /// </summary>
        public void CheckForPointingPairBox()
        {
            List<int> possibleValues;
            List<int> outsideValues;

            foreach (Cell cell in Cells.Where(x => (x.Value == 0)))
            {
                //intersected values in row 
                possibleValues = GetIntersectedPossibilitiesForRow(cell);

                //distinct values in row
                outsideValues = GetRemainingPossibilitiesForRowOutsideSection(cell);

                //values outside section in same row can be removed
                foreach (int value in possibleValues.Except(outsideValues))
                {
                    foreach (Cell c in Cells.Where(x => (x.Value == 0) && (x.Row != cell.Row) && (x.Section == cell.Section)))
                    {
                        c.PossibleValues.Remove(value);
                    }
                }

                //intersected values in column
                possibleValues = GetIntersectedPossibilitiesForColumn(cell);

                //distinct values in column 
                outsideValues = GetRemainingPossibilitiesForColumnOutsideSection(cell);

                //values outside section in same column can be removed
                foreach (int value in possibleValues.Except(outsideValues))
                {
                    foreach (Cell c in Cells.Where(x => (x.Value == 0) && (x.Col != cell.Col) && (x.Section == cell.Section)))
                    {
                        c.PossibleValues.Remove(value);
                    }
                }
            }
        }

        /// <summary>
        /// Naked triple is 3 cells in a row or column with 3 possible values between them. Can be used to eliminate other possibilities
        /// </summary>
        public void CheckForNakedTriple()
        {
            //row
            foreach (Cell c1 in Cells.Where(x => x.Value == 0))
            {
                foreach (Cell c2 in Cells.Where(x => (x.Value == 0) && (c1.Row == x.Row) && (x.Col != c1.Col)))
                {
                    foreach (Cell c3 in Cells.Where(x => x.Value == 0 && c1.Row == x.Row && (x.Col != c1.Col) && (x.Col != c2.Col)))
                    {
                        //if they have 3 possible values
                        if (c1.PossibleValues.Union(c2.PossibleValues.Union(c3.PossibleValues)).Count() == 3)
                        {
                            List<Cell> pair = new List<Cell>() { c1, c2, c3 };
                            List<Cell> cellToCheckGroup = Cells.Where(x => x.Value == 0 && c1.Row == x.Row).Except(pair).ToList();

                            foreach (Cell c in cellToCheckGroup)
                            {
                                foreach (int value in c1.PossibleValues)
                                {
                                    c.PossibleValues.Remove(value);
                                }
                            }
                        }
                    }
                }
            }

            //column
            foreach (Cell c1 in Cells.Where(x => x.Value == 0))
            {
                foreach (Cell c2 in Cells.Where(x => (x.Value == 0) && (c1.Col == x.Col) && (x.Row != c1.Row)))
                {
                    foreach (Cell c3 in Cells.Where(x => x.Value == 0 && c1.Col == x.Col && (x.Row != c1.Row) && (x.Row != c2.Row)))
                    {
                        //if they have 3 possible values
                        if (c1.PossibleValues.Union(c2.PossibleValues.Union(c3.PossibleValues)).Count() == 3)
                        {
                            List<Cell> pair = new List<Cell>() { c1, c2, c3 };
                            List<Cell> cellToCheckGroup = Cells.Where(x => x.Value == 0 && c1.Col == x.Col).Except(pair).ToList();

                            foreach (Cell c in cellToCheckGroup)
                            {
                                foreach (int value in c1.PossibleValues)
                                {
                                    c.PossibleValues.Remove(value);
                                }
                            }
                        }
                    }
                }
            }

        }

        private List<Cell> GetCellsInRowAndSection(Cell cell)
        {
            List<Cell> cells;
            cells = Cells.Where(x => (x.Value == 0) && (x.Row == cell.Row) && (x.Section == cell.Section)).ToList();

            return cells;
        }

        private List<Cell> GetCellsInColAndSection(Cell cell)
        {
            List<Cell> cells;
            cells = Cells.Where(x => (x.Value == 0) && (x.Col == cell.Col) && (x.Section == cell.Section)).ToList();

            return cells;
        }

        private List<int> GetIntersectedPossibilitiesForRow(Cell c)
        {
            List<int> values = new List<int>();

            //get list of possible values shared by the blank cells in a row
            foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Row == c.Row) && (x.Section == c.Section)))
            {
                values.AddRange(c.PossibleValues.Intersect(cell.PossibleValues));
            }

            //return possible values for cell not present in other cells of the section
            return values.Distinct().ToList();
        }

        private List<int> GetIntersectedPossibilitiesForColumn(Cell c)
        {
            List<int> values = new List<int>();

            //get list of possible values shared by the blank cells in a row
            foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Col == c.Col) && (x.Section == c.Section)))
            {
                values.AddRange(c.PossibleValues.Intersect(cell.PossibleValues));
            }

            //return possible values for cell not present in other cells of the section
            return values.Distinct().ToList();
        }

        //remaining possibilites in section excluding a single cell
        private List<int> GetRemainingPossibilitiesForSection(Cell c)
        {
            List<int> values = new List<int>();

            //get list of possible values for blank cells in same section, excluding the one we're checking
            foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Section == c.Section) && !(x.Row == c.Row && x.Col == c.Col)))
            {
                values.AddRange(cell.PossibleValues);
            }

            //return possible values for cell not present in other cells of the section
            return c.PossibleValues.Except(values).ToList();
        }

        //remaining possibilities in section excluding list of cells
        private List<int> GetRemainingPossibilitiesForSection(List<Cell> list)
        {
            List<int> values = new List<int>();
            List<Cell> cellsToCheck = Cells.Where(x => (x.Value == 0) && (x.Section == list[0].Section)).Except(list).ToList();

            //get list of possible values for blank cells in same section, excluding the one we're checking
            foreach (Cell cell in cellsToCheck)
            {
                values.AddRange(cell.PossibleValues);
            }

            //return possible values for cell not present in other cells of the section
            return values.Distinct().ToList();
        }

        private List<int> GetRemainingPossibilitiesForRowOutsideSection(Cell c)
        {
            List<int> values = new List<int>();

            //get list of possible values for blank cells in different section, same row
            foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Section != c.Section) && (x.Row == c.Row)))
            {
                values.AddRange(cell.PossibleValues);
            }

            //return possible values for cell not present in other cells of the section
            return values;
        }

        private List<int> GetRemainingPossibilitiesForColumnOutsideSection(Cell c)
        {
            List<int> values = new List<int>();

            //get list of possible values for blank cells in different section, same row
            foreach (Cell cell in Cells.Where(x => (x.Value == 0) && (x.Section != c.Section) && (x.Col == c.Col)))
            {
                values.AddRange(cell.PossibleValues);
            }

            //return possible values for cell not present in other cells of the section
            return values;
        }

        public void Solve()
        {
            Console.WriteLine("\r\nSolving...");

            //get number of blanks before calling solve
            int leftToSolve = Cells.Where(x => x.Value == 0).Count();

            //run possibility elimination checks
            CheckForPointingPairRowCol();

            CheckForPointingPairBox();

            CheckForNakedTriple();

            //check for values we can fill
            CheckForSinglePossibleValue();

            CheckForHiddenSingle();

            //get number still unsolved
            int remaining = Cells.Where(x => x.Value == 0).Count();

            //if we've made progress, solve again
            if (remaining != 0 && remaining != leftToSolve)
            {
                Print();    //print to see progress
                 Solve();
            }
            else if (remaining == 0)
            {
                Console.WriteLine("Solved.");
            }
            else //else no progress, alert user and stop trying
            {
                Console.WriteLine("Solving has stopped making progress");
            }

        }

        /// <summary>
        /// Prints the board.
        /// </summary>
        public void Print()
        {
            string sudoku = "\r\n";

            //loop through squares 
            for(int r = 1; r < 10; r++)
            {
                for (int c = 1; c < 10; c++)
                {
                    //get cell
                    Cell cell = Cells.Single(x => x.Row == r && x.Col == c);

                    //add value to output line
                    sudoku = sudoku + cell.Value;

                    //add new line after the 9th char for a row
                    if (c % 9 == 0)
                    {
                        sudoku = sudoku + "\r\n";
                    }
                }
            }

            Console.Write(sudoku);

        }
        
    }
}
