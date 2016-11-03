using System;
using System.Collections.Generic;
using TGC.Group.Model._2D;

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
            Wood,
            AlienMeat, //ñam
            WoodArmor
        }

        public enum Categories
        {
            Drink,
            Food,
            Tool,
            Armor,
            Other
        }

        /// <summary>
        ///     diccionario que define categorías para cada tipo. En caso de no poseer entrada para un tipo, se asume Other
        /// </summary>
        public static Dictionary<ObjectTypes, Categories> CategoryPerType = new Dictionary<ObjectTypes, Categories>();

        /// <summary>
        ///     diccionario que dice el daño que produce un tipo de item, si no figura entonces el daño es 1 (default)
        /// </summary>
        public static Dictionary<ObjectTypes, int> DamagePerType = new Dictionary<ObjectTypes, int>();

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
        ///     tipo del objeto para definir interacciones - refiere a qué objeto es
        /// </summary>
        public ObjectTypes Type;

        /// <summary>
        ///     indica los puntos de daño que puede generar este objeto
        /// </summary>
        public int Damage = 1;

        /// <summary>
        ///     ubicación en el inventario
        /// </summary>
        public int InventoryIndex = 0;

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

            int damage = 1;

            try
            {
                damage = DamagePerType[type];
            }
            catch (KeyNotFoundException)
            {
                //no pasa nada
            }

            this.Damage = damage;
        }

        /// <summary>
        ///     indica si un objeto es equipable
        /// </summary>
        /// <returns></returns>
        public bool isEquippable()
        {
            bool result = false;

            if (CategoryPerType.ContainsKey(this.Type))
            {
                Categories category = CategoryPerType[this.Type];
                if (Categories.Armor.Equals(category) || Categories.Tool.Equals(category))
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        ///     inicializo las distintas combinaciones posibles
        /// </summary>
        static InventoryObject()
        {
            //Daño por tipo de objeto
            DamagePerType.Add(ObjectTypes.Axe, 2);

            //Categorías
            CategoryPerType.Add(ObjectTypes.Axe, Categories.Tool);
            CategoryPerType.Add(ObjectTypes.WoodArmor, Categories.Armor);
            CategoryPerType.Add(ObjectTypes.Potion, Categories.Drink);
            CategoryPerType.Add(ObjectTypes.Water, Categories.Drink);
            CategoryPerType.Add(ObjectTypes.AlienMeat, Categories.Food);
            CategoryPerType.Add(ObjectTypes.Seed, Categories.Food);
            CategoryPerType.Add(ObjectTypes.Leaf, Categories.Other);
            CategoryPerType.Add(ObjectTypes.Rock, Categories.Other);
            CategoryPerType.Add(ObjectTypes.Wood, Categories.Other);

            //Combinaciones
            InventoryObject.ObjectCombinations = new List<ObjectCombination>();
            
            //Defino el hacha
            ObjectCombination axe = new ObjectCombination(InventoryObject.ObjectTypes.Axe);
            axe.materials.Add(ObjectTypes.Wood, 2);
            axe.materials.Add(ObjectTypes.Rock, 1);
            InventoryObject.ObjectCombinations.Add(axe);

            //Defino la armadura de madera
            ObjectCombination armor = new ObjectCombination(InventoryObject.ObjectTypes.WoodArmor);
            armor.materials.Add(ObjectTypes.Wood, 4);
            armor.materials.Add(ObjectTypes.Leaf, 2);
            InventoryObject.ObjectCombinations.Add(armor);

            //Defino la poción
            ObjectCombination potion = new ObjectCombination(InventoryObject.ObjectTypes.Potion);
            potion.materials.Add(ObjectTypes.Seed, 2);
            potion.materials.Add(ObjectTypes.Water, 1);
            InventoryObject.ObjectCombinations.Add(potion);
        }

        public static bool combineObjects(Player player, List<InventoryObject> materialsUsed)
        {
            int auxCount = 0;
            bool success = false;
            ObjectCombination chosenCombination = null;

            List<InventoryObject> materialsUsedClone = new List<InventoryObject>();
            materialsUsedClone.AddRange(materialsUsed);

            foreach (ObjectCombination combination in InventoryObject.ObjectCombinations)
            {

                success = true;

                foreach (KeyValuePair<InventoryObject.ObjectTypes, int> material in combination.materials)
                {
                    auxCount = 0;

                    foreach (InventoryObject obj in materialsUsed)
                    {
                        if (material.Key.Equals(obj.Type))
                        {
                            auxCount++;
                        }
                    }

                    if (auxCount != material.Value)
                    {
                        //cantidad de objetos errónea
                        success = false;
                        break;
                    }
                }

                if(success)
                {
                    chosenCombination = combination;
                    break;
                }
            }

            //pasó todos los controles -> OK
            if (null != chosenCombination)
            {
                //remuevo los objetos y le agrego el objeto final
                foreach (InventoryObject element in materialsUsedClone)
                {
                    player.removeInventoryObject(element);
                }
                player.addInventoryObject(new InventoryObject(chosenCombination.result));
                return true;
            }else
            {
                return false;
            }

        }
    }
}