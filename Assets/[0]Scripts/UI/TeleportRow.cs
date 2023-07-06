using TMPro;
using UnityEngine;

public class TeleportRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameLable;
    [SerializeField] private GameObject button;
    
    private TeleportUI _teleportUI = null;
    private Transform _teleportPosition = null;
    
    public void Initialize(TeleportUI teleportUI, string name, Transform teleportPosition, bool canTeleport)
    {
        _teleportUI = teleportUI;
        _teleportPosition = teleportPosition;

        nameLable.text = name;
        
        button.SetActive(canTeleport);
    }

    public void Teleport()
    {
        _teleportUI.TeleportUpdateInfo(this, _teleportPosition.position);
    }
}
