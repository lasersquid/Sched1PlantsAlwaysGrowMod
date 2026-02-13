using HarmonyLib;
using MelonLoader;
using UnityEngine.Diagnostics;
using UnityEngine.Events;
using UnityEngine;
using System.Reflection;

#if MONO_BUILD
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Growing;
using ScheduleOne.Lighting;
using Action = System.Action;
#else
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Growing;
using Il2CppScheduleOne.Lighting;
using Action = Il2CppSystem.Action;
using ActionList = Il2Cpp.ActionList;
#endif

// Yes I intended a reference comparison. Stop making yellow squigglies at me.
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast

namespace PlantsAlwaysGrow
{
    public static class Utils
    {
        public static PlantsAlwaysGrowMod Mod;

        private static Assembly S1Assembly;

        public static void Initialize(PlantsAlwaysGrowMod mod)
        {
            Mod = mod;
#if !MONO_BUILD
            S1Assembly = AppDomain.CurrentDomain.GetAssemblies().First((Assembly a) => a.GetName().Name == "Assembly-CSharp");
#endif
        }

        // Reflection convenience methods.
        // Needed to access private members in mono.
        // Also handles the property-fying of fields in IL2CPP.
        // Treturn cannot be an interface type in IL2CPP; use ToInterface for that.

        public static Treturn GetField<Ttarget, Treturn>(string fieldName, object target) where Treturn : class
        {
            return (Treturn)GetField<Ttarget>(fieldName, target);
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

        public static Treturn GetProperty<Ttarget, Treturn>(string fieldName, object target)
        {
            return (Treturn)GetProperty<Ttarget>(fieldName, target);
        }

        public static object GetProperty<Ttarget>(string fieldName, object target)
        {
            return AccessTools.Property(typeof(Ttarget), fieldName).GetValue(target);
        }

        public static void SetProperty<Ttarget>(string fieldName, object target, object value)
        {
            AccessTools.Property(typeof(Ttarget), fieldName).SetValue(target, value);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target)
        {
            return (Treturn)CallMethod<Ttarget>(methodName, target, []);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, object target, object[] args)
        {
            return (Treturn)CallMethod<Ttarget>(methodName, target, args);
        }

        public static Treturn CallMethod<Ttarget, Treturn>(string methodName, Type[] argTypes, object target, object[] args)
        {
            return (Treturn)CallMethod<Ttarget>(methodName, argTypes, target, args);
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
        

        // Type checking and conversion methods

        // In IL2CPP, do a type check before performing a forced cast, returning default (usually null) on failure.
        // In Mono, do a type check before a regular cast, returning default on type check failure.
        // You can't use CastTo with T as an Il2Cpp interface; use ToInterface for that.
#if MONO_BUILD
        public static T CastTo<T>(object o)
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                return default(T);
            }
        }
#else
        public static T CastTo<T>(Il2CppObjectBase o) where T : Il2CppObjectBase
        {
            if (typeof(T).IsAssignableFrom(GetType(o)))
            {
                return (T)System.Activator.CreateInstance(typeof(T), [o.Pointer]);
            }
            return default(T);
        }
#endif

        // Under Il2Cpp, "is" operator only looks at local scope for type info,
        // instead of checking object identity. 
        // Check against actual object type obtained via GetType.
        // In Mono, use standard "is" operator.
        // Will always return false for Il2Cpp interfaces.
#if MONO_BUILD
        public static bool Is<T>(object o)
        {
            return o is T;
        }
#else
        public static bool Is<T>(Il2CppObjectBase o) where T : Il2CppObjectBase
        {
            return typeof(T).IsAssignableFrom(GetType(o));
        }
#endif

        // You can't cast to an interface type in IL2CPP, since interface info is stripped.
        // Use this method to perform a blind cast without type checking.
        // In Mono, just do a regular cast.
#if MONO_BUILD
        public static T ToInterface<T>(object o)
        {
            return (T)o;
        }
#else
        public static T ToInterface<T>(Il2CppObjectBase o) where T : Il2CppObjectBase
        {
            return (T)System.Activator.CreateInstance(typeof(T), [o.Pointer]);
        }
#endif

        // Get actual identity of Il2Cpp objects based on their ObjectClass, and
        // convert between Il2CppScheduleOne and ScheduleOne namespaces.
        // In Mono, return object.GetType or null.
#if MONO_BUILD
        public static Type GetType(object o)
        {
            if (o == null)
            {
                return null;
            }
            return o.GetType();
        }
#else
        public static Type GetType(Il2CppObjectBase o)
        {
            if (o == null)
            {
                return null;
            }

            string typeName = Il2CppType.TypeFromPointer(o.ObjectClass).FullName;
            return S1Assembly.GetType($"Il2Cpp{typeName}");
        }
#endif

        // Convert a System.Action to a unity action.
        public static UnityAction ToUnityAction(System.Action action)
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

