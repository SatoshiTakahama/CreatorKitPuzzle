namespace Example
{
    public interface IDataFile
    {
        void Save<T>(T instance, string dataName);
        T Load<T>(string dataName);
    }
}
