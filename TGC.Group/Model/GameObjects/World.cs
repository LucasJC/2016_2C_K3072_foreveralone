using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.BoundingVolumes;
using TGC.Core.Camara;
using TGC.Core.Collision;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Terrain;
using TGC.Core.Textures;
using TGC.Examples.Optimization.GrillaRegular;
using TGC.Group.Model.Optimization;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Clase que representa al mundo en el juego
    /// </summary>
    public class World : Renderable
    {
        public enum Weather { Hot, Normal, Cold, ExtremeCold }

        private String MediaDir;
        private Microsoft.DirectX.Direct3D.Device Device;
        private TgcSceneLoader Loader;
        private TgcCamera Camera;

        public Weather CurrentWeather = Weather.Normal;

        public TgcFrustum CameraFrustum { get; set; }
        //lista de objetos del mapa
        public List<InteractiveObject> Objetos { get; set; }
        //skybox
        public TgcSkyBox SkyBox { get; set; }
        //piso del mapa
        public TgcPlane Floor { get; set; }
        //mover skybox con cámara?
        public bool MoveSkyBoxWithCamera { get; set; }
        //longitud de cada lado del mapa
        public int MapLength { get; set; }
        //distancia máxima de renderizado
        public int RenderDistance;
        //octree para optimizaciones
        //private Octree octree = new Octree();
        //quadtree para optimizaciones
        private Quadtree quad = new Quadtree();
        //viento
        public Vector2 Wind = new Vector2(1,0);

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
            this.RenderDistance = 1000;
            CreateMap();
            CreateSkyBox();

            TgcBox box = new TgcBox();
            box.Size = new Vector3(MapLength, 50, MapLength);

            //octree.create(Objetos, box.BoundingBox);
            //octree.createDebugOctreeMeshes();
            quad.create(Objetos, box.BoundingBox);
            //quad.createDebugQuadtreeMeshes();

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
            //octree.update(CameraFrustum);
            quad.update(CameraFrustum);
            
            //limito distancia de renderizado en función a dónde estoy parado
            foreach (InteractiveObject obj in this.Objetos)
            {
                if(obj.mesh.Enabled)
                {
                    float dist = GameUtils.calculateDistance(Camera.Position, obj.mesh.Position);
                    if (dist > RenderDistance)
                    {
                        obj.mesh.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        ///     Acciones correspondientes al render
        /// </summary>
        public void render()
        {
            SkyBox.render();
            Floor.render();
            quad.render(CameraFrustum, false);
        }

        /// <summary>
        ///     Acciones correspondientes al dispose
        /// </summary>
        public void dispose()
        {
            foreach (InteractiveObject element in Objetos)
            {
                element.mesh.dispose();
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
        }

        public void changeWind()
        {
            this.Wind = GameUtils.getRandomWindVector();
        }

        /// <summary>
        ///     modifica el clima aleatoriamente
        /// </summary>
        /// <param name="player"></param>
        public void changeWeather(Player player)
        {
            int random = GameUtils.Rand.Next(4);

            CurrentWeather = (Weather) random;
            player.Weather = CurrentWeather;
        }

        /// <summary>
        ///     Crea el mapa
        /// </summary>
        private void CreateMap()
        {
            //las cantidades base se pensaron para un escenario de 2000, por lo que divido la longitud actual por eso y multiplico los valores base
            int multiplier = MapLength / 2000;

            //armo el piso con un plano
            var floorTexture = TgcTexture.createTexture(Device, MediaDir + "Textures\\tierra.jpg");
            Floor = new TgcPlane(new Vector3(-(MapLength / 2), 0, -(MapLength / 2)), new Vector3(MapLength, 0, MapLength), TgcPlane.Orientations.XZplane, floorTexture, 70f * multiplier, 70f * multiplier);

            //creo los árboles
            CreateTrees(500 * multiplier);
            //creo rocas
            CreateRocks(200 * multiplier);
            //creo pasto
            CreateGrass(1000 * multiplier);
            //creo otros objetos
            CreateObjects(20 * multiplier);
        }

        /// <summary>
        ///     Genera el skymap y lo configura
        /// </summary>
        private void CreateSkyBox()
        {
            //Crear SkyBox
            SkyBox = new TgcSkyBox();
            SkyBox.Center = new Vector3(0, 0, 0);
            int SkyBoxLength = 0;

            if (MapLength > 7000)
            {
                SkyBoxLength = 7000;
            }else
            {
                SkyBoxLength = MapLength;
            }

            SkyBox.Size = new Vector3(SkyBoxLength, SkyBoxLength, SkyBoxLength);

            var texturesPath = MediaDir + "Textures\\SkyBox\\";
            String imgNameRoot = "clouds";
            String imgExtension = "png";
            //Configurar las texturas para cada una de las 6 caras
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + imgNameRoot + "_up." + imgExtension);
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + imgNameRoot + "_dn." + imgExtension);
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + imgNameRoot + "_lf." + imgExtension);
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + imgNameRoot + "_rt." + imgExtension);
            //Hay veces es necesario invertir las texturas Front y Back si se pasa de un sistema RightHanded a uno LeftHanded
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + imgNameRoot + "_bk." + imgExtension);
            SkyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + imgNameRoot + "_ft." + imgExtension);
            SkyBox.SkyEpsilon = 25f;
            //Inicializa todos los valores para crear el SkyBox
            SkyBox.Init();
        }
        // TODO modificar, por ahora es de ejemplo
        private void CreateObjects(int cantidad)
        {

            TgcMesh teapotMesh = Loader.loadSceneFromFile(MediaDir + "Meshes\\Teapot\\Teapot-TgcScene.xml").Meshes[0];
            teapotMesh.Scale = new Vector3(0.5f, 0.5f, 0.5f);

            if (null == Objetos) Objetos = new List<InteractiveObject>();

            TgcMesh instance;

            for (int i = 1; i <= cantidad; i++)
            {
                instance = teapotMesh.createMeshInstance(teapotMesh.Name + "_" + i);
                //instance.Scale = GameUtils.getRandomScaleVector();
                instance.Position = GameUtils.getRandomPositionVector();
                instance.Transform = Matrix.Scaling(instance.Scale) * Matrix.Translation(instance.Position);
                instance.updateBoundingBox();
                InteractiveObject interactiveObject = new InteractiveObject("Teapot", 3, instance, InteractiveObject.Materials.Glass, InteractiveObject.ObjectTypes.Misc);
                interactiveObject.drops.Add(InventoryObject.ObjectTypes.Water);
                interactiveObject.drops.Add(InventoryObject.ObjectTypes.Water);
                Objetos.Add(interactiveObject);
            }
        }

        /// <summary>
        ///     Genera la disposición de los árboles en el mapa
        ///     TODO: hacer que no se toquen...
        /// </summary>
        private void CreateTrees(int cantidad)
        {
            TgcMesh tree = Loader.loadSceneFromFile(MediaDir + "Meshes\\Pino\\Pino-TgcScene.xml").Meshes[0];

            if (null == Objetos) Objetos = new List<InteractiveObject>();

            TgcMesh instance;

            for (int i = 1; i <= cantidad; i++)
            {
                instance = tree.createMeshInstance(tree.Name + "_" + i);
                instance.Scale = GameUtils.getRandomScaleVector();
                instance.Position = GameUtils.getRandomPositionVector();
                instance.Transform = Matrix.Scaling(instance.Scale) * Matrix.Translation(instance.Position);
                instance.updateBoundingBox();
                instance.AlphaBlendEnable = true;
                InteractiveObject interactiveObject = new InteractiveObject("Tree", 5, instance, InteractiveObject.Materials.Wood, InteractiveObject.ObjectTypes.Tree);
                interactiveObject.drops.Add(InventoryObject.ObjectTypes.Wood);
                interactiveObject.drops.Add(InventoryObject.ObjectTypes.Leaf);
                Objetos.Add(interactiveObject);
            }
        }

        /// <summary>
        ///     Genera las piedras en el mapa
        /// </summary>
        /// <param name="cantidad"></param>
        private void CreateRocks(int cantidad)
        {
            TgcMesh rock = Loader.loadSceneFromFile(MediaDir + "Meshes\\Roca\\Roca-TgcScene.xml").Meshes[0];

            if (null == Objetos) Objetos = new List<InteractiveObject>();

            TgcMesh instance;

            for (int i = 1; i <= cantidad; i++)
            {
                instance = rock.createMeshInstance(rock.Name + "_" + i);
                instance.Scale = GameUtils.getRandomScaleVector() * 0.2f;
                instance.Position = GameUtils.getRandomPositionVector();
                instance.Transform = Matrix.Scaling(instance.Scale) * Matrix.Translation(instance.Position);
                instance.updateBoundingBox();
                //instance.AlphaBlendEnable = true;
                InteractiveObject interactiveObject = new InteractiveObject("Rock", 1, instance, InteractiveObject.Materials.Rock, InteractiveObject.ObjectTypes.Rock);
                interactiveObject.drops.Add(InventoryObject.ObjectTypes.Rock);
                Objetos.Add(interactiveObject);
            }
        }

        /// <summary>
        ///     Genera el pasto en el mapa
        /// </summary>
        /// <param name="cantidad"></param>
        private void CreateGrass(int cantidad)
        {
            TgcMesh grass = Loader.loadSceneFromFile(MediaDir + "Meshes\\Pasto\\Pasto-TgcScene.xml").Meshes[0];

            if (null == Objetos) Objetos = new List<InteractiveObject>();

            TgcMesh instance;

            for (int i = 1; i <= cantidad; i++)
            {
                instance = grass.createMeshInstance(grass.Name + "_" + i);
                instance.Scale = GameUtils.getRandomScaleVector();
                instance.Position = GameUtils.getRandomPositionVector();
                instance.Transform = Matrix.Scaling(instance.Scale) * Matrix.Translation(instance.Position);
                instance.updateBoundingBox();
                instance.AlphaBlendEnable = true;
                InteractiveObject interactiveObject = new InteractiveObject("Grass", 1, instance, InteractiveObject.Materials.Plant, InteractiveObject.ObjectTypes.Grass);
                interactiveObject.drops.Add(InventoryObject.ObjectTypes.Seed);
                Objetos.Add(interactiveObject);
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
