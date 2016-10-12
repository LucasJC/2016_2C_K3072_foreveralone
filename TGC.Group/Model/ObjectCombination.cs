using System.Collections.Generic;

namespace TGC.Group.Model
{
    /// <summary>
    ///     combinación entre varios objetos para generar uno nuevo
    /// </summary>
    public class ObjectCombination
    {
        /// <summary>
        ///     diccionario con cada objeto requerido y su cantidad
        /// </summary>
        public Dictionary<InventoryObject.ObjectTypes, int> materials { get; }
        /// <summary>
        ///     tipo de objeto resultante
        /// </summary>
        public InventoryObject.ObjectTypes result { get; set; }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="resultingObjectType"></param>
        public ObjectCombination(InventoryObject.ObjectTypes resultingObjectType)
        {
            this.materials = new Dictionary<InventoryObject.ObjectTypes, int>();
            this.result = resultingObjectType;
        }
    }
}