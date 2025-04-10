using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostOdisseyCamera : MonoBehaviour
{
    float SmoothTime = 0.3f;
    Vector3 lerpVelocity = Vector3.zero;

    public CharacterController targetCharacter;
    Transform targetCharacterT;
    public Transform offsetObject; //Objeto que no conviene que se salga de camara (parte del personaje)

    float currentOffset;

    float yAxisOffset = 2.03f; //Donde deberia quedar encima del personaje
    float lastY;  //Antes de saltar la ultima vez que tocó el suelo  

    public Quaternion targetRotationGhost;

    Vector3 cameraRelativeVelocity;
    Vector3 cameraRelativePosition;
    Vector3 expectedPositionRelativeToCamera;
    Vector3 expectedPositionRelativeToWorld;
    Vector3 expectedPositionWithYOffset;

    public bool targetOnScreen;


    // coloca el referente encima de la cabeza y guarda su ultima posicion
    void Awake()
    {
        targetCharacterT = targetCharacter.transform;

        //Hace que la posición y rotación de el objeto a perseguir sea la misma que la del personaje

        //transform.SetPositionAndRotation(new Vector3(characterT.position.x, characterT.position.y + yAxisOffset, characterT.position.z), characterT.rotation);

        transform.position = Vector3.SmoothDamp(transform.position, expectedPositionWithYOffset, ref lerpVelocity, SmoothTime);
        lastY = targetCharacterT.position.y + yAxisOffset;
    }

    private void OnLeaveGround()
    {
        //Nada más dejar el suelo el objeto se queda quieto

        this.transform.position = new Vector3(targetCharacterT.position.x, targetCharacterT.position.y + 2, targetCharacterT.position.z);
    }


    private bool IsObjectOnScreen(Transform currentObject)
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(currentObject.position);

        //Esto es el ancho total de la pantalla (funcionando parecido a aim), se puede reducir los valores

        //Los valores de .y estaban en 0 y 1 

        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0.2 && viewPos.y <= 0.8 && viewPos.z > 0)
            return true;
        else
            return false;
    }

    private void SetAimRelativeToCamera()
    {
        //Te está diciendo la dirección de la velocidad (Input basicamente) y sube o baja en el eje Y según de baja o alta tengas la camara
        //Es lo mismo que (targetCharacter.velocity - character.position) 
        cameraRelativeVelocity = Camera.main.transform.InverseTransformVector(targetCharacter.velocity);

        //currentOffset = Mathf.Clamp(cameraRelativeVelocity.normalized.x * Time.deltaTime, -2, 2);
        //Debug.Log(cameraRelativeVelocity.normalized.z);

        //Es lo mismo que (character.position - camera.position)
        cameraRelativePosition = Camera.main.transform.InverseTransformVector(targetCharacterT.position);

        //basicamente es donde deberia estar el personaje + velocidad añadida en x 
        expectedPositionRelativeToCamera = new Vector3(cameraRelativePosition.x + cameraRelativeVelocity.x / 2, cameraRelativePosition.y, cameraRelativePosition.z + cameraRelativeVelocity.z / 2);

        //Es lo mismo que hacer (camera.position + expectedPositionRelativeToCamera)
        expectedPositionRelativeToWorld = Camera.main.transform.TransformVector(expectedPositionRelativeToCamera);

        //Se mueve hacia la izquierda o derecha del jugador pero tambien sutil hacia delante o atras, cuanto mas tira hacia un lado mas tira ahcia atras
        expectedPositionWithYOffset = new Vector3(expectedPositionRelativeToWorld.x, targetCharacterT.position.y + yAxisOffset, expectedPositionRelativeToWorld.z);

        //La rotacion en Y va acorde a la de la camara
        this.transform.rotation = new Quaternion(0, Camera.main.transform.rotation.y, 0, Camera.main.transform.rotation.w);
    }

    private void Update()
    {

        SetAimRelativeToCamera();
        targetOnScreen = IsObjectOnScreen(offsetObject);

        if (targetCharacter.isGrounded && cameraRelativeVelocity != Vector3.zero)
        {
            transform.position = Vector3.SmoothDamp(transform.position, expectedPositionWithYOffset, ref lerpVelocity, SmoothTime);
            lastY = targetCharacterT.position.y + yAxisOffset; //Recoge el ultimo offset
        }
        else if (!targetOnScreen)
        {
            //Esta parte pude depender o no del diseño del nivel


            transform.position = Vector3.SmoothDamp(transform.position, expectedPositionWithYOffset, ref lerpVelocity, 0.6f);
            //lastY = characterT.position.y + yAxisOffset; //Recoge el ultimo offset
        }
        else
        {
            Vector3 expectedPositionNoY = expectedPositionWithYOffset;
            expectedPositionNoY.y = lastY;
            transform.position = Vector3.SmoothDamp(transform.position, expectedPositionNoY, ref lerpVelocity, SmoothTime);

        }

    }
}
