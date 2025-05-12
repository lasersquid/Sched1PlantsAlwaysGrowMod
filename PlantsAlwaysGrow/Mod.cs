using System;
using MelonLoader;
using HarmonyLib;


[assembly: MelonInfo(typeof(PlantsAlwaysGrow.PlantsAlwaysGrowMod), "PlantsAlwaysGrowMod", "1.0.0", "lasersquid", null)]
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
            LoggerInstance.Msg("EmployeesAlwaysWorkWithoutBedsMod Initialized.");
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
                    base.LoggerInstance.Msg("Plants Always Grow successfully initialized in scene: " + sceneName);
                }
                catch (Exception ex)
                {
                    base.LoggerInstance.Error("Failed to patch mod: " + ex.Message);
                }
            }
        }
    }



}

