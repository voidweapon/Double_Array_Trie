using System;
using System.Collections.Generic;
using System.Text;

namespace Double_Array_Trie.Trie
{
    class Trie<TElement>
        where TElement : IComparable<TElement>, IEquatable<TElement> //串元素可比较用于排序,可做相等性比较
    {
        private TElement terminator;
        private readonly TrieNode root = new TrieNode();
        public Trie(TElement terminator)
        {
            if (terminator == null)
            {
                throw new ArgumentNullException($"{nameof(terminator)} is null");
            }

            this.terminator = terminator;
        }
        public void AddPattern(IEnumerable<TElement> pattern)
        {
            if (terminator == null)
            {
                throw new ArgumentNullException($"{nameof(pattern)} is null");
            }

            TrieNode s = root;
            TrieNode node = null;
            int state_depth = -1;
            foreach (var element in pattern)
            {
                state_depth++;

                if (!s.children.TryGetValue(element, out node))
                {
                    node = new TrieNode()
                    {
                        depth = state_depth,
                    };
                    s.children.Add(element, node);
                }
                s = node;
            }

            //给每个模式串的结尾添加一个终止符
            //处理当一个模式串是另一个模式串的前缀时, 前缀串的匹配问题
            if (!s.children.ContainsKey(terminator))
            {
                s.children.Add(terminator, new TrieNode());
            }
        }
        /// <summary>
        /// 单模式匹配
        /// </summary>
        /// <param name="key"></param>
        /// <returns>匹配到的模式串</returns>
        public List<Match> Search(IEnumerable<TElement> key)
        {
            TrieNode s = root;
            TrieNode lastMatch = null;
            List<Match> patterns = new List<Match>();
            int element_index = -1;
            foreach (var element in key)
            {
                element_index++;

                if (!s.children.TryGetValue(element, out lastMatch))
                {
                    break;
                }
                if (s.children.ContainsKey(terminator))
                {
                    patterns.Add(new Match() 
                    { 
                        start = element_index - s.depth,
                        length = s.depth + 1,
                    });
                }
                s = lastMatch;
            }
            return patterns;
        }

        internal class TrieNode
        {
            /// <summary>
            /// 节点在树中的深度
            /// </summary>
            public int depth = 0;
            /// <summary>
            /// 子状态
            /// </summary>
            public readonly Dictionary<TElement, TrieNode> children = new Dictionary<TElement, TrieNode>();
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
