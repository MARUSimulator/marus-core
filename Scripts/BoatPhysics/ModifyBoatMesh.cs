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
#if CREST_OCEAN
using Crest;
#endif

namespace Marus
{
    //Generates the mesh that's below and above the water
    public class ModifyBoatMesh 
    {
        //The boat transform needed to get the global position of a vertice
        private Transform _boatTrans;
        //Coordinates of all vertices in the original boat
        Vector3[] boatVertices;
        //Positions in allVerticesArray, such as 0, 3, 5, to build triangles
        int[] boatTriangles;
        //The boats rigidbody
        private Rigidbody _boatRB;

        //So we only need to make the transformation from local to global once
        public Vector3[] boatVerticesGlobal;
        //Find all the distances to water once because some triangles share vertices, so reuse
        float[] allHeightAboveWater;
        // find all tirangle heights 
        public float[] triangleHeightAboveWater;

        //The part of the boat that's under water - needed for calculations of length / volume
        private Mesh _underwaterMesh;

        private float _boatWidth;

        public List<TriangleData> underWaterTriangleData = new List<TriangleData>();

        //The part of the boat that's above water
        public List<TriangleData> aboveWaterTriangleData = new List<TriangleData>();

        //Slamming resistance forces
        //Data that belongs to one triangle in the original boat mesh
        public List<SlammingForceData> slammingForceData = new List<SlammingForceData>();
        //To connect the submerged triangles with the original triangles
        public List<int> indexOfOriginalTriangle = new List<int>();
        //The total area of the entire boat
        public float boatArea;

        public ModifyBoatMesh(GameObject boatObj, Mesh boatMesh) 
        {
            
            _boatWidth = boatMesh.bounds.size.x;

            //Get the transform
            _boatTrans = boatObj.transform;

            //Get the rigid body
            this._boatRB = boatObj.GetComponent<Rigidbody>();

            //Save the mesh
            _underwaterMesh = new Mesh();

            //Init the arrays and lists
            boatVertices = boatMesh.vertices;
            boatTriangles = boatMesh.triangles;

            //The boat vertices in global position
            boatVerticesGlobal = new Vector3[boatVertices.Length];
            //Find all the distances to water once because some triangles share vertices, so reuse
            allHeightAboveWater = new float[boatVertices.Length];
            // reserve space for all triangle depths
            triangleHeightAboveWater = new float[boatTriangles.Length];

            //Setup the slamming force data
            for (int i = 0; i < (boatTriangles.Length / 3); i++)
            {
                slammingForceData.Add(new SlammingForceData());
            }

            //Calculate the area of the original triangles and the total area of the entire boat
            CalculateOriginalTrianglesArea();
        }

        //Generate the underwater mesh (and the abovewater mesh)
        public void GenerateUnderwaterMesh()
        {
            //Reset
            aboveWaterTriangleData.Clear();
            underWaterTriangleData.Clear();
            _underwaterMesh.Clear();

            //Switch the submerged triangle area with the one in the previous time step
            for (int j = 0; j < slammingForceData.Count; j++)
            {
                slammingForceData[j].previousSubmergedArea = slammingForceData[j].submergedArea;
            }

            indexOfOriginalTriangle.Clear();

            //Find all the distances to water once because some triangles share vertices, so reuse
            for (int j = 0; j < boatVertices.Length; j++)
            {
                //The coordinate should be in global position
                boatVerticesGlobal[j] = _boatTrans.TransformPoint(boatVertices[j]);
            }
            // get water level at positions
            GetWaterLevel(boatVerticesGlobal, allHeightAboveWater, boatVertices.Length);

            for (int j = 0; j < boatVertices.Length; j++)
            {
                //The calculate height above water
                allHeightAboveWater[j] = boatVerticesGlobal[j].y - allHeightAboveWater[j];
            }

            //Add the triangles
            AddTriangles();

            // populate underwater triangle depths
            UpdateUnderwaterTriangleDepth();
            // populate mesh so bounds can be determined
            PopulateMesh(_underwaterMesh, "UnderWater Mesh", underWaterTriangleData);
        }

