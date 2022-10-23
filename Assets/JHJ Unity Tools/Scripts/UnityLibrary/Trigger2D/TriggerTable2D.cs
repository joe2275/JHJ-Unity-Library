using UnityEngine;
using UnityFramework;

namespace Trigger2D
{
    public class TriggerTable2D<TKey> : ObjectTable<TKey, TriggerComponent2D<TKey>>
    {
        public int GetTriggeredCount(TKey key)
        {
            return Get(key).TriggeredCount;
        }
    
        public bool IsTriggered(TKey key)
        {
            return Get(key).IsTriggered;
        }

        public Collider2D GetTriggeredCollider(TKey key, int index)
        {
            return Get(key).GetTriggeredCollider(index);
        }

        public Collider2D[] GetTriggers(TKey key)
        {
            return Get(key).Triggers;
        }

        public void AddOnEnterEvent(TKey key, TriggerComponent2D<TKey>.OnTriggerEvent onEnter)
        {
            Get(key).OnEnter += onEnter;
        }

        public void RemoveOnEnterEvent(TKey key, TriggerComponent2D<TKey>.OnTriggerEvent onEnter)
        {
            Get(key).OnEnter -= onEnter;
        }

        public void AddOnExitEvent(TKey key, TriggerComponent2D<TKey>.OnTriggerEvent onExit)
        {
            Get(key).OnExit += onExit;
        }

        public void RemoveOnExitEvent(TKey key, TriggerComponent2D<TKey>.OnTriggerEvent onExit)
        {
            Get(key).OnExit -= onExit;
        }
    }
}