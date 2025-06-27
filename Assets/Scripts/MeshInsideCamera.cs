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
            Debug.Log("Dentro de camara");
            //originalRenderer.sharedMaterial.color = Color.green;
        }
        else
        {
            Debug.Log("Fuera de camara");
            //originalRenderer.sharedMaterial.color = Color.red;
        }

        Debug.Log(bounds);

    }
}
