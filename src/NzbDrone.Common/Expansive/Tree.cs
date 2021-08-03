namespace NzbDrone.Common.Expansive
{
    internal class Tree<T> : TreeNode<T>
    {
        public Tree(T rootValue)
            : base(rootValue)
        {
            Value = rootValue;
        }
    }
}
