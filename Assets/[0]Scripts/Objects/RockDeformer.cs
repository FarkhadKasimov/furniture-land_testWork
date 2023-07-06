using System;
using System.Collections;
using UnityEngine;

public class RockDeformer : MonoBehaviour
{
    [SerializeField] private Instrument instrument;
    [SerializeField] private float deformRadius = 1f;
    [SerializeField] private float deformStrength = -1f;

    private MeshDeformer meshDeformer = null;
    private bool collect = false;
    private Coroutine deformationRoutine = null;
    private PlayerController playerController;

    private void OnDisable()
    {
        StopCollecting();
    }

    public bool Deform(Vector3 position)
    {
        var radius = deformRadius + (0.1f * UpgradeSaves.Instance.playerSawScaleUpgrade);
        return meshDeformer.Deform(position, radius, deformStrength, Vector3.up);
    }

    private void OnTriggerStay(Collider other)
    {
        meshDeformer = other.GetComponent<MeshDeformer>();
        collect = meshDeformer != null;
        playerController = instrument.Player;

        if (playerController.PlayerInstruments.fuelUI.CanCollectTree() == false)
        {
            return;
        }
        
        if (collect == false)
        {
            return;
        }

        if (deformationRoutine != null)
        {
            return;
        }
        
        deformationRoutine = StartCoroutine(DeformTheRock());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<MeshDeformer>())
        {
            StopCollecting();
        }
    }

    private void StopCollecting()
    {
        StopRoutine();
        collect = false;
        if(playerController != null)
            playerController.SetPlayerSpeed(5f);
        if(meshDeformer != null)
            meshDeformer.StopParticles();
        meshDeformer = null;
    }
    
    public virtual void GiveResource(int count = 1)
    {
        if (playerController.PlayerInstruments.fuelUI.CanCollectTree() == false)
        {
            return;
        }

        if (meshDeformer == null)
        {
            return;
        }
        
        for (int i = 0; i < count; i++)
        {
            if (playerController.CanTake() == false)
            {
                break;
            }
            
            var resource = Instantiate(meshDeformer.resourceToGenerate, transform.position, Quaternion.identity);
            playerController.TakeResource(resource, resource.transform.localScale, true);
        }
    }
    
    public IEnumerator DeformTheRock()
    {
        playerController.SetPlayerSpeed(0.7f);
        meshDeformer.StartParticles();
        
        while (collect && playerController.PlayerInstruments.fuelUI.CanCollectTree())
        {
            var point = transform.position;

            if (Deform(point) == false)
            {
                StopCollecting();
                yield break;
            }
            
            GiveResource(1);
            if (meshDeformer.CanDestroy())
            {
                Destroy(meshDeformer.gameObject);
                StopCollecting();
                yield break;
            }

            var speed = Mathf.Clamp(0.5f - (UpgradeSaves.Instance.playerChoppingSpeedUpgrades * 0.05f), 0.1f, 0.5f);
            yield return new WaitForSeconds(speed);
        }

        StopCollecting();
        yield return null;
    }

    private void StopRoutine()
    {
        if (deformationRoutine != null)
        {
            StopCoroutine(deformationRoutine);
        }
        
        if(playerController != null)
            playerController.SetPlayerSpeed(5f);
        deformationRoutine = null;
    }
}
