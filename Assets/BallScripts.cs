using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BallScripts : MonoBehaviour
{
    [Header("Configurações do Som")]
    [SerializeField] private AudioClip bounceSound;      // O clipe de áudio do impacto
    [SerializeField] private float minImpactVelocity = 0.5f; // Velocidade mínima para tocar o som (evita ruídos infinitos no chão)
    [SerializeField] private float maxImpactVelocity = 15.0f; // Velocidade em que o som atinge o volume máximo (1.0)

    [Header("Variação de Pitch (Tom)")]
    [SerializeField] private float minPitch = 0.85f;      // Variação leve para os sons não soarem idênticos
    [SerializeField] private float maxPitch = 1.15f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Obtém o componente AudioSource anexado ao mesmo objeto
        audioSource = GetComponent<AudioSource>();

        // Garante que o som não vai tocar sozinho ao iniciar o jogo
        audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Se não houver clipe configurado, ignora
        if (bounceSound == null) return;

        // Calcula a força com que a bola colidiu
        float impactForce = collision.relativeVelocity.magnitude;

        // Só toca se a força da colisão for superior à velocidade mínima estipulada
        if (impactForce >= minImpactVelocity)
        {
            // Mapeia o volume entre 0.0 e 1.0 com base na força do impacto
            float volume = Mathf.Clamp01(impactForce / maxImpactVelocity);

            // Adiciona uma pequena variação aleatória de pitch (tom) para dar naturalidade
            audioSource.pitch = Random.Range(minPitch, maxPitch);

            // PlayOneShot permite que vários sons de impacto se sobreponham sem cortar o áudio
            audioSource.PlayOneShot(bounceSound, volume);
        }
    }
}