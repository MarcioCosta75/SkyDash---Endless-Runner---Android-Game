using UnityEngine;

public class AmmoSpawner : MonoBehaviour
{
    public GameObject[] ammoPickupPrefabs; // Array de prefabs de AmmoPickup
    public float[] spawnProbabilities; // Array de probabilidades de spawn correspondentes
    public float minSpawnInterval = 2f; // Tempo mínimo de espera entre spawns
    public float maxSpawnInterval = 5f; // Tempo máximo de espera entre spawns
    public float fallSpeed = 5f; // Velocidade de queda dos AmmoPickups

    private void Start()
    {
        StartSpawnCountdown();
    }

    private void StartSpawnCountdown()
    {
        float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        Invoke("SpawnAmmoPickup", spawnInterval);
    }

    private void SpawnAmmoPickup()
    {
        // Escolhe aleatoriamente um prefab de AmmoPickup
        int randomIndex = ChooseRandomIndex();
        if (randomIndex != -1)
        {
            GameObject ammoPickupPrefab = ammoPickupPrefabs[randomIndex];

            // Realiza o spawn do AmmoPickup
            GameObject ammoPickup = Instantiate(ammoPickupPrefab, transform.position, Quaternion.identity);
            
            // Obtém o componente Rigidbody2D do AmmoPickup
            Rigidbody2D ammoPickupRigidbody = ammoPickup.GetComponent<Rigidbody2D>();

            // Define a velocidade de queda do AmmoPickup
            ammoPickupRigidbody.gravityScale = fallSpeed;
        }

        // Inicia uma nova contagem regressiva para o próximo spawn
        StartSpawnCountdown();
    }

    private int ChooseRandomIndex()
    {
        // Calcula o total das probabilidades de spawn
        float totalProbability = 0f;
        foreach (float probability in spawnProbabilities)
        {
            totalProbability += probability;
        }

        // Escolhe aleatoriamente um índice baseado nas probabilidades de spawn
        float randomValue = Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;

        for (int i = 0; i < ammoPickupPrefabs.Length; i++)
        {
            cumulativeProbability += spawnProbabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                return i;
            }
        }

        return -1;
    }
}
