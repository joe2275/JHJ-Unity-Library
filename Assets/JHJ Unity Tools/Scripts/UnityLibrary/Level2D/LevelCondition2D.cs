using UnityEngine;

namespace Level2D
{
    public abstract class LevelCondition2D : MonoBehaviour
    {
        public abstract bool CanGenerate();
    }
}