using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Bodardr.Saving
{
    public class SaveMetadata
    {
        public const string MetaSuffix = ".meta";

        private Sprite thumbnail;

        public string Filename { get; set; }

        public DateTime CreationTime { get; internal set; }

        public DateTime LastSaveTime { get; internal set; }

        public TimeSpan TimePlayed => LastSaveTime - CreationTime;

        public static SaveMetadata LoadFrom(string metaFile)
        {
            if (!File.Exists(metaFile))
                return null;

            var stream = File.ReadAllText(metaFile).Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            Sprite thumbnail = null;

            var serializedTexture = stream[2];
            thumbnail = LoadThumbnail(serializedTexture);

            var fileName = Path.GetFileName(metaFile);
            fileName = fileName.Substring(fileName.LastIndexOf(MetaSuffix, StringComparison.OrdinalIgnoreCase));
            var metadata = new SaveMetadata
            {
                Filename = fileName,
                CreationTime = DateTime.Parse(stream[0]),
                LastSaveTime = DateTime.Parse(stream[1]),
                thumbnail = thumbnail
            };

            return metadata;
        }

        private static Sprite LoadThumbnail(string serializedTextureObject)
        {
            var serializedTex = JsonUtility.FromJson<SerializedTexture>(serializedTextureObject);

            var tex = new Texture2D(serializedTex.Width, serializedTex.Height);
            tex.LoadImage(serializedTex.Data);

            var output = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one);
            return output;
        }

        public void Save()
        {
            LastSaveTime = DateTime.Now;

            var str = new StringBuilder();

            if (thumbnail == null)
                thumbnail = Sprite.Create(Texture2D.grayTexture, new Rect(0, 0, 1, 1), Vector2.one);

            //Append dates
            str.Append($"\\{CreationTime.Ticks}");
            str.AppendLine($"\\{LastSaveTime.Ticks}");
            str.AppendLine(
                $"\\{JsonUtility.ToJson(new SerializedTexture { Data = thumbnail.texture.GetRawTextureData(), Height = thumbnail.texture.height, Width = thumbnail.texture.width })}");

            var filePath = Path.Combine(Application.persistentDataPath, Filename) + MetaSuffix;

            FileStream metaFile;
            metaFile = File.Exists(filePath) ? File.OpenWrite(filePath) : File.Create(filePath);

            var bytes = Encoding.Default.GetBytes(str.ToString());

            metaFile.Write(bytes, 0, bytes.Length);
        }
    }
}