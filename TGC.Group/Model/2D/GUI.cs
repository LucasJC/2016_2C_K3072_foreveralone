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
        public enum StatusBarElements { LifePoints, Weather, Hunger, Thirst,
            Player
        }
        private Drawer2D drawer;
        private List<CustomSprite> menuObjects;
        private Dictionary<StatusBarElements, TgcText2D> statusBarContent;
        private D3DDevice Device;

        static GUI() {
            InventorySprites = new Dictionary<InventoryObject.ObjectTypes, CustomSprite>();
        }

        public GUI(String mediaDir, D3DDevice device)
        {

            Device = device;

            drawer = new Drawer2D();
            menuObjects = new List<CustomSprite>();
            statusBarContent = new Dictionary<StatusBarElements, TgcText2D>();

            //creo sprites para cada tipo de objeto

            int count = 1;

            Vector2 scaling = new Vector2(0.35f, 0.35f);

            foreach (InventoryObject.ObjectTypes type in Enum.GetValues(typeof(InventoryObject.ObjectTypes)))
            {
                CustomSprite sprite = null;
                sprite = new CustomSprite();
                sprite.Bitmap = new CustomBitmap(mediaDir + "2d\\Inventory\\icon-" + type.ToString() + ".png", Device.Device);
                sprite.Scaling = scaling;

                float height = (sprite.Bitmap.Size.Height * sprite.Scaling.Y);
                float width = (((sprite.Bitmap.Size.Width * sprite.Scaling.Y)) * count) * 1.1f;
                sprite.Position = new Vector2(FastMath.Max(0 + width, 0), FastMath.Max(Device.Height - height, 0));

                //agrego objetos a la lista
                menuObjects.Add(sprite);
                count++;
            }

            //armo status bar
            CustomSprite statusBar = null;
            statusBar = new CustomSprite();
            statusBar.Bitmap = new CustomBitmap(mediaDir + "2d\\statusBar.png", Device.Device);
            statusBar.Scaling = scaling; 

            float barHeight = (statusBar.Bitmap.Size.Height * statusBar.Scaling.Y);
            float barWidth = (statusBar.Bitmap.Size.Width * statusBar.Scaling.Y);
            statusBar.Position = new Vector2(FastMath.Max(Device.Width - barWidth, 0), FastMath.Max(Device.Height - barHeight, 0));

            //agrego objetos a la lista
            menuObjects.Add(statusBar);

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
            foreach (CustomSprite sprite in menuObjects)
            {
                sprite.Dispose();
            }

            foreach (KeyValuePair<StatusBarElements, TgcText2D> text in statusBarContent)
            {
                text.Value.Dispose();
            }
        }

        public void render()
        {
            drawer.BeginDrawSprite();

            foreach(CustomSprite sprite in menuObjects)
            {
                drawer.DrawSprite(sprite);
            }

            drawer.EndDrawSprite();

            foreach(KeyValuePair<StatusBarElements, TgcText2D> text in statusBarContent)
            {
                text.Value.render();
            }
        }

        public void update(Player player)
        {
            updateStatusBarContent(player);
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

        public void update()
        {
            throw new NotImplementedException();
        }
    }
}
