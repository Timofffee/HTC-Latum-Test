using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InGame : MonoBehaviour {
    
    public Player player = new Player();

    public GameObject btnPlay;
    public Sprite imgIdle;
    public Sprite imgRun;

    AudioSource m_AudioSrc;
    Selectable m_ButtonPlay;
    Image m_ImagePlay;

    Player.State m_CurState;

    void Start() {
        m_AudioSrc = GetComponent<AudioSource>();
        m_ButtonPlay = btnPlay.GetComponent<Selectable>();
        m_ImagePlay = btnPlay.GetComponent<Image>();
    }


    void Update() {
        CheckStatus();

        if (player.getState() == Player.State.Idle && !m_ButtonPlay.interactable) {
                m_ButtonPlay.interactable = true;
        }
        player.Update();

    }


    void CheckStatus() {
        if (m_CurState == player.getState()) {
            return;
        } else {
            m_CurState = player.getState();
        }
        switch (m_CurState) {
            case Player.State.Idle:
                m_ImagePlay.sprite = imgRun;
                break;
            case Player.State.Return:
                m_ButtonPlay.interactable = false;
                m_AudioSrc.Stop();
                break;
            case Player.State.Starting:
                m_ImagePlay.sprite = imgIdle;
                m_AudioSrc.Play();
                break;
        }
    }


    public void ChangeState() {
        if (player != null) {
            player.ChangeState();
        }
    }


    public void EnterGame() {
        m_ButtonPlay.interactable = true;
        //TODO
        // Плохой вариант, но это скорее "затычка"
        player.Init(Vector3.zero, 0f, gameObject.transform);
        m_CurState = player.getState();
    }


    public void ExitGame() {
        player.Free();
        m_AudioSrc.Stop();
    }


    [System.Serializable]
    public class Player {

        public enum State {Idle, Starting, Return, Running};
        State m_CurrentState = State.Idle;

        public GameObject playingPrefab;

        public float deadZone = 0.05f;
        public float radius = 1f;
        public float objectSpeed = 2f;
        public float lookAtSpeed = 5f;

        GameObject m_Object;
        Animator m_Anim;
        Vector3 m_Dir;
        Vector3 m_DefaultObjectPos;
        float m_Angle = 0f;


        public void Init(Vector3 pos, float angle, Transform owner) {
            if (m_Object == null) {
                m_CurrentState = State.Idle;
                m_Object = Instantiate(playingPrefab as GameObject);
                m_Anim = m_Object.GetComponent<Animator>();
                m_Dir = m_Object.transform.forward;
                m_Angle = angle * Mathf.Deg2Rad;

                m_DefaultObjectPos = pos;
                m_Object.transform.position = m_DefaultObjectPos;
                m_Object.transform.rotation = Quaternion.Euler(0, angle, 0);
                m_Object.transform.parent = owner;
            }
        }

        public void Free() {
            if (m_Object != null) {
                Destroy(m_Object);
            }
        }


        public void Update() {
            if (m_Object != null) {
                // Oh yeah! Optimization!
                Vector3 objPos = m_Object.transform.position;
                switch (getState()) {
                    case State.Starting:
                        if (getTravel() < radius) {
                            objPos += m_Dir * Time.deltaTime / objectSpeed;
                            UpdatePosRot(objPos);
                        } else {
                            setState(State.Running);
                        }
                        break;
                    case State.Running:
                        objPos = new Vector3(Mathf.Sin(m_Angle), 0f, Mathf.Cos(m_Angle)) * radius;
                        objPos += m_DefaultObjectPos;
                        UpdatePosRot(objPos);

                        m_Angle = m_Angle + Time.deltaTime / objectSpeed * 2.0f * radius;
                        if (Mathf.Abs(m_Angle) >= 2 * Mathf.PI) {
                            m_Angle = (Mathf.Abs(m_Angle) - 2 * Mathf.PI) * Mathf.Sign(m_Angle);
                        }
                        break;
                    case State.Return:
                        if (getTravel() > deadZone) {
                            objPos += (m_DefaultObjectPos - objPos).normalized * Time.deltaTime / objectSpeed;
                            UpdatePosRot(objPos);
                        } else {
                            m_Anim.SetBool("running", false);
                            m_Dir = m_Object.transform.forward;
                            
                            // Можно было просто прописать `m_Angle -= Mathf.PI`, 
                            // но так рассчет будет более правильным
                            float newAngle = Vector3.Angle(m_Dir, Vector3.forward) * Mathf.Deg2Rad;
                            m_Angle = (m_Dir.x < 0) ? Mathf.PI + (Mathf.PI - newAngle) : newAngle;
                            
                            setState(State.Idle);
                        }
                        break;
                }
            }
        }


        public State getState() {
            return m_CurrentState;
        }

        void setState(State state) {
            m_CurrentState = state;
            if (m_CurrentState == State.Idle) {
                m_Anim.SetBool("running", false);
            } else {
                m_Anim.SetBool("running", true);
            }
        }


        public float getTravel() {
            if (m_Object != null) {
                return Vector3.Distance(m_Object.transform.position, m_DefaultObjectPos);
            } else {
                return 0f;
            }
        }


        void UpdatePosRot(Vector3 pos) {
            m_Object.transform.rotation = Quaternion.Lerp(
                    m_Object.transform.rotation,
                    Quaternion.LookRotation(pos - m_Object.transform.position),
                    lookAtSpeed * Time.deltaTime);
            m_Object.transform.position = pos;
        }


        public void ChangeState() {
            if (getState() == State.Idle) {
                setState(State.Starting);
            } else {
                setState(State.Return);
            }
        }
    }

}