        private void GetWaterLevel(Vector3[] points, float[] heights, int array_size)
        {
            //float[] heights = new float[points.Count];
#if CREST_OCEAN
            var collProvider = OceanRenderer.Instance?.CollisionProvider;
            if (collProvider == null)
            {
                for (int i = 0; i < array_size; i++)
                {
                    heights[i] = 0.0f;
                }
            }

            var status = collProvider.Query(GetHashCode(), _boatWidth, points, heights, null, null);

            if (!collProvider.RetrieveSucceeded(status))
            {
                for (int i = 0; i < array_size; i++)
                {
                    heights[i] = OceanRenderer.Instance.SeaLevel;
                }
            }
#else
            for (int i = 0; i < array_size; i++)
            {
                heights[i] = 0.0f;
            }
#endif
        }

        private void UpdateUnderwaterTriangleDepth()
        {
            Vector3[] globalTrianglePos = new Vector3[underWaterTriangleData.Count];

            for (int j = 0; j < underWaterTriangleData.Count; j++)
            {
                globalTrianglePos[j] = underWaterTriangleData[j].center;
            }
            // get water level
            GetWaterLevel(globalTrianglePos, triangleHeightAboveWater, underWaterTriangleData.Count);

            for (int j = 0; j < underWaterTriangleData.Count; j++)
            {
                //The coordinate should be in global position
                triangleHeightAboveWater[j] = globalTrianglePos[j].y - triangleHeightAboveWater[j];
            }
        }

        //Add all the triangles that's part of the underwater and abovewater meshes
        private void AddTriangles()
        {
            //List that will store the data we need to sort the vertices based on distance to water
            List<VertexData> vertexData = new List<VertexData>();

            //Add fake data that will be replaced
            vertexData.Add(new VertexData());
            vertexData.Add(new VertexData());
            vertexData.Add(new VertexData());


            //Loop through all the triangles (3 vertices at a time = 1 triangle)
            int i = 0;
            int triangleCounter = 0;
            while (i < boatTriangles.Length)
            {
                //Loop through the 3 vertices
                for (int x = 0; x < 3; x++)
                {
                    //Save the data we need
                    vertexData[x].distance = allHeightAboveWater[boatTriangles[i]];

                    vertexData[x].index = x;

                    vertexData[x].globalVertexPos = boatVerticesGlobal[boatTriangles[i]];

                    i++;
                }

                //All vertices are above the water
                if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance > 0f)
                {
                    Vector3 p1 = vertexData[0].globalVertexPos;
                    Vector3 p2 = vertexData[1].globalVertexPos;
                    Vector3 p3 = vertexData[2].globalVertexPos;

                    //Save the triangle
                    aboveWaterTriangleData.Add(new TriangleData(p1, p2, p3));

                    slammingForceData[triangleCounter].submergedArea = 0f;

                    continue;
                }
                //Create the triangles that are below the waterline

                //All vertices are underwater
                if (vertexData[0].distance < 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                {
                    Vector3 p1 = vertexData[0].globalVertexPos;
                    Vector3 p2 = vertexData[1].globalVertexPos;
                    Vector3 p3 = vertexData[2].globalVertexPos;

                    //Save the triangle
                    underWaterTriangleData.Add(new TriangleData(p1, p2, p3));

                    //We have already calculated the area of this triangle
                    slammingForceData[triangleCounter].submergedArea = slammingForceData[triangleCounter].originalArea;

                    indexOfOriginalTriangle.Add(triangleCounter);
                }
                //1 or 2 vertices are below the water
                else
                {
                    //Sort the vertices
                    vertexData.Sort((x, y) => x.distance.CompareTo(y.distance));

                    vertexData.Reverse();

                    //One vertice is above the water, the rest is below
                    if (vertexData[0].distance > 0f && vertexData[1].distance < 0f && vertexData[2].distance < 0f)
                    {
                        AddTrianglesOneAboveWater(vertexData, triangleCounter);
                    }
                    //Two vertices are above the water, the other is below
                    else if (vertexData[0].distance > 0f && vertexData[1].distance > 0f && vertexData[2].distance < 0f)
                    {
                        AddTrianglesTwoAboveWater(vertexData, triangleCounter);
                    }
                }

                triangleCounter += 1;
            }
        }



