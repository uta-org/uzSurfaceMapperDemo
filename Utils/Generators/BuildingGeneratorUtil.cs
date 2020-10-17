using System.Linq;
using ProceduralToolkit;
using ProceduralToolkit.Buildings;
using UnityEngine;
using uzSurfaceMapper.Model;
using uzSurfaceMapper.Core.Generators;
using uzSurfaceMapper.Extensions;

namespace uzSurfaceMapper.Utils.Generators
{
    /// <summary>
    /// Extension class for generating buildings through TerrainChunk class.
    /// </summary>
    public static class BuildingGeneratorUtil
    {
        public static void CreateBuildings(Chunk c)
        {
            CreateBuildings(c, City.DefaultBuildConfig);
        }

        public static void CreateBuildings(Chunk c, BuildingGenerator.Config config)
        {
            //foreach (var bIndex in c.listOfIndexBuildings)
            //    yield return CreateBuild(bIndex);

            foreach (var bIndex in c.listOfIndexBuildings)
                MapGenerator.CityModel.buildingGenerators.Add(GenerateBuilding(bIndex, config));
        }

        private static BuildingGenerator GenerateBuilding(int buildingIndex, BuildingGenerator.Config config)
        {
            return GenerateBuilding(buildingIndex, config, out _);
        }

        private static BuildingGenerator GenerateBuilding(int buildingIndex, BuildingGenerator.Config config, out Transform generatedBuild)
        {
            var builds = GameObject.Find("Builds");

            if (builds == null)
                builds = new GameObject("Builds");

            var build = new GameObject("Build");
            build.transform.parent = builds.transform;

            var building = MapGenerator.CityModel.buildings.ElementAt(buildingIndex);
            var height = BuildingPlanner.Instance.buildHeightCurve.GetBuildHeight(building.GetWeight());
            if (config == City.DefaultBuildConfig)
            {
                City.DefaultBuildConfig.floors = (int)height;
            }
            var generator = new BuildingGenerator();
            {
                generator.SetFacadeConstructor(BuildingPlanner.Instance.proceduralFacadeConstructor);
                generator.SetFacadePlanner(BuildingPlanner.Instance.proceduralFacadePlanner);
                generator.SetRoofConstructor(BuildingPlanner.Instance.proceduralRoofConstructor);
                generator.SetRoofPlanner(BuildingPlanner.Instance.proceduralRoofPlanner);

                var foundationPoints = building.Pol.Edges.Select(x => CityGenerator.SConv.ConvertVector(x));
                var orientation = Geometry.GetOrientation(foundationPoints.ToList());
                if (orientation == Orientation.CounterClockwise) foundationPoints = foundationPoints.Reverse();
                var foundationList = foundationPoints.ToList();

                generatedBuild = generator.Generate(foundationList, config,
                    build.transform);

                var roof = generatedBuild.GetChild(0);
                var facade = generatedBuild.GetChild(1);

                var roofMesh = roof.gameObject.GetComponent<MeshFilter>().sharedMesh;
                var facadeMesh = facade.gameObject.GetComponent<MeshFilter>().sharedMesh;

                var roofCollider = roof.gameObject.AddComponent<MeshCollider>();
                roofCollider.sharedMesh = roofMesh;

                var facadeCollider = facade.gameObject.AddComponent<MeshCollider>();
                facadeCollider.sharedMesh = facadeMesh;
            }

            build.transform.position = City.GetBuildingCenter(building);

            return generator;
        }

        #region "Old code"

        ///// <summary>
        /////     Tests the one build.
        ///// </summary>
        //internal static void TestOneBuild()
        //{
        //    // Debug.Log($"Edge Count: {city.buildings.Sum(x => x.EdgeCount)}");

        //    //Debug.Log($"Max weight: {city.buildings.Max(x => x.GetWeight())}");
        //    //Debug.Log($"Avg weight: {city.buildings.Average(x => x.GetWeight())}");
        //    //Debug.Log($"Min weight: {city.buildings.Min(x => x.GetWeight())}");

