namespace UnityFramework
{
    public interface ITable<out TKey>
    {
        TKey Key { get; }
    }
}