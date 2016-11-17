using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Particle;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Terrain;
using TGC.Core.Text;
using TGC.Core.Textures;
using TGC.Core.Utils;
using TGC.Examples.Camara;
using TGC.Group.Model.Effects;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Modelo Principal del Juego
    /// </summary>
    public class GameModel : TgcExample
    {
        //tiempo
        private float time = 0;

        public enum DayCycle { DAY, NIGHT };

        //hora del día para controlar ciclos de día y noche
        public int Day { get; set; } = 0;
        public int Hour { get; set; } = 0;
        public int Minute { get; set; } = 0;
        public int Seconds { get; set; } = 0;
        public DayCycle Cycle { get; set; }
        private float secondsAuxCounter = 0;

        //Directx device
        public Microsoft.DirectX.Direct3D.Device d3dDevice;
        //Loader del framework
        private TgcSceneLoader loader;

        //mundo
        private World MyWorld;

        //jugador
        public Player Player1 { get; set; }

        //cámara
        public TgcFpsCamera MyCamera;

        //manager de efectos
        public EffectsManager effectsManager { get; set; }

        //reproductor de sonidos
        private SoundPlayer soundPlayer;

        //gui
        private GUI MenuInterface;

        //emisor de partículas
        private ParticleEmitter emitter;
        private float emissionTime = 1f;
        private float emittedTime = 0f;
        private bool emit = false;

        //para colisiones
        private TgcPickingRay pickingRay;
        private bool collided = false;
        private Vector3 collisionPoint;
        private InteractiveObject pickedObject = null;
        private InteractiveObject collidedObject = null;

        //mensajes
        private TgcText2D TopRightText;
        private TgcText2D CenterText;
        private float timerCenterText = 0;
        private float timerTopRightText = 0;
        //Semilla para randoms
        public static int RandomSeed { get; } = 666;
        //Dimensiones de cada cuadrante del mapa
        public static int MapLength { get; } = 7000;

        private bool gameOver = false;

        private static Vector3 zeroVector = new Vector3(0f, 0f, 0f);

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            d3dDevice = D3DDevice.Instance.Device;
            
            //Instancio el loader del framework
            loader = new TgcSceneLoader();

            //creo usuario
            Player1 = new Player();
            Player1.gameModel = this;
            //genero el mundo
            MyWorld = new World(MediaDir, d3dDevice, loader, Camara, Frustum, MapLength, true);

            //inicializo efectos
            effectsManager = new EffectsManager(ShadersDir, this, MyWorld, ElapsedTime);
            //inicializo mesh para hacha
            TgcMesh axe = loader.loadSceneFromFile(MediaDir + "Meshes\\Hacha\\Hacha-TgcScene.xml").Meshes[0];
            axe.Scale = new Vector3(0.1f, 0.1f, 0.1f);

            //Inicializo cámara
            MyCamera = new TgcFpsCamera(Player1, axe, Input, (MapLength / 2), -(MapLength / 2), (MapLength / 2), -(MapLength / 2));
            Camara = MyCamera;

            Frustum.updateVolume(D3DDevice.Instance.Device.Transform.View, D3DDevice.Instance.Device.Transform.Projection);

            //emisor de partículas
            emitter = new ParticleEmitter(MediaDir + "Textures\\smokeParticle.png", 10);
            emitter.Position = new Vector3(0, 0, 0);
            emitter.MinSizeParticle = 2f;
            emitter.MaxSizeParticle = 5f;
            emitter.ParticleTimeToLive = 1f;
            emitter.CreationFrecuency = 1f;
            emitter.Dispersion = 25;
            emitter.Speed = new Vector3(5f, 5f, 5f);

            //colisiones
            pickingRay = new TgcPickingRay(Input);
            //sonidos
            soundPlayer = new SoundPlayer(DirectSound, MediaDir);
            soundPlayer.startAmbient();
            //gui
            MenuInterface = new GUI(MediaDir, D3DDevice.Instance, Player1, this);

            TopRightText = GameUtils.createText("", 0, 0, 20, true);
            TopRightText.Color = Color.LightGray;
            TopRightText.Align = TgcText2D.TextAlign.RIGHT;

            CenterText = GameUtils.createText("", 0, (D3DDevice.Instance.Height * 0.85f), 25, true);
            CenterText.Color = Color.DodgerBlue;
            CenterText.Align = TgcText2D.TextAlign.CENTER;
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            effectsManager.update();

            //si el jugador fue herido entonces reproduzco sonido de dolor
            if(Player1.Hurt)
            {
                soundPlayer.playActionSound(SoundPlayer.Actions.Hurt);
                Player1.Hurt = false;
            }

            MyWorld.update();
            MenuInterface.update();

            MyWorld.SkyBox.Center = new Vector3(Camara.Position.X, MyWorld.SkyBox.Center.Y, Camara.Position.Z);

            if (Player1.Alive)
            {
                //controlo tiempo
                updateDayTime(ElapsedTime);

                checkTimeEvents();

                checkCurrentTexts();

                //reinicio estado de colisiones
                collided = false;
                pickedObject = null;

                //determino acciones en base al input
                detectUserInput();
                //resuelvo colisiones
                testCollisions();
                //controlo tiempos de emisión de partículas
                if (emit)
                {
                    if (emittedTime <= emissionTime)
                    {
                        emittedTime += ElapsedTime;
                    }
                    else
                    {
                        emit = false;
                        emitter.Position = zeroVector;
                    }
                }
            }
            else
            {
                //muerto :(
                if (!gameOver)
                {
                    soundPlayer.playActionSound(SoundPlayer.Actions.Die);
                    MyCamera.gameOver();
                    MenuInterface.gameOver();
                }
                this.gameOver = true;
            }
        }

        /// <summary>
        ///     método que detecta el input del usuario y realiza acciones en base a eso
        /// </summary>
        private void detectUserInput()
        {
            if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                pickingRay.updateRay();
                testPicking();
            }
            else if (Input.keyPressed(Key.LeftArrow))
            {
                Player1.selectPreviousItem();
                soundPlayer.playActionSound(SoundPlayer.Actions.Menu_Next);
            }
            else if (Input.keyPressed(Key.Space))
            {
                soundPlayer.playActionSound(SoundPlayer.Actions.Jump);
            }
            else if (Input.keyPressed(Key.RightArrow))
            {
                Player1.selectNextItem();
                soundPlayer.playActionSound(SoundPlayer.Actions.Menu_Next);
            }
            else if (Input.keyPressed(Key.E))
            {
                if(Player1.SelectedItem.isEquippable())
                { 
                    soundPlayer.playActionSound(SoundPlayer.Actions.Menu_Select);
                    Player1.equipSelectedItem();
                }
                else
                {
                    //intento consumir el item ya que no era equipable
                    if (Player1.consumeItem())
                    {
                        soundPlayer.playActionSound(SoundPlayer.Actions.Drink);
                    }else
                    {
                        soundPlayer.playActionSound(SoundPlayer.Actions.Menu_Wrong);
                    }
                }
      
            }
            else if (Input.keyPressed(Key.Q))
            {
                Player1.removeInventoryObject(Player1.SelectedItem);
                soundPlayer.playActionSound(SoundPlayer.Actions.Menu_Discard);
            }
            else if (Input.keyPressed(Key.Z))
            {
                Player1.selectForCombination(Player1.SelectedItem);
                soundPlayer.playActionSound(SoundPlayer.Actions.Menu_Select);
            }
            else if (Input.keyPressed(Key.C))
            {
                if (!InventoryObject.combineObjects(Player1, Player1.combinationSelection))
                {
                    //falló la combinación
                    soundPlayer.playActionSound(SoundPlayer.Actions.Menu_Wrong);
                }
                else
                {
                    //comb ok
                    soundPlayer.playActionSound(SoundPlayer.Actions.Success);
                }
            }
        }

        /// <summary>
        ///     testea colisiones AABB
        /// </summary>
        private void testCollisions()
        {
            if (Player1.Moving)
            {
                foreach (InteractiveObject objeto in MyWorld.Objetos)
                {
                    if (objeto.mesh.Enabled && objeto.Solid)
                    {
                        MyCamera.CameraBox.Position = Camara.Position;
                        TgcBox cameraBox = MyCamera.CameraBox;

                        if (TgcCollisionUtils.testAABBAABB(cameraBox.BoundingBox, objeto.mesh.BoundingBox))
                        {
                            collidedObject = objeto;
                            MyCamera.Collisioned = true;
                            break;
                        }else
                        {
                            MyCamera.Collisioned = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     testea colisiones de picking
        /// </summary>
        private void testPicking()
        {
            //de los objetos visibles, testeo colisiones con el picking ray
            foreach (InteractiveObject objeto in MyWorld.Objetos)
            {
                if(objeto.mesh.Enabled)
                {
                    collided = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, objeto.mesh.BoundingBox, out collisionPoint);
                    if (collided)
                    {
                        Vector3 aux = new Vector3(0f, 0f, 0f);
                        aux.Add(Camara.Position);
                        aux.Subtract(objeto.mesh.Position);
                        if (FastMath.Ceiling(aux.Length()) < 65)
                        {
                            pickedObject = objeto;
                            if (pickedObject.getHit(Player1.getDamage()))
                            {
                                setCenterText(Player1.getDamage().ToString() + " Damage");
                                MyWorld.destroyObject(pickedObject);

                                if (pickedObject.Equals(collidedObject)) MyCamera.Collisioned = false;

                                List<InventoryObject> drops = pickedObject.getDrops();
                                foreach (InventoryObject invObject in drops)
                                {
                                    //agrego los drops al inventario del usuario
                                    if (!Player1.addInventoryObject(invObject))
                                    {
                                        //no pudo agregar el objeto
                                        setTopRightText("No hay espacio en el inventario");
                                    }
                                }
                            }
                            break;
                        }
                        else
                        {
                            collided = false;
                        }
                    }
                }
            }

            //si hubo colisión
            if (collided)
            {
                //a darle átomos
                emit = true;
                emittedTime = 0;
                emitter.Position = pickedObject.mesh.Position;
            }
        }

        /// <summary>
        ///     actualiza la fecha actual
        /// </summary>
        /// <param name="elapsedTime"></param>
        private void updateDayTime(float elapsedTime)
        {
            secondsAuxCounter += (elapsedTime) * 1000;

            Seconds += (int)secondsAuxCounter;
            secondsAuxCounter = 0;

            if (Seconds < 0) Seconds = 0;

            if (Seconds >= 60)
            {
                Seconds = 0;

                Minute++;

                if (Minute >= 60)
                {
                    Minute = 0;

                    Hour++;

                    if (Hour == 24)
                    {
                        Hour = 0;
                        Day++;
                    }

                    if(Hour >= 19)
                    {
                        Cycle = GameModel.DayCycle.NIGHT;
                    }else
                    {
                       Cycle = GameModel.DayCycle.DAY;
                    }
                }
            }
        }

        /// <summary>
        ///     Método que verifica los eventos temporales como cambio de clima, etc
        /// </summary>
        private void checkTimeEvents()
        {

            if (this.Seconds > 0 && this.Seconds % 2 == 0)
            {
                MyWorld.changeWind();
            }
                
            if (this.Seconds == 0 && this.Minute % 30 == 00)
            {
                //cada 30 min el jugador sufre efectos del clima
                if (Player1.Hunger <= 0) Player1.beHit(2);
                Player1.sufferWeather(MyWorld.CurrentWeather);
            }

            if (this.Seconds == 0 && this.Minute == 0 && this.Hour % 4 == 0)
            {
                //cada 4 horas cambio el clima aleatoriamente
                MyWorld.changeWeather(Player1);
            }
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();
            //habilito efecto de partículas
            D3DDevice.Instance.ParticlesEnabled = true;
            D3DDevice.Instance.EnableParticles();

            TopRightText.render();
            CenterText.render();
            MyWorld.render();
            MenuInterface.render();

            MyCamera.render();

            if(null != pickedObject)
            {
                //un objeto fue objetivo de una acción
                soundPlayer.playMaterialSound(pickedObject.material);

                if (!pickedObject.alive)
                {
                    //el objeto debe ser eliminado
                    if(pickedObject.objectType == InteractiveObject.ObjectTypes.Tree)
                    {
                        soundPlayer.playActionSound(SoundPlayer.Actions.TreeFall);
                    }
                }
            }

            //veo si emitir partículas
            if (emit)
            {
                emitter.render(emittedTime);
            }

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            soundPlayer.stopAmbient();
            MyWorld.dispose();
            emitter.dispose();
            MenuInterface.dispose();
        }

        /// <summary>
        ///     setea el texto del centro
        /// </summary>
        /// <param name="text"></param>
        public void setCenterText(String text)
        {
            timerCenterText = 0;
            this.CenterText.Text = text;
        }

        /// <summary>
        ///     setea el texto de arriba a la derecha
        /// </summary>
        /// <param name="text"></param>
        private void setTopRightText(String text)
        {
            timerTopRightText = 0;
            this.TopRightText.Text = text;
        }

        /// <summary>
        ///     chequea tiempos de muestra de los textos
        /// </summary>
        private void checkCurrentTexts()
        {
            timerCenterText += ElapsedTime;
            timerTopRightText += ElapsedTime;

            if (timerCenterText > 2)
            {
                this.CenterText.Text = "";
                timerCenterText = 0;
            }

            if (timerTopRightText > 2)
            {
                this.TopRightText.Text = "";
                timerTopRightText = 0;
            }
        }
    }
}