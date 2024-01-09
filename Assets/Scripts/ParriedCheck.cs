using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParriedCheck : MonoBehaviour
{
    private Animator animator;
    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Parry")) {
            animator.SetTrigger("isParried");
            Debug.Log("Player was Parried");
        }
    }
}
