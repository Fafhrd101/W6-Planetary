using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "DataHolders/ShieldMaterials")]
public class ShieldMaterials : SingletonScriptableObject<ShieldMaterials>
{
  public Material[] materials;
}
