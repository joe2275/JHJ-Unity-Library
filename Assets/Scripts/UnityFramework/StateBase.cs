using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace UnityFramework
{
    public class StateBase<TKey> : MonoBehaviour
    {
        [Serializable]
        public class State
        {
            [SerializeField] private TKey key;
            
            public UnityEvent OnStart;
            public UnityEvent OnEnd;
            public UnityEvent OnUpdate;
            public UnityEvent OnLateUpdate;
            public UnityEvent OnFixedUpdate;

            public TKey Key => key;
            
            public void Start()
            {
                OnStart.Invoke();
            }

            public void End()
            {
                OnEnd.Invoke();
            }

            public void Update()
            {
                OnUpdate.Invoke();
            }

            public void LateUpdate()
            {
                OnLateUpdate.Invoke();
            }

            public void FixedUpdate()
            {
                OnFixedUpdate.Invoke();
            }
        }
        
        [Header("State")]
        [SerializeField] private TKey initialState;

        [ArrayElementTitle("key")]
        [SerializeField] private State[] states;

        private TKey mCurState;
        private State mCurStateInstance;
        private TKey mPrevState;
        private State mPrevStateInstance;

        private readonly Dictionary<TKey, State> mActorStateDict = new Dictionary<TKey, State>();

        /// <summary>
        /// State 프로퍼티 <br/>
        /// 액터의 상태를 설정하고 반환할 수 있는 프로퍼티
        /// </summary>
        public TKey CurState
        {
            get => mCurState;
            set
            {
                mPrevState = mCurState;
                mCurState = value;

                mPrevStateInstance = mCurStateInstance;
                mCurStateInstance = mActorStateDict[mCurState];

                if (!(mPrevState is null) && !mPrevState.Equals(mCurState))
                {
                    mPrevStateInstance.End();
                }
                mCurStateInstance.Start();
            }
        }
        
        /// <summary>
        /// Prev State 프로퍼티 <br/>
        /// 이전에 설정된 State 의 키
        /// </summary>
        public TKey PrevState => mPrevState;

        /// <summary>
        /// Get State 함수 <br/>
        /// 전달된 key 값의 State 객체를 반환
        /// </summary>
        public State GetState(TKey key)
        {
            return mActorStateDict[key];
        }

        protected virtual void Awake()
        {
            for (int i = 0; i < states.Length; i++)
            {
                mActorStateDict.Add(states[i].Key, states[i]);
            }
        }

        protected virtual void Start()
        {
            CurState = initialState;
        }

        protected virtual void Update()
        {
            mCurStateInstance?.Update();
        }

        protected virtual void FixedUpdate()
        {
            mCurStateInstance?.FixedUpdate();
        }

        protected void LateUpdate()
        {
            mCurStateInstance?.LateUpdate();
        }
    }
}