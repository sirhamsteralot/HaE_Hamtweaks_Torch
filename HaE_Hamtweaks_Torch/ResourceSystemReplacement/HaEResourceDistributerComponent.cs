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

namespace HaE_Hamtweaks_Torch.ResourceSystemReplacement
{
    public class HaEResourceDistributerComponent : MyResourceDistributorComponent
    {
        public HaEResourceDistributerComponent(string debugName) : base(debugName)
        {

        }

        public new void UpdateBeforeSimulation()
        {

        }
    }
}
