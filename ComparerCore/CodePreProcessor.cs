using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ComparerCore
{
    class CodePreProcessor
    {
        public static Stream Process(Stream codeStream)
        {
            CodePreProcessor processor = new CodePreProcessor();
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            using (var reader = new StreamReader(codeStream))
            {
                writer.AutoFlush = true;

                while (true)
                {
                    string str = reader.ReadLine();
                    if (str == null)
                    {
                        break;
                    }
                    str = processor.ProcessLine(str);
                    if (processor.Valid(str))
                    {
                        writer.WriteLine(str);
                    }
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
        bool Valid(string str)
        {
            if(str.Length == 0)
            {
                return false;
            }

            Func<char, bool> OnlyContains = charToCheck =>
            {
                foreach (var c in str)
                {
                    if(c != charToCheck)
                    {
                        return false;
                    }
                }
                return true;
            };

            if (OnlyContains('\t'))
            {
                return false;
            }

            return true;
        }

        bool hasCommend = false;
        string RemoveCommend(string str)
        {
            string tmp = str;

            if (hasCommend && tmp.IndexOf("*/") < 0)
            {
                return "";
            }

            bool inString = false;
            while (true)
            {
                bool[] charInString = new bool[tmp.Length];
                inString = false;
                for (int i = 0; i < tmp.Length; i++)
                {
                    if(tmp[i] == '\"')
                    {
                        inString = !inString;
                    }
                    else
                    {
                        charInString[i] = inString;
                    }
                }

                var commentStartIdx = tmp.IndexOf("/*");
                var commentEndIdx = tmp.IndexOf("*/");

                bool hasCommentHead = commentStartIdx >= 0 && charInString[commentStartIdx] == false;
                bool hasCommentTail = commentEndIdx >= 0 && charInString[commentEndIdx] == false;

                if (hasCommentHead)
                {
                    hasCommend = true;
                }
                if (hasCommentTail)
                {
                    commentEndIdx += 2;
                    hasCommend = false;
                }

                if (hasCommentHead && hasCommentTail)
                {
                    tmp = tmp.Remove(commentStartIdx, commentEndIdx - commentStartIdx);
                }
                else if (hasCommentHead)
                {
                    tmp = tmp.Remove(commentStartIdx);
                }
                else if (hasCommentTail)
                {
                    tmp = tmp.Remove(0, commentEndIdx);
                }
                else break;
            }

            int shortCommentIdx = -1;
            inString = false;
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i] == '\"')
                {
                    inString = !inString;
                }

                shortCommentIdx = tmp.IndexOf("//", i);
                if (inString == false && shortCommentIdx >= 0)
                {
                    break;
                }
            }

            if (shortCommentIdx >= 0)
            {
                tmp = tmp.Remove(shortCommentIdx);
            }

            return tmp;
        }

        string RemoveRedundancy(string str)
        {
            string tmp = str;

            Action<string> ignore = s =>
            {
                while (true)
                {
                    var redundancyIdx = tmp.IndexOf(s + s);
                    if (redundancyIdx >= 0)
                    {
                        tmp = tmp.Remove(redundancyIdx, s.Length);
                    }
                    else break;
                }
            };

            string[] ignoreList = { " ", ";", "\"\"", "\t" };
            foreach (var s in ignoreList)
            {
                ignore(s);
            }

            List<string> split = tmp.Split(';').ToList();
            if(split == null)
            {
                return tmp;
            }

            for (int i = split.Count - 1; i >= 0; i--)
            {
                int sum = 0;
                foreach (var s in ignoreList)
                {
                    sum += split[i].Count(c => s.Contains(c));
                }
                if(sum != split[i].Length)
                {
                    split.RemoveAt(i);
                }
            }

            for (int i = 0; i < split.Count; i++)
            {
                tmp = tmp.Remove(tmp.IndexOf(split[i]), split[i].Length);
            }

            ignore(";");

            if (str != tmp)
            {
                CCore.Log("RemoveRedundancy: {0} -> {1}", str, tmp);
            }

            return tmp;
        }
        string ProcessLine(string str)
        {
            string tmp = str;
            if (CCore.ignoreCommend)
            {
                tmp = RemoveCommend(tmp);
            }
            if (CCore.ignoreRedundancy)
            {
                tmp = RemoveRedundancy(tmp);
            }
            return tmp;
        }
    }
}
