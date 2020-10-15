using ProceduralToolkit.Samples.Buildings;
using UnityEngine;
using UnityEngine.Core;

namespace uzSurfaceMapper.Utils.Generators
{
    public class BuildingPlanner : MonoSingleton<BuildingPlanner>
    {
        public ProceduralFacadeConstructor proceduralFacadeConstructor;
        public ProceduralFacadePlanner proceduralFacadePlanner;
        public ProceduralRoofConstructor proceduralRoofConstructor;
        public ProceduralRoofPlanner proceduralRoofPlanner;

        public AnimationCurve buildHeightCurve;

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}