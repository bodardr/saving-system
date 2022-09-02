namespace Bodardr.Saving
{
    public interface ISaveable
    {
        void OnBeforeSave();
        void OnLoad();
    }
}