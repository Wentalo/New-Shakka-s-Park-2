using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldMarioCamera : MonoBehaviour
{
    [SerializeField] float SmoothTime = 0.5f;
    Vector3 lerpVelocity = Vector3.zero;
    public CharacterController targetCharacter;
    float currentOffset = 0.0f;

    public Camera mainCamera;

    // Start is called before the first frame update
    void LateUpdate()
    {
        transform.SetPositionAndRotation(new Vector3
            (targetCharacter.transform.position.x, this.transform.position.y + 2, targetCharacter.transform.position.z),
            transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraRelativeVelocity = mainCamera.transform.InverseTransformVector(targetCharacter.velocity);

        currentOffset += cameraRelativeVelocity.normalized.x * Time.deltaTime;
        currentOffset = Mathf.Clamp(currentOffset, -2, 2);

        Vector3 cameraRelativePosition = mainCamera.transform.InverseTransformVector(targetCharacter.transform.position);

        Vector3 expectedPositionRelativeToCamera = new Vector3(cameraRelativePosition.x + cameraRelativeVelocity.x, cameraRelativePosition.y, cameraRelativePosition.z);
        Vector3 expectedPositionRelativeToWorld = mainCamera.transform.TransformVector(expectedPositionRelativeToCamera);

        Vector3 expectedPositionWithYOffset = new Vector3(expectedPositionRelativeToWorld.x, expectedPositionRelativeToWorld.y + 2, expectedPositionRelativeToWorld.z);

        //Revisar este if
        if (targetCharacter.isGrounded && cameraRelativeVelocity.magnitude != 0)
        {
            transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, expectedPositionWithYOffset, ref lerpVelocity, SmoothTime), mainCamera.transform.rotation);
        }
    }
}
