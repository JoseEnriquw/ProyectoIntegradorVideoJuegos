using UnityEngine;

namespace UHFPS.Runtime
{
    public class DrawerColliderHelper : MonoBehaviour
    {
        public Collider DrawerCollider;
        public Collider TableCollider;

        public void SetColliderEnabled(bool state)
        {
            if (DrawerCollider != null)
            {
                DrawerCollider.enabled = state;
            }
            if (TableCollider != null)
            {
                TableCollider.enabled = state;
            }
        }
    }
}
