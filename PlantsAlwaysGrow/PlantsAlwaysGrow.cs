using HarmonyLib;
using MelonLoader;
using UnityEngine.Diagnostics;
using UnityEngine.Events;
using UnityEngine;

#if MONO_BUILD
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Growing;
using ScheduleOne.Lighting;
using ScheduleOne.ObjectScripts;
using ShroomList = System.Collections.Generic.List<ScheduleOne.Growing.GrowingMushroom>;
#else
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Growing;
using Il2CppScheduleOne.Lighting;
using Il2CppScheduleOne.ObjectScripts;
using ShroomList = Il2CppSystem.Collections.Generic.List<Il2CppScheduleOne.Growing.GrowingMushroom>;
#endif

namespace PlantsAlwaysGrow
{
    public static class Utils
    {
        public static PlantsAlwaysGrowMod Mod;

        public static void PrintException(Exception e)
        {
            Utils.Warn($"Exception: {e.GetType().Name} - {e.Message}");
            Utils.Warn($"Source: {e.Source}");
            Utils.Warn($"{e.StackTrace}");
            if (e.InnerException != null)
            {
                Utils.Warn($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
                Utils.Warn($"Source: {e.InnerException.Source}");
                Utils.Warn($"{e.InnerException.StackTrace}");
                if (e.InnerException.InnerException != null)
                {
                    Utils.Warn($"Inner inner exception: {e.InnerException.InnerException.GetType().Name} - {e.InnerException.InnerException.Message}");
                    Utils.Warn($"Source: {e.InnerException.InnerException.Source}");
                    Utils.Warn($"{e.InnerException.InnerException.StackTrace}");
                }
            }
        }

        public static Treturn GetField<Ttarget, Treturn>(string fieldName, object target) where Treturn : class
        {
            return CastTo<Treturn>(GetField<Ttarget>(fieldName, target));
        }

        public static object GetField<Ttarget>(string fieldName, object target)
        {
#if MONO_BUILD
            return AccessTools.Field(typeof(Ttarget), fieldName).GetValue(target);
#else
            return AccessTools.Property(typeof(Ttarget), fieldName).GetValue(target);
#endif
        }

        public static void SetField<Ttarget>(string fieldName, object target, object value)
        {
#if MONO_BUILD
            AccessTools.Field(typeof(Ttarget), fieldName).SetValue(target, value);
#else
            AccessTools.Property(typeof(Ttarget), fieldName).SetValue(target, value);
#endif
        }

        public static Treturn GetProperty<Ttarget, Treturn>(string fieldName, object target) where Treturn : class
        {
            return CastTo<Treturn>(GetProperty<Ttarget>(fieldName, target));
        }

        public static object GetProperty<Ttarget>(string fieldName, object target)
        {
            return AccessTools.Property(typeof(Ttarget), fieldName).GetValue(target);
        }

        public static void SetProperty<Ttarget>(string fieldName, object target, object value)
        {
            AccessTools.Property(typeof(Ttarget), fieldName).SetValue(target, value);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target) where Treturn : class
        {
            return CastTo<Treturn>(CallMethod<Ttarget>(methodName, target, []));
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target, object[] args) where Treturn : class
        {
            return CastTo<Treturn>(CallMethod<Ttarget>(methodName, target, args));
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, Type[] argTypes, object target, object[] args) where Treturn : class
        {
            return CastTo<Treturn>(CallMethod<Ttarget>(methodName, argTypes, target, args));
        }

        public static object CallMethod<Ttarget>(string methodName, object target)
        {
            return AccessTools.Method(typeof(Ttarget), methodName).Invoke(target, []);
        }

        public static object CallMethod<Ttarget>(string methodName, object target, object[] args)
        {
            return AccessTools.Method(typeof(Ttarget), methodName).Invoke(target, args);
        }

        public static object CallMethod<Ttarget>(string methodName, Type[] argTypes, object target, object[] args)
        {
            return AccessTools.Method(typeof(Ttarget), methodName, argTypes).Invoke(target, args);
        }

        public static void SetMod(PlantsAlwaysGrowMod mod)
        {
            Mod = mod;
        }

        public static T CastTo<T>(object o) where T : class
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                return null;
            }
        }

        public static bool Is<T>(object o)
        {
            return o is T;
        }

#if !MONO_BUILD
        public static T CastTo<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        { 
            return o.TryCast<T>();
        }

        public static bool Is<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        {
            return o.TryCast<T>() != null;
        }
#endif

        public static UnityAction ToUnityAction(Action action)
        {
#if MONO_BUILD
            return new UnityAction(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction>(action);
#endif
        }

        public static UnityAction<T> ToUnityAction<T>(System.Action<T> action)
        {
#if MONO_BUILD
            return new UnityAction<T>(action);
#else
            return DelegateSupport.ConvertDelegate<UnityAction<T>>(action);
#endif
        }

#if MONO_BUILD
        public static T ToInterface<T>(object o)
        {
            return (T)o;
        }
#else
        public static T ToInterface<T>(Il2CppSystem.Object o) where T : Il2CppObjectBase
        {
            return CastTo<T>(System.Activator.CreateInstance(typeof(T), [o.Pointer]));
        }
#endif

        public static void Log(string message)
        {
            Utils.Mod.LoggerInstance.Msg(message);
        }

        public static void Warn(string message)
        {
            Utils.Mod.LoggerInstance.Warning(message);
        }

        // Compare unity objects by their instance ID
        public class UnityObjectComparer : IEqualityComparer<UnityEngine.Object>
        {
            public bool Equals(UnityEngine.Object a, UnityEngine.Object b)
            {
                return a.GetInstanceID() == b.GetInstanceID();
            }

            public int GetHashCode(UnityEngine.Object item)
            {
                return item.GetInstanceID();
            }
        }
    }

