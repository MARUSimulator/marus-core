// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Modified code from https://www.habrador.com/tutorials/unity-boat-tutorial/5-resistance-forces/

using UnityEngine;
using System.Collections.Generic;


//Main controller for all boat physics
namespace Marus
{
    public class BoatPhysics : MonoBehaviour 
    {
        //Drags
        public GameObject boat;
        public GameObject boatMesh;

        //Change the center of mass
        public Vector3 centerOfMass;

#if UNITY_MESH_SIMPLIFIER
        [Header("Simplify boat mesh")]
        public bool simplifyMesh;

        [SerializeField, Range(0f, 1f), Tooltip("The desired quality of the simplified mesh.")]
        private float meshQuality = 0.5f;

        private Mesh simplifiedMesh;
#endif
        //Script that's doing everything needed with the boat mesh, such as finding out which part is above the water
        private ModifyBoatMesh modifyBoatMesh;



        //The boats rigidbody
        private Rigidbody boatRB;

        //The density of the water the boat is traveling in
        private float rhoWater = BoatPhysicsMath.RHO_OCEAN_WATER;
        private float rhoAir = BoatPhysicsMath.RHO_AIR;

        void Awake() 
        {
            boatRB = this.GetComponent<Rigidbody>();
        }

        void Start() 
        {
            if(!boat)
            {
                boat = gameObject;
            }
            if(!boatMesh)
            {
                boatMesh = gameObject;
            }
            MeshFilter meshFilter;
            boatMesh.TryGetComponent<MeshFilter>(out meshFilter);

#if UNITY_MESH_SIMPLIFIER
            if(simplifyMesh)
            {
                var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
                meshSimplifier.Initialize(meshFilter.sharedMesh);
                meshSimplifier.SimplifyMesh(meshQuality);
                simplifiedMesh = meshSimplifier.ToMesh();

                modifyBoatMesh = new ModifyBoatMesh(boat, simplifiedMesh);
            }
            else
            {
                modifyBoatMesh = new ModifyBoatMesh(boat, meshFilter.sharedMesh);
            }
#else
            modifyBoatMesh = new ModifyBoatMesh(boat, meshFilter.sharedMesh);
#endif
        }

        void Update()
        {
            //Generate the under water and above water meshes
            modifyBoatMesh.GenerateUnderwaterMesh();
        }

        void FixedUpdate() 
        {
            //Change the center of mass - experimental - move to Start() later
            boatRB.centerOfMass = centerOfMass;

            //Add forces to the part of the boat that's below the water
            if (modifyBoatMesh.underWaterTriangleData.Count > 0)
            {
                AddUnderWaterForces();
            }

            //Add forces to the part of the boat that's above the water
            if (modifyBoatMesh.aboveWaterTriangleData.Count > 0)
            {
                AddAboveWaterForces();
            }
        }

