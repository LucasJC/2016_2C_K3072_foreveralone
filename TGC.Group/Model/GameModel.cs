using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;

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
        //lista de árboles
        private List<TgcMesh> trees;
        //piso del mapa
        private TgcPlane floor;
        //longitud de cada lado del mapa
        private int mapLength;
        //vector posición de cámara
        private Vector3 cameraPosition;

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

        ////Caja que se muestra en el ejemplo.
        //private TgcBox Box { get; set; }

        ////Mesh de TgcLogo.
        //private TgcMesh Mesh { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //loader
            var loader = new TgcSceneLoader();
            
            //armo el piso con un plano
            var floorTexture = TgcTexture.createTexture(d3dDevice, MediaDir + "Textures\\pasto.jpg");
            mapLength = 2000;
            floor = new TgcPlane(new Vector3(-(mapLength/2), 0, -(mapLength/2)), new Vector3(mapLength, 0, mapLength), TgcPlane.Orientations.XZplane, floorTexture, 10f, 10f);

            //creo los árboles
            //loader.loadSceneFromFile(MediaDir + "Meshes\\ArbolBosque\\ArbolBosque-TgcScene.xml").Meshes[0];
            createTrees(loader.loadSceneFromFile(MediaDir + "Meshes\\Pino\\Pino-TgcScene.xml").Meshes[0], 200);

            //Posición de la camara.
            cameraPosition = new Vector3(0, 1000, 125);
            //Quiero que la camara mire hacia el origen (0,0,0).
            var lookAt = Vector3.Empty;
            //Configuro donde esta la posicion de la camara y hacia donde mira.
            Camara.SetCamera(cameraPosition, lookAt);
            //Internamente el framework construye la matriz de view con estos dos vectores.
            //Luego en nuestro juego tendremos que crear una cámara que cambie la matriz de view con variables como movimientos o animaciones de escenas.
        }

        /// <summary>
        ///     Genera la disposición de los árboles en el mapa
        ///     TODO: hacer que no se toquen...
        /// </summary>
        private void createTrees(TgcMesh tree, int cantidad)
        {
            Random rnd = new Random();

            trees = new List<TgcMesh>();

            TgcMesh instance;
            float scale;

            for( int i = 1; i <= cantidad; i++)
            {
                instance = tree.createMeshInstance(tree.Name + "_" + i);
                scale = (float) rnd.NextDouble();
                instance.Scale = new Vector3(scale, scale, scale);
                instance.Position = new Vector3(rnd.Next(0, mapLength) - mapLength/2, 0, rnd.Next(0, mapLength) - mapLength/2);
                trees.Add(instance);
            }
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            float velocidadCamara = 350;

            //Capturar Input teclado
            if (Input.keyDown(Key.A))
            {
                Camara.SetCamera(Camara.Position + new Vector3(velocidadCamara * ElapsedTime, 0, 0), Camara.LookAt + new Vector3(velocidadCamara * ElapsedTime, 0, 0));
            }

            if (Input.keyDown(Key.D))
            {
                Camara.SetCamera(Camara.Position + new Vector3(-velocidadCamara * ElapsedTime, 0, 0), Camara.LookAt + new Vector3(-velocidadCamara * ElapsedTime, 0, 0));
            }

            if (Input.keyDown(Key.W))
            {
                Camara.SetCamera(Camara.Position + new Vector3(0, 0, -velocidadCamara * ElapsedTime), Camara.LookAt + new Vector3(0, 0, -velocidadCamara * ElapsedTime));
            }

            if (Input.keyDown(Key.S))
            {
                Camara.SetCamera(Camara.Position + new Vector3(0, 0, velocidadCamara * ElapsedTime), Camara.LookAt + new Vector3(0, 0, velocidadCamara * ElapsedTime));
            }

            if (Input.keyDown(Key.UpArrow))
            {
                Camara.SetCamera(Camara.Position + new Vector3(0, velocidadCamara * ElapsedTime, 0), Camara.LookAt + new Vector3(0, velocidadCamara * ElapsedTime, 0));
            }

            if (Input.keyDown(Key.DownArrow))
            {
                Camara.SetCamera(Camara.Position + new Vector3(0, -velocidadCamara * ElapsedTime, 0), Camara.LookAt + new Vector3(0, -velocidadCamara * ElapsedTime, 0));
            }

            if (Input.keyDown(Key.RightArrow))
            {
                Camara.SetCamera(Camara.Position, Camara.LookAt + new Vector3(velocidadCamara * ElapsedTime, 0, 0));
            }

            if (Input.keyDown(Key.LeftArrow))
            {
                Camara.SetCamera(Camara.Position, Camara.LookAt + new Vector3(-velocidadCamara * ElapsedTime, 0, 0));
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

            //Dibuja un texto por pantalla
            DrawText.drawText("W, A, S, D ←, →, ↑, ↓", 0, 20, Color.Black);

            floor.render();

            foreach (TgcMesh mesh in trees)
            {
                mesh.Transform = Matrix.Scaling(mesh.Scale) * Matrix.Translation(mesh.Position);
                mesh.AlphaBlendEnable = true;
                mesh.render();
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
            foreach(TgcMesh element in trees)
            {
                element.dispose();
            }
        }
    }
}