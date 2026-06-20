using System.Collections;
using UnityEngine;

public class IntroFlowerActivator : MonoBehaviour
{
    [Tooltip("Tiempo en segundos desde el inicio de la escena para activar la flor (cuando se agacha)")]
    public float delay = 2.2f;

    [Tooltip("El hueso de la mano que se usará como referencia de posición")]
    public HumanBodyBones handBone = HumanBodyBones.RightHand;

    [Tooltip("Multiplicador de escala para hacer un ramo pequeño")]
    public float scaleMultiplier = 0.18f;

    private GameObject flowerGrave;
    private Transform handTransform;

    private void Start()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null && animator.isHuman)
        {
            handTransform = animator.GetBoneTransform(handBone);
        }

        SetupFlowerObject();
    }

    private void SetupFlowerObject()
    {
        // 1. Encontrar el objeto original con un método de búsqueda robusto recursivo
        GameObject originalBouquet = FindOriginalBouquet();

        if (originalBouquet == null)
        {
            Debug.LogWarning("[IntroFlowerActivator] No se encontró el modelo de flores original 'SM_Buquet_Bv11' para duplicarlo.");
            return;
        }

        // 2. Duplicar el ramo de flores
        flowerGrave = GameObject.Instantiate(originalBouquet);
        flowerGrave.name = "Flower_Grave";

        // Asegurarnos de que no sea estático para poder moverlo y activarlo en runtime
        flowerGrave.isStatic = false;
        foreach (Transform t in flowerGrave.GetComponentsInChildren<Transform>())
        {
            t.gameObject.isStatic = false;
        }

        // Establecer el padre al mismo contenedor = cemetery para limpieza
        GameObject cemeteryContainer = GameObject.Find("= cemetery");
        if (cemeteryContainer != null)
        {
            flowerGrave.transform.SetParent(cemeteryContainer.transform);
        }

        // 3. Desactivar el objeto de la flor inicialmente
        flowerGrave.SetActive(false);
        Debug.Log("[IntroFlowerActivator] Ramo de flores creado y listo para colocarse.");
    }

    private GameObject FindOriginalBouquet()
    {
        // 1. Intentar buscar directamente por nombre
        GameObject go = GameObject.Find("SM_Buquet_Bv11");
        if (go != null) return go;

        // 2. Intentar buscar en el contenedor de cementerio por ruta
        GameObject cemetery = GameObject.Find("= cemetery");
        if (cemetery != null)
        {
            Transform t = cemetery.transform.Find("SM_Buquet_Bv11");
            if (t != null) return t.gameObject;
        }

        // 3. Búsqueda recursiva en las raíces de la escena activa (para buscar inactivos o profundos)
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (GameObject rootGo in activeScene.GetRootGameObjects())
        {
            foreach (Transform child in rootGo.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "SM_Buquet_Bv11")
                {
                    return child.gameObject;
                }
            }
        }

        return null;
    }

    private IEnumerator ActivateFlowerRoutine()
    {
        yield return new WaitForSeconds(delay);
        if (flowerGrave != null)
        {
            // Posicionar la flor en el punto exacto debajo de la mano en este frame
            Vector3 spawnPos = transform.position + transform.forward * 1.05f; // fallback
            if (handTransform != null)
            {
                spawnPos = handTransform.position;
            }

            // Proyectar al suelo usando un raycast desde la posición de la mano
            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 1f, Vector3.down, out hit, 4f))
            {
                spawnPos.y = hit.point.y;
            }
            else
            {
                spawnPos.y = transform.position.y; // fallback al nivel de los pies
            }

            flowerGrave.transform.position = spawnPos;
            
            // Conservar la rotación original (vertical)
            GameObject originalBouquet = FindOriginalBouquet();
            if (originalBouquet != null)
            {
                flowerGrave.transform.rotation = originalBouquet.transform.rotation;
                flowerGrave.transform.localScale = originalBouquet.transform.localScale * scaleMultiplier;
            }

            flowerGrave.SetActive(true);
            Debug.Log("[IntroFlowerActivator] ¡Flor colocada en la tumba!");
        }
    }
}
