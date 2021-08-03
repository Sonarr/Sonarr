using System.Collections.Generic;

namespace NzbDrone.Common.Expansive
{
    internal class TreeNode<T>
    {
        private List<T> _CallTree;
        private TreeNode<T> _Parent;

        public TreeNode(T value)
        {
            Value = value;
            Parent = null;
            Children = new TreeNodeList<T>(this);
            _CallTree = new List<T>();
        }

        public TreeNode(T value, TreeNode<T> parent)
        {
            Value = value;
            Parent = parent;
            Children = new TreeNodeList<T>(this);
            _CallTree = new List<T>();
        }

        public TreeNode<T> Parent
        {
            get
            {
                return _Parent;
            }

            set
            {
                if (value == _Parent)
                {
                    return;
                }

                if (_Parent != null)
                {
                    _Parent.Children.Remove(this);
                }

                if (value != null && !value.Children.Contains(this))
                {
                    value.Children.Add(this);
                }

                _Parent = value;
            }
        }

        public TreeNode<T> Root
        {
            get
            {
                //return (Parent == null) ? this : Parent.Root;

                TreeNode<T> node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                }

                return node;
            }
        }

        public TreeNodeList<T> Children { get; private set; }

        public List<T> CallTree
        {
            get
            {
                _CallTree = new List<T>();
                TreeNode<T> node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                    _CallTree.Add(node.Value);
                }

                return _CallTree;
            }

            private set
            {
                _CallTree = value;
            }
        }

        public T Value { get; set; }
    }
}
