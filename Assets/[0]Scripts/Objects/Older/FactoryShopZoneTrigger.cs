using UnityEngine;

public class FactoryShopZoneTrigger : MonoBehaviour
{
    public int magazineIndex = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            UIManager.Instance.teleportWindow.TeleportFromScene(magazineIndex);
        }
    }
}
