using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class BallGrabScript : MonoBehaviour
{
    [Header("Configurações da Bola")]
    [SerializeField] private GameObject heldBallVisual;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float pickupDistance = 2.5f;

    [Header("Configurações de Força do Arremesso")]
    [SerializeField] private float minThrowForce = 5.0f;
    [SerializeField] private float maxThrowForce = 30.0f;
    [SerializeField] private float chargeSpeed = 1.5f;
    [SerializeField] private float upwardThrowForce = 2.0f;
    [SerializeField] private Transform cameraTransform;

    [Header("UI & Gradiente")]
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient chargeGradient;

    [Header("UI - Textos de Instrução (TMP)")]
    [SerializeField] private TextMeshProUGUI pickupTextPrompt; // Texto: "Segurar no E para apanhar a bola"
    [SerializeField] private TextMeshProUGUI dropTextPrompt;   // Texto: "Segurar no E para largar a bola"

    [Header("Input Reference")]
    [SerializeField] private PlayerInput playerInput;

    private InputAction attackAction;
    private bool isHoldingBall = false;
    private bool isCharging = false;
    private float currentCharge = 0f;

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            attackAction = playerInput.actions["Attack"];

            attackAction.started += OnAttackStarted;
            attackAction.canceled += OnAttackCanceled;
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.started -= OnAttackStarted;
            attackAction.canceled -= OnAttackCanceled;
        }
    }

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (heldBallVisual != null)
        {
            heldBallVisual.SetActive(false);
        }

        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(false);
            chargeSlider.value = 0f;
        }

        // Garante que os avisos começam escondidos no arranque
        UpdateUIPrompts(false, false);
    }

    private void Update()
    {
        // Controlo da barra de carregamento do arremesso
        if (isCharging && isHoldingBall)
        {
            currentCharge += Time.deltaTime * chargeSpeed;
            currentCharge = Mathf.Clamp01(currentCharge);

            if (chargeSlider != null)
            {
                chargeSlider.value = currentCharge;
            }

            if (fillImage != null && chargeGradient != null)
            {
                fillImage.color = chargeGradient.Evaluate(currentCharge);
            }
        }

        // Atualiza a visibilidade das mensagens UI de acordo com o estado
        HandleUIPrompts();
    }

    private void HandleUIPrompts()
    {
        if (isHoldingBall)
        {
            // Se tem a bola na mão, mostra apenas o prompt de largar
            UpdateUIPrompts(false, true);
        }
        else
        {
            // Se não tem a bola, verifica se está perto de alguma para mostrar o prompt de apanhar
            bool isNearBall = IsBallNearby();
            UpdateUIPrompts(isNearBall, false);
        }
    }

    // Verifica se existe alguma bola dentro do raio de interação
    private bool IsBallNearby()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupDistance);
        foreach (var col in hitColliders)
        {
            if (col.CompareTag("bola"))
            {
                return true;
            }
        }
        return false;
    }

    // Ativa/Desativa os componentes de texto de acordo com o estado
    private void UpdateUIPrompts(bool showPickup, bool showDrop)
    {
        if (pickupTextPrompt != null)
        {
            pickupTextPrompt.gameObject.SetActive(showPickup);
        }

        if (dropTextPrompt != null)
        {
            dropTextPrompt.gameObject.SetActive(showDrop);
        }
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        if (isHoldingBall)
        {
            StartCharging();
        }
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        if (isCharging && isHoldingBall)
        {
            ThrowBall();
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        if (isHoldingBall)
        {
            DropBall();
        }
        else
        {
            TryPickupBall();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        currentCharge = 0f;

        if (chargeSlider != null)
        {
            chargeSlider.value = 0f;
            chargeSlider.gameObject.SetActive(true);
        }

        if (fillImage != null && chargeGradient != null)
        {
            fillImage.color = chargeGradient.Evaluate(0f);
        }
    }

    private void ThrowBall()
    {
        isCharging = false;

        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(false);
        }

        float calculatedForce = Mathf.Lerp(minThrowForce, maxThrowForce, currentCharge);
        Vector3 spawnPosition = dropPoint != null ? dropPoint.position : cameraTransform.position + cameraTransform.forward * 1.2f;

        if (ballPrefab != null)
        {
            GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
            Rigidbody ballRb = newBall.GetComponent<Rigidbody>();

            if (ballRb != null)
            {
                Vector3 throwDirection = cameraTransform.forward;
                Vector3 force = (throwDirection * calculatedForce) + (Vector3.up * upwardThrowForce);

                ballRb.AddForce(force, ForceMode.Impulse);
            }
        }

        if (heldBallVisual != null)
        {
            heldBallVisual.SetActive(false);
        }

        isHoldingBall = false;
        currentCharge = 0f;
    }

    private void TryPickupBall()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupDistance);

        foreach (var col in hitColliders)
        {
            if (col.CompareTag("bola"))
            {
                Destroy(col.gameObject);

                if (heldBallVisual != null)
                {
                    heldBallVisual.SetActive(true);
                }

                isHoldingBall = true;
                break;
            }
        }
    }

    private void DropBall()
    {
        if (isCharging)
        {
            isCharging = false;
            if (chargeSlider != null) chargeSlider.gameObject.SetActive(false);
        }

        Vector3 spawnPosition = dropPoint != null ? dropPoint.position : transform.position + transform.forward * 1.5f;

        if (ballPrefab != null)
        {
            Instantiate(ballPrefab, spawnPosition, transform.rotation);
        }

        if (heldBallVisual != null)
        {
            heldBallVisual.SetActive(false);
        }

        isHoldingBall = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}
