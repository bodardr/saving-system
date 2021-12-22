using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
                    .Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                
                //Parse saveables found in file.
                for (var i = 0; i < file.GetLength(0); i++)
                {
                    var obj = file[i].Split(new[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    var type = Type.GetType(obj[0]);

                    save.SavedEntries.Add(type ?? typeof(object), JsonUtility.FromJson(obj[1], type));
                }
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

            var saveableAttr = type.GetCustomAttribute(typeof(SaveableAttribute));

            if (saveableAttr == null)
            {
                Debug.LogError("Type does not contain the Saveable attribute.");
                return default;
            }

            if (SavedEntries.ContainsKey(type) && SavedEntries[type] != null)
                return (T)SavedEntries[type];

            T instance = Activator.CreateInstance<T>();
            SavedEntries.Add(type, instance);
            return instance;
        }

        public void Save(string saveAs = "", bool saveThumbnail = true)
        {
            if (!string.IsNullOrEmpty(saveAs))
                Metadata.Filename = saveAs;

            Metadata.Save(saveThumbnail);

            var str = new StringBuilder();
            foreach (var entry in SavedEntries)
            {
                str.AppendLine($"\\{entry.Key.AssemblyQualifiedName}\r");
                str.AppendLine(JsonUtility.ToJson(entry.Value));
            }

            var filePath = Path.Combine(Application.persistentDataPath, Metadata.Filename);

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