        //    //Debug.Log($"Percentile 25 weight: {city.buildings.Select(x => x.GetWeight()).AsDynamic().Percentile(.25f)}");
        //    //Debug.Log($"Percentile 75 weight: {city.buildings.Select(x => x.GetWeight()).AsDynamic().Percentile(.75f)}");

        //    oneTestBuildPlane?.SetActive(true);

        //    var shapeCollection = city.buildings.Where(x => x.Pol.EdgeCount >= 20);

        //    // Testing random ranges
        //    if (randomizeOneBuild)
        //        oneBuildShape = shapeCollection.ElementAt(Random.Range(0, shapeCollection.Count()));
        //    oneBuildShape = shapeCollection.First();

        //    CityBenchmarkData.StartBenchmark(CityBenchmark.CreateBuild);

        //    CreateBuild(0);

        //    CityBenchmarkData.StopBenchmark(CityBenchmark.CreateBuild);
        //}

        // TODO: Remove this. This is not used anymore to generate builds.
        //[WIP]
        //private static IEnumerator CreateBuild(int buildingIndex, bool correction = false)
        //{
        //    // TODO: I have to use a single instance of City class (refactorization) & I don't weant to use elementat
        //    var building = City.Instance.buildings.ElementAt(buildingIndex);

        //    // Create data for entraces @TODO
        //    building.LoadEntranceData();

        //    // Store all walls to add them to a build

        //    var buildComponents = new List<GameObject>();

        //    // Create and get some important gameobject & transforms

        //    var builds = GameObject.Find("Builds");

        //    if (builds == null)
        //        builds = new GameObject("Builds");

        //    var build = new GameObject("Build");
        //    var parent = build.transform;

        //    build.layer = LayerMask.NameToLayer(buildingLayer);

        //    var mat = GetInstance<CityGenerator>().buildMat;

        //    if (mat == null)
        //    {
        //        mat = new Material(Shader.Find("Diffuse"))
        //        {
        //            color = UnityEngine.Color.gray
        //        };
        //    }

        //    // Create walls

        //    var height = BuildingPlanner.Instance.buildHeightCurve.GetBuildHeight(building.GetWeight());

        //    yield return null;

        //    building.Pol.LoopEdges((e, i) =>
        //    {
        //        // Previous index
        //        var pI = i == 0 ? building.Pol.EdgeCount - 1 : i - 1;

        //        // Previous edge
        //        var pE = building.Pol.GetEdge(pI, correction);

        //        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //        wall.layer = LayerMask.NameToLayer(buildingLayer);

        //        wall.GetComponent<MeshRenderer>().sharedMaterial = mat;

        //        wall.transform.SetPositionBetween2Points(e, pE);

        //        wall.transform.SetScaleYZ(height);
        //        wall.transform.position += Vector3.up * height / 2;

        //        wall.transform.parent = parent;

        //        buildComponents.Add(wall);
        //    }, correction);

        //    yield return null;

        //    // Create floor

        //    var floor = building.CreateBase(parent, mat);

        //    var roof = building.CreateBase(parent, mat, "Roof");
        //    roof.transform.position += Vector3.up * height;

        //    buildComponents.Add(floor);
        //    buildComponents.Add(roof);

        //    var mesh = parent.gameObject.CombineMeshes();

        //    var buildExterior = F.GenerateGameObjectWithBasicComponents("Build Exterior", mesh, mat, parent);

        //    buildComponents.ForEach(Object.Destroy);

        //    buildExterior.transform.parent = parent;

        //    // Assign position & parent

        //    build.transform.position = City.GetBuildingCenter(building);

        //    build.transform.parent = builds.transform;

        //    build.layer = LayerMask.NameToLayer("Default");
        //}

        #endregion "Old code"
    }
}