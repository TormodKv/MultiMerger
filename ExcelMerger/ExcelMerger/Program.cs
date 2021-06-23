using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExcelMerger
{
    public enum Join { 
        outer,
        left,
        right,
        inner
    }

    class Program
    {
        public static string DELIMITER = ";";

        static void Main(string[] args)
        {
            string path1 = "";
            while (!File.Exists(path1)) { 
                Console.WriteLine("First full filepath:");
                path1 = Console.ReadLine();
                path1 = path1.Replace("\"","");
            }


            string path2 = "";
            while ((Path.GetExtension(path1) != Path.GetExtension(path2)) || !File.Exists(path2)) {
                Console.WriteLine($"\nSecond {Path.GetExtension(path1)}-file, full path:");
                path2 = Console.ReadLine();
                path2 = path2.Replace("\"", "");
            }

            //Inner is default
            Join joinType = Join.inner;

            while(true)
            {
                bool joinAssigned = true;
                Console.WriteLine("\nChoose join type:");
                Console.WriteLine("inner (default), outer, left, right");
                switch (Console.ReadLine()) {
                    case "inner": joinType = Join.inner; break;
                    case "left": joinType = Join.left; break;
                    case "outer": joinType = Join.outer; break;
                    case "right": joinType = Join.right; break;
                    case "": joinType = Join.inner; break;
                    default: joinAssigned = false; break;
                }

                if (joinAssigned)
                    break;
            }

            switch (Path.GetExtension(path1).ToUpper()) {
                case ".CSV": List<string> s = mergeCSV(path1, path2, joinType);
                    foreach (string line in s) {
                        Console.WriteLine(line);
                    }
                    break;
                default: Console.WriteLine("Unsupported file format"); break;
            }

        }

        private static List<string> mergeCSV(string path1, string path2, Join joinType)
        {
            List<string> headers1 = new List<string>();
            List<string> headers2 = new List<string>();
            List<string> commonHeaders = new List<string>();

            string[] lines1 = File.ReadAllLines(path1);
            string[] lines2 = File.ReadAllLines(path2);

            foreach (string header in lines1[0].ToUpper().Split(DELIMITER)) {
                headers1.Add(header);
            }

            foreach (string header in lines2[0].ToUpper().Split(DELIMITER))
            {
                headers2.Add(header);
            }

            foreach (string header in headers1) {
                if (headers2.Contains(header)) {
                    commonHeaders.Add(header);
                }
            }

            if (!commonHeaders.Any()) {
                Console.WriteLine("Error1: No common headers");
                return null;
            }

            string commonHeader;

            while (true) {
                Console.WriteLine($"\nChoose common header:");
                int i = 1;
                foreach (string header in commonHeaders)
                {
                    Console.WriteLine(i + ") " + header);
                    i++;
                }
                commonHeader = Console.ReadLine().ToUpper();

                try
                {
                    //input is an int
                    i = Int32.Parse(commonHeader);
                    commonHeader = commonHeaders[i-1];
                    break;
                }
                catch { 
                    //input is not an int
                    if (commonHeaders.Contains(commonHeader))
                    {
                        break;
                    }
                }
            }

            int commonHeaderIndex1 = headers1.IndexOf(commonHeader);
            int commonHeaderIndex2 = headers2.IndexOf(commonHeader);

            List<string> sortedLines1 = new List<string>();
            List<string> sortedLines2 = new List<string>();

            foreach (string line in lines1) {
                string newLine = line.Split(DELIMITER)[commonHeaderIndex1] + DELIMITER + 
                    line.Replace(line.Split(DELIMITER)[commonHeaderIndex1], "");

                newLine = newLine.Replace(DELIMITER + DELIMITER, DELIMITER);
                sortedLines1.Add(newLine);
            }

            foreach (string line in lines2)
            {
                string newLine = line.Split(DELIMITER)[commonHeaderIndex2] + DELIMITER + 
                    line.Replace(line.Split(DELIMITER)[commonHeaderIndex2], "");

                newLine = newLine.Replace(DELIMITER + DELIMITER, DELIMITER);
                sortedLines2.Add(newLine);
            }

            List<string> finalCSV = new List<string>();

            //Add header
            finalCSV.Add(sortedLines1[0].ToUpper() + sortedLines2[0].Replace(sortedLines2[0].Split(DELIMITER)[0], "").Replace(DELIMITER + DELIMITER, DELIMITER));

            sortedLines1.RemoveAt(0);
            sortedLines2.RemoveAt(0);
            sortedLines1.Sort();
            sortedLines2.Sort();

            switch (joinType) {
                 case Join.inner:
                     int currntPos = 0;
                     foreach (string line in sortedLines1) {
                         while (true) {

                            if (currntPos >= sortedLines2.Count())
                                break;

                             string line2 = sortedLines2[currntPos];
                             if (line2.Length <= 0)
                                 break;

                             string line2IDValue = line2.Split(DELIMITER)[0];
                 
                             int compareValue = line.Split(DELIMITER)[0].CompareTo(line2IDValue);
                 
                             if (compareValue < 0)
                                 break;
                 
                             if (compareValue == 0) {
                                 finalCSV.Add((line + DELIMITER + line2.Replace(line2IDValue + DELIMITER, "")).Replace(DELIMITER + DELIMITER,DELIMITER));
                                 break;
                             }
                 
                             if (compareValue > 0)
                                 currntPos++;
                         }
                     }
                    return finalCSV;

            }
            Console.WriteLine("Error2: Faulty join");
            return null;
        }
    }
}
