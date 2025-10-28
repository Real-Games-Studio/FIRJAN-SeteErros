using UnityEngine;

public class ClickParticleSpawner : MonoBehaviour
{
    public GameObject particleSystemPrefab; // Assign your particle system prefab in the Inspector

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Instantiate the particle system at the hit point
                GameObject newParticleSystem = Instantiate(particleSystemPrefab, hit.point, Quaternion.identity);

                // Optional: Destroy the particle system after it finishes playing
                ParticleSystem ps = newParticleSystem.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(newParticleSystem, ps.main.duration);
                }
            }
        }
    }
}