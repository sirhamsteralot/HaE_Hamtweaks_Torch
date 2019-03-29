using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using Havok;
using Sandbox.Engine.Physics;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using Torch.Managers.PatchManager.MSIL;
using Torch.Utils;
using Torch.Utils.Reflected;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.Entity.EntityComponents.Interfaces;
using VRage.ModAPI;
using Sandbox;
using Sandbox.Game;
using Sandbox.Engine.Utils;
using Sandbox.Game.Multiplayer;
using NLog;


namespace HaE_Hamtweaks_Torch
{
    [ReflectedLazy]
    internal static class Patching
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [ReflectedMethodInfo(typeof(MyCubeGrid), "ColorGridOrBlockRequestValidation")]
        private static readonly MethodInfo _ColorGridOrBlockRequest;

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(_ColorGridOrBlockRequest).Prefixes.Add(typeof(Patching).GetMethod("PrefixColorGridOrBlockRequestValidation", BindingFlags.Public | BindingFlags.Static));

            Log.Info("Finished Patching!");
        }


        public static bool PrefixColorGridOrBlockRequestValidation(long player, MyCubeGrid __instance, ref bool __result)
        {
            if (player == 0L)
            {
                __result = true;
                return false;
            }
            if (!Sync.IsServer)
            {
                __result = true;
                return false;
            }
            if (__instance.BigOwners.Count == 0)
            {
                __result = true;
                return false;
            }
            foreach (long current in __instance.BigOwners)
            {

                MyRelationsBetweenPlayers relation = MyPlayer.GetRelationsBetweenPlayers(current, player);
                if (relation == MyRelationsBetweenPlayers.Self ||
                    relation == MyRelationsBetweenPlayers.Allies)
                {
                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }
    }
}
