using System;

namespace Bodardr.Saving
{
    //Idea taken from : https://stackoverflow.com/questions/54159017/is-it-possible-to-serialize-sprites-in-unity/54164247
    [Serializable]
    public class SerializedTexture
    {
        public int Width;

        public int Height;

        public byte[] Data;
    }
}