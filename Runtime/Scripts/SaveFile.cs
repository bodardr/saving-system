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

        public static SaveFile Load(SaveMetadata metadata)
        {
            var save = new SaveFile
            {
                metadata = metadata
            };

            try
            {
                var file = File.ReadAllText(Path.Combine(Application.persistentDataPath, metadata.Filename))
                    .Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

                var remainingSaveables = new List<Type>(SaveManager.saveableTypes);

                //Parse saveables found in file.
                for (var i = 0; i < file.GetLength(0); i++)
                {
                    var obj = file[i].Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    var type = remainingSaveables.Find(x => x.Name == obj[0]);
                    remainingSaveables.Remove(type);

                    save.SavedEntries.Add(type ?? typeof(object), JsonUtility.FromJson(obj[1], type));
                }

                //Add new saveables from missing ones (useful when converting from a previous update, for example).
                foreach (var saveable in remainingSaveables)
                    save.SavedEntries.Add(saveable, Activator.CreateInstance(saveable));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        public T GetFile<T>()
        {
            var type = typeof(T);

            if (SaveManager.saveableTypes.Find(x => x == type) == null)
            {
                Debug.LogError("Type does not contain the Saveable attribute.");
                return default;
            }

            if (SavedEntries[type] != null)
                return (T)SavedEntries[type];

            Debug.LogError("Type was not found inside the savefile.");
            return default;
        }

        public void Save(string saveAs = "")
        {
            if (!string.IsNullOrEmpty(saveAs))
                metadata.Filename = saveAs;

            metadata.Save();

            var str = new StringBuilder();
            foreach (var entry in SavedEntries)
            {
                str.AppendLine($"\\{entry.Key}\r");
                str.AppendLine(JsonUtility.ToJson(entry.Value));
            }

            var filePath = Path.Combine(Application.persistentDataPath, metadata.Filename);

            FileStream saveFile;
            saveFile = File.Exists(filePath) ? File.OpenWrite(filePath) : File.Create(filePath);

            var bytes = Encoding.Default.GetBytes(str.ToString());

            saveFile.Write(bytes, 0, bytes.Length);
        }

        public IEnumerator GetEnumerator()
        {
            return SavedEntries.GetEnumerator();
        }
    }
}