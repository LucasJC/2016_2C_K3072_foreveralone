using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
using TGC.Core.Input;
using TGC.Core.Text;
using TGC.Core.Utils;
using TGC.Examples.Engine2D.Spaceship.Core;

namespace TGC.Group.Model
{
    /// <summary>
    ///     interfaz gráfica del juego
    /// </summary>
    class GUI : Renderable
    {
        public static Dictionary<InventoryObject.ObjectTypes, CustomSprite> InventorySprites { get; set; }
        public Dictionary<Vector2, InventoryObject> CurrentInventoryPositions {get;set;}
        public enum StatusBarElements { LifePoints, Weather, Hunger, Thirst, Player }
        private int usedSpace;
        private TgcText2D dayTime;
        private Drawer2D drawer;
        private Dictionary<StatusBarElements, TgcText2D> statusBarContent;
        private CustomSprite StatusBar;
        private CustomSprite itemSelectionSprite;
        private CustomSprite itemCombinationSprite;
        private CustomSprite itemEquippedSprite;
        private Player Player1;
        private GameModel gameModelInstance;
        private D3DDevice Device;

        public static int DefaultFontSize { get; set; } = 12;


        static GUI() {
            InventorySprites = new Dictionary<InventoryObject.ObjectTypes, CustomSprite>();
        }

        public GUI(String mediaDir, D3DDevice device, Player player, GameModel gameModel)
        {
            Device = device;
            Player1 = player;
            gameModelInstance = gameModel;
            
            drawer = new Drawer2D();
            statusBarContent = new Dictionary<StatusBarElements, TgcText2D>();

            //armo sprite de selección
            Vector2 scaling = new Vector2(0.35f, 0.35f);

            itemSelectionSprite = null;
            itemSelectionSprite = new CustomSprite();
            itemSelectionSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\items\\selection.png", Device.Device);
            itemSelectionSprite.Scaling = scaling;

            itemCombinationSprite = null;
            itemCombinationSprite = new CustomSprite();
            itemCombinationSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\items\\combination.png", Device.Device);
            itemCombinationSprite.Scaling = scaling;

            itemEquippedSprite = null;
            itemEquippedSprite = new CustomSprite();
            itemEquippedSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\items\\equipped.png", Device.Device);
            itemEquippedSprite.Scaling = scaling;

            //creo sprites para cada tipo de objeto
            foreach (InventoryObject.ObjectTypes type in Enum.GetValues(typeof(InventoryObject.ObjectTypes)))
            {
                CustomSprite sprite = null;
                sprite = new CustomSprite();
                sprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\items\\" + type.ToString() + ".png", Device.Device);
                sprite.Scaling = scaling;

                //agrego objetos a la lista
                InventorySprites.Add(type, sprite);
            }

            //armo status bar
            StatusBar = new CustomSprite();
            StatusBar.Bitmap = new CustomBitmap(mediaDir + "2d\\statusBar.png", Device.Device);
            StatusBar.Scaling = scaling; 

            float barHeight = (StatusBar.Bitmap.Size.Height * StatusBar.Scaling.Y);
            float barWidth = (StatusBar.Bitmap.Size.Width * StatusBar.Scaling.Y);
            StatusBar.Position = new Vector2(FastMath.Max(Device.Width - barWidth, 0), FastMath.Max(Device.Height - barHeight, 0));

            //textos de status
            float textHeight = (GameUtils.createText("ejemplo").Size.Height / (barHeight / 5));
            float textWidth = (Device.Width - (barWidth * 0.9f)) ;
            statusBarContent.Add(StatusBarElements.Player, GameUtils.createText("jugador: ", textWidth, Device.Height - (textHeight * 5) - barHeight * 0.1f));
            statusBarContent.Add(StatusBarElements.LifePoints, GameUtils.createText("vida: ", textWidth, Device.Height - (textHeight * 4) - barHeight * 0.1f));
            statusBarContent.Add(StatusBarElements.Weather, GameUtils.createText("clima: ", textWidth, Device.Height - (textHeight * 3) - barHeight * 0.1f));
            statusBarContent.Add(StatusBarElements.Hunger, GameUtils.createText("hambre: ", textWidth, Device.Height - (textHeight * 2) - barHeight * 0.1f));
            statusBarContent.Add(StatusBarElements.Thirst, GameUtils.createText("sed: ", textWidth, Device.Height - (textHeight * 1) - barHeight * 0.1f));

            //contabilizo texto fps
            usedSpace += (int) (DefaultFontSize * 1.1f);
            dayTime = GameUtils.createText("", 0, usedSpace);
            usedSpace += (int)(DefaultFontSize * 1.1f);

        }

