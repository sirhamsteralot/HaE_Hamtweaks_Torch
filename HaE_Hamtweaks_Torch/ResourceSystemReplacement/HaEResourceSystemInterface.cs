using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.Game;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NLog;

namespace HaE_Hamtweaks_Torch.ResourceSystemReplacement
{
    public static class HaEResourceSystemInterface
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static Dictionary<MyResourceDistributorComponent, HaEResourceDistributorComponent> _distributerToMyComp = new Dictionary<MyResourceDistributorComponent, HaEResourceDistributorComponent>();
        private const bool patchDisabled = false;

        private static HaETaskThread tasks = new HaETaskThread();

        public static bool UpdateBeforeSimulation(MyResourceDistributorComponent __instance)
        {
            var entityComponent = GetEntityComponent(__instance);
            if (entityComponent == null)
                return true;

            // the side component also needs needed data fed to it so it can actually recompute and execute without missing data.

            if (!entityComponent.CurrentlyWorking)
            {
                CopyComponentData(__instance.Entity, entityComponent);
            }

            tasks.Enqueue(entityComponent.UpdateBeforeSimulation);
            return false;
        }

        public static bool RecomputeResourceDistribution(ref MyDefinitionId typeId, bool updateChanges, MyResourceDistributorComponent __instance)
        {
            var entityComponent = GetEntityComponent(__instance);
            if (entityComponent == null)
                return true;

            var typeCopy = typeId;
            tasks.Enqueue(() => 
            {
                entityComponent.RecomputeResourceDistribution(ref typeCopy, updateChanges);
                entityComponent.CurrentlyWorking = true;
                var copy = dataPerType.GetValue(entityComponent);
                entityComponent.Copy = entityComponent.GetPerTypeDataCopy();
                entityComponent.CurrentlyWorking = false;
            });

            return false;
        }

        public static bool PrefixResourceDistributorSetter(MyCubeGridSystems __instance, MyResourceDistributorComponent value)
        {
            GetEntityComponent(value);
            return true;
        }


        #region helpers
        private static PropertyInfo resourceDistributor = typeof(MyCubeGridSystems).GetProperty("ResourceDistributor", BindingFlags.Public | BindingFlags.NonPublic);
        private static FieldInfo dataPerType = typeof(MyResourceDistributorComponent).GetField("m_dataPerType", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo typeIdToIndex = typeof(MyResourceDistributorComponent).GetField("m_typeIdToIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo forceRecalculation = typeof(MyResourceDistributorComponent).GetField("m_forceRecalculation", BindingFlags.NonPublic | BindingFlags.Instance);


        private static void CopyCalculationData(MyResourceDistributorComponent instance, HaEResourceDistributorComponent haEInstance)
        {
            typeIdToIndex.SetValue(haEInstance, typeIdToIndex.GetValue(instance));
            forceRecalculation.SetValue(haEInstance, forceRecalculation.GetValue(instance));
        }

        // Will return either the correct entity component, create one if it isnt there or return null if there is no entity
        private static HaEResourceDistributorComponent GetEntityComponent(MyResourceDistributorComponent instance)
        {
            if (patchDisabled)
                return null;

            if (instance == null)
            {
                Log.Info("instance null!");
                return null;
            }

            HaEResourceDistributorComponent entityComponent;
            if (!_distributerToMyComp.TryGetValue(instance, out entityComponent))
            {
                entityComponent = new HaEResourceDistributorComponent(instance);

                _distributerToMyComp.Add(instance, entityComponent);
            }

            return entityComponent;
        }

        private static void CopyComponentData(IMyEntity entity, HaEResourceDistributorComponent replacementComp)
        {
            MyResourceDistributorComponent originalDistributorComp = (MyResourceDistributorComponent)resourceDistributor?.GetValue(entity);

            if (originalDistributorComp != null)
            {
                dataPerType.SetValue(originalDistributorComp, replacementComp.Copy);
            }
        }
        #endregion
    }
}
