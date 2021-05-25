using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;


namespace Labust.Physics
{

    /// <summary>
    /// Script that adds bouyancy force to the object
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Buoyancy : MonoBehaviour
    {
        public float height = 1f;
        public float width = 1f;
        public float length = 1f;
        public float buoyancyScale = 1000f;
        public float fluidDragCoef = 0.01f;

        public Transform[] handles = new Transform[4];
        Rigidbody body;
        GameObject centerBody;

        [Header("Debug")]
        public float waterLevel = 0f;
        public float heightBelowWater;
        public Vector3 force;
        // Start is called before the first frame update
        void Start()
        {
            body = GetComponent<Rigidbody>();
            centerBody = new GameObject("CenterObject");
            CalculateCenterBody();

        }

        /*    void Update()
            {
                heightBelowWater = GetPartBelowWater(rb.position);
            }

            // Update is called once per frame
            void FixedUpdate()
            {
                // TODO: Calculate the volume of the submerged mesh and the center of the buoyancy
                var g = Physics.gravity.magnitude;
                body.AddForceAtPosition(buoyancyScale*body.mass*g*heightBelowWater*Vector3.up, rb.position);
            }
        */
        void FixedUpdate()
        {

            CalculateCenterBody();

            heightBelowWater = GetPartBelowWater(centerBody.transform.position);
            var g = UnityEngine.Physics.gravity.magnitude;
            var buoyancyforce = Vector3.up * (buoyancyScale * g * heightBelowWater * length * width);
            var dragForce = -0.5f * fluidDragCoef * buoyancyScale * length * width * body.velocity.magnitude * body.velocity;
            force = buoyancyforce + dragForce;
            body.AddForceAtPosition(force, centerBody.transform.position);
        }

        private void CalculateCenterBody()
        {
            // var y1 = GetPartBelowWater(handles[0].position);
            // var y4 = GetPartBelowWater(handles[3].position);
            // var dx = ((y1 - y4)/height + 1)/2;
            // var rbx = (dx * handles[0].localPosition.x + (1 - dx) * handles[3].localPosition.x)/2;

            // var y2 = GetPartBelowWater(handles[1].position);
            // var y3 = GetPartBelowWater(handles[2].position);
            // var dz = ((y2 - y3)/height + 1)/2;
            // var rbz = (dz * handles[1].localPosition.z + (1 - dz) * handles[2].localPosition.z)/2;

            var center = handles.Aggregate(Vector3.zero, (curr, next) => curr += next.position) / handles.Count();

            centerBody.transform.position = center;
        }

        float GetPartBelowWater(Vector3 position)
        {
            // waterLevel = Ocean.Instance.QueryWaves(position.x, position.z);
            waterLevel = 0;
            return height / 2 + Mathf.Clamp(waterLevel - body.position.y, -height / 2, height / 2);
        }

    }

}