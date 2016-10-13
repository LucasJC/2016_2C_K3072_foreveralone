using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Direct3D;
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
        public enum StatusBarElements { LifePoints, Weather, Hunger, Thirst, Player }
        private Drawer2D drawer;
        private Dictionary<StatusBarElements, TgcText2D> statusBarContent;
        private CustomSprite StatusBar;
        private CustomSprite itemSelectionSprite;
        private CustomSprite itemCombinationSprite;
        private CustomSprite itemEquippedSprite;
        private Player Player1;
        private D3DDevice Device;

        static GUI() {
            InventorySprites = new Dictionary<InventoryObject.ObjectTypes, CustomSprite>();
        }

        public GUI(String mediaDir, D3DDevice device, Player player)
        {
            Device = device;
            Player1 = player;

            drawer = new Drawer2D();
            statusBarContent = new Dictionary<StatusBarElements, TgcText2D>();

            //armo sprite de selección
            Vector2 scaling = new Vector2(0.35f, 0.35f);

            itemSelectionSprite = null;
            itemSelectionSprite = new CustomSprite();
            itemSelectionSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\Inventory\\icon-selection.png", Device.Device);
            itemSelectionSprite.Scaling = scaling;

            itemCombinationSprite = null;
            itemCombinationSprite = new CustomSprite();
            itemCombinationSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\Inventory\\icon-selection-combination.png", Device.Device);
            itemCombinationSprite.Scaling = scaling;

            itemEquippedSprite = null;
            itemEquippedSprite = new CustomSprite();
            itemEquippedSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\Inventory\\icon-equipped.png", Device.Device);
            itemEquippedSprite.Scaling = scaling;

            //creo sprites para cada tipo de objeto
            foreach (InventoryObject.ObjectTypes type in Enum.GetValues(typeof(InventoryObject.ObjectTypes)))
            {
                CustomSprite sprite = null;
                sprite = new CustomSprite();
                sprite.Bitmap = new CustomBitmap(mediaDir + "2d\\Inventory\\icon-" + type.ToString() + ".png", Device.Device);
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
            float textHeight = (createText("ejemplo").Size.Height / (barHeight / 5));
            float textWidth = (Device.Width - (barWidth * 0.9f)) ;
            statusBarContent.Add(StatusBarElements.Player, createText("jugador: ", textWidth, Device.Height - (textHeight * 5) - barHeight * 0.1f));
            statusBarContent.Add(StatusBarElements.LifePoints, createText("vida: ", textWidth, Device.Height - (textHeight * 4) - barHeight * 0.1f));
            statusBarContent.Add(StatusBarElements.Weather, createText("clima: ", textWidth, Device.Height - (textHeight * 3) - barHeight * 0.1f));
            statusBarContent.Add(StatusBarElements.Hunger, createText("hambre: ", textWidth, Device.Height - (textHeight * 2) - barHeight * 0.1f));
            statusBarContent.Add(StatusBarElements.Thirst, createText("sed: ", textWidth, Device.Height - (textHeight * 1) - barHeight * 0.1f));
        }

        /// <summary>
        ///     crea un texto con el valor indicado y la configuración por defecto
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private TgcText2D createText(String value)
        {
            TgcText2D text = new TgcText2D();
            text.changeFont(new Font(FontFamily.GenericMonospace, 12, FontStyle.Regular));
            text.Color = Color.DarkGray;
            text.Align = TgcText2D.TextAlign.LEFT;
            text.Text = value;
            return text;
        }

        /// <summary>
        ///     crea un texto con el valor indicado, la configuración por defecto  y la posición indicada
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private TgcText2D createText(String value, float x, float y)
        {
            TgcText2D text = createText(value);
            text.Position = new Point((int)x, (int)y);
            return text;
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
                if(element.Value == Player1.EquippedObject)
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
