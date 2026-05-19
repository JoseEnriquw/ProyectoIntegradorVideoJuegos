using UnityEngine;

namespace UHFPS.Custom
{
    public class SacudirYTirar : MonoBehaviour
    {
        [Header("Objetos a Tirar (Melones)")]
        [Tooltip("Arrastra aquí los objetos que quieres que se caigan al interactuar.")]
        public Rigidbody[] objetosQueCaen;

        [Header("Configuración de Fuerza")]
        [Tooltip("Fuerza con la que salen rebotando los objetos al caer.")]
        public float fuerzaDeEmpuje = 1.5f;

        [Header("Animador del Puesto (Opcional)")]
        [Tooltip("Si el puesto tiene una animación de sacudirse, pon el Animator aquí.")]
        public Animator animatorPuesto;
        [Tooltip("El nombre del Trigger en el Animator para sacudir.")]
        public string triggerSacudir = "sacudir";

        /// <summary>
        /// Esta función la llamaremos desde el evento On Interact de tu cubo.
        /// </summary>
        public void EjecutarCaida()
        {
            // 1. Reproducir animación de sacudir el puesto (si existe)
            if (animatorPuesto != null)
            {
                animatorPuesto.SetTrigger(triggerSacudir);
            }

            // 2. Hacer que los objetos seleccionados se caigan
            foreach (Rigidbody rb in objetosQueCaen)
            {
                if (rb != null)
                {
                    // Desactivamos el Kinematic para que la gravedad actúe y se caigan de la mesa
                    rb.isKinematic = false;

                    // Si la fuerza de empuje es mayor a 0, le damos un leve empujón hacia adelante
                    if (fuerzaDeEmpuje > 0)
                    {
                        Vector3 empujeFrontal = transform.forward * fuerzaDeEmpuje;
                        rb.AddForce(empujeFrontal, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
