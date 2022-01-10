using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using UnityEngine;

namespace Bodardr.Saving
{
    public class SaveMetadata
    {
        public const string MetaSuffix = ".meta";

        private Sprite thumbnail;

        private Dictionary<string, string> additionalData = new Dictionary<string, string>();

        public string Filename { get; set; }

        public DateTime CreationTime { get; internal set; }

        public DateTime LastSaveTime { get; internal set; }

        public TimeSpan TimePlayed => LastSaveTime - CreationTime;

        public Sprite Thumbnail => thumbnail;

        public Dictionary<string, string> AdditionalData => additionalData;

        public static SaveMetadata LoadFrom(string metaFile)
        {
            if (!metaFile.EndsWith(MetaSuffix))
                metaFile += MetaSuffix;

            var path = Path.Combine(Application.persistentDataPath, metaFile);
            if (!File.Exists(path))
                return null;

            var stream = File.ReadAllText(path).Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            Sprite thumbnail;

            var serializedTexture = stream[2];
            thumbnail = LoadThumbnail(serializedTexture);

            var fileName = Path.GetFileName(path);
            fileName = fileName.Substring(0, fileName.LastIndexOf(MetaSuffix, StringComparison.OrdinalIgnoreCase));

            var keys = stream[3].Equals("{}\r\n") ? Array.Empty<string>() : JsonUtility.FromJson<string[]>(stream[3]);
            var values = stream[4].Equals("{}\r\n") ? Array.Empty<string>() : JsonUtility.FromJson<string[]>(stream[4]);

            var metadata = new SaveMetadata
            {
                Filename = fileName,
                CreationTime = new DateTime(Convert.ToInt64(stream[0])),
                LastSaveTime = new DateTime(Convert.ToInt64(stream[1])),
                thumbnail = thumbnail,
                additionalData = new Dictionary<string, string>(keys.SelectMany(x =>
                    values.Select(y => new KeyValuePair<string, string>(x, y))))
            };

            return metadata;
        }

        private static Sprite LoadThumbnail(string serializedTextureObject)
        {
            serializedTextureObject = serializedTextureObject.Trim('\n', '\r');
            
            if (string.IsNullOrEmpty(serializedTextureObject))
                return null;
            
            var serializedTex = JsonUtility.FromJson<SerializedTexture>(serializedTextureObject);

            var tex = new Texture2D(serializedTex.Width, serializedTex.Height);
            tex.LoadImage(serializedTex.Data);

            var output = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one);
            return output;
        }

        public void Save(bool saveThumbnail = true)
        {
            LastSaveTime = DateTime.Now;

            var str = new StringBuilder();

            if (saveThumbnail && Thumbnail == null)
                CaptureThumbnail();

            //Append dates
            str.AppendLine($"\\{CreationTime.Ticks}");
            str.AppendLine($"\\{LastSaveTime.Ticks}");

            if (Thumbnail)
                str.AppendLine(
                    $"\\{JsonUtility.ToJson(new SerializedTexture { Data = Thumbnail.texture.EncodeToPNG(), Height = Thumbnail.texture.height, Width = Thumbnail.texture.width })}");
            else
                str.AppendLine("\\");

            str.AppendLine($"\\{JsonUtility.ToJson(additionalData.Keys.ToArray())}");
            str.AppendLine($"\\{JsonUtility.ToJson(additionalData.Values.ToArray())}");

            var filePath = Path.Combine(Application.persistentDataPath, Filename) + MetaSuffix;

            FileStream fileStream;
            try
            {
                fileStream = File.Exists(filePath) ? File.OpenWrite(filePath) : File.Create(filePath);
            }
            catch (IOException io)
            {
                Debug.Log($"Sharing violation : {io.Message}");
                throw;
            }

            var bytes = Encoding.Default.GetBytes(str.ToString());

            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();
        }

        public void CaptureThumbnail()
        {
            thumbnail = Sprite.Create(ScreenCapture.CaptureScreenshotAsTexture(),
                new Rect(0, 0, Screen.width, Screen.height), Vector2.one);
        }
    }
}