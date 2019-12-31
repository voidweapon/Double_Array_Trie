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
        /// 查找模式串是否存在
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<Match> SearchPattern(IEnumerable<TElement> pattern)
        {
            List<Match> matches = new List<Match>();
            Span<TElement> suffix = new Span<TElement>(new List<TElement>(pattern).ToArray());
            int c = -1;
            int s = 0;
            int t = 0;
            int check = 0;
            int str_index = -1;
            foreach (var element in pattern)
            {
                str_index++;
                c = GetElementCode(element);
                if (c == -1)
                {
                    //element没有在字符表中
                    break;
                }

                t = GetBase(s) + c;
                check = Check(t);
                if (check == int.MinValue)
                {
                    //状态t未定义
                    break;
                }

                if (IsCompeletePattern(t))
                {
                    matches.Add(new Match
                    {
                        start = str_index - GetNodeDepth(t),
                        length = GetNodeDepth(t) + 1,
                    });
                }
                if (GetSuffixIndex(t) > 0)
                {
                    //到根节点了, 对比后缀
                    var state_suffix = GetSuffix(GetSuffixIndex(t));
                    if (CompareSuffix(state_suffix, suffix.Slice(str_index + 1)))
                    {
                        matches.Add(new Match
                        {
                            start = str_index - GetNodeDepth(t),
                            length = GetNodeDepth(t) + 1,
                        });
                    }
                    break;
                }
                s = t;
            }
            return matches;
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
            Span<TElement> pattern_suffix = new Span<TElement>(new List<TElement>(parent).ToArray());
            int pattern_index = -1;
            int prefix_lenght = 0;
            int store_suffix_state = -1;
            foreach (var element in pattern)
            {
                pattern_index++;
                int c = GetElementCode(element);
                t = GetBase(s) + c;
                parent = Check(t);

                t = InsertBranch(s, c, t, pattern_index);

                if (parent == int.MinValue)
                {
                    break;
                }

                //存在公有前缀
                prefix_lenght++;
                if(GetSuffixIndex(s) > 0)
                {
                    store_suffix_state = s;
                }
                s = t;
            }

            
            if (prefix_lenght == 0)
            {
                //后缀处理, 情况1和情况2: 父节点没有储存后缀, 不会产生后缀冲突
                //将后缀存入后缀数组, 设置t的后缀索引, 模式串添加结束
                int tailIndex = AddNewSuffix(pattern_suffix.Slice(pattern_index + 1));
                SetSuffixIndex(t, tailIndex);
            }
            else if(prefix_lenght == pattern_suffix.Length)
            {
                //新的模式串是已存在模式串的完整前缀
                SetStateIsCompeletePattern(s);
            }
            else
            {
                //后缀处理, 情况3,4:提取公有前缀,把前缀插入Trie, 然后将各自的后缀的第一个一个element插入Trie, 剩余的后缀存入后缀数组,记录后缀索引
                Span<TElement> store_suffix = GetSuffix(GetSuffixIndex(store_suffix_state)).Slice(prefix_lenght - 1);
                pattern_suffix = pattern_suffix.Slice(prefix_lenght -1);

                int c = GetElementCode(store_suffix[0]);
                t = GetBase(s) + c;
                t = InsertBranch(s, c, t, pattern_index + 1);
                int tailIndex = GetSuffixIndex(store_suffix_state);
                ReduceSuffix(GetSuffixIndex(store_suffix_state), prefix_lenght);
                SetSuffixIndex(t, tailIndex);
                ClearSuffixIndex(store_suffix_state);

                c = GetElementCode(pattern_suffix[0]);
                t = GetBase(s) + c;
                t = InsertBranch(s, c, t, pattern_index + 1);
                tailIndex = AddNewSuffix(pattern_suffix.Slice(1));
                SetSuffixIndex(t, tailIndex);
            }
        }

        /// <summary>
        /// 寻找空闲槽位
        /// </summary>
        /// <returns></returns>
        private bool FindFreeSlot(int s, out int newBase)
        {
            int index = 0;
            newBase = int.MinValue;
            while (index < this.nodePool.Count)
            {
                bool find = true;
                foreach (var alphaCode in this.nodePool[s].children)
                {
                    int t = alphaCode + alphaCode;
                    if(t >= this.nodePool.Count     //子节点的新位置超出 当前池子容量
                        ||  Check(t) > 0)           //这个位置已经被使用
                    {
                        find = false;
                        break;
                    }
                }
                if (find)
                {
                    newBase = index;
                    return true;
                }
                index++;
            }

            return false;
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
        /// 调整节点位置,解决冲突
        /// </summary>
        /// <param name="s"></param>
        /// <param name="newbase"></param>
        private void Relocate(int s, int newbase)
        {
            foreach (var alphaCode in this.nodePool[s].children)
            {
                int t = GetBase(s) + alphaCode;
                TrieNode temp = this.nodePool[t];
                this.nodePool[t] = this.nodePool[newbase + alphaCode];
                this.nodePool[newbase + alphaCode] = temp;
            }
            this.nodePool[s].@base = newbase;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">父状态</param>
        /// <param name="c">转移</param>
        /// <param name="t">子状态</param>
        /// <param name="pattern_index"></param>
        private int InsertBranch(int s, int c, int t, int pattern_index)
        {
            int parent = Check(t);

            if (parent == int.MinValue)
            {
                ///槽位t为空，储存新状态到槽位t
                SetupBranch(s, t, pattern_index);
                return t;
            }
            if (parent != s)
            {
                ///槽位t已经储存有状态, 且不来自状态s，发生冲突
                ///冲突解决: 选择s 和 t的parent中 子节点数量较少的一个, 然后寻找一个新的基地址使得 这个节点的所有直接子节点都可以移动到新槽位
                int newBase = -1;
                int need_relocate_state = GetChildrenCount(parent) <= (GetChildrenCount(s) + 1) ? s : parent;
                if (!FindFreeSlot(need_relocate_state, out newBase))
                {
                    //空间不足，分配新的空间
                    newBase = this.nodePool.Capacity + 1;
                    Resize(this.nodePool.Capacity * 2);
                }
                Relocate(need_relocate_state, newBase);

                //插入新节点t
                t = GetBase(s) + c;
                SetupBranch(s, t, pattern_index);
                return t;
            }
            /*
            else
            {
                //槽位t已经储存有状态，且来自状态s. 不需要插入新节点
            }
            */

            return t;
        }
        private void SetupBranch(int s, int t, int pattern_index)
        {
            this.nodePool[t].@base = this.nodePool[s].@base;
            this.nodePool[t].parent = s;

            this.nodePool[t].depth = pattern_index;
        }
        /// <summary>
        /// 获取<paramref name="element"/>的编码值，如果<paramref name="element"/>不在码表中则返回-1
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private int GetElementCode(TElement element)
        {
            int code = -1;
            if(this.alphabetMap.elementCodeMap.TryGetValue(element, out code))
            {
                return code;
            }
            return -1;
        }

        private int GetBase(int s)
        {
            return this.nodePool[s].@base;
        }

        private int Check(int s)
        {
            return this.nodePool[s].parent;
        }

        private int GetChildrenCount(int s)
        {
            return this.nodePool[s].children.Count;
;       }

        private bool SetStateIsCompeletePattern(int s)
        {
            return this.nodePool[s].isCompeletePattern = true;
        }

        private bool IsCompeletePattern(int s)
        {
            return this.nodePool[s].isCompeletePattern;
        }


        private int GetSuffixIndex(int s)
        {
            return this.nodePool[s].tail;
        }
        private void SetSuffixIndex(int s, int tailIndex)
        {
            this.nodePool[s].tail = tailIndex;
        }
        private void ClearSuffixIndex(int s)
        {
            SetSuffixIndex(s, int.MinValue);
        }

        private int GetNodeDepth(int s)
        {
           return this.nodePool[s].depth;
        }

        #region 后缀相关
        /// <summary>
        /// 将<paramref name="pos"/>开始的后缀，从头缩短<paramref name="reduce"/>个
        /// </summary>
        /// <param name="pos">旧后缀的开始位置</param>
        /// <param name="reduce">缩短的个数</param>
        private void ReduceSuffix(int pos, int reduce) 
        {
            for (int i = 0; i <= 0; i++)
            {
                this.tail[pos + reduce + i] = this.tail[pos + i];
            }
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

        private Span<TElement> GetSuffix(int start)
        {
            int end = this.tail.FindIndex(start, (element) => { return element.Equals(terminator); });
            return new Span<TElement>(this.tail.ToArray(), start, end);
        }

        private bool CompareSuffix(Span<TElement> a, Span<TElement> b)
        {
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }

            return true;
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

            public bool isCompeletePattern = false;

            /// <summary>
            /// <see cref="TElement"/> code
            /// </summary>
            public List<int> children = new List<int>();
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
