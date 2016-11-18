using TGC.Core.SceneLoader;

namespace TGC.Group.Model.Optimization
{
    /// <summary>
    ///     Nodo del árbol Quadtree
    /// </summary>
    internal class QuadtreeNode
    {
        public QuadtreeNode[] children;
        public InteractiveObject[] models;

        public bool isLeaf()
        {
            return children == null;
        }
    }
}