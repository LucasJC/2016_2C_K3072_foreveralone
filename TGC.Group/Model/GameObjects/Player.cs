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
        ///     indica que el jugador recibió daño recientemente
        /// </summary>
        public bool Hurt { get; set; } = false;

        /// <summary>
        ///     hambre del jugador
        /// </summary>
        public int Hunger { get; set; } = 100;

        public int Stamina { get; set; } = 100;

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
        ///     herramienta actualmente equipada, define los puntos de daño que realiza el usuario
        /// </summary>
        public InventoryObject EquippedTool;
        /// <summary>
        ///     armadura equipada, define los puntos de defensa del usuario y si sufre daño por frío extremo
        /// </summary>
        public InventoryObject EquippedArmor;

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
                if (EquippedTool == objectToRemove)
                {
                    EquippedTool = null;
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
            this.Hurt = true;
            if(this.LifePoints < 0)
            {
                this.LifePoints = 0;
            }

            if (this.LifePoints == 0) this.Alive = false;

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
            if(null != this.EquippedTool)
            {
                damage = this.EquippedTool.Damage;
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

                InventoryObject.Categories category = InventoryObject.CategoryPerType[SelectedItem.Type];

                if (InventoryObject.Categories.Armor.Equals(category))
                {
                    //es armadura
                    if (EquippedArmor == SelectedItem)
                    {
                        EquippedArmor = null;
                    }
                    else
                    {
                        EquippedArmor = SelectedItem;
                    }
                }
                else if (InventoryObject.Categories.Tool.Equals(category))
                {
                    //es una herramienta / arma
                    if (EquippedTool == SelectedItem)
                    {
                        EquippedTool = null;
                    }
                    else
                    {
                        EquippedTool = SelectedItem;
                    }
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

        /// <summary>
        ///     el jugador sufre los efectos del clima
        /// </summary>
        /// <param name="weather"></param>
        public void sufferWeather(World.Weather weather)
        {

            if(World.Weather.Hot.Equals(weather))
            {
                //hace calor
                if (this.Thirst > 0) this.Thirst--;
                if (this.Stamina > 0) this.Stamina--;

            }else if (World.Weather.Cold.Equals(weather))
            {
                //hace frío
                if (this.Hunger > 0) this.Hunger--;
            }
            else if (World.Weather.ExtremeCold.Equals(weather))
            {
                //hace muucho frío
                if (this.Hunger > 0) this.Hunger--;
                if (null == this.EquippedArmor) this.beHit(2);
            }
            else if (World.Weather.Normal.Equals(weather))
            {
                if(this.Stamina < 100) this.Stamina++;
            }
        }

        /// <summary>
        ///     intenta consumir el objeto seleccionado. Retorna true si se consumió
        /// </summary>
        public bool consumeItem()
        {
            bool result = false;
            InventoryObject obj = this.SelectedItem;

            if (InventoryObject.ObjectTypes.Seed.Equals(obj.Type))
            {   //Seed
                this.Hunger = this.Hunger + 5;
                if (this.Hunger > 100) this.Hunger = 100;
                result = true;
            }
            else if (InventoryObject.ObjectTypes.Rock.Equals(obj.Type))
            {   //Roca ?
                this.beHit(25);
                result = true;
            }
            else if (InventoryObject.ObjectTypes.AlienMeat.Equals(obj.Type))
            {   //AlienMeat!
                this.Hunger = this.Hunger + 5;
                if (this.Hunger > 100) this.Hunger = 100;
                result = true;
            }
            else if (InventoryObject.ObjectTypes.Water.Equals(obj.Type))
            {   //Agua
                this.Thirst = this.Thirst + 5;
                if (this.Thirst > 100) this.Thirst = 100;
                result = true;
            }
            else if (InventoryObject.ObjectTypes.Potion.Equals(obj.Type))
            {   //Poción
                this.Thirst = this.Thirst + 50;
                if (this.Thirst > 100) this.Thirst = 100;
                this.LifePoints = 100;
                result = true;
            }

            if (result) this.removeInventoryObject(obj);

            return result;
        }
    }
}
