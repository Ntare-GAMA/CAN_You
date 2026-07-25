using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Player
{
    /// <summary>
    /// 2D player controller: Rigidbody2D-based movement, health via
    /// IDamageable, interaction detection via IInteractable, and two
    /// ability slots implementing the Strategy pattern via IAbility.
    /// Also implements ISaveable so SaveManager can persist health across
    /// sessions without knowing anything about PlayerController itself.
    ///
    /// Ability slots are assigned in the Inspector as MonoBehaviours that
    /// implement IAbility (e.g. EmberDashAbility, VaultPulseAbility) —
    /// swapping which component sits in a slot changes the ability with
    /// zero changes to this script, which is the whole point of Strategy
    /// pattern here.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IDamageable, ISaveable
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Combat")]
        [SerializeField] private int maxHealth = 100;

        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 1.2f;

        [Header("Abilities (Strategy pattern) - assign components implementing IAbility")]
        [SerializeField] private MonoBehaviour abilitySlot1;
        [SerializeField] private MonoBehaviour abilitySlot2;

        private Rigidbody2D rb;
        private Vector2 _moveInput;
        private int _currentHealth;
        private bool _isDead;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;

        private IAbility Ability1 => abilitySlot1 as IAbility;
        private IAbility Ability2 => abilitySlot2 as IAbility;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            _currentHealth = maxHealth;
        }

        private void Update()
        {
            if (_isDead) return;

            _moveInput.x = Input.GetAxisRaw("Horizontal");
            _moveInput.y = Input.GetAxisRaw("Vertical");

            if (Input.GetKeyDown(KeyCode.E)) TryInteract();
            if (Input.GetKeyDown(KeyCode.Q)) TryActivateAbility(Ability1);
            if (Input.GetKeyDown(KeyCode.R)) TryActivateAbility(Ability2);

            if (_moveInput.x != 0f)
            {
                var scale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Sign(_moveInput.x) * Mathf.Abs(scale.x), scale.y, scale.z);
            }
        }

        private void FixedUpdate()
        {
            if (_isDead) return;
            rb.linearVelocity = _moveInput.normalized * moveSpeed;
        }

        private void TryInteract()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    interactable.Interact(gameObject);
                    break;
                }
            }
        }

        private void TryActivateAbility(IAbility ability)
        {
            if (ability != null && ability.IsReady)
            {
                ability.Activate(gameObject);
                GameEvents.RaiseAbilityActivated(ability.AbilityId);
            }
        }

        public void TakeDamage(int amount)
        {
            if (_isDead) return;

            // If either ability slot holds a GuardianWardAbility and it's
            // currently active, reduce incoming damage accordingly. This
            // is the only place ward damage reduction needs to be
            // checked, since every guardian's attack routes through
            // IDamageable.TakeDamage() on the player regardless of which
            // guardian dealt the hit.
            var ward = (abilitySlot1 as GuardianWardAbility) ?? (abilitySlot2 as GuardianWardAbility);
            if (ward != null && ward.IsWardActive)
            {
                amount = Mathf.RoundToInt(amount * ward.DamageMultiplierWhileActive);
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            GameEvents.RaiseHealthChanged(_currentHealth, maxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            _isDead = true;
            rb.linearVelocity = Vector2.zero;
            GameEvents.RaisePlayerDied();
            GameManager.Instance.TriggerGameOver();
        }

        // ---- ISaveable ----

        [System.Serializable]
        private class PlayerSaveState
        {
            public int currentHealth;
        }

        public object CaptureState()
        {
            return new PlayerSaveState { currentHealth = _currentHealth };
        }

        public void RestoreState(object state)
        {
            if (state is PlayerSaveState saved)
            {
                _currentHealth = saved.currentHealth;
                GameEvents.RaiseHealthChanged(_currentHealth, maxHealth);
            }
        }
    }
}
