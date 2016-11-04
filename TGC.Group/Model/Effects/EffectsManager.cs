using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Utils;

namespace TGC.Group.Model.Effects
{
    public class EffectsManager : Renderable
    {
        private Effect effect;
        private String ShadersDir;
        private World MyWorld;
        private GameModel MyGameModel;
        private float time;
        private float ElapsedTime;
        private Dictionary<int, float> lightByHour = new Dictionary<int, float>();
        private Color skyBoxColor;
        public EffectsManager(string shadersDir, GameModel gameModel, World myWorld, float elapsedTime)
        {
            ShadersDir = shadersDir;
            MyWorld = myWorld;
            MyGameModel = gameModel;
            ElapsedTime = elapsedTime;
            
            skyBoxColor = MyWorld.SkyBox.Color;

            effect = TgcShaders.loadEffect(ShadersDir + "custom\\BasicShader.fx");

            foreach (InteractiveObject obj in MyWorld.Objetos)
            {
                obj.mesh.Effect = effect;
                obj.mesh.Technique = "RenderScene";
            }

            MyWorld.Floor.Effect = effect;
            MyWorld.Floor.Technique = "RenderScene";

            foreach (TgcMesh mesh in MyWorld.SkyBox.Faces)
            {
                mesh.Effect = effect;
                mesh.Technique = "RenderScene";
            }

            //luz por hora
            lightByHour.Add(0, 0.1f);
            lightByHour.Add(1, 0.1f);
            lightByHour.Add(2, 0.1f);
            lightByHour.Add(3, 0.1f);
            lightByHour.Add(4, 0.2f);
            lightByHour.Add(5, 0.3f);
            lightByHour.Add(6, 0.4f);
            lightByHour.Add(7, 0.6f);
            lightByHour.Add(8, 0.7f);
            lightByHour.Add(9, 0.8f);
            lightByHour.Add(10, 0.8f);
            lightByHour.Add(11, 0.9f);
            lightByHour.Add(12, 1f);
            lightByHour.Add(13, 0.95f);
            lightByHour.Add(14, 0.85f);
            lightByHour.Add(15, 0.75f);
            lightByHour.Add(16, 0.7f);
            lightByHour.Add(17, 0.65f);
            lightByHour.Add(18, 0.6f);
            lightByHour.Add(19, 0.5f);
            lightByHour.Add(20, 0.3f);
            lightByHour.Add(21, 0.2f);
            lightByHour.Add(22, 0.1f);
            lightByHour.Add(23, 0.1f);
            lightByHour.Add(24, 0.1f);
        }

        public void update()
        {
            float luz = calculateLight() + 0.1f;
            time += ElapsedTime;
            effect.SetValue("time", time);
            effect.SetValue("luz", luz);
        }

        private float calculateLight()
        {
            float result;

            float light1 = lightByHour[MyGameModel.Hour];
            float ligth2 = lightByHour[MyGameModel.Hour+1];

            float peso1 = (MyGameModel.Minute * 1 / 60);

            result = (light1 * peso1 + ligth2 * (1 - peso1)) / 2;

            return result;
        }

        public void render()
        {
            throw new NotImplementedException();
        }

        public void dispose()
        {
            effect.Dispose();
        }
    }
}
