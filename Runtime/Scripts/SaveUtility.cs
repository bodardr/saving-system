using System.Collections;
using Bodardr.Saving;
using UnityEngine;

public class SaveUtility : MonoBehaviour
{
    public void Save()
    {
        StartCoroutine(SaveCoroutine());
    }

    public void CaptureThumbnail()
    {
        StartCoroutine(SaveCoroutine());
    }
    
    private IEnumerator SaveCoroutine()
    {
        yield return new WaitForEndOfFrame();
        SaveManager.CurrentSave.Metadata.CaptureThumbnail();
        SaveManager.CurrentSave?.Save();
    }
}