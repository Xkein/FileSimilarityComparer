using System;
using System.Collections.Generic;
using System.IO;

namespace ComparerCore
{
    public class CCore
    {
        const string suspectStr = "-suspect";
        static bool Parse(string arg)
        {
            var lower = arg.ToLower();
            if(lower.Contains(suspectStr))
            {
                CCore.Log("Parse: {0}", arg);
                suspectedSim = double.Parse(lower.Substring(lower.IndexOf(suspectStr) + suspectStr.Length));
                return true;
            }
            switch (lower)
            {
                case "-ignoreredundancy":
                    CCore.Log("Parse: {0}", arg);
                    ignoreRedundancy = true;
                    return true;
                case "-ignorecommend":
                    CCore.Log("Parse: {0}", arg);
                    ignoreCommend = true;
                    return true;
                default:
                    return false;
            }
        }
        static public bool ignoreCommend = false;
        static public bool ignoreRedundancy = false;
        static public double suspectedSim = 80.0;
        /*
        static void Main(string[] args)
        {

#if DEBUG
            Console.ReadKey();
#endif
            Console.WriteLine();

            List<string> fileName = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (Parse(args[i]))
                {
                    continue;
                }
                else
                {
                    fileName.Add(args[i]);
                }
            }

            try
            {
                for (int i = 0; i < fileName.Count - 1; i++)
                {
                    using (FileStream file1 = new FileStream(fileName[i], FileMode.Open))
                    {
                        var analyze1 = new CodeAnalyzer(file1);
                        for (int j = i + 1; j < fileName.Count; j++)
                        {
                            using (FileStream file2 = new FileStream(fileName[j], FileMode.Open))
                            {
                                var analyze2 = new CodeAnalyzer(file2);
                                Console.WriteLine("Compare {0} to {1}", fileName[i], fileName[j]);
                                var sim = analyze1.GetSimilarityTo(analyze2);
                                Console.WriteLine("Similarity: {0}", sim);
                                if (sim >= suspectedSim)
                                {
                                    Warning();
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("[ERROR] {0} file not found!", e.FileName);
                throw;
            }
#if DEBUG
            Console.ReadLine();
#endif
        }
        */
        static public ValueTuple<double, double> CompareFile(string path1, string path2)
        {
            ValueTuple<double, double> tuple = (double.NaN, double.NaN);

            try
            {
                using (FileStream file1 = new FileStream(path1, FileMode.Open, FileAccess.Read))
                {
                    var analyze1 = new CodeAnalyzer(file1);
                    using (FileStream file2 = new FileStream(path2, FileMode.Open, FileAccess.Read))
                    {
                        var analyze2 = new CodeAnalyzer(file2);
                        CCore.Log("Compare {0} to {1}", path1, path2);
                        tuple = analyze1.GetSimilarityTo(analyze2);
                        CCore.Log("Similarity: {0}", tuple);
                        if (tuple.Item1 >= suspectedSim || tuple.Item2 >= suspectedSim)
                        {
                            Warning();
                        }
                        CCore.Log();
                    }
                }

            }
            catch (FileNotFoundException e)
            {
                CCore.Log("[ERROR] {0} file not found!", e.FileName);
                throw;
            }

            return tuple;
        }
        static void Warning()
        {
            CCore.Log("[WARNING] too high similarity!");
            //MessageBox.Show("[WARNING] too high similarity!", "Compare Result");
        }

        static public bool showLog = false;
        static public void Log(string format, params object[] arg)
        {
            if (showLog)
            {
                Console.WriteLine(format, arg);
            }
        }
        static public void Log()
        {
            if (showLog)
            {
                Console.WriteLine();
            }
        }

    }
}
