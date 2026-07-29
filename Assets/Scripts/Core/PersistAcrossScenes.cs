using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Attach to any GameObject that needs to survive scene loads but
    /// isn't a singleton manager — namely the Player and Main Camera,
    /// once the project moved to one-scene-per-vault. Guards against
    /// duplicates the same way the singleton managers do, in case a
    /// vault scene accidentally contains its own Player/Camera too.
    /// </summary>
    public class PersistAcrossScenes : MonoBehaviour
    {
        private static readonly System.Collections.Generic.HashSet<string> _persistedNames
            = new System.Collections.Generic.HashSet<string>();

        [Tooltip("Unique id for this persistent object, e.g. 'Player' or 'MainCamera'. Prevents duplicates if a scene accidentally contains its own copy.")]
        [SerializeField] private string persistentId = "Player";

        private void Awake()
        {
            if (_persistedNames.Contains(persistentId))
            {
                Destroy(gameObject);
                return;
            }

            _persistedNames.Add(persistentId);
            DontDestroyOnLoad(gameObject);
        }
    }
}
