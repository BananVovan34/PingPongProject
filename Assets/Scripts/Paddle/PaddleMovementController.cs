using System;
using UnityEngine;

public class PaddleMovementController : MonoBehaviour, IMoveableController
{
    [SerializeField] private float speed;
    
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            throw new Exception("PaddleMovementController needs a Rigidbody2D component");
        }
    }

    public void Move(Vector2 direction)
    {
        _rb.linearVelocity = direction * speed;
    }
}
