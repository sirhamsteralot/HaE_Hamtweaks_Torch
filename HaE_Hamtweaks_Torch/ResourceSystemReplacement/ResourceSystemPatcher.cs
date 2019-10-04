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

namespace HaE_Hamtweaks_Torch.ResourceSystemReplacement
{
    [ReflectedLazy]
    public static class ResourceSystemPatcher
    {
        public static void Patch(PatchContext ctx)
        {
            MethodInfo[] patchMethods = typeof(HaEResourceSystemInterface).GetMethods(BindingFlags.Static | BindingFlags.Public);

            foreach (var method in typeof(MyCubeGridSystems).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public))
            {
                foreach (var patchMethod in patchMethods)
                {
                    if (MethodComparer(method, patchMethod))
                    {
                        ctx.GetPattern(method).Prefixes.Add(patchMethod);
                        break;
                    }
                }
            }
        }

        private static bool MethodComparer(MethodInfo MethodA, MethodInfo MethodB)
        {
            bool methodsEqual = true;
            methodsEqual &= MethodA.Name == MethodB.Name;

            return methodsEqual;
        }
    }
}
