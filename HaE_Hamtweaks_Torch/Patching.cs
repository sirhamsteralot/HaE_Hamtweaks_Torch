using System;
using System.Text;
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

        public static void Patch(PatchContext ctx)
        {
            // PAINTING PATCH (allows faction mates to paint too)
            ctx.GetPattern(typeof(MyCubeGrid).GetMethod("ColorGridOrBlockRequestValidation", BindingFlags.NonPublic | BindingFlags.Instance)).Prefixes
                .Add(typeof(Patching).GetMethod("PrefixColorGridOrBlockRequestValidation", BindingFlags.Public | BindingFlags.Static));

            // LCD PATCH
            //ctx.GetPattern(typeof(MyTextPanel).GetConstructor(Type.EmptyTypes)).Suffixes
            //    .Add(typeof(Patching).GetMethod("SuffixLCDInitialized", BindingFlags.Public | BindingFlags.Static));

            //UpdateMassFromInventoriesPatch
            ctx.GetPattern(typeof(MyGridShape).GetMethod("UpdateMassFromInventories", BindingFlags.Public | BindingFlags.Instance)).Prefixes
                .Add(typeof(Patching).GetMethod("PrefixUpdateMassFromInventories", BindingFlags.Public | BindingFlags.Static));

            //UpdateSounds MyGasGenerator
            ctx.GetPattern(typeof(MyGasGenerator).GetMethod("UpdateSounds", BindingFlags.NonPublic | BindingFlags.Instance)).Prefixes
                .Add(typeof(Patching).GetMethod("PrefixJustNo", BindingFlags.Public | BindingFlags.Static));

            Log.Info("Finished Patching!");
        }

        private static Dictionary<MyGridShape, TimeSpan> lastUpdateTimestamp = new Dictionary<MyGridShape, TimeSpan>();
        public static bool PrefixUpdateMassFromInventories(List<MyCubeBlock> blocks, MyPhysicsBody rb, MyGridShape __instance)
        {
            TimeSpan time;
            if (!lastUpdateTimestamp.TryGetValue(__instance, out time))
            {
                lastUpdateTimestamp[__instance] = MySession.Static.ElapsedGameTime;
                return true;
            }

            if ((MySession.Static.ElapsedGameTime - time).TotalMilliseconds < 1000)
                return false;

            lastUpdateTimestamp[__instance] = MySession.Static.ElapsedGameTime;

            return true;
        }

        public static bool PrefixJustNo()
        {
            return false;
        }

        public static void SuffixLCDInitialized(MyTextPanel __instance)
        {
            typeof(MyTextPanel).GetField("m_publicTitle", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, new StringBuilder(32000));
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
