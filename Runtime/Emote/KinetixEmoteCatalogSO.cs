using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Kinetix/FreeEmoteCatalog")]
public class KinetixEmoteCatalogSO : ScriptableObject
{
    public EmoteFetchInfo[] infos;
}

[System.Serializable]
public struct EmoteFetchInfo
{
    public string contractAddress;
    public string tokenId;
}
