using System.Collections;
using UnityEngine;

public class Button : MonoBehaviour {
    
    public void SendEmail() {
        Application.OpenURL("mailto:krougz@live.ru?subject=Feedback");
    }

}
