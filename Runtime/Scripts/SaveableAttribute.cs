using System;

namespace Bodardr.Saving
{
    [AttributeUsage(AttributeTargets.Class)]
    [Serializable]
    public class SaveableAttribute : Attribute
    {
    }
}