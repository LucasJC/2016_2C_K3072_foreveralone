using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;

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
        public SortedList<int, InventoryObject> Inventory { get; set; } = new SortedList<int, InventoryObject>();

        private int inventoryIndexCounter = 0;

        /// <summary>
        ///     tamaño del inventario, indica el máximo
        /// </summary>
        private int InventorySize = 10;

        /// <summary>
        ///     item seleccionado
        /// </summary>
        public InventoryObject SelectedItem { set; get; }

        /// <summary>
        ///     index del item seleccionado
        /// </summary>
        public int SelectedItemIndex { set; get; }

        /// <summary>
        ///     items seleccionados para combinación
        /// </summary>
        public List<InventoryObject> combinationSelection = new List<InventoryObject>();

        /// <summary>
        ///     indica si el jugador está vivo
        /// </summary>
        public bool Alive { get; set; } = true;

        /// <summary>
        ///     objeto actualmente equipado, define los puntos de daño que realiza el usuario
        /// </summary>
        public InventoryObject EquippedObject;

        /// <summary>
        ///     indica si el usuario se está moviendo
        /// </summary>
        public bool Moving { set; get; }

        public Player()
        {
        }

        /// <summary>
        ///     Método que retorna true al agregar un objeto al inventario del jugador.
        ///     Si el jugador no tiene espacio disponible en su inventario devuelve false y el objeto no se agrega.
        /// </summary>
        /// <param name="newObject"></param>
        /// <returns></returns>
        public bool addInventoryObject(InventoryObject newObject)
        {
            if(this.Inventory.Count < this.InventorySize)
            {
                inventoryIndexCounter++;
                newObject.InventoryIndex = inventoryIndexCounter;
                this.Inventory.Add(newObject.InventoryIndex, newObject);
                if(this.Inventory.Count == 1)
                {
                    //primer objeto levantado -> lo pongo como seleccionado
                    SelectedItem = newObject;
                    SelectedItemIndex = newObject.InventoryIndex;
                }
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

            if(null != objectToRemove)
            {
                this.Inventory.Remove(objectToRemove.InventoryIndex);
                if (EquippedObject == objectToRemove)
                {
                    EquippedObject = null;
                }
                if (SelectedItem == objectToRemove)
                {
                    SelectedItem = null;

                    if (Inventory.Count == 1)
                    {
                        SelectedItem = Inventory.First().Value;
                        SelectedItemIndex = SelectedItem.InventoryIndex;
                    }
                    else
                    {
                        selectNextItem();
                        if (null == SelectedItem)
                        {
                            selectPreviousItem();
                        }
                    }
                }
                if (combinationSelection.Contains(objectToRemove))
                {
                    combinationSelection.Remove(objectToRemove);
                }

                //si no tengo más items, reinicio el contador de objetos
                if (0 == Inventory.Count)
                {
                    inventoryIndexCounter = 0;
                }
            }
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

        /// <summary>
        ///     intenta seleccionar el item siguiente al actual
        /// </summary>
        public void selectNextItem()
        {
            if(Inventory.Count > 1 && SelectedItemIndex < inventoryIndexCounter)
            {
                InventoryObject result = null;
                int i = SelectedItemIndex;
                while(i <= inventoryIndexCounter)
                {
                    i++;
                    result = getItemByIndex(i);
                    if(null != result)
                    {
                        SelectedItemIndex = i;
                        SelectedItem = result;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     agrega un item a la selección de combinación
        /// </summary>
        /// <param name="selectedItem"></param>
        public void selectForCombination(InventoryObject selectedItem)
        {
            if(!this.combinationSelection.Contains(selectedItem))
            {
                combinationSelection.Add(selectedItem);
            }else
            {
                combinationSelection.Remove(selectedItem);
            }
        }

        /// <summary>
        ///     intenta seleccionar el item previo al actual
        /// </summary>
        public void selectPreviousItem()
        {
            if (SelectedItemIndex > 1 && Inventory.Count > 1)
            {
                InventoryObject result = null;
                int i = SelectedItemIndex;
                while (i > 1)
                {
                    i--;
                    result = getItemByIndex(i);
                    if (null != result)
                    {
                        SelectedItemIndex = i;
                        SelectedItem = result;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     equipa o desequipa el item actual (toggle)
        /// </summary>
        public void equipSelectedItem()
        {
            if(null != SelectedItem)
            {
                if (EquippedObject == SelectedItem)
                {
                    EquippedObject = null;
                }
                else
                {
                    EquippedObject = SelectedItem;
                }
            }
        }

        /// <summary>
        ///     devuelve un item del inventario por su índice
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <returns></returns>
        private InventoryObject getItemByIndex(int itemIndex)
        {
            InventoryObject result = null;

            foreach(KeyValuePair<int,InventoryObject> item in Inventory)
            {
                if(itemIndex == item.Value.InventoryIndex)
                {
                    result = item.Value;
                    break;
                }
            }

            return result;
        }
    }
}
