using System;
using System.Collections.Generic;
using System.Text;

namespace Double_Array_Trie.Double_Array_Trie
{
    public class DoubleArrayTrie_Test
    {
        public void Test()
        {
            List<string> patterns = new List<string>();
            patterns.Add("习近平");
            patterns.Add("习近");
            patterns.Add("handle");
            patterns.Add("the");
            patterns.Add("ha");
            patterns.Add("han");
            patterns.Add("and");
            patterns.Add("pot");
            patterns.Add("pork");
            patterns.Add("port");
            patterns.Add("e");
            patterns.Add("portabc");
            patterns.Add("portacc");
            patterns.Add("portacb");
            patterns.Add("portabe");
            patterns.Add("portaeb");

            DoubleArrayTrie<char>.AlphabetMap alphabetMap = new DoubleArrayTrie<char>.AlphabetMap();
            int code = 0;
            foreach (var pattern in patterns)
            {
                foreach (var element in pattern)
                {
                    if (!alphabetMap.elementCodeMap.ContainsKey(element))
                    {
                        alphabetMap.elementCodeMap.Add(element, code);
                        alphabetMap.reverse_Map.Add(code, element);
                        code++;
                    }
                }
            }
            

            DoubleArrayTrie<char> doubleArrayTrie = new DoubleArrayTrie<char>(16, '\x80', alphabetMap);

            foreach (var pattern in patterns)
            {
                doubleArrayTrie.AddPattern(pattern);
            }
            doubleArrayTrie.Debug();

            foreach (var pattern in patterns)
            {
                var matches = doubleArrayTrie.SearchPattern(pattern);
                Console.Write($"{pattern}:\t");
                if (matches.Count == 0)
                {
                    Console.WriteLine("miss match");
                }
                foreach (var match in matches)
                {
                    Console.WriteLine($"[{match.start}, {match.start + match.length}]: {pattern.Substring(match.start, match.length)}");
                }
                Console.WriteLine("=========================================");
            }

        }
    }
}
