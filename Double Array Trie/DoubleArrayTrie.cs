using System;
using System.Collections.Generic;
using System.Text;

namespace Double_Array_Trie.Double_Array_Trie
{
    public class DoubleArrayTrie<TElement>
        where TElement : IComparable<TElement>, IEquatable<TElement> //串元素可比较用于排序,可做相等性比较
    {
        private TElement terminator;
        private List<TrieNode> nodePool = null;
        private AlphabetMap alphabetMap = null;
        private List<TElement> tail = null;
        public DoubleArrayTrie(int initalCapaicty, TElement terminator, AlphabetMap alphabetMap)
        {
            this.terminator = terminator;
            this.alphabetMap = alphabetMap;

            tail = new List<TElement>(initalCapaicty);
            //tail[0]填充终止符,使得后缀数组从1开始
            tail.Add(terminator);

            nodePool = new List<TrieNode>(0);
            Resize(initalCapaicty);
            nodePool[0].@base = 1;
            nodePool[0].depth = 0;
        }

        /// <summary>
        /// 添加模式串
        /// </summary>
        /// <param name="pattern"></param>
        public void AddPattern(IEnumerable<TElement> pattern)
        {
            int s = 0;
            int t = int.MinValue;
            int parent = int.MinValue;
            Span<TElement> suffix = new Span<TElement>(new List<TElement>(parent).ToArray());
            int pattern_index = -1;
            foreach (var element in pattern)
            {
                pattern_index++;
                int c = GetElementCode(element);
                t = GetBase(s) + c;
                parent = Check(t);

                if(parent == int.MinValue)
                {
                    ///槽位t为空，储存新状态到槽位t
                    InsertBranch(s, t, pattern_index);
                    //将后缀存入后缀数组, 设置t的后缀索引, 模式串添加结束
                    int tailIndex = AddNewSuffix(suffix.Slice(pattern_index + 1));
                    SetSuffixIndex(t, tailIndex);
                    return;
                }
                else if (parent != s)
                {
                    ///槽位t已经储存有状态, 且不来自状态s，发生冲突
                    int s_newBase = -1;
                    if(!FindFreeSlot(out s_newBase))
                    {
                        //空间不足，分配新的空间
                        s_newBase = this.nodePool.Capacity + 1;
                        Resize(this.nodePool.Capacity * 2);                
                    }
                    Relocate(s, s_newBase);
                    t = GetBase(s) + c;
                    InsertBranch(s, t, pattern_index);
                }
                else
                {
                    //槽位t已经储存有状态，且来自状态s. 不需要插入新节点
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
            newBase = nodePool.FindIndex((node)=> { return node.parent == int.MinValue; });
            return newBase > 0;
        }

        /// <summary>
        /// 调整节点池大小
        /// </summary>
        private void Resize(int newCapacity)
        {
            int oldCapacity = nodePool.Capacity;
            nodePool.Capacity = newCapacity;

            for (int i = oldCapacity; i < newCapacity; i++)
            {
                nodePool.Add(new TrieNode());
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

        private void InsertBranch(int s, int t, int pattern_index)
        {
            this.nodePool[t].@base = this.nodePool[s].@base;
            this.nodePool[t].parent = s;

            this.nodePool[t].depth = pattern_index;
        }

        private int GetElementCode(TElement element)
        {
            int code = -1;
            this.alphabetMap.elementCodeMap.TryGetValue(element, out code);
            return code;
        }

        private int GetBase(int s)
        {
            return this.nodePool[s].@base;
        }

        private int GetSuffixIndex(int s)
        {
            return this.nodePool[s].tail;
        }
        private void SetSuffixIndex(int s, int tailIndex)
        {
            this.nodePool[s].tail = tailIndex;
        }


        private int Check(int s)
        {
            return this.nodePool[s].parent;
        }

        #region 后缀相关
        /// <summary>
        /// 将<paramref name="pos"/>开始的后缀，从头缩短<paramref name="reduce"/>个
        /// </summary>
        /// <param name="pos">旧后缀的开始位置</param>
        /// <param name="reduce">新后缀</param>
        private void ReduceSuffix(int pos, int reduce) 
        {

        }

        /// <summary>
        /// 添加新的后缀到后缀数组, 并在结尾添加一个终止符
        /// </summary>
        /// <param name="suffix">需要添加的后缀</param>
        /// <returns>后缀的开始位置</returns>
        private int AddNewSuffix(Span<TElement> suffix)
        {
            int pos = this.tail.Count;
            foreach (var item in suffix)
            {
                this.tail.Add(item);
            }
            //添加终止符
            this.tail.Add(terminator);
            return pos;
        }
        #endregion

        internal class TrieNode
        {
            /// <summary>
            /// 子节点基地址
            /// </summary>
            public int @base = int.MinValue;
            /// <summary>
            /// 父节点索引
            /// </summary>
            public int parent = int.MinValue;
            /// <summary>
            /// 尾缀的索引
            /// </summary>
            public int tail = int.MinValue;
            /// <summary>
            /// 节点在树中的深度
            /// </summary>
            public int depth = int.MinValue;
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
