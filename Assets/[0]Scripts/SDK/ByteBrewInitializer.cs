using ByteBrewSDK;
using UnityEngine;

public class ByteBrewInitializer : MonoBehaviour
{
    private void Awake()
    {
        ByteBrew.InitializeByteBrew();
    }
}
