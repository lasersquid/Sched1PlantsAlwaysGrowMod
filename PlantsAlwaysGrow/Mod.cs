using System;
using MelonLoader;
using HarmonyLib;


[assembly: MelonInfo(typeof(PlantsAlwaysGrow.PlantsAlwaysGrowMod), "PlantsAlwaysGrowMod", "1.2.0", "lasersquid", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace PlantsAlwaysGrow
{
    public class PlantsAlwaysGrowMod : MelonMod
    {
        private static bool _isPatchApplied;

        private static HarmonyLib.Harmony _harmonyInstance;

        public override void OnInitializeMelon()
        {
            MelonEvents.OnSceneWasLoaded.Subscribe(OnSceneLoaded);
            Utils.Initialize(this);
            LoggerInstance.Msg("PlantsAlwaysGrow initialized.");
        }

        private void OnSceneLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Main" && !_isPatchApplied)
            {
                try
                {
                    _harmonyInstance = new HarmonyLib.Harmony("PlantsAlwaysGrowMod");
                    _harmonyInstance.PatchAll();
                    _isPatchApplied = true;
                }
                catch (Exception ex)
                {
                    base.LoggerInstance.Error("Failed to patch mod: " + ex.Message);
                }
            }
        }
    }
}

// todo
// update project configuration - done
// pull in generic utils class - done
// fix initialization msg - done
// shrooms update - done
// storage update - done (1.2.0)
