using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Central damage-scaling algorithm. Every guardian's base damage
    /// values (bite, sting, shot, breath, constrict) are multiplied
    /// through GetMultiplier()/ScaleDamage() before being applied to the
    /// player, keyed by vault index (0-4). This is the single source of
    /// truth for difficulty scaling — tune the curve here once, and every
    /// guardian across every vault reflects the change automatically,
    /// rather than hand-editing damage numbers on five separate scripts.
    ///
    /// Counts as one of the project's required Gameplay Logic algorithms
    /// (damage calculation) — README algorithm write-up: purpose is
    /// smooth difficulty progression across 5 vaults; approach is a
    /// simple linear-ish multiplier curve; selected over a flat/random
    /// approach because it's predictable, easy to balance-test, and easy
    /// to explain/justify in the presentation.
    /// </summary>
    public static class DifficultyCurve
    {
        // Index-aligned with vault index: 0=Scorpion, 1=Anaconda,
        // 2=Armed Guard, 3=Dragon, 4=3 Dinosaurs (final).
        private static readonly float[] DamageMultipliers = { 1.00f, 1.15f, 1.30f, 1.50f, 1.75f };

        // Enemy health can scale too, independent of damage output, so a
        // vault can be "more damage" or "more tanky" or both.
        private static readonly float[] HealthMultipliers = { 1.00f, 1.10f, 1.25f, 1.40f, 1.60f };

        public static float GetDamageMultiplier(int vaultIndex)
        {
            if (vaultIndex < 0 || vaultIndex >= DamageMultipliers.Length) return 1f;
            return DamageMultipliers[vaultIndex];
        }

        public static float GetHealthMultiplier(int vaultIndex)
        {
            if (vaultIndex < 0 || vaultIndex >= HealthMultipliers.Length) return 1f;
            return HealthMultipliers[vaultIndex];
        }

        /// <summary>Scales a base damage value by the given vault's difficulty multiplier, rounded to a whole number.</summary>
        public static int ScaleDamage(int baseDamage, int vaultIndex)
        {
            return Mathf.RoundToInt(baseDamage * GetDamageMultiplier(vaultIndex));
        }

        /// <summary>Scales a base health value by the given vault's difficulty multiplier, rounded to a whole number.</summary>
        public static int ScaleHealth(int baseHealth, int vaultIndex)
        {
            return Mathf.RoundToInt(baseHealth * GetHealthMultiplier(vaultIndex));
        }
    }
}
