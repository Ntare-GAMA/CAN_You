using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>
    /// Base class for all five guardian types (Scorpion, Anaconda, Armed
    /// Guard, Dragon, Dinosaur). Implements the shared IEnemyBehaviour +
    /// IDamageable contracts and drives the State pattern FSM
    /// (EnemyState) common to every guardian.
    ///
    /// 2D movement: uses Rigidbody2D directly rather than NavMeshAgent
    /// (a 3D-only component) — each guardian moves straight toward the
    /// player on the XY plane while in Chase state. If you later want
    /// obstacle-avoiding pathfinding in 2D, the NavMeshPlus package can
    /// slot in here without touching any concrete guardian script, since
    /// they only ever call Move() / Attack() through this base class.
    ///
    /// Concrete guardians only need to override Attack() with their
    /// specific move (bite, sting, shoot, breath, constrict) and
    /// implement their era-specific interface's extra methods.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public abstract class Guardian : MonoBehaviour, IEnemyBehaviour, IDamageable
    {
        [Header("Guardian Identity")]
        [Tooltip("Which vault (0-4) this guardian belongs to — used to spawn the correct relic on death.")]
        [SerializeField] protected int vaultIndex;
        [SerializeField] protected GameObject relicPrefab;

        [Header("Stats")]
        [SerializeField] protected int maxHealth = 100;
        [SerializeField] protected float detectionRange = 8f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float runSpeed = 3.5f;
        [SerializeField] protected float attackCooldown = 2f;

        protected Rigidbody2D rb;
        protected Animator animator;
        protected Transform playerTransform;
        protected float attackTimer;
        protected int currentHealth;

        public EnemyState CurrentState { get; protected set; } = EnemyState.Idle;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // Health scales per vault via the shared DifficultyCurve, so
            // later guardians are tankier without hand-tuning each one.
            maxHealth = DifficultyCurve.ScaleHealth(maxHealth, vaultIndex);
            currentHealth = maxHealth;
        }

        protected virtual void Update()
        {
            if (CurrentState == EnemyState.Dead) return;

            attackTimer -= Time.deltaTime;

            switch (CurrentState)
            {
                case EnemyState.Idle:
                    TickIdle();
                    rb.linearVelocity = Vector2.zero;
                    break;
                case EnemyState.Chase:
                    Move();
                    break;
                case EnemyState.Attack:
                    TickAttack();
                    break;
                case EnemyState.Defend:
                    TickDefend();
                    break;
                case EnemyState.Stagger:
                    rb.linearVelocity = Vector2.zero;
                    break;
            }

            // Drive the Animator's locomotion blend tree from actual
            // velocity, so Idle/Walk/Run falls out of real movement speed
            // instead of being hardcoded per state.
            if (animator != null)
            {
                animator.SetFloat("Speed", rb.linearVelocity.magnitude);
            }
        }

        protected virtual void TickIdle()
        {
            if (playerTransform == null) return;
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= detectionRange)
            {
                OnPlayerDetected(playerTransform);
            }
        }

        protected virtual void TickAttack()
        {
            if (playerTransform == null) { CurrentState = EnemyState.Idle; return; }

            rb.linearVelocity = Vector2.zero;
            float dist = Vector2.Distance(transform.position, playerTransform.position);

            if (dist > attackRange * 1.2f)
            {
                CurrentState = EnemyState.Chase;
                return;
            }

            if (attackTimer <= 0f)
            {
                Attack();
                attackTimer = attackCooldown;
            }
        }

        protected virtual void TickDefend() { }

        /// <summary>Moves directly toward the player on the XY plane. Transitions to Attack once in range.</summary>
        public virtual void Move()
        {
            if (playerTransform == null) return;

            Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
            rb.linearVelocity = direction * runSpeed;

            // Flip sprite to face movement direction — standard 2D facing approach.
            if (direction.x != 0f)
            {
                var scale = transform.localScale;
                transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(scale.x), scale.y, scale.z);
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= attackRange)
            {
                CurrentState = EnemyState.Attack;
            }
        }

        /// <summary>Concrete guardians implement their specific attack (bite, sting, shoot, breath, constrict).</summary>
        public abstract void Attack();

        public virtual void OnPlayerDetected(Transform player)
        {
            playerTransform = player;
            if (CurrentState == EnemyState.Idle)
            {
                CurrentState = EnemyState.Chase;
            }
        }

        public virtual void TakeDamage(int amount)
        {
            if (CurrentState == EnemyState.Dead) return;

            currentHealth = Mathf.Max(0, currentHealth - amount);
            animator?.SetTrigger("Hit");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public virtual void Die()
        {
            CurrentState = EnemyState.Dead;
            rb.linearVelocity = Vector2.zero;
            animator?.SetTrigger("Death");

            GameEvents.RaiseEnemyDefeated(gameObject);

            if (relicPrefab != null)
            {
                Instantiate(relicPrefab, transform.position, Quaternion.identity);
            }

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Destroy(gameObject, 3f);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && CurrentState == EnemyState.Idle)
            {
                OnPlayerDetected(other.transform);
            }
        }
    }
}
