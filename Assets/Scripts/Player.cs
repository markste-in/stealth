using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static event Action ReachedFinish; 
    public float speed = 3;
    private bool disable = false;
    public float maxRotSpeed = 1;

    private Rigidbody _rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        Agent.OnGuardHasSpottedPlayer += DisablePlayer;
        _rigidbody = GetComponent<Rigidbody>();
    }

    
    private void FixedUpdate()
    {
        if (!disable)
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputZ = Input.GetAxis("Vertical");
            _rigidbody.MovePosition(transform.position + inputZ * Time.fixedDeltaTime * speed * transform.forward);
            Vector3 targetDirection = inputX * Mathf.Sign(inputZ) * transform.right;  // Determines if you rotate left or right -> changes rotation if you move backwards
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection,
                maxRotSpeed * Time.fixedDeltaTime * Mathf.Deg2Rad, 0.0f);
            _rigidbody.MoveRotation(Quaternion.LookRotation(newDirection));
        }
    }

    void DisablePlayer()
    {
        disable = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position,transform.position + transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        DisablePlayer();
        if (ReachedFinish != null)
            ReachedFinish();
    }

    private void OnDestroy()
    {
        Agent.OnGuardHasSpottedPlayer -= DisablePlayer;
    }
}
