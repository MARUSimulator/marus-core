using System;
using MissionWaypointNS;
using UnityEngine;

namespace Labust.Controllers
{
    /// <summary>
    /// Controller that positions the object with respect to the diver
    /// 
    /// Implements a P regulator for position and orientation of the object with
    /// respect to the diver. Object reference is set on the straight line that connects the diver
    /// and target object at the given distance.
    /// </summary>
    public class DiverGuidanceController : MonoBehaviour
    {
        public GameObject Diver;
        public GameObject Target;
        public MissionControl MissionControl;
        public float Distance = 2f;
        public float AngSpeed = 150f;
        public float LinSpeed = 2f;

        Vector3 _refPoint;

        void Start()
        {
            MissionControl.OnWaypointChange += OnWaypointChange;
        }

        private void OnWaypointChange(MissionWaypoint obj)
        {
            Target = obj.gameObject;
        }

        void FixedUpdate()
        {
            _refPoint = CalculateRefPoint(Target.transform.position, Distance);
            OrientationController();
            PositionController();
        }


        public void SetTarget(Transform target)
        {
            Target = target.gameObject;
        }

        private void PositionController()
        {
            var direction = (_refPoint - transform.position).normalized;
            var newPosition = transform.position + direction * LinSpeed * Time.fixedDeltaTime;
            if (Mathf.Abs((_refPoint - transform.position).magnitude) > LinSpeed * Time.fixedDeltaTime)
            {
                transform.position = newPosition;
            }
        }

        private void OrientationController()
        {
            var direction = (Target.transform.position - transform.position);
            direction.y = 0f;
            direction = direction.normalized;
            var delta = Vector3.SignedAngle(direction, transform.forward, Vector3.up);
            // var delta = (transform.eulerAngles.y - angle) * Mathf.Deg2Rad;
            if (Mathf.Abs(delta) > Mathf.Abs(AngSpeed * Time.fixedDeltaTime))
            {
                transform.Rotate(Vector3.up, -Mathf.Sign(delta) * AngSpeed * Time.fixedDeltaTime, Space.World);
            }
        }

        Vector3 CalculateRefPoint(Vector3 targetPoint, float distance)
        {
            var currentPos = Diver.transform.position;
            var ray = (targetPoint - currentPos).normalized;

            return Diver.transform.position + ray * distance;
            
        }

    }
}