using System.Collections.Generic;
using Bodardr.Databinding.Runtime;
using UnityEngine;

namespace Bodardr.Saving
{
    [RequireComponent(typeof(BindingCollectionBehavior))]
    public class SaveSlotSelector : MonoBehaviour
    {
        private List<SaveMetadata> slots;

        [SerializeField]
        private int saveSlots;

        private void Start()
        {
            slots = new List<SaveMetadata>(saveSlots);

            slots.AddRange(SaveManager.saveMetadatas);
        }
    }
}