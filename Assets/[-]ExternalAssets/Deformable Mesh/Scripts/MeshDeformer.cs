using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshCollider))]
public class MeshDeformer : MonoBehaviour
{
    public bool recalculateNormals;
    public bool collisionDetection;
    public CollectableObject resourceToGenerate;
    [SerializeField] private ParticleSystem collectingParticles;
    [SerializeField] private ParticleSystem collectingParticles_2;
    [SerializeField] private int resourcesCount;

    private Mesh mesh;
    private MeshCollider meshCollider;
    private List<Vector3> vertices;
    private Coroutine partclesPlayCoroutine;
    private int currentResourcesCount = 0;
    private int totalResources = 0;

    private void Start () {
        mesh = GetComponent<MeshFilter> ().mesh;
        meshCollider = GetComponent<MeshCollider> ();
        vertices = mesh.vertices.ToList ();
        currentResourcesCount = 0;
    }

    public void StartParticles()
    {
        collectingParticles.Play();
        collectingParticles_2.Play();
    }

    public void StopParticles()
    {
        collectingParticles.Stop();
        collectingParticles_2.Stop();
    }

    public bool Deform (Vector3 point, float radius, float strength, Vector3 direction)
    {
        totalResources = resourcesCount + UpgradeSaves.Instance.playerChoppingSpeedUpgrades * 2;

        if (IsVerticiesAround(point, radius, 20) == false)
        {
            return false;
        }

        if (CanDestroy())
        {
            return false;
        }
        
        collectingParticles.transform.position = point;
        collectingParticles_2.transform.position = point;

        for (int i = 0; i < vertices.Count; i++) {
            Vector3 vi = transform.TransformPoint (vertices[i]);
            float distance = Vector3.Distance (point, vi);
            float s = strength;
            
            if (distance <= radius) {
                Vector3 deformation = direction * s;
                vertices[i] = transform.InverseTransformPoint (vi + deformation);
            }
        }
        if (recalculateNormals)
            mesh.RecalculateNormals ();
            
        if (collisionDetection) {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;   
        }           
        mesh.SetVertices (vertices);
        currentResourcesCount += 1;
        return true;
    }

    public bool CanDestroy()
    {
        return currentResourcesCount >= totalResources;
    }
    
    private bool IsVerticiesAround(Vector3 point, float radius, int vecticiesCount)
    {
        int v = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vi = transform.TransformPoint (vertices[i]);
            float distance = Vector3.Distance (point, vi);
            if (distance <= radius)
            {
                v++;
            }

            if (v >= vecticiesCount)
            {
                break;
            }
        }
        
        return v >= vecticiesCount;
    }
}