using UnityEngine;
using VaultsOfTheElixir.Core;
using VaultsOfTheElixir.Enemies;

namespace VaultsOfTheElixir.Player
{
    /// <summary>
    /// Ranged attack — spawns a pooled projectile from FirePoint, same
    /// pattern as ArmedGuardGuardian's FireShot(), just player-controlled.
    /// </summary>
    public class PlayerShoot : MonoBehaviour
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private string projectilePoolKey = "player_bullet";
        [SerializeField] private int shootDamage = 12;
        [SerializeField] private float shootCooldown = 0.4f;
        [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;

        private float _cooldownTimer;

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;

            if (Input.GetKeyDown(shootKey) && _cooldownTimer <= 0f)
            {
                DoShoot();
            }
        }

        /// <summary>
        /// Public entry point for a UI shoot button's OnClick — same cooldown
        /// gating as the mouse-click path. Wire a Button's OnClick to this in
        /// the Inspector for touch/web builds.
        /// </summary>
        public void TryShoot()
        {
            if (_cooldownTimer > 0f) return;
            DoShoot();
        }

        private void DoShoot()
        {
            if (firePoint == null || ObjectPoolManager.Instance == null) return;

            Vector2 facing = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
            GameObject bullet = ObjectPoolManager.Instance.Spawn(projectilePoolKey, firePoint.position, Quaternion.identity);
            var proj = bullet.GetComponent<Projectile>();
            proj?.Launch(facing, shootDamage, gameObject);

            _cooldownTimer = shootCooldown;
        }
    }
}