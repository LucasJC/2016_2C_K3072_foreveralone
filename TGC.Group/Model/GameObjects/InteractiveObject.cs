using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Objeto con el cual el jugador puede interactuar
    /// </summary>
    public class InteractiveObject
    {
        public enum Materials { Wood, Metal, Glass, Plant, None, Rock };
        public enum ObjectTypes { Tree, Rock , Misc, Grass };

        public TgcMesh mesh;
        public int lifePoints { get; set; }
        public String name { get; set; }
        public bool selected { get; set; }
        public Materials material;
        public ObjectTypes objectType;
        public List<InventoryObject.ObjectTypes> drops = new List<InventoryObject.ObjectTypes>();
        public bool alive = true;
        public bool Solid = true;

        /// <summary>
        ///     constructor
        /// </summary>
        public InteractiveObject(String name, int lifePoints, TgcMesh mesh, Materials material, ObjectTypes type)
        {
            this.name = name;
            this.lifePoints = lifePoints;
            this.mesh = mesh;
            this.material = material;
            this.objectType = type;
            if (material.Equals(Materials.Plant)) this.Solid = false;
        }

        /// <summary>
        ///     método que representa que el objeto es golpeado, por lo tanto pierde los puntos indicados
        /// </summary>
        /// <param name="points"></param>
        /// <returns>devuelve true si el objeto fue destruído</returns>
        public bool getHit(int points)
        {
            this.lifePoints -= points;
            if(this.lifePoints <= 0)
            {
                this.alive = false;
                return true;
            }else
            {
                return false;
            }
        }

        /// <summary>
        ///     método que genera los objetos de drop para el usuario
        /// </summary>
        /// <returns></returns>
        public List<InventoryObject> getDrops()
        {
            List<InventoryObject> result = new List<InventoryObject>();

            foreach(InventoryObject.ObjectTypes type in drops)
            {
                result.Add(new InventoryObject(type));
            }

            return result;
        }
    }
}
