using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Text;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Clase utilitaria
    /// </summary>
    public class GameUtils
    {
        //Random
        public static Random Rand { get; } = new Random(GameModel.RandomSeed);

        /// <summary>
        ///     Genera un vector de posición aleatoriamente para la configuración actual de cada cuadrante del mapa
        /// </summary>
        /// <returns></returns>
        public static Vector3 getRandomPositionVector()
        {
            return new Vector3(Rand.Next(0, GameModel.MapLength) - GameModel.MapLength / 2, 0, Rand.Next(0, GameModel.MapLength) - GameModel.MapLength / 2);
        }

        /// <summary>
        ///     Genera un vector de escala de manera aleatoria.
        ///     Las coordenadas son todas iguales y van de 0.3f a 1f
        /// </summary>
        /// <returns></returns>
        public static Vector3 getRandomScaleVector()
        {
            float scale = (float)Rand.NextDouble();
            if (scale < 0.3f) scale = 0.3f;
            return new Vector3(scale, scale, scale);
        }

        /// <summary>
        ///     crea un texto con el valor indicado y la configuración por defecto
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TgcText2D createText(String value)
        {
            TgcText2D text = new TgcText2D();
            text.changeFont(new Font(FontFamily.GenericMonospace, GUI.DefaultFontSize, FontStyle.Regular));
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
        public static TgcText2D createText(String value, float x, float y)
        {
            TgcText2D text = createText(value);
            text.Position = new Point((int)x, (int)y);
            return text;
        }

        /// <summary>
        ///     crea un texto con el valor indicado, la configuración por defecto  y la posición indicada
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TgcText2D createText(String value, float x, float y, int fontSize, bool bold)
        {
            TgcText2D text = createText(value, x, y);
            text.changeFont(new Font(FontFamily.GenericMonospace, fontSize, (bold? FontStyle.Bold : FontStyle.Regular)));
            return text;
        }
    }
}
