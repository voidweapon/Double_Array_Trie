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

        public void Debug()
        {
            StringBuilder alphabetMapInfo = new StringBuilder();
            StringBuilder nodeInfo = new StringBuilder();
            StringBuilder tialInfo = new StringBuilder();
            alphabetMapInfo.Append("Code: ");

            foreach (var key in alphabetMap.elementCodeMap.Keys)
            {
                alphabetMapInfo.Append($"{key} \t");
            }
            alphabetMapInfo.AppendLine();
            alphabetMapInfo.Append("Key:  ");
            foreach (var key in alphabetMap.elementCodeMap.Keys)
            {
                alphabetMapInfo.Append($"{alphabetMap.elementCodeMap[key]} \t");
            }


            nodeInfo.AppendLine("===============================================================");
            nodeInfo.AppendLine("Node");
            nodeInfo.AppendLine("Index\t\t Code\t\t Base\t\t\t Tail\t\t Check\t\t\t Full");
            int i = 0;
            foreach (var item in this.nodePool)
            {
                string tail = "";
                if(item.tail != int.MinValue)
                {
                    var ary = GetSuffix(item.tail).ToArray();
                    foreach (var a_e in ary)
                    {
                        tail += $"{a_e}";
                    }
                }
                nodeInfo.Append($"{i++}\t\t ");
                nodeInfo.Append($"{item.code}\t\t ");
                if (item.@base == int.MinValue)
                {
                    nodeInfo.Append($"{item.@base}\t\t ");
                }
                else
                {
                    nodeInfo.Append($"{item.@base}\t\t\t ");
                }
                nodeInfo.Append($"{tail}\t\t ");
                if (item.parent == int.MinValue)
                {
                    nodeInfo.Append($"{item.parent}\t\t ");
                }
                else
                {
                    nodeInfo.Append($"{item.parent}\t\t\t ");
                }
                nodeInfo.Append($"{item.isCompeletePattern}");
                nodeInfo.AppendLine();
            }

            tialInfo.AppendLine("===============================================================");
            tialInfo.AppendLine("Tail");
            tialInfo.AppendLine("Index\t\t Code");
            i = 0;
            foreach (var item in this.tail)
            {
                tialInfo.AppendLine($"{i++}\t\t {item.ToString()}");
            }

            Console.WriteLine(alphabetMapInfo);
            Console.WriteLine(nodeInfo);
            Console.WriteLine(tialInfo);
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

                if (IsCompeletePattern(t) && str_index == suffix.Length - 1)
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
                            length = GetNodeDepth(t) + 1 + state_suffix.Length,
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
        public void AddPattern(Span<TElement> pattern)
        {
            int s = 0;
            int t = int.MinValue;
            int c = 0;
            int check = int.MinValue;
            int pattern_index = 0;
            for (pattern_index = 0; pattern_index < pattern.Length; pattern_index++)
            {
                TElement element = pattern[pattern_index];
                c = GetElementCode(element);
                //计算代表element的状态t的预期位置
                t = GetBase(s) + c;
                t = FindChildSlot(s, c, t, pattern_index, out check);

                if (check == int.MinValue)
                {
                    break;
                }
                s = t;
            }

            int temp_s = s;
            //模式串在节点树中已存在的部分
            int commonPrefex_length = pattern_index;
            Span<TElement> pattern_suffix = pattern.Slice(commonPrefex_length);
            Span<TElement> trie_suffix = GetSuffix(GetSuffixIndex(s));
            //2个后缀的公共前缀
            int suffixCommonPrefixLenght = GetCommonPrefixLenght(trie_suffix, pattern_suffix);
            Span<TElement> suffixCommonPrefix = pattern_suffix.Slice(0, suffixCommonPrefixLenght);
            //将公共前缀插入节点树
            foreach (var element in suffixCommonPrefix)
            {
                c = GetElementCode(element);
                t = GetBase(s) + c;
                t = FindChildSlot(s, c, t, pattern_index, out check);
                SetupBranch(s, c, t, pattern_index);
                this.nodePool[t].code = this.alphabetMap.reverse_Map[c];
                s = t;
                pattern_index++;
            }

            //将模式串剩余后缀的第一个元素插入节点树,其余存入尾缀数组
            pattern_suffix = pattern_suffix.Slice(suffixCommonPrefixLenght);
            if(pattern_suffix.Length > 0)
            {
                c = GetElementCode(pattern_suffix[0]);
                t = GetBase(s) + c;
                t = FindChildSlot(s, c, t, pattern_index, out check);
                SetupBranch(s, c, t, pattern_index);
                this.nodePool[t].code = this.alphabetMap.reverse_Map[c];
                if (pattern_suffix.Length > 1)
                {
                    int tailIndex = AddNewSuffix(pattern_suffix.Slice(1));
                    SetSuffixIndex(t, tailIndex);
                }
            }
            SetStateIsCompeletePattern(t, true);

            //将节点数剩余后缀的第一个元素插入节点树,其余存入原尾缀位置
            trie_suffix = trie_suffix.Slice(suffixCommonPrefixLenght);
            if (trie_suffix.Length > 0)
            {
                if (temp_s != t) SetStateIsCompeletePattern(temp_s, false);

                c = GetElementCode(trie_suffix[0]);
                t = GetBase(s) + c;
                t = FindChildSlot(s, c, t, pattern_index, out check);
                SetupBranch(s, c, t, pattern_index);
                this.nodePool[t].code = this.alphabetMap.reverse_Map[c];
                SetStateIsCompeletePattern(t, true);
                ReduceSuffix(GetSuffixIndex(temp_s), suffixCommonPrefixLenght + 1);
                if (trie_suffix.Length > 1)
                {
                    SetSuffixIndex(t, GetSuffixIndex(temp_s));
                }
            }
            ClearSuffixIndex(temp_s);
        }

        /// <summary>
        /// 寻找空闲槽位
        /// </summary>
        /// <returns></returns>
        private bool FindFreeSlot(int s, int c, out int newBase)
        {
            int index = this.nodePool[s].@base + 1;
            int t = 0;
            newBase = int.MinValue;
            List<int> children = new List<int>();
            children.Add(c);
            children.AddRange(this.nodePool[s].children);
            while (index < this.nodePool.Count)
            {
                bool find = true;

                foreach (var alphaCode in children)
                {
                    t = index + alphaCode;
                    if (t >= this.nodePool.Count     //子节点的新位置超出 当前池子容量
                        || Check(t) >= 0)           //这个位置已经被使用
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
            newBase = t;
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
            int oldBase = GetBase(s);
            foreach (var alphaCode in this.nodePool[s].children)
            {
                //移动s的子节点t到新位置
                int t = oldBase + alphaCode;
                TrieNode temp = this.nodePool[t];
                this.nodePool[t] = this.nodePool[newbase + alphaCode];
                this.nodePool[newbase + alphaCode] = temp;

                //将子节点t的子节点的父节点设置为新位置
                foreach (var t_alphaCode in this.nodePool[newbase + alphaCode].children)
                {
                    t = GetBase(newbase + alphaCode) + t_alphaCode;
                    this.nodePool[t].parent = newbase + alphaCode;
                }
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
        private int FindChildSlot(int s, int c, int t, int pattern_index, out int check)
        {
            if(t >= this.nodePool.Count)
            {
                int capacity = this.nodePool.Capacity * 2;
                while (capacity < t)
                {
                    capacity *= 2;
                }
                Resize(capacity);
            }

            int parent = Check(t);
            check = parent;

            if (parent == int.MinValue)
            {
                return t;
            }
            if (parent != s)
            {
                ///槽位t已经储存有状态, 且不来自状态s，发生冲突
                ///冲突解决: 选择s 和 t的parent中 子节点数量较少的一个, 然后寻找一个新的基地址使得 这个节点的所有直接子节点都可以移动到新槽位
                ///TODO:要插入的模式串是某个模式串的前缀是会发生s的parent是t的parent这种情况，这时对parent进行Relocate需要对索引持有s索引的地方进行更新
                int newBase = -1;
                int need_relocate_state = s;
                if (!FindFreeSlot(need_relocate_state, c, out newBase))
                {
                    //空间不足，分配新的空间            
                    int oldCapacity = this.nodePool.Capacity;
                    int newCapacity = this.nodePool.Capacity * 2;
                    while(newCapacity < newBase)
                    {
                        newCapacity *= 2;
                    }
                    Resize(newCapacity);
                    newBase = oldCapacity + 1;
                }
                Relocate(need_relocate_state, newBase);

                //插入新节点t
                t = GetBase(s) + c;
                //冲突解决后 t槽位为空
                check = int.MinValue;
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
        private void SetupBranch(int s, int c, int t, int pattern_index)
        {
            this.nodePool[t].@base = this.nodePool[s].@base;
            this.nodePool[t].parent = s;

            this.nodePool[t].depth = pattern_index;

            this.nodePool[s].children.Add(c);
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
            if (s >= this.nodePool.Capacity)
            {
                int newCapacity = this.nodePool.Capacity * 2;
                while (newCapacity < s)
                {
                    newCapacity *= 2;
                }
                Resize(newCapacity);
            }
            return this.nodePool[s].parent;
        }

        /// <summary>
        /// 获取节点<paramref name="s"/>的直接子节点数量
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private int GetChildrenCount(int s)
        {
            return this.nodePool[s].children.Count;
;       }

        /// <summary>
        /// 设置节点<paramref name="s"/>是一个完成模式串的叶节点
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool SetStateIsCompeletePattern(int s, bool enable)
        {
            return this.nodePool[s].isCompeletePattern = enable;
        }

        /// <summary>
        /// 节点<paramref name="s"/>是否是一个完成模式串的叶节点
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
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
            for (int i = 0; ; i++)
            {
                this.tail[pos + i] = this.tail[pos + reduce + i];
                if (this.tail[pos + i].Equals(this.terminator)) return;
            }
        }

        /// <summary>
        /// 添加新的后缀到后缀数组, 并在结尾添加一个终止符
        /// </summary>
        /// <param name="suffix">需要添加的后缀</param>
        /// <returns>后缀的开始位置</returns>
        private int AddNewSuffix(Span<TElement> suffix)
        {
            if (suffix.Length == 0) return int.MinValue;

            int pos = this.tail.Count;
            foreach (var item in suffix)
            {
                this.tail.Add(item);
            }
            //添加终止符
            this.tail.Add(terminator);
            return pos;
        }
        /// <summary>
        /// 获取从<paramref name="start"/>开始的后缀，不包括终结符
        /// </summary>
        /// <param name="start">后缀开始索引</param>
        /// <returns>后缀</returns>
        private Span<TElement> GetSuffix(int start)
        {
            if (start == int.MinValue) return new Span<TElement>();

            int end = this.tail.FindIndex(start, (element) => { return element.Equals(terminator); });
            //不包括终止符
            return new Span<TElement>(this.tail.ToArray(), start, end - start);
        }
        /// <summary>
        /// 比较两个后缀是否相等
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 获取两个后缀的公共前缀
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>后缀的公共前缀</returns>
        private int GetCommonPrefixLenght(Span<TElement> a, Span<TElement> b)
        {
            int length = Math.Min(a.Length, b.Length);
            for (int i = 0; i < length; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    return i ;
                }
            }
            return length;
        }
        #endregion

        internal class TrieNode
        {
            public TElement code;
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
            public Dictionary<int, TElement> reverse_Map = new Dictionary<int, TElement>();
        }
    }
}
