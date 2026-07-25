using System.Collections.Generic;
using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Singleton. Object Pooling pattern implementation — manages reusable
    /// GameObjects (projectiles, hit VFX) instead of Instantiate/Destroy
    /// on every shot, which matters a lot once the Dragon and Armed Guard
    /// are both firing projectiles regularly. Pools are pre-registered in
    /// the Inspector by key + prefab + initial size; Spawn()/Return() are
    /// the only two calls the rest of the codebase needs.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string key;
            public GameObject prefab;
            public int initialSize = 10;
        }

        [SerializeField] private List<Pool> pools = new List<Pool>();

        private readonly Dictionary<string, Queue<GameObject>> _poolDict = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> _prefabLookup = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var pool in pools)
            {
                var queue = new Queue<GameObject>();
                _prefabLookup[pool.key] = pool.prefab;

                for (int i = 0; i < pool.initialSize; i++)
                {
                    var obj = Instantiate(pool.prefab, transform);
                    obj.SetActive(false);
                    queue.Enqueue(obj);
                }
                _poolDict[pool.key] = queue;
            }
        }

        /// <summary>Retrieves an inactive object from the named pool (or grows the pool if exhausted) and activates it at the given position/rotation.</summary>
        public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
        {
            if (!_poolDict.ContainsKey(key))
            {
                Debug.LogWarning($"[ObjectPoolManager] No pool registered for key '{key}'");
                return null;
            }

            GameObject obj = _poolDict[key].Count > 0
                ? _poolDict[key].Dequeue()
                : Instantiate(_prefabLookup[key], transform); // pool exhausted — grow rather than stall gameplay

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        /// <summary>Deactivates an object and returns it to its pool for reuse.</summary>
        public void Return(string key, GameObject obj)
        {
            obj.SetActive(false);

            if (!_poolDict.ContainsKey(key))
            {
                _poolDict[key] = new Queue<GameObject>();
            }
            _poolDict[key].Enqueue(obj);
        }
    }
}
