using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private PlayerInput playerInput;
    private Animator animator;
    private CharacterController characterController;

    private enum PlayerState
    {
        Idle,
        Walk
    }

    private PlayerState currentPlayerState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        playerInput = new PlayerInput();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }
}
