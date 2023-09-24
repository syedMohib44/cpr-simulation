using UnityEngine;

public abstract class CharacterStateBase : ICharacterState
{
    public static readonly ICharacterState GROUNDED_STATE = new GroundedCharacterState();
    public static readonly ICharacterState JUMPING_STATE = new JumpingCharacterState();
    public static readonly ICharacterState IN_AIR_STATE = new InAirCharacterState();

    public virtual void OnEnter(Character character) { }

    public virtual void OnExit(Character character) { }

    public virtual void Update(Character character)
    {
        character.ApplyGravity();
        // UnityEngine.Debug.Log(CharacterAnimator.IS_PLAYING);
        if (!character.IsBreathing && !character.IsCPR && !character.IsCPROption && !CharacterAnimator.IS_PLAYING)
            character.MoveVector = PlayerInput.GetMovementInput(character.Camera);
        else
            character.MoveVector = Vector3.zero;
        character.ControlRotation = PlayerInput.GetMouseRotationInput();
    }

    public virtual void ToState(Character character, ICharacterState state)
    {
        character.CurrentState.OnExit(character);
        character.CurrentState = state;
        character.CurrentState.OnEnter(character);
    }
}
