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
        public static Dictionary<World.Weather, CustomSprite> WeatherSprites { get; set; }
        public enum StatusBarElements { LifePoints, Weather, Hunger, Thirst, Player }
        private int usedSpace;
        private TgcText2D dayTime;
        private Drawer2D drawer;
        private Dictionary<StatusBarElements, TgcText2D> statusBarContent;
        private CustomSprite itemSelectionSprite;
        private CustomSprite itemCombinationSprite;
        private CustomSprite itemEquippedSprite;
        private CustomSprite gameOverSprite;

        //status sprites
        private CustomSprite hpBarSprite;
        private CustomSprite hpBarFrameSprite;
        private CustomSprite hungerBarSprite;
        private CustomSprite hungerBarFrameSprite;
        private CustomSprite thirstBarSprite;
        private CustomSprite thirstBarFrameSprite;
        private CustomSprite staminaBarSprite;
        private CustomSprite staminaBarFrameSprite;

        private bool GameOver = false;
        private Player Player1;
        private GameModel gameModelInstance;
        private D3DDevice Device;
        private String gameOverSpriteLocation;
       
        public static int DefaultFontSize { get; set; } = 20;


        static GUI() {
            InventorySprites = new Dictionary<InventoryObject.ObjectTypes, CustomSprite>();
            WeatherSprites = new Dictionary<World.Weather, CustomSprite>();
        }

        public GUI(String mediaDir, D3DDevice device, Player player, GameModel gameModel)
        {
            Device = device;
            Player1 = player;
            gameModelInstance = gameModel;

            gameOverSpriteLocation = mediaDir + "2d\\gui\\gameover.png";

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

            //creo sprites para cada tipo de clima
            foreach (World.Weather weather in Enum.GetValues(typeof(World.Weather)))
            {
                CustomSprite sprite = null;
                sprite = new CustomSprite();
                sprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\weather_" + weather.ToString() + ".png", Device.Device);
                sprite.Scaling = scaling;

                //agrego objetos a la lista
                WeatherSprites.Add(weather, sprite);
            }

            //fecha y hora
            //contabilizo texto fps
            usedSpace += (int)(DefaultFontSize * 1.1f);
            dayTime = GameUtils.createText("", 0, usedSpace);

            //barras de status
            hpBarSprite = new CustomSprite();
            hpBarSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\hp-bar.png", Device.Device);
            hpBarSprite.Position = new Vector2(Device.Width - (hpBarSprite.Bitmap.Width * hpBarSprite.Scaling.X), Device.Height - (hpBarSprite.Bitmap.Height * 1.05f));

            hpBarFrameSprite = new CustomSprite();
            hpBarFrameSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\frame-hp-bar.png", Device.Device);
            hpBarFrameSprite.Position = new Vector2(Device.Width - (hpBarSprite.Bitmap.Width * hpBarSprite.Scaling.X), Device.Height - (hpBarSprite.Bitmap.Height * 1.05f));

            hungerBarSprite = new CustomSprite();
            hungerBarSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\hunger-bar.png", Device.Device);
            hungerBarSprite.Position = new Vector2(Device.Width - (hungerBarSprite.Bitmap.Width * hungerBarSprite.Scaling.X), hpBarSprite.Position.Y - (hungerBarSprite.Bitmap.Height * 1.05f));

            hungerBarFrameSprite = new CustomSprite();
            hungerBarFrameSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\frame-hunger-bar.png", Device.Device);
            hungerBarFrameSprite.Position = new Vector2(Device.Width - (hungerBarSprite.Bitmap.Width * hungerBarSprite.Scaling.X), hpBarSprite.Position.Y - (hungerBarSprite.Bitmap.Height * 1.05f));

            thirstBarSprite = new CustomSprite();
            thirstBarSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\thirst-bar.png", Device.Device);
            thirstBarSprite.Position = new Vector2(Device.Width - (thirstBarSprite.Bitmap.Width * thirstBarSprite.Scaling.X), hungerBarSprite.Position.Y - (thirstBarSprite.Bitmap.Height * 1.05f));

            thirstBarFrameSprite = new CustomSprite();
            thirstBarFrameSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\frame-thirst-bar.png", Device.Device);
            thirstBarFrameSprite.Position = new Vector2(Device.Width - (thirstBarSprite.Bitmap.Width * thirstBarSprite.Scaling.X), hungerBarSprite.Position.Y - (thirstBarSprite.Bitmap.Height * 1.05f));

            staminaBarSprite = new CustomSprite();
            staminaBarSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\stamina-bar.png", Device.Device);
            staminaBarSprite.Position = new Vector2(Device.Width - (staminaBarSprite.Bitmap.Width * staminaBarSprite.Scaling.X), thirstBarSprite.Position.Y - (staminaBarSprite.Bitmap.Height * 1.05f));

            staminaBarFrameSprite = new CustomSprite();
            staminaBarFrameSprite.Bitmap = new CustomBitmap(mediaDir + "2d\\gui\\frame-stamina-bar.png", Device.Device);
            staminaBarFrameSprite.Position = new Vector2(Device.Width - (staminaBarSprite.Bitmap.Width * staminaBarSprite.Scaling.X), thirstBarSprite.Position.Y - (staminaBarSprite.Bitmap.Height * 1.05f));

            foreach (World.Weather weather in Enum.GetValues(typeof(World.Weather)))
            {
                CustomSprite sprite = WeatherSprites[weather];
                sprite.Position = new Vector2(Device.Width - sprite.Bitmap.Width / 2.5f, staminaBarSprite.Position.Y - (sprite.Bitmap.Height));
            }

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

            //dibujo barras de estado
            drawer.DrawSprite(hpBarSprite);
            drawer.DrawSprite(hpBarFrameSprite);
            drawer.DrawSprite(staminaBarSprite);
            drawer.DrawSprite(staminaBarFrameSprite);
            drawer.DrawSprite(thirstBarSprite);
            drawer.DrawSprite(thirstBarFrameSprite);
            drawer.DrawSprite(hungerBarSprite);
            drawer.DrawSprite(hungerBarFrameSprite);
            drawer.DrawSprite(WeatherSprites[Player1.Weather]);

            if (GameOver)
            {
                drawer.DrawSprite(gameOverSprite);
            }

            drawer.EndDrawSprite();

            //dibujo textos de estado
            foreach(KeyValuePair<StatusBarElements, TgcText2D> text in statusBarContent)
            {
                text.Value.render();
            }
        }

        /// <summary>
        ///     método que muestra el sprite de game over
        /// </summary>
        internal void gameOver()
        {
            gameOverSprite = new CustomSprite();
            gameOverSprite.Bitmap = new CustomBitmap(gameOverSpriteLocation, Device.Device);
            gameOverSprite.Position = new Vector2(FastMath.Max((Device.Width - gameOverSprite.Bitmap.Width) / 2, 0), FastMath.Max((Device.Height -gameOverSprite.Bitmap.Height) / 2, 0));
            GameOver = true;
        }

        public void update()
        {
            dayTime.Text = "day: " + gameModelInstance.Day + ", time: " + gameModelInstance.Hour + ":" + ((gameModelInstance.Minute.ToString().Length == 1) ? "0" : "") + gameModelInstance.Minute + ":" + ((gameModelInstance.Seconds.ToString().Length == 1) ? "0" : "") + gameModelInstance.Seconds + "(" + gameModelInstance.Cycle + ")";

            //actualiz barras de status
            hpBarSprite.Scaling = new Vector2(((float)Player1.LifePoints) / 100f, 1);
            hungerBarSprite.Scaling = new Vector2(((float)Player1.Hunger) / 100f, 1);
            thirstBarSprite.Scaling = new Vector2(((float)Player1.Thirst) / 100f, 1);
            staminaBarSprite.Scaling = new Vector2(((float)Player1.Stamina) / 100f, 1);
        }
    }
}
