using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    // Serialized fields
    [SerializeField]
    private new Camera camera = null;

    [SerializeField]
    private MovementSettings movementSettings = null;

    [SerializeField]
    private GravitySettings gravitySettings = null;

    [SerializeField]
    [HideInInspector]
    private RotationSettings rotationSettings = null;

    [SerializeField]
    private GameObject duration_Obj;
    [SerializeField]
    private GameObject rate_Obj;
    [SerializeField]
    private GameObject depth_Obj;
    [SerializeField]
    private GameObject options_Obj;
    [SerializeField]
    private GameObject dead_Obj;
    [SerializeField]
    private GameObject feedback_Obj;
    [SerializeField]
    private GameObject success_Obj;
    [SerializeField]
    private TextMeshProUGUI timerText;
    [SerializeField]
    private Material spot_Mat;
    [SerializeField]
    private int simulation_Time;
    [SerializeField]
    private TextMeshProUGUI sequence_Text;
    [SerializeField]
    private TextMeshProUGUI cpr_Text;
    [SerializeField]
    private TextMeshProUGUI response_Text;
    [SerializeField]
    private TextMeshProUGUI sequence_Correct_Text;
    [SerializeField]
    private TextMeshProUGUI cpr_Correct_Text;
    [SerializeField]
    private TextMeshProUGUI response_Correct_Text;



    // Private fields
    private Vector3 moveVector;
    private Quaternion controlRotation;
    private CharacterController controller;
    private bool isWalking;
    private bool isActive;
    private bool isCPR;
    private bool isCPROption;
    private bool isBreathing;
    private bool isJogging;
    private bool isSprinting;
    private bool isDead;
    private float maxHorizontalSpeed; // In meters/second
    private float targetHorizontalSpeed; // In meters/second
    private float currentHorizontalSpeed; // In meters/second
    private float currentVerticalSpeed; // In meters/second
    private bool isCalling;
    private float score = 20;
    private float timer;
    private float cprTimer;
    private int MAX_TIMER = 60;
    private int TIMER_TO_DEAD = 80;
    private AudioSource audio;
    private RaycastHit hit;
    private bool breathChecked = false;
    private bool ambulanceCalledChecked = false;
    private bool cprChecked = false;
    private bool done = false;
    private string sequenceStr = "You did not follow any steps";
    private string cprStr = "You did not follow any steps";
    private string responseStr = null;
    private int layerMask = 1 << 8; // Bit shift the index of the layer (8) to get a bit mask
    DataManager dataManager;
    private const float DURATION_IN_SECONDS = 1;
    public ICharacterState CurrentState { get; set; }

    #region Unity Methods

    protected virtual void Awake()
    {
        dataManager = new DataManager();
        dataManager.LoadScores();
        // Debug.Log(dataManager.scores["Mohib"].Name + dataManager.scores["Mohib"].Score);
        depth_Obj.SetActive(false);
        duration_Obj.SetActive(false);
        rate_Obj.SetActive(false);
        audio = gameObject.GetComponent<AudioSource>();

        sequence_Correct_Text.color = Color.white;

        sequence_Correct_Text.text = "Incorrect";
        cpr_Correct_Text.text = "Incorrect";
        response_Correct_Text.text = "Timed Out!";
        cpr_Correct_Text.color = Color.white;

        response_Correct_Text.color = Color.white;

        this.controller = this.GetComponent<CharacterController>();

        this.CurrentState = CharacterStateBase.GROUNDED_STATE;
        this.IsJogging = true;
        layerMask = ~layerMask; // This would cast rays only against colliders in layer 8, so we just inverse the mask.
    }

    protected virtual void Update()
    {
        // Mathf.PingPong returns values between 0 and given length.
        // You already pass in 6 so you only need to shift it back by -3.
        if (done)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0);
            }
        }
        if (!done)
        {
            spot_Mat.color = new Color(spot_Mat.color.r, spot_Mat.color.g, spot_Mat.color.b, Mathf.PingPong(Time.time / DURATION_IN_SECONDS, 3) - 1.5f);
            this.CurrentState.Update(this);
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), transform.forward, Color.green);

            // Checks when person is dead.
            if (IsDead)
            {
                cprTimer += Time.deltaTime;
                // Waiting for animation of CPR to be completed for 4 secs
                if ((int)cprTimer == simulation_Time)
                {
                    responseStr = "You were on time but you have killed the person";
                    sequence_Text.text = sequenceStr;
                    cpr_Text.text = cprStr;
                    response_Text.text = responseStr;
                    response_Correct_Text.text = "On Time!";

                    dead_Obj.SetActive(true);
                }
                // Waiting for feedback to appear on screen after above animation is completed.
                if ((int)cprTimer == simulation_Time + 4)
                {
                    feedback_Obj.SetActive(true);
                    PlayerScore playerScore = new PlayerScore()
                    {
                        Name = PlayerPrefs.GetString("Name"),
                        Score = this.score
                    };
                    done = true;
                    dataManager.SaveScores(playerScore);
                }
            }

            // Deduting 1 each each passes after 60 secs.
            if ((int)timer > MAX_TIMER)
            {
                MAX_TIMER = (int)timer;
                score--;
                responseStr = "You saved the person but took too long. Aim to act within 60 seconds for better outcomes.";
            }
            // When get closer to person ray cast detects if there is a person using CheckCollision all people have box colliders attached to their heads
            // to detect the person.
            if (!IsCPR)
            {
                IsActive = CheckCollision();
            }
            else
            {
                // If person is saved then the rest like dead.
                responseStr = "You have saved the person on time";


                cprTimer += Time.deltaTime;

                if ((int)cprTimer == simulation_Time)
                {
                    sequence_Text.text = sequenceStr;
                    cpr_Text.text = cprStr;
                    response_Text.text = responseStr;
                    response_Correct_Text.text = "On Time!";
                    success_Obj.SetActive(true);
                }
                if ((int)cprTimer == simulation_Time + 4)
                {
                    feedback_Obj.SetActive(true);
                    PlayerScore playerScore = new PlayerScore()
                    {
                        Name = PlayerPrefs.GetString("Name"),
                        Score = this.score
                    };
                    dataManager.SaveScores(playerScore);
                    done = true;
                }
            }

            timer += Time.deltaTime;


            if (timer > TIMER_TO_DEAD)
            {
                responseStr = "You are too late sorry you could not save a life";
                response_Correct_Text.text = "Timed Out!";
                response_Correct_Text.color = Color.white;
                IsDead = true;
            }

            timerText.SetText($"Time: {Convert.ToInt32(timer).ToString()}");
            this.UpdateHorizontalSpeed();
            this.ApplyMotion();
        }
    }


    #endregion Unity Methods

    public Vector3 MoveVector
    {
        get
        {
            return this.moveVector;
        }
        set
        {
            float moveSpeed = value.magnitude * this.maxHorizontalSpeed;
            if (moveSpeed < Mathf.Epsilon)
            {
                this.targetHorizontalSpeed = 0f;
                return;
            }
            else if (moveSpeed > 0.01f && moveSpeed <= this.MovementSettings.WalkSpeed)
            {
                this.targetHorizontalSpeed = this.MovementSettings.WalkSpeed;
            }
            else if (moveSpeed > this.MovementSettings.WalkSpeed && moveSpeed <= this.MovementSettings.JogSpeed)
            {
                this.targetHorizontalSpeed = this.MovementSettings.JogSpeed;
            }
            else if (moveSpeed > this.MovementSettings.JogSpeed)
            {
                this.targetHorizontalSpeed = this.MovementSettings.SprintSpeed;
            }

            this.moveVector = value;
            if (moveSpeed > 0.01f)
            {
                this.moveVector.Normalize();
            }
        }
    }

    public Camera Camera
    {
        get
        {
            return this.camera;
        }
    }

    public CharacterController Controller
    {
        get
        {
            return this.controller;
        }
    }

    public MovementSettings MovementSettings
    {
        get
        {
            return this.movementSettings;
        }
        set
        {
            this.movementSettings = value;
        }
    }

    public GravitySettings GravitySettings
    {
        get
        {
            return this.gravitySettings;
        }
        set
        {
            this.gravitySettings = value;
        }
    }

    public RotationSettings RotationSettings
    {
        get
        {
            return this.rotationSettings;
        }
        set
        {
            this.rotationSettings = value;
        }
    }

    public Quaternion ControlRotation
    {
        get
        {
            return this.controlRotation;
        }
        set
        {
            this.controlRotation = value;
            this.AlignRotationWithControlRotationY();
        }
    }

    public bool IsDead
    {
        get
        {
            return this.isDead;
        }
        set
        {
            this.isDead = value;
            if (this.isDead)
            {
                this.isBreathing = false;
                this.isCPR = false;
                this.isWalking = false;
                this.IsJogging = false;
                this.IsSprinting = false;
            }
        }
    }

    public bool IsWalking
    {
        get
        {
            return this.isWalking;
        }
        set
        {
            this.isWalking = value;
            if (this.isWalking)
            {
                this.maxHorizontalSpeed = this.MovementSettings.WalkSpeed;
                this.IsJogging = false;
                this.IsSprinting = false;
            }
        }
    }

    public bool IsActive
    {
        get
        {
            return this.isActive;
        }
        set
        {
            this.isActive = value;
        }
    }

    public bool IsBreathing
    {
        get
        {
            return this.isBreathing;
        }
        set
        {
            this.isBreathing = value;
            if (this.isBreathing)
            {
                this.isCPR = false;
                this.isWalking = false;
                this.IsJogging = false;
                this.IsSprinting = false;
            }
        }
    }

    public bool IsCalling
    {
        get
        {
            return this.isCalling;
        }
        set
        {
            this.isCalling = value;
            if (this.isCalling)
            {
                this.isWalking = false;
                this.IsJogging = false;
                this.IsSprinting = false;
            }
        }
    }


    public bool IsCPR
    {
        get
        {
            return this.isCPR;
        }
        set
        {
            this.isCPR = value;
            if (this.isCPR)
            {
                this.IsWalking = false;
                this.IsJogging = false;
                this.IsSprinting = false;
            }
        }
    }

    public bool IsCPROption
    {
        get
        {
            return this.isCPROption;
        }
        set
        {
            this.isCPROption = value;
            if (this.isCPROption)
            {
                this.IsWalking = false;
                this.IsJogging = false;
                this.IsSprinting = false;
            }
        }
    }


    public bool IsJogging
    {
        get
        {
            return this.isJogging;
        }
        set
        {
            this.isJogging = value;
            if (this.isJogging)
            {
                this.maxHorizontalSpeed = this.MovementSettings.JogSpeed;
                this.IsWalking = false;
                this.IsSprinting = false;
            }
        }
    }

    public bool IsSprinting
    {
        get
        {
            return this.isSprinting;
        }
        set
        {
            this.isSprinting = value;
            if (this.isSprinting)
            {
                this.maxHorizontalSpeed = this.MovementSettings.SprintSpeed;
                this.IsWalking = false;
                this.IsJogging = false;
            }
            else
            {
                if (!(this.IsWalking || this.IsJogging))
                {
                    this.IsJogging = true;
                }
            }
        }
    }

    public bool IsGrounded
    {
        get
        {
            return this.controller.isGrounded;
        }
    }

    public Vector3 Velocity
    {
        get
        {
            return this.controller.velocity;
        }
    }

    public Vector3 HorizontalVelocity
    {
        get
        {
            return new Vector3(this.Velocity.x, 0f, this.Velocity.z);
        }
    }

    public Vector3 VerticalVelocity
    {
        get
        {
            return new Vector3(0f, this.Velocity.y, 0f);
        }
    }

    public float HorizontalSpeed
    {
        get
        {
            return new Vector3(this.Velocity.x, 0f, this.Velocity.z).magnitude;
        }
    }

    public float VerticalSpeed
    {
        get
        {
            return this.Velocity.y;
        }
    }

    #region Methods
    public void Jump()
    {
        this.currentVerticalSpeed = this.MovementSettings.JumpForce;
    }


    public void ToggleCPR()
    {
        if (!IsActive)
            return;
        this.IsCPR = !this.IsCPR;
    }

    public void ToggleCheckBreath()
    {
        if (!IsActive || IsCPR || IsCalling || IsCPROption)
            return;
        this.IsBreathing = !this.IsBreathing;
        if (this.IsBreathing)
            audio.Play();
        else
            audio.Stop();
    }


    public void ToggleCallAmbulance()
    {
        this.isCalling = !this.isCalling;
    }

    public void ToggleUnWalk()
    {
        this.IsWalking = false;
    }
    public void ToggleWalk()
    {
        this.IsWalking = true && !IsCPR && !IsBreathing && !IsCPROption;//!this.IsWalking;

        if (!(this.IsWalking || this.IsJogging))
        {
            this.IsJogging = true;
        }
    }

    public void ApplyGravity(bool isGrounded = false)
    {
        if (!isGrounded)
        {
            this.currentVerticalSpeed =
                MathfExtensions.ApplyGravity(this.VerticalSpeed, this.GravitySettings.GravityStrength, this.GravitySettings.MaxFallSpeed);
        }
        else
        {
            this.currentVerticalSpeed = -this.GravitySettings.GroundedGravityForce;
        }
    }

    public void ResetVerticalSpeed()
    {
        this.currentVerticalSpeed = 0f;
    }

    private void UpdateHorizontalSpeed()
    {
        float deltaSpeed = Mathf.Abs(this.currentHorizontalSpeed - this.targetHorizontalSpeed);
        if (deltaSpeed < 0.1f)
        {
            this.currentHorizontalSpeed = this.targetHorizontalSpeed;
            return;
        }

        bool shouldAccelerate = (this.currentHorizontalSpeed < this.targetHorizontalSpeed);

        this.currentHorizontalSpeed +=
            this.MovementSettings.Acceleration * Mathf.Sign(this.targetHorizontalSpeed - this.currentHorizontalSpeed) * Time.deltaTime;

        if (shouldAccelerate)
        {
            this.currentHorizontalSpeed = Mathf.Min(this.currentHorizontalSpeed, this.targetHorizontalSpeed);
        }
        else
        {
            this.currentHorizontalSpeed = Mathf.Max(this.currentHorizontalSpeed, this.targetHorizontalSpeed);
        }
    }

    private void ApplyMotion()
    {
        this.OrientRotationToMoveVector(this.MoveVector);

        Vector3 motion = this.MoveVector * this.currentHorizontalSpeed + Vector3.up * this.currentVerticalSpeed;
        this.controller.Move(motion * Time.deltaTime);
    }

    private bool AlignRotationWithControlRotationY()
    {
        if (this.RotationSettings.UseControlRotation)
        {
            this.transform.rotation = Quaternion.Euler(0f, this.ControlRotation.eulerAngles.y, 0f);
            return true;
        }

        return false;
    }

    private bool OrientRotationToMoveVector(Vector3 moveVector)
    {
        if (this.RotationSettings.OrientRotationToMovement && moveVector.magnitude > 0f)
        {
            Quaternion rotation = Quaternion.LookRotation(moveVector, Vector3.up);
            if (this.RotationSettings.RotationSmoothing > 0f)
            {
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, this.RotationSettings.RotationSmoothing * Time.deltaTime);
            }
            else
            {
                this.transform.rotation = rotation;
            }

            return true;
        }

        return false;
    }

    public void OnClickDepth(int index)
    {
        depth_Obj.SetActive(false);
        duration_Obj.SetActive(true);

        if (index == (int)GameController.CharacterType)
        {
            score += 20;
            cprStr = "Provided depth to person was Correct \n";
        }
        else
        {
            IsDead = true;
            cprStr = "Provided depth to person was Wrong \n";
            cpr_Correct_Text.color = Color.white;
        }
    }

    public void OnClickDuration(int index)
    {
        duration_Obj.SetActive(false);
        rate_Obj.SetActive(true);
        if (index == 0)
        {
            score += 20;
            cprStr += "Provided duration to person was Correct \n";
        }
        else
        {
            IsDead = true;
            cprStr += "Provided duration to person was Wrong \n";
            cpr_Correct_Text.color = Color.white;
        }
    }

    public void OnClickRate(int index)
    {
        if (index == 0)
        {
            score += 20;
            cprStr += "Compression given to person was Correct \n";
            cpr_Correct_Text.text = "Correct";
            if (IsDead)
            {
                cpr_Correct_Text.text = "Incorrect";
            }
        }
        else
        {
            IsDead = true;
            cprStr += "Compression given to person was Wrong \n";
            cpr_Correct_Text.text = "Incorrect";
            cpr_Correct_Text.color = Color.white;
        }

        rate_Obj.SetActive(false);
        options_Obj.SetActive(true);
        // score += depthScore[index];
        IsCPR = true;
        IsActive = false;
    }

    private bool CheckCollision()
    {
        // if (Physics.Raycast(new Vector3(transform.position.x, transform.position.x + 0.1f, transform.position.z), transform.forward, out hit, 0.5f, layerMask))
        //     return true;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), transform.forward, out hit, 0.7f, layerMask))
        {
            Debug.Log(hit.transform.tag + " ===== ");
            return hit.transform.tag == "Character";
        }
        return false;
    }

    public void OnClickGiveCPR()
    {
        if (!IsActive || IsBreathing || IsCalling)
            return;

        if (cprChecked)
        {
            if (breathChecked && ambulanceCalledChecked)
            {
                score += (20.0f / 3.0f);
                sequenceStr = "Awesome! you have followed all the steps corectly!";
                sequence_Correct_Text.text = "Correct";
            }
            else
            {
                sequenceStr = "You had to check breath of person first then you had call the ambulance";
                sequence_Correct_Text.text = "Incorrect";
                sequence_Correct_Text.color = Color.white;
            }
        }
        cprChecked = true;
        depth_Obj.SetActive(true);
        options_Obj.SetActive(false);
        IsCPR = false;
        IsCPROption = true;
    }

    public void OnClickCallAmbulance()
    {
        if (!IsActive || IsBreathing || IsCPR)
            return;
        Debug.Log(ambulanceCalledChecked + " " + breathChecked);
        if (!ambulanceCalledChecked)
        {
            if (breathChecked)
            {
                score += (20.0f / 3.0f);
            }
            else
            {
                sequenceStr = "You had to check breath of person first ";
                sequence_Correct_Text.text = "Incorrect";
                sequence_Correct_Text.color = Color.white;
            }
        }
        Debug.Log("OnClickCallAmbulance " + score);
        ambulanceCalledChecked = true;
        this.ToggleCallAmbulance();
    }

    public void OnClickCheckBreath()
    {
        if (!IsActive)
            return;

        if (!ambulanceCalledChecked && !cprChecked)
        {
            if (!breathChecked)
            {
                score += (20.0f / 3.0f);
            }
            sequence_Correct_Text.text = "Correct";
        }
        
        Debug.Log("OnClickCheckBreath " + score);
        breathChecked = true;
        this.ToggleCheckBreath();
    }
    #endregion Methods
}
