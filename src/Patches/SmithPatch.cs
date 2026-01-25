using HarmonyLib;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace BlessedClasses.src.Patches {

    /// <summary>
    /// gives items crafted by players with the "smith" trait a durability bonus.
    ///
    /// pattern used here:
    /// 1. hook into OnCreatedByCrafting to detect when a smith crafts something
    /// 2. store a bonus multiplier in the item's Attributes (persists with the item)
    /// 3. hook into GetMaxDurability to apply that bonus whenever durability is queried
    ///
    /// this same pattern can be adapted for other crafting bonuses:
    /// - different traits checking for different bonuses
    /// - storing different attribute values (quality, special effects, etc.)
    /// - applying bonuses to other item properties
    /// </summary>
    [HarmonyPatch(typeof(CollectibleObject))]
    [HarmonyPatchCategory(BlessedClassesModSystem.SmithPatchCategory)]
    public class SmithPatch {

        /// <summary>
        /// runs AFTER an item is crafted. checks if the crafter has the smith trait,
        /// and if so, marks the crafted item with a durability bonus.
        ///
        /// key Harmony parameters available:
        /// - allInputslots: all ingredients used in the recipe
        /// - outputSlot: the resulting crafted item
        /// - byRecipe: the recipe that was used
        /// note: player is NOT a parameter - get it from outputSlot.Inventory
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CollectibleObject.OnCreatedByCrafting))]
        private static void SmithCraftedPostfix(
            ItemSlot[] allInputslots,
            ItemSlot outputSlot,
            GridRecipe byRecipe) {

            // get player from the crafting inventory (InventoryBasePlayer stores playerUID)
            IPlayer byPlayer = (outputSlot.Inventory as InventoryBasePlayer)?.Player;

            if (byPlayer == null || outputSlot?.Itemstack == null) {
                return;
            }

            // only apply to items with durability (tools, weapons, armor)
            // items without durability return 0 or 1
            int maxDurability = outputSlot.Itemstack.Collectible.GetMaxDurability(outputSlot.Itemstack);
            if (maxDurability <= 1) {
                return;
            }

            // to check if a player has a specific trait:
            // 1. get their character class code from WatchedAttributes
            // 2. look up the CharacterClass object from the CharacterSystem
            // 3. check if that class's Traits list contains the trait you want
            string classcode = byPlayer.Entity.WatchedAttributes.GetString("characterClass");
            CharacterClass charclass = byPlayer.Entity.Api.ModLoader
                .GetModSystem<CharacterSystem>()
                .characterClasses
                .FirstOrDefault(c => c.Code == classcode);

            if (charclass != null && charclass.Traits.Contains("smith")) {
                // ItemStack.Attributes is a persistent key-value store on each item
                // anything stored here survives saving/loading and travels with the item
                // use a namespaced key (e.g., "blessedclasses:smithCrafted") to avoid conflicts
                outputSlot.Itemstack.Attributes.SetFloat(
                    BlessedClassesModSystem.SmithCraftedAttribute,
                    BlessedClassesModSystem.SmithDurabilityBonus);
            }
        }

        /// <summary>
        /// runs AFTER GetMaxDurability calculates the base durability.
        /// if the item was crafted by a smith, multiply the result by the bonus.
        ///
        /// key Harmony parameter:
        /// - ref int __result: the return value of the original method (can be modified)
        /// - ItemStack itemstack: the item being queried
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CollectibleObject.GetMaxDurability))]
        private static void SmithDurabilityBonusPostfix(ref int __result, ItemStack itemstack) {
            if (itemstack == null) {
                return;
            }

            // check if this item has our smith-crafted attribute
            if (itemstack.Attributes.HasAttribute(BlessedClassesModSystem.SmithCraftedAttribute)) {
                float bonus = itemstack.Attributes.GetFloat(
                    BlessedClassesModSystem.SmithCraftedAttribute,
                    1f); // default to 1.0 (no change) if something goes wrong
                __result = (int)MathF.Round(__result * bonus);
            }
        }
    }
}
