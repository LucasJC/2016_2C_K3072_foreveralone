using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.BoundingVolumes;
using TGC.Core.Camara;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Core.Textures;
using TGC.Examples.Optimization.GrillaRegular;

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

        public TgcFrustum CameraFrustum { get; set; }
        //lista de árboles
        public List<InteractiveObject> Trees { get; set; }
        //lista de objetos del mapa
        public List<InteractiveObject> Objetos { get; set; }
        //skybox
        private TgcSkyBox SkyBox;
        //piso del mapa
        public TgcPlane Floor { get; set; }
        //mover skybox con cámara?
        public bool MoveSkyBoxWithCamera { get; set; }
        //longitud de cada lado del mapa
        public int MapLength { get; set; }
        //activar optimizaciones
        private bool Optimizations = false;
        private GrillaRegular OptimizationGrid = new GrillaRegular();
        //effects
        public Effect lightEffect { get; set; } = null;
        
        /// <summary>
        /// 
        ///     Constructor principal del Mundo
        /// 
        /// </summary>
        /// <param name="mediaDir"></param>
        /// <param name="device"></param>
        /// <param name="loader"></param>
        /// <param name="camera"></param>
        public World(String mediaDir, Microsoft.DirectX.Direct3D.Device device, TgcSceneLoader loader, TgcCamera camera, TgcFrustum frustum, int mapLength, bool optimizations)
        {
            MediaDir = mediaDir;
            Device = device;
            Loader = loader;
            Camera = camera;
            CameraFrustum = frustum;
            MapLength = mapLength;
            Optimizations = optimizations;

            CreateMap();
            CreateSkyBox();

            if(Optimizations)
            {
                TgcBox box = new TgcBox();
                box.Size = new Vector3(Floor.Size.X, 500, Floor.Size.Z);
                OptimizationGrid.create(Trees, box.BoundingBox);
                OptimizationGrid.createDebugMeshes();
            }

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

            if(Optimizations)
            {
                OptimizationGrid.render(CameraFrustum, false);

                foreach (InteractiveObject objecto in Objetos)
                {
                    objecto.mesh.render();
                }
            }
            else
            {
                foreach (InteractiveObject tree in Trees)
                {
                    tree.mesh.render();
                }

                foreach (InteractiveObject objecto in Objetos)
                {
                    objecto.mesh.render();
                }
            }


        }

        /// <summary>
        ///     Acciones correspondientes al dispose
        /// </summary>
        public void dispose()
        {
            foreach (InteractiveObject element in Trees)
            {
                element.mesh.dispose();
            }
            foreach (InteractiveObject element in Objetos)
            {
                element.mesh.dispose();
            }
        }

        public void updateEffects()
        {
            if(null != this.lightEffect)
            {
                String effectTechnique = "RenderScene";

                foreach(InteractiveObject objeto in Trees)
                {
                    objeto.mesh.Effect = lightEffect;
                    objeto.mesh.Technique = effectTechnique;
                }
                foreach (InteractiveObject objeto in Objetos)
                {
                    objeto.mesh.Effect = lightEffect;
                    objeto.mesh.Technique = effectTechnique;
                }

                this.Floor.Effect = lightEffect;
                this.Floor.Technique = effectTechnique;
            }
        }

        /// <summary>
        ///     elimina un objeto del mundo
        /// </summary>
        /// <param name="objeto"></param>
        public void destroyObject(InteractiveObject objeto)
        {
            if (this.Objetos.Contains(objeto))
            {
                this.Objetos.Remove(objeto);
            }
            else{
                if(this.Trees.Contains(objeto)) this.Trees.Remove(objeto);
            }
        }

        /// <summary>
        ///     Crea el mapa
        /// </summary>
        private void CreateMap()
        {
            //armo el piso con un plano
            var floorTexture = TgcTexture.createTexture(Device, MediaDir + "Textures\\tierra.jpg");
            Floor = new TgcPlane(new Vector3(-(MapLength / 2), 0, -(MapLength / 2)), new Vector3(MapLength, 0, MapLength), TgcPlane.Orientations.XZplane, floorTexture, 70f, 70f);

            //creo los árboles
            //loader.loadSceneFromFile(MediaDir + "Meshes\\ArbolBosque\\ArbolBosque-TgcScene.xml").Meshes[0];
            CreateTrees(200);
            //creo otros objetos
            CreateObjects();
        }

        private void CreateObjects()
        {
            TgcMesh teapotMesh = Loader.loadSceneFromFile(MediaDir + "Meshes\\Teapot\\Teapot-TgcScene.xml").Meshes[0];
            teapotMesh.Scale = new Vector3(0.5f, 0.5f, 0.5f);
            teapotMesh.Position = new Vector3(0, teapotMesh.BoundingBox.PMax.Y * 0.75f, 0);
            teapotMesh.Transform = Matrix.Scaling(teapotMesh.Scale) * Matrix.Translation(teapotMesh.Position);
            teapotMesh.updateBoundingBox();
            Objetos = new List<InteractiveObject>();
            Objetos.Add(new InteractiveObject("teapot", 2, teapotMesh, InteractiveObject.Materials.Glass, InteractiveObject.ObjectTypes.Misc));
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

            

            Trees = new List<InteractiveObject>();

            TgcMesh instance;

            for (int i = 1; i <= cantidad; i++)
            {
                instance = tree.createMeshInstance(tree.Name + "_" + i);
                instance.Scale = GameUtils.getRandomScaleVector();
                instance.Position = GameUtils.getRandomPositionVector();
                instance.Transform = Matrix.Scaling(instance.Scale) * Matrix.Translation(instance.Position);
                instance.updateBoundingBox();
                instance.AlphaBlendEnable = true;
                Trees.Add(new InteractiveObject("Tree", 5, instance, InteractiveObject.Materials.Wood, InteractiveObject.ObjectTypes.Tree));
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
