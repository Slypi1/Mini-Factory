public interface ISaveService
{
    void Save(FactoryState state);
    FactoryState Load();
    bool HasSave();
    void DeleteSave();
}
