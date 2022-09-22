using System.Collections.Generic;
using UnityEngine;

namespace UnityFramework
{
    public class ObjectTable<TKey, TObject> : MonoBehaviour where TObject : MonoBehaviour, ITable<TKey>
    {
        private readonly Dictionary<TKey, TObject> mObjectMap = new Dictionary<TKey, TObject>();

        public bool Has(TKey key)
        {
            return mObjectMap.ContainsKey(key);
        }
        
        public TObject Get(TKey key)
        {
            return mObjectMap[key];
        }

        protected virtual void Awake()
        {
            TObject[] objects = GetComponentsInChildren<TObject>(true);

            foreach (TObject obj in objects)
            {
                mObjectMap.Add(obj.Key, obj);
            }
        }
    }
}