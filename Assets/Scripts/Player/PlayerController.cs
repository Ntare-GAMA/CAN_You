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
    /// SIDE-SCROLLER MOVEMENT: horizontal movement comes from input;
    /// vertical movement comes ONLY from gravity + jump impulses, never
    /// directly from input. This is what keeps the player grounded —
    /// there's no way to hold a direction and float upward, only a single
    /// timed jump impulse from the ground. Jump is just enough to clear
    /// gaps/obstacles on the ground plane; it is not flight and cannot be
    /// chained mid-air (see isGrounded gating in TryJump()).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IDamageable, ISaveable
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 8f;
        [Tooltip("Empty child GameObject positioned at the player's feet, used to detect ground contact.")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [Tooltip("Set this to whatever Layer your ground/platform colliders use.")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Combat")]
        [SerializeField] private int maxHealth = 100;

        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 1.2f;

        [Header("Abilities (Strategy pattern) - assign components implementing IAbility")]
        [SerializeField] private MonoBehaviour abilitySlot1;
        [SerializeField] private MonoBehaviour abilitySlot2;

        private Rigidbody2D rb;
        private float _horizontalInput;
        private bool _jumpQueued;
        private bool _isGrounded;
        private int _currentHealth;
        private bool _isDead;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsGrounded => _isGrounded;

        private IAbility Ability1 => abilitySlot1 as IAbility;
        private IAbility Ability2 => abilitySlot2 as IAbility;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            _currentHealth = maxHealth;

            if (groundCheck == null)
            {
                Debug.LogWarning("[PlayerController] No Ground Check assigned — jump will never register as grounded. " +
                                  "Create an empty child GameObject at the player's feet and assign it in the Inspector.");
            }
        }

        private void Update()
        {
            if (_isDead) return;

            _horizontalInput = Input.GetAxisRaw("Horizontal");

            // Jump input: Space (default "Jump" button) or W, either triggers it.
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W))
            {
                _jumpQueued = true;
            }

            if (Input.GetKeyDown(KeyCode.E)) TryInteract();
            if (Input.GetKeyDown(KeyCode.Q)) TryActivateAbility(Ability1);
            if (Input.GetKeyDown(KeyCode.R)) TryActivateAbility(Ability2);

            if (_horizontalInput != 0f)
            {
                var scale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Sign(_horizontalInput) * Mathf.Abs(scale.x), scale.y, scale.z);
            }
        }

        private void FixedUpdate()
        {
            if (_isDead) return;

            // Ground check happens in FixedUpdate to stay in sync with physics.
            _isGrounded = groundCheck != null &&
                          Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // TEMPORARY DIAGNOSTIC — remove once jump is confirmed working.
            Debug.Log($"[PlayerController] isGrounded: {_isGrounded} | groundCheck pos: {groundCheck?.position} | radius: {groundCheckRadius} | groundLayer mask: {groundLayer.value}");

            // Horizontal movement is fully player-controlled.
            // Vertical velocity is left alone here so gravity keeps acting on it —
            // this is what stops the player from ever moving straight up on input.
            float verticalVelocity = rb.linearVelocity.y;

            if (_jumpQueued)
            {
                if (_isGrounded)
                {
                    verticalVelocity = jumpForce;
                }
                // Consume the queued jump either way — a jump press while
                // airborne is simply dropped, not buffered, so there's no
                // way to jump again before landing.
                _jumpQueued = false;
            }

            rb.linearVelocity = new Vector2(_horizontalInput * moveSpeed, verticalVelocity);
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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
#endif
    }
}