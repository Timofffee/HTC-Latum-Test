using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public float rotationSpeed = 50.0f;
    
    float defaultCameraRotY;
    bool inGame = false;
    
    void Start() {
        defaultCameraRotY = gameObject.transform.rotation.eulerAngles.y;
    }

    void Update() {
        if (inGame) {
            float y = gameObject.transform.rotation.eulerAngles.y;
            if(Input.GetKey(KeyCode.A)) {
                y += rotationSpeed * Time.deltaTime;
            } else if(Input.GetKey(KeyCode.D)) {
                y -= rotationSpeed * Time.deltaTime;
            }
            gameObject.transform.rotation = Quaternion.Euler(0,y,0);
        }
    }


    public void EnterGame() {
        inGame = true;
    }


    public void ExitGame() {
        gameObject.transform.rotation = Quaternion.Euler(0,defaultCameraRotY,0);
        inGame = false;
    }
}
