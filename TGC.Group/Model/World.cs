using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Core.Textures;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Clase que representa al mundo en el juego
    /// </summary>
    class World : Renderable
    {
        private String MediaDir;
        private Microsoft.DirectX.Direct3D.Device Device;
        private TgcSceneLoader Loader;
        private TgcCamera Camera;

        //lista de árboles
        private List<TgcMesh> Trees;
        //lista de objetos del mapa
        private List<TgcMesh> Objetos;
        //skybox
        private TgcSkyBox SkyBox;
        //piso del mapa
        public TgcPlane Floor { get; set; }
        //mover skybox con cámara?
        public bool MoveSkyBoxWithCamera { get; set; }
        //longitud de cada lado del mapa
        public int MapLength { get; set; }
        //activar optimizaciones
        public bool Optimizations { get; set; }

        /// <summary>
        /// 
        ///     Constructor principal del Mundo
        /// 
        /// </summary>
        /// <param name="mediaDir"></param>
        /// <param name="device"></param>
        /// <param name="loader"></param>
        /// <param name="camera"></param>
        public World(String mediaDir, Microsoft.DirectX.Direct3D.Device device, TgcSceneLoader loader, TgcCamera camera, int mapLength)
        {
            MediaDir = mediaDir;
            Device = device;
            Loader = loader;
            Camera = camera;
            MapLength = mapLength;
            Optimizations = false;

            CreateMap();
            CreateSkyBox();
        }

        /// <summary>
        /// 
        ///     Acciones correspondientes al update
        /// 
        /// </summary>
        public void update()
        {
            if (MoveSkyBoxWithCamera)
            {
                SkyBox.Center = Camera.Position;
            }
        }

        /// <summary>
        ///     Acciones correspondientes al render
        /// </summary>
        public void render()
        {
            SkyBox.render();
            Floor.render();

            foreach (TgcMesh mesh in Trees)
            {
                mesh.Transform = Matrix.Scaling(mesh.Scale) * Matrix.Translation(mesh.Position);
                mesh.AlphaBlendEnable = true;
                mesh.render();
                
            }

            foreach (TgcMesh mesh in Objetos)
            {
                mesh.Transform = Matrix.Scaling(mesh.Scale) * Matrix.Translation(mesh.Position);
                mesh.render();
            }
        }

        /// <summary>
        ///     Acciones correspondientes al dispose
        /// </summary>
        public void dispose()
        {
            foreach (TgcMesh element in Trees)
            {
                element.dispose();
            }
        }

        /// <summary>
        ///     Crea el mapa
        /// </summary>
        private void CreateMap()
        {
            //armo el piso con un plano
            var floorTexture = TgcTexture.createTexture(Device, MediaDir + "Textures\\pasto.jpg");
            Floor = new TgcPlane(new Vector3(-(MapLength / 2), 0, -(MapLength / 2)), new Vector3(MapLength, 0, MapLength), TgcPlane.Orientations.XZplane, floorTexture, 10f, 10f);

            //creo los árboles
            //loader.loadSceneFromFile(MediaDir + "Meshes\\ArbolBosque\\ArbolBosque-TgcScene.xml").Meshes[0];
            CreateTrees(200);
            //creo otros objetos
            CreateObjects();
        }

        private void CreateObjects()
        {
            TgcMesh teapot = Loader.loadSceneFromFile(MediaDir + "Meshes\\Teapot\\Teapot-TgcScene.xml").Meshes[0];
            teapot.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            teapot.updateBoundingBox();
            teapot.Position = new Vector3(0, teapot.BoundingBox.PMax.Y * 0.75f, 0);
            Objetos = new List<TgcMesh>();
            Objetos.Add(teapot);
        }

        /// <summary>
        ///     Genera el skymap y lo configura
        /// </summary>
        private void CreateSkyBox()
        {
            //Crear SkyBox
            SkyBox = new TgcSkyBox();
            SkyBox.Center = new Vector3(0, 0, 0);
            SkyBox.Size = new Vector3(10000, 10000, 10000);

            //indico si se mueve o no con la cámara
            MoveSkyBoxWithCamera = false;

            //Configurar color
            //skyBox.Color = Color.OrangeRed;

            var texturesPath = MediaDir + "Textures\\SkyBox\\";

            //Configurar las texturas para cada una de las 6 caras
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "phobos_up.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "phobos_dn.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "phobos_lf.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "phobos_rt.jpg");

            //Hay veces es necesario invertir las texturas Front y Back si se pasa de un sistema RightHanded a uno LeftHanded
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "phobos_bk.jpg");
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "phobos_ft.jpg");
            SkyBox.SkyEpsilon = 25f;
            //Inicializa todos los valores para crear el SkyBox
            SkyBox.Init();
        }

        /// <summary>
        ///     Genera la disposición de los árboles en el mapa
        ///     TODO: hacer que no se toquen...
        /// </summary>
        private void CreateTrees(int cantidad)
        {
            TgcMesh tree = Loader.loadSceneFromFile(MediaDir + "Meshes\\Pino\\Pino-TgcScene.xml").Meshes[0];

            Random rnd = new Random();

            Trees = new List<TgcMesh>();

            TgcMesh instance;
            float scale;

            for (int i = 1; i <= cantidad; i++)
            {
                instance = tree.createMeshInstance(tree.Name + "_" + i);
                scale = (float)rnd.NextDouble();
                if (scale < 0.3f) scale = 0.3f;
                instance.Scale = new Vector3(scale, scale, scale);
                instance.Position = new Vector3(rnd.Next(0, MapLength) - MapLength / 2, 0, rnd.Next(0, MapLength) - MapLength / 2);
                
                Trees.Add(instance);
            }
        }

        /// <summary>
        /// 
        ///     Método que retorna si una posición x,y está libre en base a una lista de meshes
        /// 
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool CheckPositionNotUsed(Vector3 candidate, List<TgcMesh> list)
        {
            foreach(TgcMesh mesh in list)
            {
                if(mesh.Position.X == candidate.X && mesh.Position.Y == candidate.Y)
                {
                    Console.WriteLine("posición usada, recalculando...");
                    return false;
                }
            }

            return true;
        }
    }
}
