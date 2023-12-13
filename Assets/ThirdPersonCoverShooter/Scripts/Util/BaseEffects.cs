using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Spawns prefab instances on various events.
    /// </summary>
    public class BaseEffects : MonoBehaviour
    {
        private class Instance
        {
            public GameObject Object;
            public float Age;
        }

        private class InstanceStorage
        {
            public List<Instance> Active;
            public List<Instance> Old;
        }

        private List<Coroutine> _coroutines = new List<Coroutine>();

        private static Dictionary<GameObject, InstanceStorage> instanceMap = new Dictionary<GameObject, InstanceStorage>();
        private static List<InstanceStorage> instances = new List<InstanceStorage>();
        private static int frame;

        protected virtual void LateUpdate()
        {
            if (Time.frameCount > frame)
            {
                frame = Time.frameCount;

                for (int i = 0; i < instances.Count; i++)
                    UpdateInstances(instances[i], Time.deltaTime);
            }

            for (int i = _coroutines.Count - 1; i >= 0; i--)
            {
                var coroutine = _coroutines[i];

                if (coroutine == null)
                    _coroutines.RemoveAt(i);
            }
        }

        protected virtual void OnDisable()
        {
            for (int i = 0; i < _coroutines.Count; i++)
            {
                var coroutine = _coroutines[i];

                if (coroutine != null)
                    StopCoroutine(coroutine);
            }

            _coroutines.Clear();
        }
        

        /// <summary>
        /// Ages all instances and disables ones that are old.
        /// </summary>
        private void UpdateInstances(InstanceStorage storage, float dt)
        {
            if (storage.Active == null || storage.Active.Count == 0)
                return;

            for (int i = storage.Active.Count - 1; i >= 0; i--)
            {
                var instance = storage.Active[i];

                if (instance.Object == null)
                {
                    storage.Active.RemoveAt(i);
                    continue;
                }

                instance.Age += dt;

                if (instance.Age >= 3)
                {
                    instance.Object.SetActive(false);
                    storage.Active.RemoveAt(i);

                    if (storage.Old == null)
                        storage.Old = new List<Instance>();

                    storage.Old.Add(instance);
                }
            }

            if (storage.Old != null)
                for (int i = storage.Old.Count - 1; i >= 0; i--)
                    if (storage.Old[i].Object == null)
                        storage.Old.RemoveAt(i);
        }

        protected void Disable(GameObject prefab)
        {
            InstanceStorage storage;
            if (!instanceMap.TryGetValue(prefab, out storage))
                return;

            if (storage.Active != null && storage.Active.Count > 0)
            {
                for (int i = 0; i < storage.Active.Count; i++)
                {
                    var instance = storage.Active[i];

                    if (instance.Object != null)
                    {
                        instance.Object.SetActive(false);

                        if (storage.Old == null)
                            storage.Old = new List<Instance>();

                        storage.Old.Add(instance);
                    }
                }

                storage.Active.Clear();
            }

            if (storage.Old != null)
            {
                for (int i = storage.Old.Count - 1; i >= 0; i--)
                    if (storage.Old[i].Object == null)
                        storage.Old.RemoveAt(i);
            }
        }

        private InstanceStorage getStorage(GameObject prefab)
        {
            InstanceStorage storage;
            if (instanceMap.TryGetValue(prefab, out storage))
                return storage;

            storage = new InstanceStorage();
            instanceMap[prefab] = storage;
            instances.Add(storage);

            return storage;
        }

        /// <summary>
        /// Helper function to instantiate effect prefabs.
        /// </summary>
        protected void Instantiate(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return;

            var storage = getStorage(prefab);

            while (storage.Old != null && storage.Old.Count > 0)
            {
                var found = storage.Old[storage.Old.Count - 1];
                storage.Old.RemoveAt(storage.Old.Count - 1);

                if (found == null || found.Object == null)
                    continue;

                found.Object.transform.position = position;
                found.Object.SetActive(true);
                found.Age = 0;

                storage.Active.Add(found);
                return;
            }

            var instance = new Instance();

            instance.Object = GameObject.Instantiate(prefab);
            instance.Object.transform.SetParent(null);
            instance.Object.transform.position = position;
            instance.Object.SetActive(true);

            if (storage.Active == null)
                storage.Active = new List<Instance>();

            storage.Active.Add(instance);
        }

        /// <summary>
        /// Helper function to instantiate effect prefabs.
        /// </summary>
        protected void InstantiateLocal(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                return;

            var storage = getStorage(prefab);

            while (storage.Old != null && storage.Old.Count > 0)
            {
                var found = storage.Old[storage.Old.Count - 1];
                storage.Old.RemoveAt(storage.Old.Count - 1);

                if (found == null || found.Object == null)
                    continue;

                found.Object.transform.SetParent(parent);
                found.Object.transform.localPosition = position;
                found.Object.transform.localRotation = rotation;
                found.Object.SetActive(true);
                found.Age = 0;

                storage.Active.Add(found);
                return;
            }

            var instance = new Instance();

            instance.Object = GameObject.Instantiate(prefab);
            instance.Object.transform.SetParent(parent);
            instance.Object.transform.localPosition = position;
            instance.Object.transform.localRotation = rotation;
            instance.Object.SetActive(true);

            if (storage.Active == null)
                storage.Active = new List<Instance>();

            storage.Active.Add(instance);
        }

        protected void InstantiateIn(float delay, GameObject prefab, Vector3 position)
        {
            _coroutines.Add(StartCoroutine(play(delay, prefab, position)));
        }

        protected void InstantiateLocallyIn(float delay, GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            _coroutines.Add(StartCoroutine(playLocally(delay, prefab, parent, position, rotation)));
        }

        private IEnumerator play(float delay, GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                yield break;

            yield return new WaitForSeconds(delay);

            Instantiate(prefab, position);
        }

        private IEnumerator playLocally(float delay, GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
                yield break;

            yield return new WaitForSeconds(delay);

            InstantiateLocal(prefab, parent, position, rotation);
        }
    }
}
