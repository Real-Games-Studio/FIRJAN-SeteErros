using UnityEngine;

public class UIScaleBounce : MonoBehaviour
{
    [Header("Configurações de Escala")]
    [Tooltip("Tempo total para a animação (sem contar o overshoot).")]
    public float tempoPrincipal = 0.5f;

    [Tooltip("Tempo adicional para o overshoot (retorno ao 1.0 final).")]
    public float tempoOvershoot = 0.2f;

    [Tooltip("Escala máxima durante o overshoot (ex: 1.2 = 20% maior).")]
    public float fatorOvershoot = 1.2f;

    [Tooltip("Tempo de espera antes de iniciar a animação.")]
    public float delayInicial = 0f;

    private Vector3 escalaInicial;
    private Vector3 escalaFinal = Vector3.one;
    private float tempoAtual = 0f;
    private bool terminou = false;
    private bool emOvershoot = false;
    private bool aguardandoDelay = false;

    void OnEnable()
    {
        // Reinicia o estado sempre que o objeto for ativado
        escalaInicial = Vector3.zero;
        transform.localScale = escalaInicial;

        tempoAtual = 0f;
        terminou = false;
        emOvershoot = false;

        // Se tiver delay, começa uma corrotina para aguardar
        if (delayInicial > 0f)
        {
            aguardandoDelay = true;
            StartCoroutine(AguardarDelay());
        }
        else
        {
            aguardandoDelay = false;
        }
    }

    private System.Collections.IEnumerator AguardarDelay()
    {
        yield return new WaitForSeconds(delayInicial);
        aguardandoDelay = false;
    }

    void Update()
    {
        if (terminou || aguardandoDelay) return;

        tempoAtual += Time.deltaTime;

        if (!emOvershoot)
        {
            float t = Mathf.Clamp01(tempoAtual / tempoPrincipal);
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal * fatorOvershoot, t);

            if (t >= 1f)
            {
                // Começa fase de retorno
                emOvershoot = true;
                tempoAtual = 0f;
            }
        }
        else
        {
            float t = Mathf.Clamp01(tempoAtual / tempoOvershoot);
            transform.localScale = Vector3.Lerp(escalaFinal * fatorOvershoot, escalaFinal, t);

            if (t >= 1f)
                terminou = true;
        }
    }
}

