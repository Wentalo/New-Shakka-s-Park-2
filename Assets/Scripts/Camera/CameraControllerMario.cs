using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerMario : MonoBehaviour
{
    [SerializeField] float SmoothTime = 0.5f;
    private Vector3 _lerpVelocity = Vector3.zero;
    public CharacterController _targetCharacter;
    float _currentOffset = 0.0f;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 position = _targetCharacter.transform.position;

        transform.SetPositionAndRotation(new Vector3 (position.x, this.transform.position.y + 2, position.z), transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        //Transforma la velocidad (vector) del world space al local (al de la camara)
        Vector3 cameraRelativeVelocity = Camera.main.transform.InverseTransformVector(_targetCharacter.velocity);

        //normaliza la velocidad en X y es el offset actual (lo que permite alejarse la camara detras )
        Debug.Log(cameraRelativeVelocity.normalized.x);
        //_currentOffset += cameraRelativeVelocity.normalized.x;
        _currentOffset += cameraRelativeVelocity.normalized.x * Time.deltaTime;

        Mathf.Clamp(_currentOffset, -2, 2);

        //Trnasforma la posicion local en posicion global
        Vector3 cameraRelativePosition = Camera.main.transform.InverseTransformVector(_targetCharacter.transform.position);

        Vector3 expectedPositionRelativeToCamera = new Vector3(cameraRelativePosition.x + cameraRelativeVelocity.x, cameraRelativePosition.y, cameraRelativePosition.z);
        //Vector3 expectedPositionRelativeToCamera = new Vector3(cameraRelativePosition.x + _currentOffset, cameraRelativePosition.y, cameraRelativePosition.z);
        Vector3 expectedPositionRelativeToWorld = Camera.main.transform.TransformVector(expectedPositionRelativeToCamera);

        Vector3 expectedPositionWithYOffset = new Vector3(expectedPositionRelativeToWorld.x, expectedPositionRelativeToWorld.y + 2, expectedPositionRelativeToWorld.z);

        //Si se esta moviendo
        if(_targetCharacter.isGrounded && cameraRelativeVelocity.magnitude != 0)
        {
            //Funciona si te mueves(?)
            transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, expectedPositionWithYOffset, ref _lerpVelocity, SmoothTime), Camera.main.transform.rotation);
        }
        /*
        else if(characterViewPos.y > 0.6f || characterViewPos.y < 0.4) 
        {
            transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, desiredPosition, ref _lerpVelocity, SmoothTime), transform.rotation);
        }
        else if (!_targetCharacter.isGrounded)
        {
            transform.SetPositionAndRotation(Vector3.SmoothDamp(transform.position, new Vector3(desiredPositionAsWorldVector.x, transform.position.y, desiredPositionAsVector.z), ref _lerpVelocity, SmoothTime), transform.position);
        }
        */

    }
}
