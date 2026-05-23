using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    [System.Serializable]
    public class RespawnPoint
    {
        [Tooltip("Nombre descriptivo para este punto (ej. Punto Suero 1)")]
        public string pointName;

        [Tooltip("Transform donde aparecerá el item. Si está vacío, se usará la posición de este script.")]
        public Transform spawnLocation;

        [Tooltip("El prefab del item a instanciar (suero, sal, chatarra, etc.)")]
        public GameObject itemPrefab;

        [Tooltip("Tiempo en segundos para que reaparezca el item después de ser recogido.")]
        public float respawnTime = 60f;

        // Variables internas ocultas en el inspector
        [HideInInspector] public GameObject currentItem;
        [HideInInspector] public float currentTimer;
        [HideInInspector] public bool isWaitingForRespawn;
    }

    public class ItemRespawner : MonoBehaviour
    {
        [Header("Configuración de Respawn")]
        [Tooltip("Lista de puntos donde pueden aparecer (y reaparecer) los items.")]
        public List<RespawnPoint> respawnPoints = new List<RespawnPoint>();

        private void Start()
        {
            // Al inicio, instanciamos todos los items en sus puntos correspondientes
            foreach (var point in respawnPoints)
            {
                if (point.itemPrefab != null)
                {
                    SpawnItem(point);
                }
                else
                {
                    Debug.LogWarning($"[ItemRespawner] El punto '{point.pointName}' no tiene un prefab asignado.");
                }
            }
        }

        private void Update()
        {
            foreach (var point in respawnPoints)
            {
                // Chequeamos si el item actual ya no existe en la escena o fue desactivado (recogido)
                if (!point.isWaitingForRespawn && (point.currentItem == null || !point.currentItem.activeInHierarchy))
                {
                    // Si el item fue desactivado en lugar de destruido por el sistema de inventario,
                    // soltamos la referencia para que empiece el timer de respawn.
                    // Nota: Si el objeto solo se desactivó, lo ideal es no destruirlo nosotros por las dudas de que 
                    // el sistema de inventario todavía lo use (aunque en UHFPS usualmente se puede destruir si DisableType es Destroy).
                    // Pero liberamos la referencia para poder instanciar uno nuevo luego.
                    point.currentItem = null; 

                    point.isWaitingForRespawn = true;
                    point.currentTimer = point.respawnTime;
                    
                    // Opcional: Debug log para ver que empezó el respawn
                    // Debug.Log($"[ItemRespawner] Item recogido en '{point.pointName}'. Respawn en {point.respawnTime}s.");
                }

                // Manejamos el tiempo de espera
                if (point.isWaitingForRespawn)
                {
                    point.currentTimer -= Time.deltaTime;
                    if (point.currentTimer <= 0)
                    {
                        SpawnItem(point);
                    }
                }
            }
        }

        private void SpawnItem(RespawnPoint point)
        {
            Transform spawnTrans = point.spawnLocation != null ? point.spawnLocation : transform;

            // Instanciamos el nuevo item
            point.currentItem = Instantiate(point.itemPrefab, spawnTrans.position, spawnTrans.rotation);
            
            // Si el prefab original está desactivado por error, nos aseguramos de que el instanciado esté activo
            point.currentItem.SetActive(true);

            point.isWaitingForRespawn = false;
        }
    }
}
