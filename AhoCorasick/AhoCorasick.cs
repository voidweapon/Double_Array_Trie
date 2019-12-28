using System;
using System.Collections.Generic;
using System.Text;

namespace Double_Array_Trie.AhoCorasick
{
    class AhoCorasick<TElement>
        where TElement : IComparable<TElement>, IEquatable<TElement> //串元素可比较用于排序,可做相等性比较

    {
        private TElement terminator;

        private State root = new State() { fail = null };
        public AhoCorasick(TElement terminator)
        {
            if(terminator == null)
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

            State s = root;
            State t;
            int state_depth = -1;
            foreach (var element in pattern)
            {
                state_depth++;
                if (!s.children.TryGetValue(element, out t))
                {
                    t = new State() 
                    { 
                        depth = state_depth,
                    };
                    s.children.Add(element, t);
                }
                s = t;
            }
            //给每个模式串的结尾添加一个终止符
            //处理当一个模式串是另一个模式串的前缀时, 前缀串的匹配问题
            if (!s.children.ContainsKey(terminator))
            {
                s.children.Add(terminator, new State());
            }
        }

        public void BuildFailPoint()
        {
            State s = null;
            State temp = null;
            State fail = null;
            Queue<State> queue = new Queue<State>();
            //第一层节点的fail执行root
            foreach (var key in root.children.Keys)
            {
                root.children[key].fail = root;
                queue.Enqueue(root.children[key]);
            }
           
            while(queue.Count > 0)
            {
                s = queue.Dequeue();
                var keys = s.children.Keys;
                foreach (var key in keys)
                {
                    //子节点的fail节点 指向父节点的fail节点 的具有相同key的子节点
                    //没有就跳转父节点的fail节点,继续执行
                    //直到跳转到root节点, 也无法找到匹配节点, 这时将子节点的fail节点指向root
                    s.children[key].fail = root;
                    temp = s;
                    while (temp != null)
                    {
                        if (temp.fail != null && temp.fail.children.TryGetValue(key, out fail))
                        {
                            s.children[key].fail = fail;
                            break;
                        }
                        temp = temp.fail;
                    }

                    queue.Enqueue(s.children[key]);
                }
            }
        }

        public List<Match> Search(IEnumerable<TElement> str)
        {
            if (terminator == null)
            {
                throw new ArgumentNullException($"{nameof(str)} is null");
            }

            State s = root;
            State temp = null;
            List<Match> matches = new List<Match>();

            int str_index = -1;
            foreach (var element in str)
            {
                str_index++;

                while (!s.children.ContainsKey(element) && s != root)
                {
                    //匹配失败
                    //跳转到当前节点的fail节点继续匹配
                    s = s.fail;
                }

                //此时当前状态是root 或者子节点中匹配成功
                if (!s.children.TryGetValue(element, out temp))
                {
                    //在root中匹配失败, 继续下一个字符的匹配
                    temp = root;
                    continue;
                }
                else
                {
                    //当前状态转移到子状态
                    s = temp;
                }

                while(temp != root)
                {
                    //子节点含有终止符,则标识匹配到一个完成模式串
                    if (temp.children.ContainsKey(terminator))
                    {
                        matches.Add(new Match()
                        {
                            start = str_index - temp.depth,
                            length = temp.depth + 1,
                        }); ;
                    }
                    //通过跳转到fail节点来检测所有的前缀
                    temp = temp.fail;
                }
            }

            return matches;
        }

        internal class State
        {
            /// <summary>
            /// 节点在树中的深度
            /// </summary>
            public int depth = 0;
            /// <summary>
            /// 失败指针
            /// </summary>
            public State fail = null;
            /// <summary>
            /// 子状态
            /// </summary>
            public readonly Dictionary<TElement, State> children = new Dictionary<TElement, State>();
        }

        public struct Match{
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
