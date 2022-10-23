using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework
{
    public class ObjectPool<TKey, TObject> : MonoBehaviour where TObject : MonoBehaviour, IPool<TKey>
    {
        [Serializable]
        public class SinglePool
        {
            [SerializeField] private TObject pooling;
            [SerializeField] private int count;

            private TObject[] mPoolingArray;
            private int mIndex;

            public TKey Key => pooling.Key;

            public void Initialize(Transform parent)
            {
                mPoolingArray = new TObject[count];

                for (int i = 0; i < count; i++)
                {
                    TObject poolingObject = Instantiate(pooling, parent);
                    poolingObject.gameObject.SetActive(false);
                    mPoolingArray[i] = poolingObject;
                }
            }

            public TObject Get()
            {
                TObject obj = null;

                for (int i = 0; i < mPoolingArray.Length; i++)
                {
                    int curIndex = mIndex++;
                    mIndex = mIndex < mPoolingArray.Length ? mIndex : 0;
                    if (!mPoolingArray[curIndex].IsPooled)
                    {
                        continue;
                    }

                    obj = mPoolingArray[curIndex];
                    break;
                }

                return obj;
            }
        }

        private static Dictionary<TKey, SinglePool> _poolMap;

        [Header("Options")]
        [SerializeField] private bool dontDestroy;
        [SerializeField] private Transform holder;

        [Header("Pooling")] [ArrayElementTitle("Key")] 
        [SerializeField] private SinglePool[] poolArray;
        
        public static TObject Get(TKey key)
        {
            return _poolMap[key].Get();
        }

        public static bool Has(TKey key)
        {
            return _poolMap.ContainsKey(key);
        }

        protected virtual void Awake()
        {
            if (dontDestroy)
            {
                if (_poolMap is not null)
                {
                    Destroy(gameObject);
                    return;
                }
                
                DontDestroyOnLoad(gameObject);
                if (holder)
                {
                    DontDestroyOnLoad(holder.gameObject);
                }
            }

            _poolMap = new Dictionary<TKey, SinglePool>();
            for (int i = 0; i < poolArray.Length; i++)
            {
                poolArray[i].Initialize(holder ? holder : transform);
                _poolMap.Add(poolArray[i].Key, poolArray[i]);
            }
        }

        private void OnDestroy()
        {
            _poolMap = null;
        }
    }
}