        // Convert a delegate to a predicate that IL2CPP ienumerable functions can actually use.
#if MONO_BUILD
        public static Predicate<T> ToPredicate<T>(Func<T, bool> func)
        {
            return new Predicate<T>(func);
        }
#else
        public static Il2CppSystem.Predicate<T> ToPredicate<T>(Func<T, bool> func)
        {
            return DelegateSupport.ConvertDelegate<Il2CppSystem.Predicate<T>>(func);
        }
#endif

#if MONO_BUILD
        public static Action GetActionFromList(ActionList list, Func<Action, bool> predicate)
        {
            return list.GetInvocationList().Find(Utils.ToPredicate<Action>(predicate));
        }
#else
        public static Action GetActionFromList(Il2Cpp.ActionList list, Func<Action, bool> predicate)
        {
            return list.list.Find(Utils.ToPredicate<Action>(predicate));
        }
#endif

#if MONO_BUILD
        public static void RemoveActionFromList(ActionList list, Action action)
        {
            list -= action;
        }
#else
        public static void RemoveActionFromList(ActionList list, Action action)
        {
            list.Remove(action);
        }
#endif

#if MONO_BUILD
        public static void AddActionToList(ActionList list, Action action)
        {
            list += action;
        }
#else
        public static void AddActionToList(ActionList list, Action action)
        {
            list.Add(action);
        }
#endif

public static void Log(string message)
        {
            Mod.LoggerInstance.Msg(message);
        }

        public static void Warn(string message)
        {
            Mod.LoggerInstance.Warning(message);
        }

        public static void PrintException(Exception e)
        {
            Warn($"Exception: {e.GetType().Name} - {e.Message}");
            Warn($"Source: {e.Source}");
            Warn($"{e.StackTrace}");
            if (e.InnerException != null)
            {
                Warn($"Inner exception: {e.InnerException.GetType().Name} - {e.InnerException.Message}");
                Warn($"Source: {e.InnerException.Source}");
                Warn($"{e.InnerException.StackTrace}");
                if (e.InnerException.InnerException != null)
                {
                    Warn($"Inner inner exception: {e.InnerException.InnerException.GetType().Name} - {e.InnerException.InnerException.Message}");
                    Warn($"Source: {e.InnerException.InnerException.Source}");
                    Warn($"{e.InnerException.InnerException.StackTrace}");
                }
            }
        }

        // Check if a particular MelonMod is loaded.
        public static bool OtherModIsLoaded(string modName)
        {
            List<MelonBase> registeredMelons = new List<MelonBase>(MelonBase.RegisteredMelons);
            MelonBase melon = registeredMelons.Find(new Predicate<MelonBase>(m => m.Info.Name == modName));
            return (melon != null);
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
        public static void PlantMinPassPostfix(Plant __instance, int mins)
        {
            if (__instance.NormalizedGrowthProgress >= 1f)
            {
                return;
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
            return;
        }

        [HarmonyPatch(typeof(ShroomColony), "OnMinPass")]
        [HarmonyPostfix]
        public static void ShroomOnMinPassPostfix(ShroomColony __instance)
        {
            if (NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
            {
                int growTime = (int)Utils.GetField<ShroomColony>("_growTime", __instance);
                float currentGrowthRate = (float)Utils.CallMethod<ShroomColony>("GetCurrentGrowthRate", __instance);
                float change = currentGrowthRate / ((float)growTime * 60f);
                Utils.CallMethod<ShroomColony>("ChangeGrowthPercentage", __instance, [change]);
            }
            return;
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

        [HarmonyPatch(typeof(GrowContainer), "InitializeGridItem")]
        [HarmonyPostfix]
        public static void InitializeGridItemPostfix(GrowContainer __instance)
        {
            // Remove GrowContainer.OnMinPass from onMinutePass list and add it to onUncappedMinutePass
            TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
            Action action = Utils.GetActionFromList(timeManager.onMinutePass, (Action a) => 
                a.Method?.Name == "OnMinPass" && a.Target == __instance
            );
            if (action != null)
            {
                Utils.RemoveActionFromList(timeManager.onMinutePass, action);
                Utils.AddActionToList(timeManager.onUncappedMinutePass, action);
            }
        }

        [HarmonyPatch(typeof(GrowContainer), "Destroy")]
        [HarmonyPrefix]
        public static void DestroyPrefix(GrowContainer __instance)
        {
            // Put GrowContainer.OnMinPass back on the onMinutesPass list so this container can be destroyed cleanly
            TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
            Action action = Utils.GetActionFromList(timeManager.onUncappedMinutePass, (Action a) => 
                a.Method?.Name == "OnMinPass" && a.Target == __instance
            );
            if (action != null)
            {
                Utils.RemoveActionFromList(timeManager.onUncappedMinutePass, action);
                Utils.AddActionToList(timeManager.onMinutePass, action);
            }
        }
    }
}
