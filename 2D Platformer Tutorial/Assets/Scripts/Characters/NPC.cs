using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public float interactionRange = 5f;

    public GameObject player;
    private Transform playerTransform;
    private Animator animator;

    [SerializeField]
    private bool _isIdle = true;
    public bool IsIdle
    {
        get
        {
            return _isIdle;
        }
        private set
        {
            _isIdle = value;
            animator.SetBool(AnimationStrings.npc_isIdle, value); // link to animator parameter
        }
    }

    [SerializeField]
    private bool _isInteracting = false;
    public bool IsInteracting
    {
        get
        {
            return _isInteracting;
        }
        private set
        {
            _isInteracting = value;
            animator.SetBool(AnimationStrings.npc_isInteracting, value); // link to animator parameter
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null && playerTransform != null)
        {
            bool wasIdle = IsIdle;
            bool wasInteracting = IsInteracting;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            bool isPlayerInRange = distanceToPlayer <= interactionRange;

            if (isPlayerInRange) {
                if (!wasInteracting)
                {
                    IsInteracting = true;

                    if (wasIdle)
                    {
                        IsIdle = false;
                    }
                }
            } 
            else
            {
                if (wasInteracting)
                {
                    IsInteracting = false;
                }
                if (!wasIdle)
                {
                    IsIdle = true;
                }
            }

            
            if (IsInteracting)
            {
                PlayInteractions();
            }
        }
    }


    private void PlayInteractions()
    {

    }

}
