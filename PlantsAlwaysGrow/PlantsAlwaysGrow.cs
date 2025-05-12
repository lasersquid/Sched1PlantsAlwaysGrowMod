using HarmonyLib;

#if MONO_BUILD
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Growing;
#else
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Growing;
#endif

namespace PlantsAlwaysGrow
{
    [HarmonyPatch(typeof(Plant), "MinPass")]
    public static class PlantMinPassPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Plant __instance)
        {
            if (__instance.NormalizedGrowthProgress >= 1f)
            {
                return false;
            }
            TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
            if ((UnityEngine.Object)(object)timeManager == null || !timeManager.IsEndOfDay)
            {
                return true;
            }
            float num = 1f / ((float)__instance.GrowthTime * 60f);
            num *= __instance.Pot.GetAdditiveGrowthMultiplier();
            num *= __instance.Pot.GetAverageLightExposure(out var growSpeedMultiplier);
            num *= __instance.Pot.GrowSpeedMultiplier;
            num *= growSpeedMultiplier;
            if (__instance.Pot.NormalizedWaterLevel <= 0f || __instance.Pot.NormalizedWaterLevel > 1f)
            {
                num *= 0f;
            }
            __instance.SetNormalizedGrowthProgress(__instance.NormalizedGrowthProgress + num);
            return false;
        }
    }
}
