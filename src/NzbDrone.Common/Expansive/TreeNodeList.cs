using System.Collections.Generic;

namespace NzbDrone.Common.Expansive
{
    internal class TreeNodeList<T> : List<TreeNode<T>>
    {
        public TreeNode<T> Parent;

        public TreeNodeList(TreeNode<T> parent)
        {
            this.Parent = parent;
        }

        public new TreeNode<T> Add(TreeNode<T> node)
        {
            base.Add(node);
            node.Parent = Parent;
            return node;
        }

        public TreeNode<T> Add(T value)
        {
            return Add(new TreeNode<T>(value));
        }

        public override string ToString()
        {
            return "Count=" + Count.ToString();
        }
    }
}
