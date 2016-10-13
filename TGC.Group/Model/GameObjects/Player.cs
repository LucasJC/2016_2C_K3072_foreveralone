using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Clase que representa al jugador
    /// </summary>
    public class Player
    {
        /// <summary>
        ///     nombre del jugador
        /// </summary>
        public String Name { get; set; } = "Player1";

        /// <summary>
        ///     puntos de vida del jugador
        /// </summary>
        public int LifePoints { get; set; } = 100;

        /// <summary>
        ///     clima actual
        /// </summary>
        public World.Weather Weather { get; set; } = World.Weather.Normal;

        /// <summary>
        ///     sed del jugador
        /// </summary>
        public int Thirst { get; set; } = 100;

        /// <summary>
        ///     hambre del jugador
        /// </summary>
        public int Hunger { get; set; } = 100;

        /// <summary>
        ///     elementos del inventario
        /// </summary>
        public List<InventoryObject> Inventory { get; set; } = new List<InventoryObject>();

        /// <summary>
        ///     tamaño del inventario, indica el máximo
        /// </summary>
        private int InventorySize = 20;

        /// <summary>
        ///     indica si el jugador está vivo
        /// </summary>
        public bool Alive { get; set; } = true;

        /// <summary>
        ///     objeto actualmente equipado, define los puntos de daño que realiza el usuario
        /// </summary>
        public InventoryObject EquippedObject;

        /// <summary>
        ///     Método que retorna true al agregar un objeto al inventario del jugador.
        ///     Si el jugador no tiene espacio disponible en su inventario devuelve false y el objeto no se agrega.
        /// </summary>
        /// <param name="newObject"></param>
        /// <returns></returns>
        public bool addInventoryObject(InventoryObject newObject)
        {
            if(this.InventorySize < this.Inventory.Count)
            {
                this.Inventory.Add(newObject);
                return true;
            }else
            {
                return false;
            }
        }

        /// <summary>
        ///     método que remueve un objeto del inventario del jugador
        /// </summary>
        /// <param name="objectToRemove"></param>
        public void removeInventoryObject(InventoryObject objectToRemove) {
            this.Inventory.Remove(objectToRemove);
        }

        /// <summary>
        ///     método que indica al jugador que fue lastimado con x puntos de vida
        /// </summary>
        /// <param name="hitPoints"></param>
        /// <returns></returns>
        public int beHit(int hitPoints)
        {
            this.LifePoints -= hitPoints;
            if(this.LifePoints < 0)
            {
                this.LifePoints = 0;
                this.Alive = false;
            }
            return this.LifePoints;
        }

        /// <summary>
        ///     método que retorna el daño que genera el jugador
        /// </summary>
        /// <returns></returns>
        public int getDamage()
        {
            //default damage
            int damage = 1;
            if(null != this.EquippedObject)
            {
                damage = this.EquippedObject.Damage;
            }
            return damage;
        }
    }
}
