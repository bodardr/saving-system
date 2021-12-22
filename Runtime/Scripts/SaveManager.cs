using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Bodardr.Saving
{
    public static class SaveManager
    {
        public static SaveFile CurrentSave;

        public static List<SaveMetadata> saveMetadatas;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void LoadSavesMetadata()
        {
            var files = Directory.EnumerateFiles(Application.persistentDataPath);

            saveMetadatas = new List<SaveMetadata>();
            foreach (var metaFile in files.Where(x => x.EndsWith(SaveMetadata.MetaSuffix)))
                saveMetadatas.Add(SaveMetadata.LoadFrom(metaFile));
        }

        public static void NewSaveFile(string fileName = "")
        {
            if (CurrentSave != null)
                Debug.LogWarning("A save has already been loaded, make sure you've dumped the save file first.");

            if (string.IsNullOrEmpty(fileName))
                fileName = Guid.NewGuid().ToString();

            CurrentSave = new SaveFile(fileName);
        }

        public static void LoadSave(SaveMetadata metadata)
        {
            CurrentSave = SaveFile.Load(metadata);
        }

        public static bool Exists(string filePath)
        {
            return File.Exists(Path.Combine(Application.persistentDataPath,filePath));
        }
    }
}