    [HarmonyPatch]
    public static class AlwaysGrowPatches
    {
        [HarmonyPatch(typeof(Plant), "MinPass")]
        [HarmonyPostfix]
        public static bool PlantMinPassPostfix(Plant __instance, int mins)
        {
            if (__instance.NormalizedGrowthProgress >= 1f)
            {
                return false;
            }
            if (NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
            {
                float num = 1f / ((float)__instance.GrowthTime * 60f * (float)mins);
                num *= __instance.Pot.GetTemperatureGrowthMultiplier();
                float num2;
                num *= GetAverageLightExposure(__instance.Pot, out num2);
                num *= __instance.Pot.GrowSpeedMultiplier;
                num *= num2;
                if (GameManager.IS_TUTORIAL)
                {
                    num *= 0.3f;
                }
                if (__instance.Pot.NormalizedMoistureAmount <= 0f)
                {
                    num *= 0f;
                }
                __instance.SetNormalizedGrowthProgress(__instance.NormalizedGrowthProgress + num);
            }
            return false;
        }

        [HarmonyPatch(typeof(ShroomColony), "OnMinPass")]
        [HarmonyPostfix]
        public static void ShroomOnMinPassPostfix(ShroomColony __instance)
        {
            if (NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
            {
                int growTime = (int)Utils.GetField<ShroomColony>("_growTime", __instance);
                float currentGrowthRate = GetCurrentGrowthRate(__instance);
                ChangeGrowthPercentage(__instance, (currentGrowthRate / ((float)growTime * 60f)));
            }
            return;
        }

        private static float GetCurrentGrowthRate(ShroomColony colony)
        {
            try
            {
                return (float)Utils.CallMethod<ShroomColony>("GetCurrentGrowthRate", colony);
            }
            catch (Exception e)
            {
                Utils.Warn($"ShroomColony.GetCurrentGrowthRate is inlined, probably");
            }

            if (colony.IsTooHotToGrow)
            {
                return 0f;
            }
            if (Utils.GetField<ShroomColony, MushroomBed>("_parentBed", colony).NormalizedMoistureAmount <= 0.0001f)
            {
                return 0f;
            }
            return 1f;
        }

        private static void ChangeGrowthPercentage(ShroomColony colony, float change)
        {
            try
            {
                Utils.CallMethod<ShroomColony>("ChangeGrowthPercentage", colony, [change]);
                return;
            }
            catch (Exception e)
            {
                Utils.Warn($"ShroomColony.ChangeGrowthPercentage was inlined");
            }

            try
            {
                Utils.CallMethod<ShroomColony>("SetGrowthPercentage", colony, [colony.GrowthProgress + change]);
                return;
            }
            catch (Exception e)
            {
                Utils.Warn($"ShroomColony.SetGrowthPercentage was inlined also");
            }

            float percent = colony.GrowthProgress + change;
            if (Mathf.Approximately(percent, colony.GrowthProgress))
            {
                return;
            }
            Utils.SetProperty<ShroomColony>("GrowthProgress", colony, percent);
            ShroomList growingShrooms = Utils.GetField<ShroomColony, ShroomList>("_growingMushrooms", colony);
            foreach (GrowingMushroom growingMushroom in growingShrooms)
            {
                // if this is inlined i swear to god
                growingMushroom.SetGrowthPercent(colony.GrowthProgress);
            }

            ParticleSystem fullyGrownParticles = Utils.GetField<ShroomColony, ParticleSystem>("_fullyGrownParticles", colony);
            if (colony.IsFullyGrown)
            {
                if (!fullyGrownParticles.isPlaying)
                {
                    fullyGrownParticles.Play();
                    return;
                }
            }
            else if (fullyGrownParticles.isPlaying)
            {
                fullyGrownParticles.Stop();
            }
        }


        // This function has been optimized out completely--vtable entry is blank??
        // Call a local copy
        private static float GetAverageLightExposure(GrowContainer container, out float growSpeedMultiplier)
        {
            growSpeedMultiplier = 1f;
            UsableLightSource lightSourceOverride = Utils.GetField<GrowContainer, UsableLightSource>("_lightSourceOverride", container);
            if (lightSourceOverride != null)
            {
                return lightSourceOverride.GrowSpeedMultiplier;
            }
            float num = 0f;
            for (int i = 0; i < container.CoordinatePairs.Count; i++)
            {
                float num2;
                num += container.OwnerGrid.GetTile(container.CoordinatePairs[i].coord2).LightExposureNode.GetTotalExposure(out num2);
                growSpeedMultiplier += num2;
            }
            growSpeedMultiplier /= (float)container.CoordinatePairs.Count;
            return num / (float)container.CoordinatePairs.Count;
        }
    }
}
