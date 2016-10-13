using Microsoft.DirectX;
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

namespace TGC.Group.Model
{
    /// <summary>
    ///     Modelo Principal del Juego
    /// </summary>
    public class GameModel : TgcExample
    {
        //tiempo
        private float time = 0;
        //Directx device
        Microsoft.DirectX.Direct3D.Device d3dDevice;
        //Loader del framework
        private TgcSceneLoader loader;

        //vector posición de cámara
        private Vector3 cameraPosition;

        //mundo
        private World MyWorld;

        //jugador
        public Player Player1;

        //reproductor de sonidos
        private SoundPlayer soundPlayer;

        //gui
        private GUI MenuInterface;

        //shaders
        private Microsoft.DirectX.Direct3D.Effect lightEffect;

        //emisor de partículas
        private ParticleEmitter emitter;
        private float emissionTime = 1f;
        private float emittedTime = 0f;
        private bool emit = false;

        //para colisiones
        private TgcPickingRay pickingRay;
        private bool collided = false;
        private Vector3 collisionPoint;
        private InteractiveObject collidedObject = null;
        private TgcText2D Message;
        
        //Semilla para randoms
        public static int RandomSeed { get; } = 666;
        //Dimensiones de cada cuadrante del mapa
        public static int MapLength { get; } = 5000;

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
            //Shaders
            lightEffect = TgcShaders.loadEffect(ShadersDir + "CustomLightShader.fx");

            //Instancio el loader del framework
            loader = new TgcSceneLoader();
            //Inicializo cámara
            Camara = new TgcFpsCamera(Input, (MapLength / 2) , - (MapLength / 2), (MapLength / 2), -(MapLength / 2));

            Frustum.updateVolume(D3DDevice.Instance.Device.Transform.View, D3DDevice.Instance.Device.Transform.Projection);
            //genero el mundo
            MyWorld = new World(MediaDir, d3dDevice, loader, Camara, Frustum, MapLength, true);

            //creo usuario
            Player1 = new Player();

            //emisor de partículas
            emitter = new ParticleEmitter(MediaDir + "Textures\\smokeParticle.png", 10);
            emitter.Position = new Vector3(0, 0, 0);
            emitter.MinSizeParticle = 2f;
            emitter.MaxSizeParticle = 5f;
            emitter.ParticleTimeToLive = 1f;
            emitter.CreationFrecuency = 1f;
            emitter.Dispersion = 25;
            emitter.Speed = new Vector3(5f, 5f, 5f);

            //MyWorld.lightEffect = lightEffect;
            //MyWorld.updateEffects();

            //colisiones
            pickingRay = new TgcPickingRay(Input);
            //sonidos
            soundPlayer = new SoundPlayer(DirectSound, MediaDir);

            //gui
            MenuInterface = new GUI(MediaDir, D3DDevice.Instance, Player1);

            Message = new TgcText2D();
            Message.changeFont(new Font(FontFamily.GenericMonospace, 20, FontStyle.Bold));
            Message.Color = Color.Aqua;
            Message.Align = TgcText2D.TextAlign.RIGHT;

        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();
            //reinicio estado de colisiones
            collided = false;
            collidedObject = null;

            MyWorld.update();
            MenuInterface.update();

            //TODO pasar esto a un método
            if(Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                pickingRay.updateRay();

                //de todos mis objetos veo si colisiona
                //TODO acá debería traer los que están cercanos al usuario - Por ahora pruebo todos
                foreach(InteractiveObject objeto in MyWorld.Objetos)
                {
                    collided = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, objeto.mesh.BoundingBox, out collisionPoint);
                    if (collided)
                    {
                        collidedObject = objeto;
                        if (collidedObject.getHit(1))
                        {
                            MyWorld.destroyObject(collidedObject);
                            List<InventoryObject> drops = collidedObject.getDrops();
                            foreach (InventoryObject invObject in drops)
                            {
                                //agrego los drops al inventario del usuario
                                if (!Player1.addInventoryObject(invObject))
                                {
                                    //no pudo agregar el objeto
                                    Message.Text = "No hay espacio en el inventario... Objetos descartados";
                                }
                            }
                        }
                        break;
                    }    
                }
                if(!collided)
                {
                    //veo si tocó un árbol
                    foreach (InteractiveObject tree in MyWorld.Trees)
                    {
                        collided = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, tree.mesh.BoundingBox, out collisionPoint);
                        if (collided)
                        {
                            collidedObject = tree;
                            if (collidedObject.getHit(1))
                            {
                                MyWorld.destroyObject(collidedObject);
                                List<InventoryObject> drops = collidedObject.getDrops();
                                foreach(InventoryObject invObject in drops)
                                {
                                    //agrego los drops al inventario del usuario
                                    if(!Player1.addInventoryObject(invObject))
                                    {
                                        //no pudo agregar el objeto
                                        Message.Text = "No hay espacio en el inventario... Objetos descartados";
                                    }
                                }
                            }
                            break;
                        }
                    }
                }

                //si hubo colisión
                if(collided)
                {
                    //a darle átomos
                    emit = true;
                    emittedTime = 0;
                    emitter.Position = collidedObject.mesh.Position;
                }
            }

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

            time += ElapsedTime;

            //Variables para el shader
            lightEffect.SetValue("time", time);

            //Dibuja un texto por pantalla
            DrawText.drawText("W, A, S, D ←, →, CLICK", 0, 20, Color.DarkSalmon);
            DrawText.drawText("Camera: " + Camara.Position.X + "," + Camara.Position.Y + "," + Camara.Position.Z, 0, 40, Color.DarkOrange);

            Message.render();
            MyWorld.render();
            MenuInterface.render();

            if(null != collidedObject)
            {
                //un objeto fue objetivo de una acción
                soundPlayer.playMaterialSound(collidedObject.material);

                if (collidedObject.alive)
                {
                    //el objeto debe ser eliminado
                    if(collidedObject.objectType == InteractiveObject.ObjectTypes.Tree)
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
            MyWorld.dispose();
            emitter.dispose();
            MenuInterface.dispose();
        }
    }
}