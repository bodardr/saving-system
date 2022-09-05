using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Bodardr.Saving
{
    public class SaveFile : IEnumerable
    {
        private SaveMetadata metadata;

        internal const string defaultFilename = "save.dat";

        private Dictionary<Type, object> savedEntries;

        public Dictionary<Type, object> SavedEntries => savedEntries;

        public SaveMetadata Metadata => metadata;

        public static SaveFile Load(SaveMetadata metadata)
        {
            var save = new SaveFile
            {
                metadata = metadata
            };

            try
            {
                var file = File.ReadAllText(Path.Combine(Application.persistentDataPath, metadata.Filename))
                    .Split(new[] { '\u241E' }, StringSplitOptions.RemoveEmptyEntries);

                //Parse saveables found in file.
                for (var i = 0; i < file.GetLength(0); i++)
                {
                    var obj = file[i].Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    var type = Type.GetType(obj[0]);

                    if (type == null)
                    {
                        Debug.LogWarning($"Couldn't find type {obj[0]}, continuing load...");
                        continue;
                    } 
                    
                    if (save.SavedEntries.ContainsKey(type))
                    {
                        Debug.LogWarning($"{obj[0]} is a duplicate and was already loaded, continuing load");
                        continue;
                    }

                    var loadedObj = JsonUtility.FromJson(obj[1], type);

                    if (typeof(ISaveable).IsAssignableFrom(type))
                        ((ISaveable)loadedObj).OnLoad();

                    save.SavedEntries.Add(type ?? typeof(object), loadedObj);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }

            return save;
        }

        public SaveFile(string filename = "")
        {
            savedEntries = new Dictionary<Type, object>();

            if (string.IsNullOrEmpty(filename))
            {
                filename = defaultFilename;
            }
            else
            {
                //Remove entries with extensions already in them.
                var indexOf = filename.IndexOf('.');

                if (indexOf >= 0)
                    filename = filename.Substring(0, indexOf);

                filename = string.Concat(filename, ".dat");
            }

            metadata = new SaveMetadata
            {
                Filename = filename,
                CreationTime = DateTime.Now,
                LastSaveTime = DateTime.Now
            };
        }

        public T GetOrCreate<T>()
        {
            var type = typeof(T);
            
            if (SavedEntries.ContainsKey(type) && SavedEntries[type] != null)
                return (T)SavedEntries[type];

            T instance = Activator.CreateInstance<T>();
            if (typeof(ISaveable).IsAssignableFrom(type))
                ((ISaveable)instance).OnLoad();

            SavedEntries.Add(type, instance);
            return instance;
        }

        public void Save(string saveAs = "", bool saveThumbnail = false)
        {
            if (!string.IsNullOrEmpty(saveAs))
                Metadata.Filename = saveAs;

            Metadata.Save(saveThumbnail);

            var saveableType = typeof(ISaveable);
            var str = new StringBuilder();
            foreach (var (key, value) in SavedEntries)
            {
                if (saveableType.IsAssignableFrom(key))
                    ((ISaveable)value).OnBeforeSave();

                str.AppendLine($"\u241E{key.AssemblyQualifiedName}\r");
                str.AppendLine(JsonUtility.ToJson(value));
            }

            var filePath = Path.Combine(Application.persistentDataPath, Metadata.Filename);

            FileStream saveFile;
            using (saveFile = File.Exists(filePath) ? File.OpenWrite(filePath) : File.Create(filePath))
            {
                var bytes = Encoding.Default.GetBytes(str.ToString());
                saveFile.Write(bytes, 0, bytes.Length);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return SavedEntries.GetEnumerator();
        }
    }
}