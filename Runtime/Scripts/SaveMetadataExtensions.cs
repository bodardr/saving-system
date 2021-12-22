namespace Bodardr.Saving
{
    public static class SaveMetadataExtensions
    {
        public static void LoadSave(this SaveMetadata metadata)
        {
            SaveManager.CurrentSave = SaveFile.Load(metadata);
        }
    }
}