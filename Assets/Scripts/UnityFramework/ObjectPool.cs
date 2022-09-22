using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework
{
    public class ObjectPool<TKey, TObject> : MonoBehaviour where TObject : MonoBehaviour, IPool<TKey>
    {
        public class SinglePool
        {
            private readonly TObject[] mObjectArray;
            private int mIndex;

            public SinglePool(TObject prefab, int count, Transform parent)
            {
                mObjectArray = new TObject[count];

                for (int i = 0; i < count; i++)
                {
                    TObject inst = Instantiate(prefab, parent);
                    inst.gameObject.SetActive(false);
                    mObjectArray[i] = inst;
                }
            }

            public TObject Get()
            {
                TObject obj = null;

                for (int i = 0; i < mObjectArray.Length; i++)
                {
                    int curIndex = mIndex++;
                    mIndex = mIndex < mObjectArray.Length ? mIndex : 0;
                    if (!mObjectArray[curIndex].IsPooled)
                    {
                        continue;
                    }

                    obj = mObjectArray[curIndex];
                    break;
                }

                return obj;
            }
        }

        private static Dictionary<TKey, SinglePool> _poolMap;

        [SerializeField] private bool dontDestroy;
        [SerializeField] private Transform holder;
        [SerializeField] private TObject[] objectArray;
        [SerializeField] private int[] countArray;

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
                if (!(_poolMap is null))
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
            for (int i = 0; i < objectArray.Length; i++)
            {
                _poolMap.Add(objectArray[i].Key, new SinglePool(objectArray[i], countArray[i], holder ? holder : transform));
            }
        }

        private void OnDestroy()
        {
            _poolMap = null;
        }
    }
}