        public void dispose()
        {
            foreach (KeyValuePair<InventoryObject.ObjectTypes, CustomSprite> element in InventorySprites)
            {
                element.Value.Dispose();
            }

            foreach (KeyValuePair<StatusBarElements, TgcText2D> text in statusBarContent)
            {
                text.Value.Dispose();
            }
        }

        public void render()
        {
            //muestro fecha
            dayTime.render();

            drawer.BeginDrawSprite();
            int count = 1;
            CustomSprite sprite = null;

            //ordeno items del inventario
            
            //por cada elemento del inventario del usuario dibujo un sprite de ese tipo
            foreach (KeyValuePair<int,InventoryObject> element in Player1.Inventory)
            {
                sprite = InventorySprites[element.Value.Type];
                float height = (sprite.Bitmap.Size.Height * sprite.Scaling.Y);
                float width = (((sprite.Bitmap.Size.Width * sprite.Scaling.Y)) * count) * 1.1f;
                sprite.Position = new Vector2(FastMath.Max(0 + width, 0), FastMath.Max(Device.Height - height, 0));
                drawer.DrawSprite(sprite);

                if(element.Value.InventoryIndex == Player1.SelectedItemIndex)
                {
                    //es el seleccionado, dibujo el recuadro
                    itemSelectionSprite.Position = sprite.Position;
                    drawer.DrawSprite(itemSelectionSprite);
                }
                if(element.Value == Player1.EquippedTool || element.Value == Player1.EquippedArmor)
                {
                    itemEquippedSprite.Position = sprite.Position;
                    drawer.DrawSprite(itemEquippedSprite);
                }
                if(Player1.combinationSelection.Contains(element.Value))
                {
                    itemCombinationSprite.Position = sprite.Position;
                    drawer.DrawSprite(itemCombinationSprite);
                }

                count++;
            }

            //dibujo barra de estado
            drawer.DrawSprite(StatusBar);

            drawer.EndDrawSprite();

            //dibujo textos de estado
            foreach(KeyValuePair<StatusBarElements, TgcText2D> text in statusBarContent)
            {
                text.Value.render();
            }
        }

        public void update()
        {
            updateStatusBarContent(Player1);
            dayTime.Text = "day: " + gameModelInstance.Day + ", time: " + gameModelInstance.Hour + ":" + ((gameModelInstance.Minute.ToString().Length == 1) ? "0" : "") + gameModelInstance.Minute + ":" + ((gameModelInstance.Seconds.ToString().Length == 1) ? "0" : "") + gameModelInstance.Seconds + "(" + gameModelInstance.Cycle + ")";
        }

        /// <summary>
        ///     actualiza los textos de la barra de estado
        /// </summary>
        /// <param name="player"></param>
        private void updateStatusBarContent(Player player)
        {
            this.statusBarContent[StatusBarElements.Player].Text = "jugador: " + player.Name;
            this.statusBarContent[StatusBarElements.LifePoints].Text = "vida: " + player.LifePoints;
            this.statusBarContent[StatusBarElements.Weather].Text = "clima: " + player.Weather;
            this.statusBarContent[StatusBarElements.Hunger].Text = "hambre: " + player.Hunger;
            this.statusBarContent[StatusBarElements.Thirst].Text = "sed: " + player.Thirst;
        }
    }
}
