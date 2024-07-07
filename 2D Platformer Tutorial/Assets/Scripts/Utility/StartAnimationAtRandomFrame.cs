using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// based on https://www.youtube.com/watch?v=m-6QjaDfigs&ab_channel=PitiIT
public class StartAnimationAtRandomFrame : MonoBehaviour
{
    private void Start()
    {
        var animator = GetComponent<Animator>();
        var state = animator.GetCurrentAnimatorStateInfo(0);
        animator.Play(state.fullPathHash, 0, Random.Range(0.0f, 1.0f));
    }
}
