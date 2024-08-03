using System.Collections;
using System.Collections.Generic;
using Globals;
using SteeringCalcs;
using UnityEngine;

public class Snake : MonoBehaviour
{
    // Obstacle avoidance parameters (see the assignment spec for an explanation).
    public AvoidanceParams AvoidParams;

    // Steering parameters.
    public float MaxSpeed;
    public float MaxAccel;
    public float AccelTime;

    // Use this as the arrival radius for all states where the steering behaviour == arrive.
    public float ArriveRadius;

    // Parameters controlling transitions in/out of the Aggro state.
    public float AggroRange;
    public float DeAggroRange;

    // The snake's initial position (the target for the PatrolHome and Harmless states).
    private Vector2 _home;

    // The patrol point (the target for the PatrolAway state).
    public Transform PatrolPoint;

    // Reference to the frog (the target for the Aggro state).
    public GameObject Frog;
    private Frog frogScript;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _animator;

    // Direction IDs used by the snake animator (don't edit these).
    private enum Direction : int
    {
        Up = 0,
        Left = 1,
        Down = 2,
        Right = 3
    }

    public SnakeState State;

    public enum SnakeState : int
    {
        PatrolAway = 0,
        PatrolHome = 1,
        Aggro = 2,
        Harmless = 3
    }

    public enum SnakeEvent : int
    {
        FrogInRange = 0,
        FrogOutOfRange = 1,
        HitFrog = 2,
        ReachedTarget = 3
    }

    void Start()
    {   
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _home = transform.position;
        State = SnakeState.PatrolAway;

        frogScript = Frog.GetComponent<Frog>();
    }

    void FixedUpdate()
    {
        if ((Frog.transform.position - transform.position).magnitude < AggroRange)
        {
            HandleEvent(SnakeEvent.FrogInRange);
        }
        if ((Frog.transform.position - transform.position).magnitude > DeAggroRange)
        {
            HandleEvent(SnakeEvent.FrogOutOfRange);
        }
        if (
            (
                State == SnakeState.PatrolAway
                && (transform.position - PatrolPoint.position).magnitude
                    <= Constants.TARGET_REACHED_TOLERANCE
            )
        )
        {
            HandleEvent(SnakeEvent.ReachedTarget);
        }
        if (
            (
                (State == SnakeState.PatrolHome || State == SnakeState.Harmless)
                && ((Vector2)transform.position - _home).magnitude
                    <= Constants.TARGET_REACHED_TOLERANCE
            )
        )
        {
            HandleEvent(SnakeEvent.ReachedTarget);
        }

        // Move towards the target via seek.
        // Note: You will need to edit this so that the steering behaviour
        // depends on the FSM state (see the spec).
        Vector2 desiredVel = Steering.Seek(
            transform.position,
            Frog.transform.position,
            MaxSpeed,
            AvoidParams
        );

        switch (State)
        {
            case SnakeState.PatrolAway:
                desiredVel = Steering.Arrive(
                    transform.position,
                    PatrolPoint.position,
                    ArriveRadius,
                    MaxSpeed,
                    AvoidParams
                );
                break;
            case SnakeState.PatrolHome:
                desiredVel = Steering.Arrive(
                    transform.position,
                    _home,
                    ArriveRadius,
                    MaxSpeed,
                    AvoidParams
                );
                break;
            case SnakeState.Aggro:
                desiredVel = Steering.Seek(
                    transform.position,
                    Frog.transform.position,
                    MaxSpeed,
                    AvoidParams
                );
                break;
            case SnakeState.Harmless:
                desiredVel = Steering.Arrive(
                    transform.position,
                    _home,
                    ArriveRadius,
                    MaxSpeed,
                    AvoidParams
                );
                break;
            default:
                break;
        }
        // Convert the desired velocity to a force, then apply it.
        Vector2 steering = Steering.DesiredVelToForce(desiredVel, _rb, AccelTime, MaxAccel);
        _rb.AddForce(steering);

        UpdateAppearance();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Frog"))
            if (State == SnakeState.Aggro)
            {   
                frogScript.health--;
                HandleEvent(SnakeEvent.HitFrog);
            }
        { }
        if (collision.gameObject.CompareTag("Bubble") && State == SnakeState.Aggro)
        {
            HandleEvent(SnakeEvent.HitFrog);
        }
    }

    void SetState(SnakeState newState)
    {
        if (newState != State)
        {
            State = newState;
        }
    }

    public void HandleEvent(SnakeEvent e)
    {
        if (e == SnakeEvent.HitFrog)
        {
            SetState(SnakeState.Harmless);
            return;
        }

        switch (State)
        {
            case SnakeState.PatrolAway:
                if (e == SnakeEvent.FrogInRange)
                {
                    SetState(SnakeState.Aggro);
                }
                else if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolHome);
                }
                break;
            case SnakeState.PatrolHome:
                if (e == SnakeEvent.FrogInRange)
                {
                    SetState(SnakeState.Aggro);
                }
                else if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolAway);
                }
                break;
            case SnakeState.Harmless:
                if (e == SnakeEvent.ReachedTarget)
                {
                    SetState(SnakeState.PatrolAway);
                }
                break;
            case SnakeState.Aggro:
                if (e == SnakeEvent.FrogOutOfRange)
                {
                    SetState(SnakeState.PatrolHome);
                }
                else if (e == SnakeEvent.HitFrog)
                {
                    SetState(SnakeState.Harmless);
                }
                break;
            default:
                break;
        }
    }

    private void UpdateAppearance()
    {
        if (_rb.velocity.magnitude > Constants.MIN_SPEED_TO_ANIMATE)
        {
            // Determine the bearing of the snake in degrees (between -180 and 180)
            float angle = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;

            if (angle > -135.0f && angle <= -45.0f) // Down
            {
                transform.up = new Vector2(0.0f, -1.0f);
                _animator.SetInteger("Direction", (int)Direction.Down);
            }
            else if (angle > -45.0f && angle <= 45.0f) // Right
            {
                transform.up = new Vector2(1.0f, 0.0f);
                _animator.SetInteger("Direction", (int)Direction.Right);
            }
            else if (angle > 45.0f && angle <= 135.0f) // Up
            {
                transform.up = new Vector2(0.0f, 1.0f);
                _animator.SetInteger("Direction", (int)Direction.Up);
            }
            else // Left
            {
                transform.up = new Vector2(-1.0f, 0.0f);
                _animator.SetInteger("Direction", (int)Direction.Left);
            }
        }

        if (State == SnakeState.Aggro)
        {
            _sr.enabled = true;
            _sr.color = new Color(1.0f, 0.7f, 0.7f);
        }
        else if (State == SnakeState.Harmless)
        {
            _sr.enabled = true;
            _sr.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            _sr.color = Color.white;
        }
    }
}
