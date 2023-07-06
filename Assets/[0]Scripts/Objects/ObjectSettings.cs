using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "CollectableObjectSettings", menuName = "CollectableObjectSettings")] 
public class ObjectSettings : ScriptableObject
{
    public ObjectType type;
    public int cost;
    public Sprite image;
}
