using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGame : MonoBehaviour {
    public GameObject audioSource;
    public GameObject playingObject;
    public GameObject btnPlay;
    public Sprite imgIdle;
    public Sprite imgRun;

    public float deadZone = 0.05f;
    public float radius = 1f;
    public float objectSpeed = 2f;

    AudioSource m_AudioSrc;
    Animator m_Anim;

    bool canRun = false;
    bool isRunning = false;
    bool onCir = false;
    Vector3 dir;
    
    Vector3 defaultObjectPos;
    Quaternion defaultObjectRot;

    float posX, posZ, angle = 0f;
    

    void Start() {
        defaultObjectPos = playingObject.transform.position;
        defaultObjectRot = playingObject.transform.rotation;

        m_AudioSrc = audioSource.GetComponent<AudioSource>();
        m_Anim = playingObject.GetComponent<Animator>();

        dir = playingObject.transform.forward;
    }


    void Update() {
        if (isRunning) {
            // Oh yeah! Optimization!
            Vector3 objPos = playingObject.transform.position;
            if (canRun) {
                if (Vector3.Distance(objPos, defaultObjectPos) < radius && !onCir) {
                    objPos += dir * Time.deltaTime / objectSpeed;
                } else {
                    onCir = true;
                    // TODO
                    // objPos += Quaternion.Euler(0, 90, 0) * (objPos - defaultObjectPos).normalized * Time.deltaTime / objectSpeed;
                    posZ = defaultObjectPos.z + Mathf.Cos(angle) * radius;
                    posX = defaultObjectPos.x + Mathf.Sin(angle) * radius;
                    objPos = new Vector3(posX, 0f, posZ);
                    angle = angle + Time.deltaTime / objectSpeed * 2.0f * radius;
                    if (angle >= 2 * Mathf.PI)
                        angle = 0f;
                }
                
            } else if (isRunning) {
                if (Vector3.Distance(objPos, defaultObjectPos) > deadZone) {
                    objPos += (defaultObjectPos - objPos).normalized * Time.deltaTime / objectSpeed;
                } else {
                    btnPlay.GetComponent<Selectable>().interactable = true;
                    m_Anim.SetBool("running", false);
                    isRunning = false;
                    dir = playingObject.transform.forward;

                    float dd = Vector3.Angle(dir, Vector3.forward) * Mathf.Deg2Rad;
                    dd = (dir.x < 0) ? Mathf.PI + (Mathf.PI - dd) : dd;
                    angle = dd;
                    onCir = false;
                    return;
                }
            }
            
            if (isRunning) {
                playingObject.transform.rotation = Quaternion.Lerp(
                        playingObject.transform.rotation,
                        Quaternion.LookRotation(objPos - playingObject.transform.position),
                        5f * Time.deltaTime);
                playingObject.transform.position = objPos;    
            }
            
        }
    }


    void CheckStatus() {
        if (canRun) {
            btnPlay.GetComponent<Image>().sprite = imgIdle;
            isRunning = true;
            m_Anim.SetBool("running", true);
            m_AudioSrc.Play();
        } else {
            btnPlay.GetComponent<Image>().sprite = imgRun;
            if (isRunning) {
                btnPlay.GetComponent<Selectable>().interactable = false;
                m_AudioSrc.Stop();
            }
        }
    }


    public void ChangeState() {
        canRun = !canRun;
        CheckStatus();
    }


    public void EnterGame() {
        btnPlay.GetComponent<Selectable>().interactable = true;
        m_Anim.enabled = true;
        CheckStatus();
    }


    public void ExitGame() {
        canRun = false;
        isRunning = false;
        onCir = false;
        m_Anim.SetBool("running", false);
        m_AudioSrc.Stop();
        playingObject.transform.position = defaultObjectPos;
        playingObject.transform.rotation = defaultObjectRot;
        dir = playingObject.transform.forward;
        angle = 0f;
        // ... Optimization?!
        m_Anim.enabled = false;
    }
}
