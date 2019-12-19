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

        private static Dictionary<long, HaEResourceDistributorComponent> _entityToDistributer = new Dictionary<long, HaEResourceDistributorComponent>();
        private const bool patchDisabled = false;

        private static HaETaskThread tasks = new HaETaskThread();

        public static bool UpdateBeforeSimulation(MyResourceDistributorComponent __instance)
        {
            var entityComponent = GetEntityComponent(__instance);
            if (entityComponent == null)
                return true;

            if (!entityComponent.CurrentlyWorking)
            {
                Log.Info("Copying component data...");
                CopyComponentData(__instance.Entity, entityComponent);
            }

            Log.Info("Enqueuing updatebeforesimulation");
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
                Log.Info("Processing resource distribution In parralel thread");
                entityComponent.RecomputeResourceDistribution(typeCopy, updateChanges);
                entityComponent.CurrentlyWorking = true;
                entityComponent.Copy = GetCopy(dataPerType.GetValue(entityComponent));
                entityComponent.CurrentlyWorking = false;
            });

            return false;
        }

        public static bool PrefixResourceDistributorSetter(MyResourceDistributorComponent __instance)
        {
            // patch the setter for the resourcedistributor in mycubegridsystems so that we always have an up to date link
            // create the customn resource distributor, replace it and link it with an entityId via checking if its the same reference or something?
            // but shouldnt actually replace it because uhhhh then will run into race conditions?
            // have it parallel to eachother and copy data over to eachother on a game update
            // also need to check if its even neccesairy to copy over the data like that
            // but its just replacing a reference so its probably fine and not even that slow?
            // also needs to check probably if the entity already has a distributor and then just transfer it? create a new one? idfk



            return true;
        }


        #region helpers
        // Will return either the correct entity component, create one if it isnt there or return null if there is no entity
        private static HaEResourceDistributorComponent GetEntityComponent(MyResourceDistributorComponent instance)
        {
            if (patchDisabled)
                return null;

            IMyEntity instanceEntity = instance.Entity;
            if (instanceEntity == null)
            {
                return null;
            }

            Log.Info("Instance.Entity is not NULL!");

            HaEResourceDistributorComponent entityComponent;
            if (!_entityToDistributer.TryGetValue(instanceEntity.EntityId, out entityComponent))
            {
                entityComponent = new HaEResourceDistributorComponent(instance);

                _entityToDistributer.Add(instanceEntity.EntityId, entityComponent);
                ReplaceOriginalComponent(instanceEntity, entityComponent);
            }

            return entityComponent;
        }

        private static PropertyInfo resourceDistributor = typeof(MyCubeGridSystems).GetProperty("ResourceDistributor", BindingFlags.Public | BindingFlags.NonPublic);
        private static PropertyInfo dataPerType = typeof(MyResourceSinkComponent).GetProperty("m_dataPerType");
        private static void CopyComponentData(IMyEntity entity, HaEResourceDistributorComponent replacementComp)
        {
            MyResourceSinkComponent originalDistributorComp = (MyResourceSinkComponent)resourceDistributor?.GetValue(entity);

            if (originalDistributorComp != null)
            {
                dataPerType.SetValue(originalDistributorComp, replacementComp.Copy);
            }
        }

        private static object GetCopy(object input)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, input);
                stream.Position = 0;
                return formatter.Deserialize(stream);
            }
        }

        private static void ReplaceOriginalComponent(IMyEntity entity, HaEResourceDistributorComponent newComponent)
        {
            resourceDistributor?.SetValue(entity, newComponent);
        }
        #endregion
    }
}
