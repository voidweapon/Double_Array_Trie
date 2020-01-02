using System;
using Double_Array_Trie.AhoCorasick;
namespace Double_Array_Trie
{
    class Program
    {
        static void Main(string[] args)
        {
            DAT_Test();

            Console.ReadLine();
        }

        private static void AC_Test()
        {
            var AhoCorasickTest = new AhoCorasick_Test();
            AhoCorasickTest.Test();
        }

        private static void DAT_Test()
        {
            var dAT_tester = new Double_Array_Trie.DoubleArrayTrie_Test();
            dAT_tester.Test();
        }
    }
}
