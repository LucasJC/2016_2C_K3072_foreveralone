using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Clase encargada de la reproducción de sonidos
    /// </summary>
    class SoundPlayer
    {
        //TODO mover a donde corresponda
        public enum EnvironmentConditions { Rain, Wind};
        public enum Actions { TreeFall, Drink, OpenChest, Hurt, Die, Jump, Success, Menu_Wrong, Menu_Next, Menu_Select, Menu_Discard }

        private Dictionary<InteractiveObject.Materials, TgcStaticSound> MaterialSounds;
        private Dictionary<EnvironmentConditions, TgcStaticSound> EnvironmentSounds;
        private Dictionary<Actions, TgcStaticSound> ActionSounds;

        private String SoundsPath;

        private TgcMp3Player mp3Player;

        /// <summary>
        ///     constructor principal
        /// </summary>
        /// <param name="directSound"></param>
        /// <param name="mediaDir"></param>
        public SoundPlayer(TgcDirectSound directSound, String mediaDir)
        {
            mp3Player = new TgcMp3Player();

            SoundsPath = mediaDir + "Sounds\\";
            //inicializo sonidos

            //materiales
            TgcStaticSound glassSound = new TgcStaticSound();
            glassSound.loadSound(SoundsPath + "Materials\\glass.wav", directSound.DsDevice);
            TgcStaticSound metalSound = new TgcStaticSound();
            metalSound.loadSound(SoundsPath + "Materials\\metal.wav", directSound.DsDevice);
            //TODO buscar un sonido de planta, por ahora uso default
            TgcStaticSound plantSound = new TgcStaticSound();
            plantSound.loadSound(SoundsPath + "Materials\\default.wav", directSound.DsDevice);
            TgcStaticSound woodSound = new TgcStaticSound();
            woodSound.loadSound(SoundsPath + "Materials\\wood.wav", directSound.DsDevice);
            TgcStaticSound noneSound = new TgcStaticSound();
            noneSound.loadSound(SoundsPath + "Materials\\default.wav", directSound.DsDevice);
            TgcStaticSound rockSound = new TgcStaticSound();
            rockSound.loadSound(SoundsPath + "Materials\\rock.wav", directSound.DsDevice);

            MaterialSounds = new Dictionary<InteractiveObject.Materials, TgcStaticSound>();
            MaterialSounds.Add(InteractiveObject.Materials.Wood, woodSound);
            MaterialSounds.Add(InteractiveObject.Materials.Glass, glassSound);
            MaterialSounds.Add(InteractiveObject.Materials.Metal, metalSound);
            MaterialSounds.Add(InteractiveObject.Materials.Plant, noneSound);
            MaterialSounds.Add(InteractiveObject.Materials.None, noneSound);
            MaterialSounds.Add(InteractiveObject.Materials.Rock, rockSound);

            //acciones
            TgcStaticSound drinkSound = new TgcStaticSound();
            drinkSound.loadSound(SoundsPath + "Actions\\drink.wav", directSound.DsDevice);
            TgcStaticSound treeFallSound = new TgcStaticSound();
            treeFallSound.loadSound(SoundsPath + "Actions\\tree_fall.wav", directSound.DsDevice);
            TgcStaticSound chestOpenSound = new TgcStaticSound();
            chestOpenSound.loadSound(SoundsPath + "Actions\\chest_open.wav", directSound.DsDevice);
            TgcStaticSound successSound = new TgcStaticSound();
            successSound.loadSound(SoundsPath + "Actions\\success.wav", directSound.DsDevice);
            TgcStaticSound hurtSound = new TgcStaticSound();
            hurtSound.loadSound(SoundsPath + "Actions\\hurt.wav", directSound.DsDevice);
            TgcStaticSound dieSound = new TgcStaticSound();
            dieSound.loadSound(SoundsPath + "Actions\\die.wav", directSound.DsDevice);
            TgcStaticSound jumpSound = new TgcStaticSound();
            jumpSound.loadSound(SoundsPath + "Actions\\jump.wav", directSound.DsDevice);
            //menu
            TgcStaticSound menuWrongSound = new TgcStaticSound();
            menuWrongSound.loadSound(SoundsPath + "Actions\\menu_wrong.wav", directSound.DsDevice);
            TgcStaticSound menuNextSound = new TgcStaticSound();
            menuNextSound.loadSound(SoundsPath + "Actions\\menu_next.wav", directSound.DsDevice);
            TgcStaticSound menuSelectSound = new TgcStaticSound();
            menuSelectSound.loadSound(SoundsPath + "Actions\\menu_select.wav", directSound.DsDevice);
            TgcStaticSound menuDiscardSound = new TgcStaticSound();
            menuDiscardSound.loadSound(SoundsPath + "Actions\\menu_discard.wav", directSound.DsDevice);

            ActionSounds = new Dictionary<Actions, TgcStaticSound>();
            ActionSounds.Add(Actions.Drink, drinkSound);
            ActionSounds.Add(Actions.TreeFall, treeFallSound);
            ActionSounds.Add(Actions.OpenChest, chestOpenSound);
            ActionSounds.Add(Actions.Success, successSound);
            ActionSounds.Add(Actions.Hurt, hurtSound);
            ActionSounds.Add(Actions.Die, dieSound);
            ActionSounds.Add(Actions.Jump, jumpSound);
            ActionSounds.Add(Actions.Menu_Discard, menuDiscardSound);
            ActionSounds.Add(Actions.Menu_Wrong, menuWrongSound);
            ActionSounds.Add(Actions.Menu_Next, menuNextSound);
            ActionSounds.Add(Actions.Menu_Select, menuSelectSound);

            //environment
            TgcStaticSound rainSound = new TgcStaticSound();
            rainSound.loadSound(SoundsPath + "Environment\\rain.wav", directSound.DsDevice);
            TgcStaticSound windSound = new TgcStaticSound();
            windSound.loadSound(SoundsPath + "Environment\\wind.wav", directSound.DsDevice);

            EnvironmentSounds = new Dictionary<EnvironmentConditions, TgcStaticSound>();
            EnvironmentSounds.Add(EnvironmentConditions.Rain, rainSound);
            EnvironmentSounds.Add(EnvironmentConditions.Wind, windSound);

            mp3Player.FileName = SoundsPath + "Environment\\ambient.mp3";
        }

        /// <summary>
        ///     inicia reproducción de sonido ambiente en loop
        /// </summary>
        public void startAmbient()
        {
            mp3Player.play(true);
        }

        /// <summary>
        ///     frena reproducción de sonido ambiente
        /// </summary>
        public void stopAmbient()
        {
            mp3Player.stop();
        }

        /// <summary>
        ///     reproduce un sonido para un material en particular
        /// </summary>
        /// <param name="material"></param>
        public void playMaterialSound(InteractiveObject.Materials material)
        {
            try
            {
                this.MaterialSounds[material].play(false);
            }catch(Exception e)
            {
                Console.WriteLine("Error reproduciendo sonido para material " + material.ToString());
                Console.WriteLine(e.ToString());
            }
            
        }

        /// <summary>
        ///     reproduce un sonido para una acción en particular
        /// </summary>
        /// <param name="action"></param>
        public void playActionSound(Actions action)
        {
            try
            {
                this.ActionSounds[action].play(false);
            }catch(Exception e)
            {
                Console.WriteLine("Error reproduciendo sonido para la acción " + action.ToString());
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     comienza a reproducir un sonido de environment infinitamente
        /// </summary>
        /// <param name="condition"></param>
        public void startEnvironmentSound(EnvironmentConditions condition)
        {
            try
            {
                this.EnvironmentSounds[condition].play(true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reproduciendo sonido ambiente " + condition.ToString());
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     detiene un sonido de environment
        /// </summary>
        /// <param name="condition"></param>
        public void stopEnvironmentSound(EnvironmentConditions condition)
        {
            try
            {
                this.EnvironmentSounds[condition].stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error deteniendo sonido ambiente " + condition.ToString());
                Console.WriteLine(e.ToString());
            }
        }
    }
}
