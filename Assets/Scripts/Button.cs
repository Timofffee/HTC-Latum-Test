using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button : MonoBehaviour {
    
    public void SendEmail() {
        Application.OpenURL("mailto:krougz@live.ru?subject=Feedback");
    }

}
