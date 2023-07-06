using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TrashBin : MagazineObjects
{
    [SerializeField] private float destroyTime = 0.5f;
    
    private bool exitTrigger = false;
    private Vector3 initialScale = Vector3.zero;
    private void Start()
    {
        initialScale = transform.localScale;
        base.Initialize(true, true);
    }

    public override void OnPlayerEnter(PlayerController controller)
    {
        StartCoroutine(DestroyResourcesFromPlayer(controller));
    }

    public override void OnPlayerExit(PlayerController controller)
    {
        transform.DOScale(initialScale, 0.2f);
        exitTrigger = true;
    }

    private IEnumerator DestroyResourcesFromPlayer(PlayerController controller)
    {
        transform.DOScale(initialScale + (Vector3.one * 0.2f), 0.2f);
        exitTrigger = false;
        
        yield return new WaitForSeconds(0.2f);
        
        var resourcesCount = controller.ObjectsCountInHand();
        while (resourcesCount > 0 || exitTrigger == false)
        {
            var resource = controller.GiveResource();
            if (resource == null)
            {
                break;
            }

            Destroy(resource.gameObject);
            resourcesCount = controller.ObjectsCountInHand();
            
            yield return new WaitForSeconds(destroyTime);
        }

        yield return null;
    }
}
