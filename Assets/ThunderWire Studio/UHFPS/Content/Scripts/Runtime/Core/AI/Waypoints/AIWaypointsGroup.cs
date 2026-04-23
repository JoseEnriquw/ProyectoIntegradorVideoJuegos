using System.Collections.Generic;
using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("AI Waypoints Group"), ExecuteInEditMode]
    public class AIWaypointsGroup : MonoBehaviour
    {
        public List<AIWaypoint> Waypoints = new();

        [Header("Gizmos")]
        public Color GroupColor = Color.red;
        public bool ConnectedGizmos;
        public bool ConnectEndWithStart;
        public bool ConnectAllWithAll;

        private void OnValidate()
        {
            RefreshWaypoints();
        }

        private void RefreshWaypoints()
        {
            Waypoints.Clear();

            foreach (Transform t in transform)
            {
                if (t.TryGetComponent(out AIWaypoint waypoint))
                {
                    Waypoints.Add(waypoint);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (Waypoints.Count == 0) return;

            if (ConnectedGizmos && ConnectAllWithAll)
            {
                foreach (var curr in Waypoints)
                {
                    foreach (var other in Waypoints)
                    {
                        if (curr == other) continue;

                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(curr.transform.position, other.transform.position);
                    }
                }
                return;
            }

            for (int i = 0; i < Waypoints.Count - 1; i++)
            {
                if (!ConnectedGizmos) continue;

                Gizmos.color = Color.white;
                Gizmos.DrawLine(
                    Waypoints[i].transform.position,
                    Waypoints[i + 1].transform.position
                );
            }

            if (ConnectEndWithStart && Waypoints.Count > 1)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(
                    Waypoints[Waypoints.Count - 1].transform.position,
                    Waypoints[0].transform.position
                );
            }
        }

        void OnDrawGizmos()
        {
            foreach (var curr in Waypoints)
            {
                Gizmos.color = GroupColor.Alpha(0.5f);
                Gizmos.DrawSphere(curr.transform.position, 0.1f);
            }
        }
    }
}