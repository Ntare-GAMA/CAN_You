using UnityEngine;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Drop this on an empty GameObject in each vault scene. On start,
    /// it tells the existing AudioManager singleton to play this vault's
    /// specific music track — keeps music selection local to each scene
    /// instead of hardcoding vault-to-clip mapping inside AudioManager.
    /// </summary>
    public class VaultAmbience : MonoBehaviour
    {
        [SerializeField] private AudioClip vaultMusic;
        [SerializeField] private bool loop = true;

        private void Start()
        {
            if (vaultMusic == null)
            {
                Debug.LogWarning($"[VaultAmbience] No music clip assigned in {gameObject.scene.name}.");
                return;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(vaultMusic, loop);
            }
        }
    }
}
