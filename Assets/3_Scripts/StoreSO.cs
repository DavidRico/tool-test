using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Store", menuName = "Store", order = 0)]
public class StoreSO : ScriptableObject
{
    public List<StoreItem> StoreItems;
}