namespace UnityFramework
{
    public interface IPool<out TKey> : ITable<TKey>
    {
        bool IsPooled { get; }
    }
}