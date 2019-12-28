using System;
using System.Collections.Generic;
using System.Text;

namespace Double_Array_Trie.Double_Array_Trie
{
    public class DoubleArrayTrie<TElement>
        where TElement : IComparable<TElement>, IEquatable<TElement> //串元素可比较用于排序,可做相等性比较
    {
        private List<TrieNode> nodes = null;

        public DoubleArrayTrie(int initalCapaicty)
        {
            nodes = new List<TrieNode>(initalCapaicty);
        }

        /// <summary>
        /// 添加模式串
        /// </summary>
        /// <param name="pattern"></param>
        public void AddPattern(IEnumerable<TElement> pattern)
        {

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
        private int FindFreeSlot()
        {
            return -1;
        }

        /// <summary>
        /// 调整节点池大小
        /// </summary>
        private void Resize()
        {

        }

        /// <summary>
        /// 调整节点位置,已解决冲突
        /// </summary>
        /// <param name="s"></param>
        /// <param name="newbase"></param>
        private void Relocate(int s, int newbase)
        {

        }

        internal struct TrieNode
        {
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
    }
}