        //Build the new triangles where one of the old vertices is above the water
        private void AddTrianglesOneAboveWater(List<VertexData> vertexData, int triangleCounter)
        {
            //H is always at position 0
            Vector3 H = vertexData[0].globalVertexPos;

            //Left of H is M
            //Right of H is L

            //Find the index of M
            int M_index = vertexData[0].index - 1;
            if (M_index < 0)
            {
                M_index = 2;
            }

            //We also need the heights to water
            float h_H = vertexData[0].distance;
            float h_M = 0f;
            float h_L = 0f;

            Vector3 M = Vector3.zero;
            Vector3 L = Vector3.zero;

            //This means M is at position 1 in the List
            if (vertexData[1].index == M_index)
            {
                M = vertexData[1].globalVertexPos;
                L = vertexData[2].globalVertexPos;

                h_M = vertexData[1].distance;
                h_L = vertexData[2].distance;
            }
            else
            {
                M = vertexData[2].globalVertexPos;
                L = vertexData[1].globalVertexPos;

                h_M = vertexData[2].distance;
                h_L = vertexData[1].distance;
            }


            //Now we can calculate where we should cut the triangle to form 2 new triangles
            //because the resulting area will always form a square

            //Point I_M
            Vector3 MH = H - M;

            float t_M = -h_M / (h_H - h_M);

            Vector3 MI_M = t_M * MH;

            Vector3 I_M = MI_M + M;


            //Point I_L
            Vector3 LH = H - L;

            float t_L = -h_L / (h_H - h_L);

            Vector3 LI_L = t_L * LH;

            Vector3 I_L = LI_L + L;


            //Save the data, such as normal, area, etc      
            //2 triangles below the water  
            underWaterTriangleData.Add(new TriangleData(M, I_M, I_L));
            underWaterTriangleData.Add(new TriangleData(M, I_L, L));
            //1 triangle above the water
            aboveWaterTriangleData.Add(new TriangleData(I_M, H, I_L));

            //Calculate the total submerged area
            float totalArea = TriangleData.GetTriangleArea(M, I_M, I_L) + TriangleData.GetTriangleArea(M, I_L, L);

            slammingForceData[triangleCounter].submergedArea = totalArea;

            indexOfOriginalTriangle.Add(triangleCounter);
            //Add 2 times because 2 submerged triangles need to connect to the same original triangle
            indexOfOriginalTriangle.Add(triangleCounter);
        }



