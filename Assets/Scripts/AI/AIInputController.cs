using System;
using UnityEngine;

public class AIInputController : MonoBehaviour, IInputController
{
    [SerializeField] private Transform paddle;
    [SerializeField] private float distanceCheck = 0.2f;
    
    private Transform _ball;

    private void Awake()
    {
        _ball = GameObject.FindGameObjectWithTag("Ball").transform;
        
        if (_ball == null)
            throw new NullReferenceException("ball is null");
        
        if (paddle == null)
            throw new NullReferenceException("paddle is null");
    }

    public Vector2 GetMovement()
    {
        if (_ball == null) return Vector2.zero;
        
        float difference = _ball.position.y - paddle.position.y;
        
        if (Mathf.Abs(difference) < distanceCheck)
            return Vector2.zero;
        
        float direction = Mathf.Sign(difference);
        
        return new Vector2(0, direction);
    }
}