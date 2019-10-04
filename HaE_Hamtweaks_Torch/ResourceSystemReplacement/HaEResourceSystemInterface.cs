using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace HaE_Hamtweaks_Torch.ResourceSystemReplacement
{
    public static class HaEResourceSystemInterface
    {
        private static Dictionary<long, HaEResourceDistributorComponent> _entityToDistributer = new Dictionary<long, HaEResourceDistributorComponent>();
        private const bool patchDisabled = false;

        // Will return either the correct entity component, create one if it isnt there or return null if there is no entity
        private static HaEResourceDistributorComponent GetEntityComponent(MyResourceDistributorComponent instance)
        {
            if (patchDisabled)
                return null;

            IMyEntity instanceEntity = instance.Entity;
            if (instanceEntity == null)
                return null;

            HaEResourceDistributorComponent entityComponent;
            if (!_entityToDistributer.TryGetValue(instanceEntity.EntityId, out entityComponent))
            {
                entityComponent = new HaEResourceDistributorComponent(instance);

                _entityToDistributer.Add(instanceEntity.EntityId, entityComponent);

                ReplaceOriginalComponent(instanceEntity, entityComponent);
            }

            return entityComponent;
        }

        private static void ReplaceOriginalComponent(IMyEntity entity, HaEResourceDistributorComponent newComponent)
        {
            typeof(MyCubeGridSystems).GetProperty("ResourceDistributor", BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(entity, newComponent);
        }

        public static bool UpdateBeforeSimulation(MyResourceDistributorComponent __instance)
        {
            var entityComponent = GetEntityComponent(__instance);
            if (entityComponent == null)
                return true;

            entityComponent.UpdateBeforeSimulation();
            return false;
        }
    }
}
