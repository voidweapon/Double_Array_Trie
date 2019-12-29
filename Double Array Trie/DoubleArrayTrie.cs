using System;
using System.Collections.Generic;
using System.Text;

namespace Double_Array_Trie.Double_Array_Trie
{
    public class DoubleArrayTrie<TElement>
        where TElement : IComparable<TElement>, IEquatable<TElement> //串元素可比较用于排序,可做相等性比较
    {
        private List<TrieNode> @base = null;
        private List<int> check = null;
        private AlphabetMap alphabetMap = null;
        public DoubleArrayTrie(int initalCapaicty, AlphabetMap alphabetMap)
        {
            this.alphabetMap = alphabetMap;
            @base = new List<TrieNode>(0);
            check = new List<int>(0);
            Resize(initalCapaicty);
        }

        /// <summary>
        /// 添加模式串
        /// </summary>
        /// <param name="pattern"></param>
        public void AddPattern(IEnumerable<TElement> pattern)
        {
            int s = 0;
            int t = -1;
            int parent = -1;
            foreach (var element in pattern)
            {
                int c = GetElementCode(element);
                t = GetBase(s) + c;
                parent = Check(t);

                if(parent == s)
                {
                    //槽位t已经储存有状态，且来自状态s
                    continue;
                }

                if(parent == -1)
                {
                    ///槽位t为空，储存新状态到槽位t
                }
                else
                {
                    ///槽位t已经储存有状态, 且不来自状态s，发生冲突
                    int s_newBase = -1;
                    if(!FindFreeSlot(out s_newBase))
                    {
                        //空间不足，分配新的空间
                        s_newBase = this.@base.Capacity + 1;
                        Resize(this.@base.Capacity * 2);                
                    }
                    
                }

            }
        }

        /// <summary>
        /// 单模式匹配
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<Match> Search(IEnumerable<TElement> key)
        {
            return null;
        }

        /// <summary>
        /// 寻找空闲槽位
        /// </summary>
        /// <returns></returns>
        private bool FindFreeSlot(out int newBase)
        {
            newBase = check.IndexOf(-1);
            return newBase > -1;
        }

        /// <summary>
        /// 调整节点池大小
        /// </summary>
        private void Resize(int newCapacity)
        {
            int oldCapacity = @base.Capacity;
            @base.Capacity = newCapacity;
            check.Capacity = newCapacity;

            for (int i = oldCapacity; i < newCapacity; i++)
            {
                @base.Add(new TrieNode());
                check.Add(-1);
            }
        }

        /// <summary>
        /// 调整节点位置,已解决冲突
        /// </summary>
        /// <param name="s"></param>
        /// <param name="newbase"></param>
        private void Relocate(int s, int newbase)
        {

        }

        private int GetElementCode(TElement element)
        {
            int code = -1;
            this.alphabetMap.elementCodeMap.TryGetValue(element, out code);
            return code;
        }

        private int GetBase(int s)
        {
            return this.@base[s].@base;
        }

        private int Check(int s)
        {
            return this.check[s];
        }



        internal struct TrieNode
        {
            public int @base;
            /// <summary>
            /// 节点在树中的深度
            /// </summary>
            public int depth;
        }

        public struct Match
        {
            /// <summary>
            /// 匹配在搜索串中的开始位置
            /// </summary>
            public int start;
            /// <summary>
            /// 匹配到的模式串的长度
            /// </summary>
            public int length;
        }

        public class AlphabetMap
        {
            public Dictionary<TElement, int> elementCodeMap = new Dictionary<TElement, int>();
        }
    }
}
