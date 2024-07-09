using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    public GameObject player;
    private Transform playerTransform;
    private Animator animator;

    public float interactionRange = 20f;

    public Canvas speechCanvas;
    public Text speechText;
    public int speechIndex = 0;

    public float horizontalOffset = 7f;
    public float verticalOffset = 7f;
    public bool showToRight = true;


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


    private string[] speech = {
        "Welcome to the realm \nbetween life & death.\n\nJump and run to traverse \nthese lands, but remain vigilant!",

        "There is a big gap in front of you. \nTry to activate the dash ability.",

        "Here, collect this powerful orb! \nThis will surely be of use\n when trying to apply force."
    };



    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (speechCanvas != null)
        {
            float xOffset = showToRight ? horizontalOffset : -horizontalOffset;
            float yOffset = verticalOffset;

            Vector3 newPos = transform.position + new Vector3(xOffset, yOffset, 0);
            speechCanvas.transform.position = newPos;
            speechCanvas.gameObject.SetActive(false);
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
                    //speechCanvas.gameObject.SetActive(false);
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
        if (speechCanvas != null && speechText != null)
        {
            speechIndex = Mathf.Clamp(speechIndex, 0, speech.Length - 1);
            speechText.text = speech[speechIndex];
            speechCanvas.gameObject.SetActive(true);
        }
    }


}
