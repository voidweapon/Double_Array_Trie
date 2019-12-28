using System;
using System.Collections.Generic;
using System.Text;

namespace Double_Array_Trie.AhoCorasick
{
    public class AhoCorasick_Test
    {
        public void Test()
        {
            AhoCorasick<char> ahoCorasick = new AhoCorasick<char>('\x80');
            ahoCorasick.AddPattern("习近平");
            ahoCorasick.AddPattern("习近");
            ahoCorasick.AddPattern("handle");
            ahoCorasick.AddPattern("the");
            ahoCorasick.AddPattern("ha");
            ahoCorasick.AddPattern("han");
            ahoCorasick.AddPattern("and");
            ahoCorasick.AddPattern("pork");
            ahoCorasick.AddPattern("port");
            ahoCorasick.AddPattern("pot");
            ahoCorasick.AddPattern("e");

            ahoCorasick.BuildFailPoint();

            string str = "你好 习近平. The pot had a handle";
            var matches = ahoCorasick.Search(str.ToLower());
            Console.WriteLine(str);
            foreach (var match in matches)
            {
                Console.WriteLine($"[{match.start}, {match.start + match.length -1}]: {str.Substring(match.start, match.length)}");
            }
        }
    }
}
