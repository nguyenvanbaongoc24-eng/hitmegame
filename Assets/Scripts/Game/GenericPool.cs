using System;
using System.Collections.Generic;
using UnityEngine;

namespace HitMe.Game
{
    /// <summary>
    /// Zero-allocation generic object pool for MonoBehaviour components.
    /// Pre-warms at construction; Get/Return never allocate after warm-up.
    /// </summary>
    public sealed class GenericPool<T> where T : Component
    {
        private readonly Stack<T> _available;
        private readonly Transform _parent;
        private readonly Func<Transform, T> _factory;

        public int CountAll { get; private set; }
        public int CountActive => CountAll - _available.Count;
        public int CountInactive => _available.Count;

        public GenericPool(Func<Transform, T> factory, Transform parent, int warmCount)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _parent = parent;
            _available = new Stack<T>(warmCount);
            CountAll = 0;

            for (int i = 0; i < warmCount; i++)
            {
                T instance = _factory(_parent);
                instance.gameObject.SetActive(false);
                _available.Push(instance);
                CountAll++;
            }
        }

        /// <summary>
        /// Get an instance from the pool. Activates the GameObject.
        /// If pool is empty, creates a new instance (no allocation after warm-up if sized correctly).
        /// </summary>
        public T Get()
        {
            T instance;
            if (_available.Count > 0)
            {
                instance = _available.Pop();
            }
            else
            {
                instance = _factory(_parent);
                CountAll++;
            }
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Return an instance to the pool. Deactivates the GameObject.
        /// </summary>
        public void Return(T instance)
        {
            if (instance == null) return;
            instance.gameObject.SetActive(false);
            _available.Push(instance);
        }

        /// <summary>
        /// Deactivates and clears pool tracking.
        /// </summary>
        public void Clear()
        {
            while (_available.Count > 0)
            {
                T item = _available.Pop();
                if (item != null) item.gameObject.SetActive(false);
            }
            CountAll = 0;
        }
    }
}
