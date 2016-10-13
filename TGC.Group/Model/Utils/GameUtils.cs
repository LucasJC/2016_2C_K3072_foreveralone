using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
