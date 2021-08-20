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

        internal static List<Type> saveableTypes;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            saveableTypes = new List<Type>();
            var assembly = Assembly.GetCallingAssembly();

            foreach (var t in assembly.GetTypes())
                if (t.GetCustomAttributes(typeof(SaveableAttribute), true).Length > 0)
                {
                    Debug.Log($"Added {t}");
                    saveableTypes.Add(t);
                }

            LoadSavesMetadata();
        }

        private static void LoadSavesMetadata()
        {
            var files = Directory.EnumerateFiles(Application.persistentDataPath);

            foreach (var metaFile in files.Where(x => x.EndsWith(SaveMetadata.MetaSuffix)))
                saveMetadatas.Add(SaveMetadata.LoadFrom(metaFile));
        }

        public static void NewSaveFile(string fileName = "")
        {
            if (CurrentSave != null)
                Debug.LogWarning("A save has already been loaded, make sure you've dumped the save file first.");

            CurrentSave = new SaveFile(fileName);

            foreach (var saveable in saveableTypes)
            {
                var instance = Activator.CreateInstance(saveable);
                CurrentSave.SavedEntries.Add(saveable, instance);
            }
        }

        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}