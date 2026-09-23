using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -19.62f;

    [Header("Câmera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float verticalClamp = 80.0f;

    [Header("Chute na Bola")]
    [SerializeField] private float kickForce = 8.0f;       // Força do impacto pra frente
    [SerializeField] private float upwardKickForce = 2.0f; // Pequeno impulso para cima

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;
    private float verticalVelocity;
    private bool jumpRequested;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && controller.isGrounded)
        {
            jumpRequested = true;
        }
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (jumpRequested)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        Vector3 finalVelocity = move * speed;
        finalVelocity.y = verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    // Detecta colisões do CharacterController com outros objetos da cena
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Verifica se o objeto tocado tem a Tag "bola"
        if (hit.gameObject.CompareTag("bola"))
        {
            Rigidbody ballRigidbody = hit.gameObject.GetComponent<Rigidbody>();

            // Confirma que o objeto realmente possui um Rigidbody
            if (ballRigidbody != null)
            {
                // Pega a direção para onde a câmera/jogador está a olhar
                Vector3 kickDirection = transform.forward;

                // Adiciona um componente para cima no vetor da direção
                kickDirection.y = 0.2f;
                kickDirection.Normalize();

                // Calcula o vetor de força final
                Vector3 force = (kickDirection * kickForce) + (Vector3.up * upwardKickForce);

                // Aplica o chute usando Impulse (ideal para impactos instantâneos)
                ballRigidbody.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}
