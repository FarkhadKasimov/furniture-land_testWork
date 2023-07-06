using UnityEngine;

public class PlayerAnimationEventReciever : MonoBehaviour
{
    [SerializeField] private PlayerController controller;

    public void ChopHit()
    {
        controller.ChopHit();
    }
}
