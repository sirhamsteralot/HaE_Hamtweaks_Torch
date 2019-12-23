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
            // patch the setter for the resourcedistributor in mycubegridsystems so that we always have an up to date link
            // create the custom resource distributor, replace it and link it with an entityId via checking if its the same reference or something?
            // but shouldnt actually replace it because uhhhh then will run into race conditions?
            // have it parallel to eachother and copy data over to eachother on a game update
            // also need to check if its even neccesary to copy over the data like that
            // but its just replacing a reference so its probably fine and not even that slow?
            // also needs to check probably if the entity already has a distributor and then just transfer it? create a new one? idfk

            GetEntityComponent(value);
            return true;
        }


        #region helpers
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

        private static PropertyInfo resourceDistributor = typeof(MyCubeGridSystems).GetProperty("ResourceDistributor", BindingFlags.Public | BindingFlags.NonPublic);
        private static FieldInfo dataPerType = typeof(MyResourceDistributorComponent).GetField("m_dataPerType", BindingFlags.NonPublic | BindingFlags.Instance);
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
