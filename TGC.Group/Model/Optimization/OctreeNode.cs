using TGC.Core.SceneLoader;

namespace TGC.Group.Model.Optimization
{
    /// <summary>
    ///     Nodo del árbol Octree
    /// </summary>
    internal class OctreeNode
    {
        public OctreeNode[] children;
        public InteractiveObject[] models;

        public bool isLeaf()
        {
            return children == null;
        }
    }
}