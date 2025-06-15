using UnityEngine;

public class MeshInsideCamera : MonoBehaviour
{

    Camera activeCamera;
    SkinnedMeshRenderer originalRenderer;
    SkinnedMeshRenderer copyRenderer;
    Plane[] cameraFrustrum;
    public Collider targetCollider;
    //

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activeCamera = Camera.main;
        originalRenderer = GetComponent<SkinnedMeshRenderer>();
        copyRenderer = originalRenderer;
    }

    // Update is called once per frame
    void Update()
    {
        var bounds = targetCollider.bounds;
        cameraFrustrum = GeometryUtility.CalculateFrustumPlanes(activeCamera);
        if (GeometryUtility.TestPlanesAABB(cameraFrustrum, bounds))
        {
            originalRenderer.sharedMaterial.color = Color.green;
        }
        else
        {
            originalRenderer.sharedMaterial.color = Color.red;
        }

    }
}
