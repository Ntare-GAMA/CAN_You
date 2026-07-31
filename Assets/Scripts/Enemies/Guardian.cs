using UnityEngine;
using UnityEngine.Events;
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
        [Tooltip("If true, this guardian does NOT spawn a relic prefab on death and does NOT log the 'no relic assigned' warning. " +
                 "Use this for guardians whose defeat is tracked some other way (e.g. Vault 4's kill-count-based completion via onDeath).")]
        [SerializeField] protected bool skipRelicSpawn = false;

        [Header("Stats")]
        [SerializeField] protected int maxHealth = 100;
        [SerializeField] protected float detectionRange = 8f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float runSpeed = 3.5f;
        [SerializeField] protected float attackCooldown = 2f;

        [Header("Events")]
        [Tooltip("Invoked once, right when this guardian dies. Wire this in the Inspector to hook up " +
                 "custom completion logic (e.g. Vault4BossManager.RegisterDinosaurDefeated) without editing this script.")]
        public UnityEvent onDeath;

        [Header("Obstacle Handling")]
        [Tooltip("If a guardian's actual movement stays below this each frame while trying to chase, it's considered blocked by an obstacle.")]
        [SerializeField] protected float stuckMovementThreshold = 0.02f;
        [Tooltip("How long a guardian must be blocked before it switches to pacing instead of pushing uselessly.")]
        [SerializeField] protected float stuckTimeBeforePacing = 0.5f;
        [Tooltip("How far (in world units) a guardian retreats from a blocking obstacle before trying to chase again.")]
        [SerializeField] protected float retreatDistance = 8f;

        private Vector2 _lastPosition;
        private Vector2 _retreatStartPosition;
        private Vector2 _retreatCheckPosition;
        private float _stuckTimer;
        private float _retreatStuckTimer;
        private float _paceTimer;
        private bool _isPacing;
        private float _paceDirection = 1f;

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
            _lastPosition = transform.position;

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

        /// <summary>
        /// Moves toward the player on the X axis only — this is a side-scroller,
        /// so guardians chase horizontally and let gravity (not this method)
        /// control their vertical position. This is what stops a guardian from
        /// climbing/floating upward to match the player's height when the
        /// player jumps.
        ///
        /// Since enemies (like the player) can never go above ground level,
        /// there's no way to path "over" an obstacle blocking the direct line
        /// to the player. Instead of freezing and pushing uselessly into it
        /// forever, a blocked guardian paces side-to-side near the obstacle —
        /// still visibly active and still a threat if the player gets close,
        /// but not permanently stuck either.
        /// </summary>
        public virtual void Move()
        {
            if (playerTransform == null) return;

            float horizontalDirection = Mathf.Sign(playerTransform.position.x - rb.position.x);
            float verticalVelocity = rb.linearVelocity.y;

            // Detect blocked movement: how far did we actually move last frame?
            // We check ACTUAL POSITION CHANGE, not rb.linearVelocity — physics
            // engines commonly zero out velocity the instant a collision happens,
            // so checking velocity here would make a guardian look like it "never
            // tried to move" the moment it hits something, and the stuck-timer
            // would never start. Position delta doesn't lie about that.
            float actualMovement = Vector2.Distance(transform.position, _lastPosition);
            _lastPosition = transform.position;

            if (!_isPacing && actualMovement < stuckMovementThreshold)
            {
                _stuckTimer += Time.deltaTime;
            }
            else if (!_isPacing)
            {
                _stuckTimer = 0f;
            }

            if (!_isPacing && _stuckTimer >= stuckTimeBeforePacing)
            {
                _isPacing = true;
                _paceTimer = 0f;
                _retreatStartPosition = transform.position;
                _retreatCheckPosition = transform.position;
                _retreatStuckTimer = 0f;
                _paceDirection = -horizontalDirection; // fall back the way we came, away from the obstacle
            }

            float moveDirection;

            if (_isPacing)
            {
                _paceTimer += Time.deltaTime;
                moveDirection = _paceDirection;

                // Also check whether the retreat ITSELF is blocked (e.g. backed
                // into a wall or a second rock). Without this, a guardian caught
                // between two obstacles would just stand still for up to
                // maxRetreatTime doing nothing, since it never covers
                // retreatDistance but also never technically "times out" yet.
                float retreatStepMovement = Vector2.Distance(transform.position, _retreatCheckPosition);
                _retreatCheckPosition = transform.position;
                if (retreatStepMovement < stuckMovementThreshold)
                {
                    _retreatStuckTimer += Time.deltaTime;
                }
                else
                {
                    _retreatStuckTimer = 0f;
                }

                float retreatedSoFar = Mathf.Abs(transform.position.x - _retreatStartPosition.x);
                bool coveredFullRetreat = retreatedSoFar >= retreatDistance;

                // Time cap is derived from how long the retreat SHOULD take at
                // current speed, with generous headroom — this is a safety net
                // for a genuinely wedged guardian, not a limit that can cut a
                // legitimate retreat short just because retreatDistance is large.
                float expectedRetreatTime = runSpeed > 0.01f ? (retreatDistance / runSpeed) : 1f;
                float dynamicMaxRetreatTime = Mathf.Max(expectedRetreatTime * 3f, 2f);
                bool timedOut = _paceTimer >= dynamicMaxRetreatTime;

                bool retreatItselfBlocked = _retreatStuckTimer >= stuckTimeBeforePacing;

                if (coveredFullRetreat || timedOut || retreatItselfBlocked)
                {
                    _isPacing = false;
                    _stuckTimer = 0f; // give chase another chance once the retreat is done
                    _lastPosition = transform.position; // reset baseline so the next chase attempt gets a fair check
                }
            }
            else
            {
                moveDirection = horizontalDirection;
            }

            rb.linearVelocity = new Vector2(moveDirection * runSpeed, verticalVelocity);

            // Flip sprite to face movement direction — standard 2D facing approach.
            var scale = transform.localScale;
            transform.localScale = new Vector3(moveDirection * Mathf.Abs(scale.x), scale.y, scale.z);

            // Only check attack range while actually chasing, not while retreating.
            if (!_isPacing)
            {
                float horizontalDist = Mathf.Abs(transform.position.x - playerTransform.position.x);
                if (horizontalDist <= attackRange)
                {
                    CurrentState = EnemyState.Attack;
                }
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

        /// <summary>
        /// Instant death: no lingering corpse, no death animation window.
        /// The moment health hits 0, the guardian vanishes. If a relic
        /// prefab is assigned (and skipRelicSpawn is false), it spawns in
        /// the guardian's place as usual. Either way, onDeath fires once —
        /// this is what lets Vault 4's three Dinosaurs trigger completion
        /// by kill count instead of by relic pickup.
        /// </summary>
        public virtual void Die()
        {
            CurrentState = EnemyState.Dead;

            GameEvents.RaiseEnemyDefeated(gameObject);

            if (!skipRelicSpawn)
            {
                if (relicPrefab != null)
                {
                    Instantiate(relicPrefab, transform.position, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning($"[Guardian] {gameObject.name} has no Relic Prefab assigned — vault won't be completable!");
                }
            }

            onDeath?.Invoke();

            Destroy(gameObject);
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