﻿using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
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
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
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

        //reproductor de sonidos
        private SoundPlayer soundPlayer;

        //shaders
        private Microsoft.DirectX.Direct3D.Effect lightEffect;

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
            //genero el mundo
            MyWorld = new World(MediaDir, d3dDevice, loader, Camara, MapLength);

            //MyWorld.lightEffect = lightEffect;
            //MyWorld.updateEffects();

            //colisiones
            pickingRay = new TgcPickingRay(Input);
            //sonidos
            soundPlayer = new SoundPlayer(DirectSound, MediaDir);

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
                        }
                        Message.Text = "Objeto: " + collidedObject.name + " (" + collidedObject.lifePoints + ")";
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
                            }
                            Message.Text = "Objeto: " + collidedObject.name + " (" + collidedObject.lifePoints + ")";
                            break;
                        }
                    }
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

            time += ElapsedTime;

            //Variables para el shader
            lightEffect.SetValue("time", time);

            //Dibuja un texto por pantalla
            DrawText.drawText("W, A, S, D ←, →, CLICK", 0, 20, Color.DarkSalmon);
            DrawText.drawText("Camera: " + Camara.Position.X + "," + Camara.Position.Y + "," + Camara.Position.Z, 0, 40, Color.DarkOrange);

            Message.render();
            MyWorld.render();
            
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
        }
    }
}