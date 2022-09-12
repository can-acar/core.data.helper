namespace CoreEntityHelper.Infrastructures;

public interface ICache
{
    void Clear();
    object Get(string key);
    void Remove(string key);
    void Store(string key, object value, int minutesValidFor);
}
#pragma warning disable CS8603
public class NullCache : ICache
{
    public void Clear() { }

    public object Get(string key)
    {
        return null;
    }

    public void Remove(string key) { }

    public void Store(string key, object value, int minutesValidFor) { }
}