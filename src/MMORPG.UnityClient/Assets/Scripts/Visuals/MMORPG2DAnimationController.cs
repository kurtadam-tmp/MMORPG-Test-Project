using UnityEngine;

public enum CharacterAnimState
{
    Idle,
    Walk,
    Attack,
    CastSkill,
    Hit,
    Die
}

public enum Direction8Way
{
    South,
    SouthWest,
    West,
    NorthWest,
    North,
    NorthEast,
    East,
    SouthEast
}

public class MMORPG2DAnimationController : MonoBehaviour
{
    [Header("2.5D Animation State")]
    public CharacterAnimState CurrentState = CharacterAnimState.Idle;
    public Direction8Way FacingDirection = Direction8Way.South;

    [Header("Visual References")]
    public Transform SpriteBillboard;
    public Renderer SpriteRenderer;

    private CharacterAnimState _previousState = CharacterAnimState.Idle;

    private void Start()
    {
        if (SpriteBillboard == null)
        {
            SpriteBillboard = transform.Find("HeroVisual") ?? transform;
        }

        if (SpriteRenderer == null && SpriteBillboard != null)
        {
            SpriteRenderer = SpriteBillboard.GetComponent<Renderer>();
        }
    }

    public void UpdateAnimationState(Vector3 moveVector, bool isAttacking = false)
    {
        if (isAttacking)
        {
            SetState(CharacterAnimState.Attack);
            return;
        }

        if (moveVector.magnitude >= 0.1f)
        {
            SetState(CharacterAnimState.Walk);
            Calculate8WayDirection(moveVector);
        }
        else if (CurrentState != CharacterAnimState.Attack)
        {
            SetState(CharacterAnimState.Idle);
        }
    }

    public void SetState(CharacterAnimState newState)
    {
        if (CurrentState == newState) return;

        _previousState = CurrentState;
        CurrentState = newState;

        // Visual feedback color tints for 2.5D animation testing
        if (SpriteRenderer != null)
        {
            Color stateColor = newState switch
            {
                CharacterAnimState.Idle => new Color(0f, 0.95f, 0.95f),
                CharacterAnimState.Walk => new Color(0f, 1f, 0.5f),
                CharacterAnimState.Attack => new Color(1f, 0.85f, 0f),
                CharacterAnimState.CastSkill => new Color(0.7f, 0.3f, 1f),
                CharacterAnimState.Hit => new Color(1f, 0.2f, 0.2f),
                CharacterAnimState.Die => new Color(0.3f, 0.3f, 0.3f),
                _ => Color.white
            };
            SpriteRenderer.material.color = stateColor;
        }
    }

    private void Calculate8WayDirection(Vector3 moveDir)
    {
        float angle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        FacingDirection = angle switch
        {
            >= 337.5f or < 22.5f => Direction8Way.North,
            >= 22.5f and < 67.5f => Direction8Way.NorthEast,
            >= 67.5f and < 112.5f => Direction8Way.East,
            >= 112.5f and < 157.5f => Direction8Way.SouthEast,
            >= 157.5f and < 202.5f => Direction8Way.South,
            >= 202.5f and < 247.5f => Direction8Way.SouthWest,
            >= 247.5f and < 292.5f => Direction8Way.West,
            _ => Direction8Way.NorthWest
        };
    }
}
