using UnityEngine;
using VaultsOfTheElixir.Interfaces;

namespace VaultsOfTheElixir.Player
{
    /// <summary>
    /// Strategy pattern implementation: a temporary damage-reduction
    /// shield on the user. A third IAbility option alongside EmberDash
    /// and VaultPulse — kept purely defensive/support so a player has a
    /// non-damage-dealing option worth picking, which matters most once
    /// multiple players can each equip their own 2 of these 3+ abilities.
    ///
    /// Damage reduction is applied by PlayerController checking
    /// IsWardActive before applying incoming damage — see the
    /// TakeDamage() integration note in PlayerController.
    /// </summary>
    public class GuardianWardAbility : MonoBehaviour, IAbility
    {
        [SerializeField] private float wardDuration = 3f;
        [SerializeField] [Range(0f, 1f)] private float damageReductionPercent = 0.5f;
        [SerializeField] private float cooldownDuration = 8f;

        private float _cooldownTimer;
        private float _wardTimer;

        public string AbilityId => "guardians_ward";
        public float CooldownDuration => cooldownDuration;
        public bool IsReady => _cooldownTimer <= 0f;

        /// <summary>True while the shield is currently active — PlayerController checks this before applying damage.</summary>
        public bool IsWardActive => _wardTimer > 0f;

        /// <summary>Multiplier to apply to incoming damage while the ward is active (e.g. 0.5 = half damage taken).</summary>
        public float DamageMultiplierWhileActive => 1f - damageReductionPercent;

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
            if (_wardTimer > 0f) _wardTimer -= Time.deltaTime;
        }

        public void Activate(GameObject user)
        {
            if (!IsReady) return;

            _wardTimer = wardDuration;
            _cooldownTimer = cooldownDuration;

            // Hook up a shield VFX/sprite outline toggle here, driven by
            // IsWardActive, once the visual asset is in place.
        }
    }
}
