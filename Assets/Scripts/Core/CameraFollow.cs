using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Simple smoothed 2D camera follow. Attach to Main Camera. Finds
    /// the Player by tag automatically on each scene load (since Player
    /// persists via PersistAcrossScenes and Main Camera does too, but
    /// Unity doesn't automatically keep a serialized reference pointing
    /// at the same instance across scene loads reliably in every case,
    /// so re-finding by tag in LateUpdate is the simplest robust approach
    /// given the project's tight timeline).
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        private Transform _target;

        private void LateUpdate()
        {
            if (_target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) _target = player.transform;
                else return;
            }

            Vector3 desiredPosition = _target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