        //Add all forces that act on the squares below the water
        void AddUnderWaterForces()
        {
            //The resistance coefficient - same for all triangles
            float Cf = BoatPhysicsMath.ResistanceCoefficient(
                rhoWater, 
                boatRB.velocity.magnitude,
                modifyBoatMesh.CalculateUnderWaterLength());

            //To calculate the slamming force we need the velocity at each of the original triangles
            List<SlammingForceData> slammingForceData = modifyBoatMesh.slammingForceData;

            CalculateSlammingVelocities(slammingForceData);

            //Need this data for slamming forces
            float boatArea = modifyBoatMesh.boatArea;
            float boatMass = boatRB.mass; //Replace this line with your boat's total mass

            //To connect the submerged triangles with the original triangles
            List<int> indexOfOriginalTriangle = modifyBoatMesh.indexOfOriginalTriangle;

            //Get all triangles
            List<TriangleData> underWaterTriangleData = modifyBoatMesh.underWaterTriangleData;
            float[] underwaterTriangleHeights = modifyBoatMesh.triangleHeightAboveWater;
        
            for (int i = 0; i < underWaterTriangleData.Count; i++)
            {
                TriangleData triangleData = underWaterTriangleData[i];
                float triangleHeightAboveWater = underwaterTriangleHeights[i];
                
                Vector3 triangleVelocity = boatRB.GetPointVelocity(triangleData.center);

                //Calculate the forces
                Vector3 forceToAdd = Vector3.zero;

                //Force 1 - The hydrostatic force (buoyancy)
                forceToAdd += BoatPhysicsMath.BuoyancyForce(rhoWater, triangleData, -triangleHeightAboveWater);

                //Force 2 - Viscous Water Resistance
                forceToAdd += BoatPhysicsMath.ViscousWaterResistanceForce(rhoWater, triangleData, triangleVelocity, Cf);

                //Force 3 - Pressure drag
                forceToAdd += BoatPhysicsMath.PressureDragForce(triangleData, triangleVelocity);

                //Force 4 - Slamming force
                //Which of the original triangles is this triangle a part of
                int originalTriangleIndex = indexOfOriginalTriangle[i];

                SlammingForceData slammingData = slammingForceData[originalTriangleIndex];

                forceToAdd += BoatPhysicsMath.SlammingForce(slammingData, triangleData, triangleVelocity, boatArea, boatMass);

                //Add the forces to the boat
                boatRB.AddForceAtPosition(forceToAdd, triangleData.center);

                //Debug

                //Normal
                //Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);

                //Buoyancy
                //Debug.DrawRay(triangleData.center, BoatPhysicsMath.BuoyancyForce(rhoWater, triangleData).normalized * -3f, Color.blue);

                //Velocity
                //Debug.DrawRay(triangleData.center, boatRB.GetPointVelocity(triangleData.center) * 3f, Color.white);

                //Viscous Water Resistance
                //Debug.DrawRay(triangleCenter, viscousWaterResistanceForce.normalized * 3f, Color.black);
            }
        }



        //Add all forces that act on the squares above the water
        void AddAboveWaterForces()
        {
            //Get all triangles
            List<TriangleData> aboveWaterTriangleData = modifyBoatMesh.aboveWaterTriangleData;

            //Loop through all triangles
            for (int i = 0; i < aboveWaterTriangleData.Count; i++)
            {
                TriangleData triangleData = aboveWaterTriangleData[i];

                Vector3 triangleVelocity = boatRB.GetPointVelocity(triangleData.center);

                //Calculate the forces
                Vector3 forceToAdd = Vector3.zero;

                //Force 1 - Air resistance 
                //Replace VisbyData.C_r with your boat's drag coefficient
                forceToAdd += BoatPhysicsMath.AirResistanceForce(rhoAir, triangleData, triangleVelocity, 0.1f);

                //Add the forces to the boat
                boatRB.AddForceAtPosition(forceToAdd, triangleData.center);

                //Debug

                //The normal
                //Debug.DrawRay(triangleData.center, triangleData.normal * 3f, Color.white);

                //The velocity
                //Debug.DrawRay(triangleData.center, boatRB.GetPointVelocity(triangleData.center) * 3f, Color.black);

                // if (triangleData.cosTheta > 0f)
                // {
                //     //Debug.DrawRay(triangleCenter, triangleVelocityDir * 3f, Color.black);
                // }

                //Air resistance
                //-3 to show it in the opposite direction to see what's going on
                //Debug.DrawRay(triangleData.center, forceToAdd, Color.red);
            }
        }



        //Calculate the current velocity at the center of each triangle of the original boat mesh
        private void CalculateSlammingVelocities(List<SlammingForceData> slammingForceData)
        {
            for (int i = 0; i < slammingForceData.Count; i++)
            {
                //Set the new velocity to the old velocity
                slammingForceData[i].previousVelocity = slammingForceData[i].velocity;

                //Center of the triangle in world space
                Vector3 center = transform.TransformPoint(slammingForceData[i].triangleCenter);

                //Get the current velocity at the center of the triangle
                slammingForceData[i].velocity = boatRB.GetPointVelocity(center);
            }
        }
    }
}