using Sandbox.Definitions;
using Sandbox.Engine.Utils;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.Game.GameSystems.Conveyors;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Library.Collections;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRageRender;
using Sandbox.Game.EntityComponents;
using Torch.Utils.Reflected;
using Torch.Managers.PatchManager;
using NLog;

namespace HaE_Hamtweaks_Torch.ResourceSystemReplacement
{
    [ReflectedLazy]
    public static class HaEResourceSystemPatcher
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Patch(PatchContext ctx)
        {
            MethodInfo[] patchMethods = typeof(HaEResourceSystemInterface).GetMethods(BindingFlags.Static | BindingFlags.Public);

            int patchCount = 0;

            foreach (var method in typeof(MyResourceDistributorComponent).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var patchMethod in patchMethods)
                {
                    if (MethodComparer(method, patchMethod))
                    {
                        ctx.GetPattern(method).Prefixes.Add(patchMethod);
                        patchCount++;
                        break;
                    }
                }
            }

            var resourceDistSetter = typeof(MyCubeGridSystems).GetProperty("ResourceDistributor", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
            var resourceDistSetterReplacement = typeof(HaEResourceSystemInterface).GetMethod("PrefixResourceDistributorSetter", BindingFlags.Public | BindingFlags.Static);
            ctx.GetPattern(resourceDistSetter).Prefixes.Add(resourceDistSetterReplacement);
            patchCount++;

            Log.Info($"Succesfully Patched {patchCount} out of {patchMethods.Length} Methods!");
        }

        private static bool MethodComparer(MethodInfo MethodA, MethodInfo MethodB)
        {
            bool methodsEqual = true;
            methodsEqual &= MethodA.Name == MethodB.Name;

            return methodsEqual;
        }
    }
}
