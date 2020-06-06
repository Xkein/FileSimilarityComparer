using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ComparerCore
{
    // Simple Analyze
    class CodeAnalyzer
    {
        public List<string> lines = new List<string>();
        ulong totalCh = 0;
        public CodeAnalyzer(Stream codeStream)
        {
            using (var reader = new StreamReader(CodePreProcessor.Process(codeStream)))
            {
                while (true)
                {
                    string str = reader.ReadLine();
                    if (str == null)
                    {
                        break;
                    }
                    lines.Add(str);
                    totalCh += (ulong)str.Length;
                }

                // use min size
                difDimension = (100, 100);
                AllocDif(difDimension);
            }
        }


        // too slow!
        double GetSimilarity(string str1, string str2)
        {
            if (str1 == str2)
            {
                return 1.0;
            }

            double sim = 0.0;

            var union = str1.Union(str2);

            int[] count1 = new int[union.Count()];
            int[] count2 = new int[union.Count()];

            for (int i = 0; i < union.Count(); i++)
            {
                count1[i] = str1.Count(c => c == union.ElementAt(i));
                count2[i] = str2.Count(c => c == union.ElementAt(i));
            }

            double s = 0;
            double den1 = 0;
            double den2 = 0;
            for (int i = 0; i < union.Count(); i++)
            {
                s += count1[i] * count2[i];
                //den1 += Math.Pow(count1[i], 2);
                //den2 += Math.Pow(count2[i], 2);
                den1 += count1[i] * count1[i];
                den2 += count2[i] * count2[i];
            }

            sim =  Math.Sqrt(s * s / (den1 * den2));

            return sim;
        }

        ValueTuple<int, int> difDimension;
        int[,] dif;
        private void AllocDif(ValueTuple<int, int> tuple)
        {
            dif = new int[tuple.Item1, tuple.Item2];

            for (int a = 0; a < tuple.Item1; a++)
            {
                dif[a, 0] = a;
            }
            for (int a = 0; a < tuple.Item2; a++)
            {
                dif[0, a] = a;
            }
        }

        // get string similarity
        float Levenshtein(string str1, string str2)
        {
            // check equal case first
            if (str1 == str2)
            {
                return 1.0f;
            }

            int len1 = str1.Length;
            int len2 = str2.Length;

            if (difDimension.Item1 < len1 + 1 || difDimension.Item2 < len2 + 1)
            { // reduce alloc times
                difDimension = (Math.Max(len1 + 1, difDimension.Item1), Math.Max(len2 + 1, difDimension.Item2));
                AllocDif(difDimension);
            }

            int temp;
            for (int i = 1; i <= len1; i++)
            {
                for (int j = 1; j <= len2; j++)
                {
                    if (str1[i - 1] == str2[j - 1])
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 1;
                    }

                    dif[i, j] = Math.Min(Math.Min(dif[i - 1, j - 1] + temp, dif[i, j - 1] + 1),
                            dif[i - 1, j] + 1);
                }
            }

            return (float)(1.0 - dif[len1, len2] / (float)Math.Max(len1, len2));
            //return 1.0f - dif[len1, len2] / (float)Math.Max(len1, len2);// some unknown problem when build with Release
            //return (float)(1.0 - dif[len1, len2] / (double)Math.Max(str1.Length, str2.Length));
        }

        public ValueTuple<double, double> GetSimilarityTo(CodeAnalyzer analyzer)
        {
            int lineToCompareCount = analyzer.lines.Count;
            bool[] isSet = new bool[lineToCompareCount];
            int[] highestSimIdxes = new int[lineToCompareCount];
            float[] highestSims = new float[lineToCompareCount];
            Queue<int> queue = new Queue<int>();

            bool reEstimate = false;

            Action<int> EstimateSimilarity = idx =>
            {
                var str1 = lines[idx];
                CCore.Log("EstimateSimilarity: {0}", str1);

                float highestSim = 0.0f;
                int highestIdx = -1;
                // get most similar line
                for (int j = 0; j < analyzer.lines.Count; j++)
                {
                    // skip if already find the same line
                    if (highestSims[j] == 1.0f)
                    {
                        continue;
                    }

                    var str2 = analyzer.lines[j];

                    (int min, int max) pair = (Math.Min(str1.Length, str2.Length), Math.Max(str1.Length, str2.Length));
                    if (reEstimate)
                    {
                        // skip estimate if highestSim absolutely < highestSims[j]
                        if (pair.min / (float)pair.max < highestSims[j])
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // skip estimate if longer string length > 2 * shorter && already has >50% sim string
                        if(pair.max >> 1 >= pair.min && highestSims[j] >= 0.5f)
                        {
                            continue;
                        }
                    }

                    float strSim = Levenshtein(str1, str2);
                    if (highestSim < strSim && strSim > highestSims[j])
                    {
                        highestSim = strSim;
                        highestIdx = j;
                    }

                    // find same line, quit loop
                    if (highestSim == 1.0f)
                    {
                        break;
                    }
                }

                if (highestIdx >= 0)
                {
                    if (isSet[highestIdx])
                    {
                        int lastIdx = highestSimIdxes[highestIdx];
                        if (queue.Contains(lastIdx) == false)
                        { // enqueue lines to re-estimate 
                            queue.Enqueue(lastIdx);
                        }
                    }
                    else
                    {
                        isSet[highestIdx] = true;
                    }

                    highestSimIdxes[highestIdx] = idx;
                    highestSims[highestIdx] = highestSim;

                    CCore.Log("EstimateSimilarity: higest similar({0}) line: {1}", highestSim, analyzer.lines[highestIdx]);
                    CCore.Log();
                }
                else
                {
                    CCore.Log("EstimateSimilarity: higest similar line not found!");
                    CCore.Log();
                }
            };

            for (int i = 0; i < lines.Count; i++)
            {
                EstimateSimilarity(i);
            }

            CCore.Log("EstimateSimilarity: re-estimate lines");
            reEstimate = true;

            while (queue.Count > 0)
            {
                EstimateSimilarity(queue.Dequeue());
            }

            ValueTuple<double, double> tuple = (0.0, 0.0);
            for (int i = 0; i < lineToCompareCount; i++)
            {
                if (isSet[i])
                {
                    tuple.Item1 += highestSims[i] * lines[highestSimIdxes[i]].Length / totalCh;
                    tuple.Item2 += highestSims[i] * analyzer.lines[i].Length / analyzer.totalCh;
                }
            }

            return tuple;
        }
    }
}
