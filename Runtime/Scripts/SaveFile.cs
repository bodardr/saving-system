using System;
using System.Collections;
using System.Collections.Generic;

namespace Bodardr.saving
{
    public class SaveFile : IEnumerable
    {
        private const string defaultFilename = "save.dat";

        private string fileName;

        private static Dictionary<Type, object> currentSave;

        public SaveFile(string fileName = "")
        {
            if (string.IsNullOrEmpty(fileName))
                this.fileName = defaultFilename;
            else
            {
                //Remove entries with extensions already in them.
                fileName = fileName.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries)[0];
                
                this.fileName = string.Concat(fileName, ".dat");
            }
        }

        public IEnumerator GetEnumerator()
        {
            return currentSave.GetEnumerator();
        }
    }
}