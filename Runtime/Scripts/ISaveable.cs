namespace Bodardr.Saving
{
    public interface ISaveable
    {
        void OnLoad();
        void OnBeforeSave();
    }
}