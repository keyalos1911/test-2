using BlessedClasses.src.BlockBehaviors;
using BlessedClasses.src.CollectibleBehaviors;
using BlessedClasses.src.Diagnostics;
using BlessedClasses.src.Diagnostics.Patches;
using BlessedClasses.src.EntityBehaviors;
using BlessedClasses.src.Blocks;
using BlessedClasses.src.Patches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Vintagestory;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;


namespace BlessedClasses.src
{

    public class BlessedClassesModSystem : ModSystem
    {
        public static Harmony harmony;

        public const string ClayformingPatchesCategory = "BlessedClassesClayformingPatchesCatagory";
        public const string SilverTonguePatchesCategory = "BlessedClassesSilverTonguePatchCategory";
        public const string SpecialStockPatchesCategory = "BlessedClassesSpecialStockPatchesCategory";
        public const string DragonskinPatchCategory = "BlessedClassesDragonskinPatchCategory";
        public const string DiagnosticPatchCategory = "BlessedClassesDiagnosticsPatchCategory";
        public const string CrockCraftingPatchCategory = "BlessedClassesCrockCraftingPatchCategory";
    public override void StartPre(ICoreAPI api) {
    Api = api;
    Logger = Mod.Logger;
    ModID = Mod.Info.ModID;

    // Initialize diagnostic systems
    DiagnosticLogger.Initialize(api, Logger);
    //MeshDiagnostics.Initialize(api);
}
        public static ICoreAPI Api;
        public static ICoreClientAPI CApi;
        public static ICoreServerAPI SApi;
        public static ILogger Logger;
        public static string ModID;
        public const string FlaxRateStat = "flaxFiberChance";
        public const string BonusClayVoxelsStat = "clayformingPoints";
        public override void Start(ICoreAPI api)
        {
            api.RegisterCollectibleBehaviorClass("HealHackedBehavior", typeof(HealHackedLocustsBehavior));
            api.RegisterBlockBehaviorClass("UnlikelyHarvestBehavior", typeof(UnlikelyHarvestBlockBehavior));
            api.RegisterEntityBehaviorClass("EntityBehaviorDread", typeof(DreadBehavior));
            api.RegisterEntityBehaviorClass("EntityBehaviorFanatic", typeof(FanaticBehavior));
            api.RegisterEntityBehaviorClass("EntityBehaviorTemporalTraits", typeof(TemporalStabilityTraitBehavior));
            api.RegisterEntityBehaviorClass("EntityBehaviorDragonskin", typeof(DragonskinTraitBehavior));
            api.RegisterBlockClass("BlockCarvedCrock", typeof(BlockCarvedCrock));

            ApplyPatches();

            // log startup diagnostics
            Logger.Notification("═══════════════════════════════════════════");
            Logger.Notification("blessedclasses v{0}", Mod.Info.Version);
            Logger.Notification("═══════════════════════════════════════════");
            Logger.Notification("");
            Logger.Notification("Diagnostic Features:");
            Logger.Notification("  • Mod compatibility detection");
            Logger.Notification("  • Character/trait system monitoring");
            Logger.Notification("  • Environment detection logging");
            Logger.Notification("");

            // run diagnostic checks
            DiagnosticLogger.LogModLoadOrder(api);
            DiagnosticLogger.LogThirdPartyModPresence(api);
            DiagnosticLogger.LogCharselCommandNote();
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            SApi = api;

            // register player join event for class migration
            api.Event.PlayerJoin += OnPlayerJoin;
        }

        /// <summary>
        /// mapping of vanilla class codes to their BlessedClasses equivalents.
        /// used to auto-migrate players who had vanilla classes before installing this mod.
        /// </summary>
        private static readonly Dictionary<string, string> VanillaToBlessedClassMap = new() {
            { "commoner", "lordofthewilds" },
            { "hunter", "lordofthehunt" },
            { "malefactor", "lordoftheruins" },
            { "clockmaker", "lordofinvention" },
            { "blackguard", "lordofdeath" },
            { "tailor", "lordofthecloth" }
        };

        private void OnPlayerJoin(IServerPlayer player)
        {
            try
            {
                if (player.Entity == null) return;

                string currentClass = player.Entity.WatchedAttributes.GetString("characterClass");
                if (string.IsNullOrEmpty(currentClass)) return;

                // check if the player has a vanilla class that needs migration
                if (VanillaToBlessedClassMap.TryGetValue(currentClass, out string newClass))
                {
                    // migrate to BlessedClasses equivalent
                    player.Entity.WatchedAttributes.SetString("characterClass", newClass);
                    player.Entity.WatchedAttributes.MarkPathDirty("characterClass");

                    DiagnosticLogger.LogClassMigration(player.PlayerName, currentClass, newClass);

                    // notify the player
                    player.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        $"[BlessedClasses] Your character class has been migrated from '{currentClass}' to '{newClass}'.",
                        EnumChatType.Notification
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Error("[BlessedClasses] REPORT THIS! Error during player class migration: {0}", ex.Message);
            }
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            // verify character system state after assets load
            DiagnosticLogger.LogCharacterSystemState(api);
            DiagnosticLogger.LogAvailableCharacterClasses(api);
        }

        private static void ApplyPatches()
        {
            if (harmony != null)
            {
                return;
            }

            harmony = new Harmony(ModID);
            Logger.VerboseDebug("Harmony is starting Patches!");
            harmony.PatchCategory(ClayformingPatchesCategory);
            harmony.PatchCategory(SilverTonguePatchesCategory);
            harmony.PatchCategory(SpecialStockPatchesCategory);
            harmony.PatchCategory(DragonskinPatchCategory);
            harmony.PatchCategory(CrockCraftingPatchCategory);

            // apply diagnostic patches
            TraitSystemDiagnostics.ApplyTraitSystemPatches(harmony);

            // apply CharacterSystem diagnostic patches (uses Harmony attributes)
            try
            {
                harmony.CreateClassProcessor(typeof(CharacterSystemDiagnosticPatches)).Patch();
                Logger.VerboseDebug("[BlessedClasses] CharacterSystem diagnostic patches applied");
            }
            catch (Exception ex)
            {
                Logger.Error("[BlessedClasses] Failed to apply CharacterSystem diagnostic patches: {0}", ex.Message);
            }

            Logger.VerboseDebug("[BlessedClasses] Diagnostic patches applied");

            Logger.VerboseDebug("Finished patching for Trait purposes.");
        }

        private static void HarmonyUnpatch()
        {
            Logger.VerboseDebug("Unpatching Harmony Patches.");
            harmony.UnpatchAll(ModID);
            harmony = null;
        }

        public override void Dispose()
        {
            HarmonyUnpatch();
            Logger = null;
            ModID = null;
            Api = null;
            base.Dispose();
        }
    }
}
