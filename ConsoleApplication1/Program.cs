using SudokuSolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    class Program
    {
        static void Main(string[] args)
        {
            string FilePath;
            bool run = true;
            while (run)
            {
                Board board = new Board();
                if (args.Length != 0)
                {
                    FilePath = args[0];

                    //if file is passed as argument, solve
                    if (File.Exists(FilePath))
                    {
                        //load board and fill values
                        board.Load(FilePath);

                        //output solved puzzle
                        board.Print();

                        run = false;
                    }
                }
                else //if no args, show list of puzzles 
                {
                    //get list of puzzles 
                    Console.WriteLine("Sudokus found in puzzles directory:");
                    List<string> puzzles = Directory.GetFiles("..\\..\\Puzzles\\").ToList();
                    foreach (string puzzle in puzzles)
                    {
                        Console.WriteLine(Path.GetFileName(puzzle));
                    }

                    //query user for puzzle to solve
                    Console.WriteLine("\r\nWhich puzzle would you like to solve?");
                    string FileName = Console.ReadLine();
                    FilePath = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\Puzzles\\" + FileName;

                    //load board and fill values
                    if (board.Load(FilePath))
                    {
                        //attempt to solve any remaining empty cells
                        board.Solve();

                        //output solved puzzle
                        board.Print();
                    }

                    //continue?
                    Console.WriteLine("\r\nWould you like to solve another? [Y to continue/any other key to exit]");
                    string response = Console.ReadLine();
                    if (response.ToUpper() != "Y")
                    {
                        Console.WriteLine("Exiting...");
                        run = false;
                    }
                }
            }
        }
    }
}
