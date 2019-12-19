﻿using Sandbox.Game.Entities.Cube;
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

namespace HaE_Hamtweaks_Torch.ResourceSystemReplacement
{
    public static class HaEResourceSystemInterface
    {
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
                entityComponent.RecomputeResourceDistribution(typeCopy, updateChanges);
                entityComponent.CurrentlyWorking = true;
                entityComponent.Copy = GetCopy(dataPerType.GetValue(entityComponent));
                entityComponent.CurrentlyWorking = false;
            });

            return false;
        }

        #region helpers
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

        private static PropertyInfo resourceDistributor = typeof(MyCubeGridSystems).GetProperty("ResourceDistributor", BindingFlags.Public | BindingFlags.NonPublic);
        private static PropertyInfo dataPerType = typeof(MyResourceSinkComponent).GetProperty("m_dataPerType");
        private static void CopyComponentData(IMyEntity entity, HaEResourceDistributorComponent replacementComp)
        {
            MyResourceSinkComponent originalDistributorComp = (MyResourceSinkComponent)resourceDistributor?.GetValue(entity);

            if (originalDistributorComp != null)
            {
                dataPerType.SetValue(originalDistributorComp, replacementComp.Copy);

                //object data = dataPerType.GetValue(originalDistributorComp);

                //foreach (object obj in (data as IEnumerable<object>))
                //{
                //    Type objType = obj.GetType();
                //    MyDefinitionId id = (MyDefinitionId)objType.GetField("TypeId").GetValue(originalDistributorComp);
                //    objType.GetField("ResourceState").SetValue(originalDistributorComp, replacementComp.ResourceStateByType(id, false));
                //    objType.GetField("MaxAvailableResource").SetValue(originalDistributorComp, replacementComp.MaxAvailableResourceByType(id));
                //    objType.GetField("DistributionGroups").SetValue(originalDistributorComp, typeof(MyResourceSinkComponent).GetField("DistributionGroups").GetValue(replacementComp));
                //}
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
