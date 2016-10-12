using System;
using System.Collections.Generic;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Objeto que puede estar en el inventario del jugador
    /// </summary>
    public class InventoryObject
    {
        /// <summary>
        ///     contador de ids de objeto utilizados
        /// </summary>
        private static long IdCounter = 0;
        /// <summary>
        ///     objetos disponibles en el mundo
        /// </summary>
        public enum ObjectTypes {
            Axe,
            Leaf,
            Potion,
            Rock,
            Seed,
            Water,
            Wood
        }
        /// <summary>
        ///     tipos de combinaciones posibles
        /// </summary>
        public static List<ObjectCombination> ObjectCombinations { get; set; }

        /// <summary>
        ///     id para control interno
        /// </summary>
        private long Id;
        /// <summary>
        ///     nombre visible en inventario
        /// </summary>
        private String Name;
        /// <summary>
        ///     tipo del objeto para definir interacciones
        /// </summary>
        public ObjectTypes Type;
        /// <summary>
        ///     indica los puntos de daño que puede generar este objeto
        /// </summary>
        public int Damage = 1;

        /// <summary>
        ///     constructor principal
        /// </summary>
        /// <param name="type"></param>
        public InventoryObject(ObjectTypes type)
        {
            InventoryObject.IdCounter++;
            this.Id = InventoryObject.IdCounter;
            this.Type = type;
            this.Name = type.ToString();
        }

        /// <summary>
        ///     inicializo las distintas combinaciones posibles
        /// </summary>
        static InventoryObject()
        {
            InventoryObject.ObjectCombinations = new List<ObjectCombination>();
            
            //Defino el hacha
            ObjectCombination axe = new ObjectCombination(InventoryObject.ObjectTypes.Axe);
            axe.materials.Add(ObjectTypes.Wood, 2);
            axe.materials.Add(ObjectTypes.Rock, 1);
            InventoryObject.ObjectCombinations.Add(axe);

            //Defino la poción
            ObjectCombination potion = new ObjectCombination(InventoryObject.ObjectTypes.Potion);
            potion.materials.Add(ObjectTypes.Seed, 2);
            potion.materials.Add(ObjectTypes.Water, 1);
            InventoryObject.ObjectCombinations.Add(potion);

            //TODO definir más combinaciones!
        }

        public static bool combineObjects(Player player, List<InventoryObject> materialsUsed, ObjectCombination combination)
        {
            int auxCount = 0;

            foreach(KeyValuePair<InventoryObject.ObjectTypes, int> material in combination.materials)
            {
                auxCount = 0;

                foreach (InventoryObject obj in materialsUsed)
                {
                    if(material.Key.Equals(obj.Type))
                    {
                        auxCount++;
                    }
                }

                if(auxCount != material.Value)
                {
                    //cantidad de objetos errónea
                    return false;
                }
            }
            //pasó todos los controles -> OK
            //remuevo los objetos y le agrego el objeto final
            foreach(InventoryObject materialUsed in materialsUsed)
            {
                player.Inventory.Remove(materialUsed);
            }
            player.Inventory.Add(new InventoryObject(combination.result));
            return true;
        }
    }
}