using System;
using System.Collections.Generic;
using UnityEngine;
using UnityFramework;

namespace Trigger2D
{
    public class TriggerComponent2D<TKey> : MonoBehaviour, ITable<TKey>
    {
        public delegate void OnTriggerEvent(Collider2D collider);
    
        [SerializeField] private TKey key;
        [SerializeField] private bool fixToWorld;
    
        private Collider2D[] mTriggers;
        private readonly List<Collider2D> mColliderList = new List<Collider2D>();
    
        public TKey Key => key;
        public event OnTriggerEvent OnEnter;
        public event OnTriggerEvent OnExit;

        public bool IsTriggered => mColliderList.Count > 0;

        public int TriggeredCount => mColliderList.Count;

        public Collider2D[] Triggers => mTriggers;

        public Collider2D GetTriggeredCollider(int index)
        {
            return mColliderList[index];
        }

        private void Awake()
        {
            mTriggers = GetComponents<Collider2D>();

            for (int i = 0; i < mTriggers.Length; i++)
            {
                mTriggers[i].isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            mColliderList.Add(other);
            OnEnter?.Invoke(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            mColliderList.Remove(other);
            OnExit?.Invoke(other);
        }
    }
}