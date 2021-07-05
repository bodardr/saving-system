using System;

namespace Bodardr.saving
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SaveableAttribute : Attribute
    {
    }
}