using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneObjects : MonoBehaviour {
    
    public Player player = new Player();
    public InteractiveCamera interactiveCamera = new InteractiveCamera();

    public Transform spawnPoint;

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

        if (player.GetState() == Player.State.Idle && !m_ButtonPlay.interactable) {
                m_ButtonPlay.interactable = true;
                m_ButtonPlay.Select();
        }
        player.Update();
        interactiveCamera.Update();
    }


    void CheckStatus() {
        if (m_CurState == player.GetState()) {
            return;
        } else {
            m_CurState = player.GetState();
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
        // Вторым аргументом передаётся только поворот по Y.
        player.Init(spawnPoint.position, spawnPoint.rotation.eulerAngles.y, gameObject.transform);
        interactiveCamera.SetActive(true);
        m_CurState = player.GetState();
    }


    public void ExitGame() {
        player.Free();
        interactiveCamera.SetActive(false);
        m_AudioSrc.Stop();
    }

    
    // Неправильно хранить это здесь, но это куда правильней,
    // чем держать целый скрипт ради одной функции. 
    public void OpenURL(string url) {
        Application.OpenURL(url);
    }


    [System.Serializable]
    public class Player {

        public enum State {Idle, Starting, Return, Running};
        State m_CurrentState = State.Idle;

        public GameObject playingPrefab;

        [Range(0, 0.1f)]
        public float deadZone = 0.05f;
        [Range(0.3f, 100f)]
        public float radius = 1f;
        [Range(0, 10f)]
        public float objectSpeed = 2f;
        [Range(0, 10f)]
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
                Vector3 objPos = m_Object.transform.position;
                switch (GetState()) {
                    case State.Starting:
                        if (GetTravel() < radius) {
                            objPos += m_Dir * Time.deltaTime * objectSpeed;
                            UpdatePosRot(objPos);
                        } else {
                            SetState(State.Running);
                        }
                        break;
                    case State.Running:
                        objPos = new Vector3(Mathf.Sin(m_Angle), 0f, Mathf.Cos(m_Angle)) * radius;
                        objPos += m_DefaultObjectPos;
                        UpdatePosRot(objPos);

                        m_Angle = m_Angle + Time.deltaTime * objectSpeed * 2.0f * radius;
                        if (Mathf.Abs(m_Angle) >= 2 * Mathf.PI) {
                            m_Angle = (Mathf.Abs(m_Angle) - 2 * Mathf.PI) * Mathf.Sign(m_Angle);
                        }
                        break;
                    case State.Return:
                        if (GetTravel() > deadZone) {
                            objPos += (m_DefaultObjectPos - objPos).normalized * Time.deltaTime * objectSpeed;
                            UpdatePosRot(objPos);
                        } else {
                            m_Anim.SetBool("running", false);
                            m_Dir = m_Object.transform.forward;
                            
                            // Можно было просто прописать `m_Angle -= Mathf.PI`, 
                            // но так рассчет будет более правильным
                            float newAngle = Vector3.Angle(m_Dir, Vector3.forward) * Mathf.Deg2Rad;
                            m_Angle = (m_Dir.x < 0) ? Mathf.PI + (Mathf.PI - newAngle) : newAngle;
                            
                            SetState(State.Idle);
                        }
                        break;
                }
            }
        }


        public State GetState() {
            return m_CurrentState;
        }

        void SetState(State state) {
            m_CurrentState = state;
            if (m_CurrentState == State.Idle) {
                m_Anim.SetBool("running", false);
            } else {
                m_Anim.SetBool("running", true);
            }
        }


        public float GetTravel() {
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
            if (GetState() == State.Idle) {
                SetState(State.Starting);
            } else {
                SetState(State.Return);
            }
        }
    }


    [System.Serializable]
    public class InteractiveCamera {
        [Range(-180f, 180f)]
        public float angleHorizontal = 30f;
        [Range(-180f, 180f)]
        public float angleVertical = 0f;
        [Range(0, 1000f)]
        public float rotationSpeed = 50f;
        [Range(0, 100f)]
        public float rotationSpeedSmooth = 1f;
        [Range(0, 1000f)]
        public float distance = 1f;
        [Range(0, 100f)]
        public float distanceSpeed = 1f;
        [Range(0, 100f)]
        public float distanceSpeedSmooth = 1f;
        [Range(0, 1000f)]
        public float distanceMin = 1f;
        [Range(0, 1000f)]
        public float distanceMax = 2f;

        public Transform m_Camera;
        public Transform m_Target;

        bool m_IsActive = true;
        float m_Distance;


        public InteractiveCamera() {
            m_Distance = distance;
        }

        public void Update() {
            if (m_IsActive && m_Target) {
                UpdateRot();
                UpdatePos();
            }
        }


        void UpdateRot() {
            if(Input.GetKey(KeyCode.A)) {
                angleVertical += rotationSpeed * Time.deltaTime;
            } else if(Input.GetKey(KeyCode.D)) {
                angleVertical -= rotationSpeed * Time.deltaTime;
            }

            Quaternion newAngle = Quaternion.Euler(0, angleVertical, 0);
            newAngle *= Quaternion.Euler(angleHorizontal, 180, 0);
            m_Camera.rotation = Quaternion.Lerp(m_Camera.rotation, newAngle, rotationSpeedSmooth * Time.deltaTime);
        }


        void UpdatePos() {

            if (Input.GetKey(KeyCode.S)) {
                distance += distanceSpeed * Time.deltaTime;
            } else if (Input.GetKey(KeyCode.W)) {
                distance -= distanceSpeed * Time.deltaTime;
            }

            distance = Mathf.Clamp(distance, distanceMin, distanceMax);

            m_Distance = Mathf.Lerp(m_Distance, distance, distanceSpeedSmooth * Time.deltaTime);
            
            Vector3 pos = new Vector3(0, 
                    Mathf.Sin(angleHorizontal * Mathf.Deg2Rad) * m_Distance, 
                    Mathf.Cos(angleHorizontal * Mathf.Deg2Rad) * m_Distance);
            m_Camera.position = Vector3.Lerp(m_Camera.position, 
                    Quaternion.Euler(0, angleVertical, 0) * pos + m_Target.position, 
                    rotationSpeedSmooth * Time.deltaTime);
        }


        public void SetActive(bool active) {
            m_IsActive = active;
        }
    }
}