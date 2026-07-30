using UnityEngine;

namespace VaultsOfTheElixir.Player
{
    /// <summary>
    /// Short hop/dash-style jump for a top-down game (no gravity, so this
    /// is a quick repositioning burst rather than a vertical arc). Grants
    /// brief invincibility so it doubles as a dodge.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerJump : MonoBehaviour
    {
        [SerializeField] private float hopDistance = 2.5f;
        [SerializeField] private float hopCooldown = 0.8f;
        [SerializeField] private float invincibilityTime = 0.3f;
        [SerializeField] private KeyCode jumpKey = KeyCode.LeftShift;

        private Rigidbody2D _rb;
        private float _cooldownTimer;

        public bool IsInvincible { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(jumpKey) && _cooldownTimer <= 0f)
            {
                DoJump();
            }
        }

        private void DoJump()
        {
            Vector2 facing = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
            _rb.MovePosition(_rb.position + facing * hopDistance);
            _cooldownTimer = hopCooldown;
            StartCoroutine(InvincibilityWindow());
        }

        private System.Collections.IEnumerator InvincibilityWindow()
        {
            IsInvincible = true;
            yield return new WaitForSeconds(invincibilityTime);
            IsInvincible = false;
        }
    }
} 