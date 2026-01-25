using HarmonyLib;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace BlessedClasses.src.Patches {

    [HarmonyPatch(typeof(CollectibleObject))]
    [HarmonyPatchCategory(BlessedClassesModSystem.SmithPatchCategory)]
    public class SmithPatch {

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CollectibleObject.OnCreatedByCrafting))]
        private static void SmithCraftedPostfix(
            ItemSlot[] allInputslots,
            ItemSlot outputSlot,
            GridRecipe byRecipe,
            IPlayer byPlayer) {

            if (byPlayer == null || outputSlot?.Itemstack == null) {
                return;
            }

            // only apply to items with durability (tools, weapons, armor)
            int maxDurability = outputSlot.Itemstack.Collectible.GetMaxDurability(outputSlot.Itemstack);
            if (maxDurability <= 1) {
                return;
            }

            // check if crafter has smith trait
            string classcode = byPlayer.Entity.WatchedAttributes.GetString("characterClass");
            CharacterClass charclass = byPlayer.Entity.Api.ModLoader
                .GetModSystem<CharacterSystem>()
                .characterClasses
                .FirstOrDefault(c => c.Code == classcode);

            if (charclass != null && charclass.Traits.Contains("smith")) {
                // mark item as smith-crafted with durability bonus
                outputSlot.Itemstack.Attributes.SetFloat(
                    BlessedClassesModSystem.SmithCraftedAttribute,
                    BlessedClassesModSystem.SmithDurabilityBonus);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CollectibleObject.GetMaxDurability))]
        private static void SmithDurabilityBonusPostfix(ref int __result, ItemStack itemstack) {
            if (itemstack == null) {
                return;
            }

            if (itemstack.Attributes.HasAttribute(BlessedClassesModSystem.SmithCraftedAttribute)) {
                float bonus = itemstack.Attributes.GetFloat(
                    BlessedClassesModSystem.SmithCraftedAttribute,
                    1f);
                __result = (int)MathF.Round(__result * bonus);
            }
        }
    }
}