        //Build the new triangles where two of the old vertices are above the water
        private void AddTrianglesTwoAboveWater(List<VertexData> vertexData, int triangleCounter)
        {
            //H and M are above the water
            //H is after the vertice that's below water, which is L
            //So we know which one is L because it is last in the sorted list
            Vector3 L = vertexData[2].globalVertexPos;

            //Find the index of H
            int H_index = vertexData[2].index + 1;
            if (H_index > 2)
            {
                H_index = 0;
            }


            //We also need the heights to water
            float h_L = vertexData[2].distance;
            float h_H = 0f;
            float h_M = 0f;

            Vector3 H = Vector3.zero;
            Vector3 M = Vector3.zero;

            //This means that H is at position 1 in the list
            if (vertexData[1].index == H_index)
            {
                H = vertexData[1].globalVertexPos;
                M = vertexData[0].globalVertexPos;

                h_H = vertexData[1].distance;
                h_M = vertexData[0].distance;
            }
            else
            {
                H = vertexData[0].globalVertexPos;
                M = vertexData[1].globalVertexPos;

                h_H = vertexData[0].distance;
                h_M = vertexData[1].distance;
            }


            //Now we can find where to cut the triangle

            //Point J_M
            Vector3 LM = M - L;

            float t_M = -h_L / (h_M - h_L);

            Vector3 LJ_M = t_M * LM;

            Vector3 J_M = LJ_M + L;


            //Point J_H
            Vector3 LH = H - L;

            float t_H = -h_L / (h_H - h_L);

            Vector3 LJ_H = t_H * LH;

            Vector3 J_H = LJ_H + L;


            //Save the data, such as normal, area, etc
            //1 triangle above the water
            underWaterTriangleData.Add(new TriangleData(L, J_H, J_M));
            //2 triangles below the water
            aboveWaterTriangleData.Add(new TriangleData(J_H, H, J_M));
            aboveWaterTriangleData.Add(new TriangleData(J_M, H, M));

            //Calculate the submerged area
            slammingForceData[triangleCounter].submergedArea = TriangleData.GetTriangleArea(L, J_H, J_M);

            indexOfOriginalTriangle.Add(triangleCounter);
        }



        //Help class to store triangle data so we can sort the distances
        private class VertexData
        {
            //The distance to water
            public float distance;
            //We also need to store a index so we can form clockwise triangles
            public int index;
            //The global Vector3 position of the vertex
            public Vector3 globalVertexPos;
        }



        //Display the underwater or abovewater mesh
        public void PopulateMesh(Mesh mesh, string name, List<TriangleData> triangesData)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            
            //Build the mesh
            for (int i = 0; i < triangesData.Count; i++)
            {
                //From global coordinates to local coordinates
                Vector3 p1 = _boatTrans.InverseTransformPoint(triangesData[i].p1);
                Vector3 p2 = _boatTrans.InverseTransformPoint(triangesData[i].p2);
                Vector3 p3 = _boatTrans.InverseTransformPoint(triangesData[i].p3);

                vertices.Add(p1);
                triangles.Add(vertices.Count - 1);

                vertices.Add(p2);
                triangles.Add(vertices.Count - 1);

                vertices.Add(p3);
                triangles.Add(vertices.Count - 1);
            }

            //Remove the old mesh
            mesh.Clear();

            //Give it a name
            mesh.name = name;

            //Add the new vertices and triangles
            mesh.vertices = vertices.ToArray();

            mesh.triangles = triangles.ToArray();

            //Important to recalculate bounds because we need the bounds to calculate the length of the underwater mesh
            mesh.RecalculateBounds();
        }

        //Calculate the length of the mesh that's below the water
        public float CalculateUnderWaterLength()
        {
            //Approximate the length as the length of the underwater mesh
            float underWaterLength = _underwaterMesh.bounds.size.z;

            //Debug.Log(underwaterMesh.bounds.size.z);

            return underWaterLength;
        }

        //Calculate the area of each triangle in the boat mesh and store them in an array
        private void CalculateOriginalTrianglesArea()
        {
            //Loop through all the triangles (3 vertices at a time = 1 triangle)
            int i = 0;
            int triangleCounter = 0;
            while (i < boatTriangles.Length)
            {
                Vector3 p1 = boatVertices[boatTriangles[i]];

                i++;

                Vector3 p2 = boatVertices[boatTriangles[i]];

                i++;

                Vector3 p3 = boatVertices[boatTriangles[i]];

                i++;

                //Calculate the area of the triangle
                float triangleArea = TriangleData.GetTriangleArea(p1, p2, p3);
        
                //Store the area in a list
                slammingForceData[triangleCounter].originalArea = triangleArea;

                //The total area
                boatArea += triangleArea;

                triangleCounter += 1;
            }
        }
    }
}