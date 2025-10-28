using UnityEngine;

public class MouseParticleController : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Velocidade de atualização (opcional, pode deixar 1.0 para seguir 100%)")]
    public float suavizacao = 1f;

    private ParticleSystem particleSystemFilho;
    private RectTransform rectTransform;

    void Start()
    {
        // Pega o Particle System do filho
        particleSystemFilho = GetComponentInChildren<ParticleSystem>();
        if (particleSystemFilho == null)
            Debug.LogWarning("Nenhum Particle System encontrado como filho!");

        // Tenta obter o RectTransform, se estiver em UI
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        SeguirMouse();
        DetectarClique();
    }

    void SeguirMouse()
    {
        Vector3 posMouse = Input.mousePosition;

        // Se estivermos no Canvas (UI)
        if (rectTransform != null)
        {
            rectTransform.position = posMouse;
        }
        else
        {
            // Caso seja um objeto 2D normal na cena
            Vector3 posMundo = posMouse;
            posMundo.z = 0f;
            transform.position = Vector3.Lerp(transform.position, posMundo, Time.deltaTime * suavizacao);
        }
    }

    void DetectarClique()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (particleSystemFilho != null)
                particleSystemFilho.Play();
        }
    }
}
