using System.Diagnostics;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public static readonly int HORIZONTAL_SPEED = Animator.StringToHash("HorizontalSpeed");
    public static readonly int VERTICAL_SPEED = Animator.StringToHash("VerticalSpeed");
    public static readonly int IS_GROUNDED = Animator.StringToHash("IsGrounded");
    public static readonly int IDLE = Animator.StringToHash("Idle");
    public static readonly int WALK = Animator.StringToHash("WALK");
    public static readonly int CPR = Animator.StringToHash("CPR");
    public static readonly int CALL = Animator.StringToHash("CALL");
    public static readonly int CHECK = Animator.StringToHash("CHECK");
    public static readonly int DEAD = Animator.StringToHash("DEAD");
    public static readonly int ACTIVE = Animator.StringToHash("ACTIVE");
    public static readonly int IDLE_THINKING = Animator.StringToHash("IdleThinking");
    public static readonly int IDLE_REJECTED = Animator.StringToHash("IdleRejected");
    public static bool IS_PLAYING = false;

    public Animator animator;
    public Animator option;
    private Character character;

    protected virtual void Awake()
    {
        // this.animator = this.GetComponent<Animator>();
        this.character = this.GetComponent<Character>();
    }

    protected virtual void Update()
    {
        IS_PLAYING = isPlaying("Administering Cpr") || isPlaying("Call") || isPlaying("Check") || this.character.IsCalling || this.character.IsCPR || this.character.IsBreathing;
        this.animator.SetFloat(HORIZONTAL_SPEED, this.character.HorizontalSpeed);
        this.animator.SetFloat(VERTICAL_SPEED, this.character.VerticalSpeed);
        this.animator.SetBool(WALK, this.character.IsWalking);
        this.animator.SetBool(CPR, this.character.IsCPR);
        this.animator.SetBool(CALL, this.character.IsCalling);
        if (this.character.IsDead)
            this.animator.SetTrigger(DEAD);
        this.animator.SetBool(CHECK, this.character.IsBreathing);
        this.animator.SetBool(IS_GROUNDED, this.character.IsGrounded);
        this.animator.SetBool(IS_GROUNDED, this.character.IsGrounded);
        this.option.SetBool(ACTIVE, this.character.IsActive);
    }

    public bool isPlaying(string stateName)
    {
        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            return true;
        else
            return false;
    }
}
