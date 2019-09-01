using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using NLog;

namespace HaE_Hamtweaks_Torch
{
    public class HaEHamtweaks : TorchPluginBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            var pgmr = new HaEPatchTweaksManager(torch);
            torch.Managers.AddManager(pgmr);

            Log.Info("Hamtweaks loaded!");
        }

        public override void Update()
        {
            base.Update();
        }

    }
}
