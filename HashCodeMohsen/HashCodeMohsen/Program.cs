using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace HashCodeMohsen
{
    class Program
    {
        static void Main(string[] args)

        {
            PPlate pPlate = loadData("a_example.in");

            List<PSlice> slices = pPlate.PerformSlice();

            if (pPlate.IsValidSlicing(slices) == false)
                Console.WriteLine("ERROR: invalid slicing");

            Console.WriteLine("Max theretical score: {0}", pPlate.GetSize());
            int solutionScore = slices.Sum(item => item.GetSize());
            Console.WriteLine("Solution score: {0}", solutionScore);

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("a_example" + ".out"))
            {
                sw.WriteLine(slices.Count);
                foreach (PSlice slice in slices)
                {
                    sw.Write(slice.RowMin);
                    sw.Write(' ');
                    sw.Write(slice.ColumnMin);
                    sw.Write(' ');
                    sw.Write(slice.RowMax);
                    sw.Write(' ');
                    sw.Write(slice.ColumnMax);

                    sw.WriteLine();
                       
                }
            }
        }

        private static PPlate loadData(string fileName)
        {
            //
            using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName))
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(' ');
                int rows = int.Parse(parts[0]);
                int columns = int.Parse(parts[1]);
                int minIng = int.Parse(parts[2]);
                int maxIng = int.Parse(parts[3]);
                int[,] plate = new int[rows, columns];
                for (int r = 0; r < rows; r++)
                {
                    line = sr.ReadLine();
                    for (int c = 0; c < columns; c++)
                    {
                        if (line[c] == 'T')
                            plate[r, c] = 1;
                        else if (line[c] == 'M')
                            plate[r, c] = 2;
                        else throw new Exception("Invalid data in row: " + line);
                    }
                }

                return new PPlate(rows, columns, plate, minIng, maxIng);
            }
        }
    }
}
