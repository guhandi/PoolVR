//======================================================================================================
// Copyright 2016, NaturalPoint Inc.
//======================================================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

//A class that streams data from an Optitrack Motiv rigidbody object to control the cue stick in the Unity environment
public class OptitrackRigidBody : MonoBehaviour
{
    public OptitrackStreamingClient StreamingClient;
    public Int32 RigidBodyId; //Id of rigidbody object being streamed from Motiv

    public Transform cueTip; //reference to the tip of the cue stick - used to apply force to the cue ball
    private Vector3 frontPos; //position of marker closest to front hand
    private Vector3 backPos; //position of marker closest to back hand
    private Vector3 cuePos; //cue position at current timestep in Unity
    private Vector3 prevPos; //cue position at previous timestep
    private Vector3 Opticuepos; //cue position in Optitrack environment
    private Vector3 cueVelocity; //velocity of cue stick as it is being moved
    private Vector3 avgVelocity;
    private List<Vector3> VelocityList; //list of last 5 cue velocities
    private List<Vector3> C1;
    private List<Vector3> C2;
    private List<Vector3> C3;
    private float[,] M; //Transformation matrix to convert between Optritrack and Unity coordinate systems
    private float yratio; //scaling ratio between Optitrack and Unity environment heights (y axis)

    private int numCalSamples;
    private int corner1ID;
    private int corner2ID;
    private int corner3ID;

    private bool test;

    void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if (this.StreamingClient == null)
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if (this.StreamingClient == null)
            {
                Debug.LogError(GetType().FullName + ": Streaming client not set, and no " + typeof(OptitrackStreamingClient).FullName + " components found in scene; disabling this component.", this);
                this.enabled = false;
                return;
            }
        }

        prevPos = CollectData.cuestart;
        yratio = CollectData.yratio; //yratio = table height in Optitrack / table height in Unity

        cueVelocity = new Vector3(0f, 0f, 0f);
        VelocityList = new List<Vector3>();
        for (int i = 0; i < CollectData.numVelocitiesAverage; i++)
        {
            VelocityList.Add(cueVelocity);
        }

        C1 = new List<Vector3>();
        C2 = new List<Vector3>();
        C3 = new List<Vector3>();
        corner1ID = -1;
        corner2ID = -1;
        corner3ID = -1;


        // Calibrate positions of three pockets of table in Unity to the same pockets in Optitrack
        M = CollectData.transformCoordinateMatrix; //Transformation matrix to convert future points

        test = true;
    }


#if UNITY_2017_1_OR_NEWER
    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }


    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender()
    {
        UpdatePose();
    }
#endif


    void FixedUpdate()
    {
        if (CollectData.experiment == 0)
        {
            Calibration();
        }
        else
        {
            UpdatePose();
        }
    }

    //Update the position of the cue rigidbody every frame
    void UpdatePose()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId); //access rigidbody
        List<OptitrackMarkerState> markerStates = new List<OptitrackMarkerState>(); //initialize list of marker objects from rigidbody
        List<Vector3> markerPositions = new List<Vector3>(); //initialize list of marker positions

        if (rbState != null)
        {
            //Get marker positions
            markerStates = StreamingClient.GetLatestMarkerStates(); //get objects of all markers from rigidbody
            float maxx = -1000000;
            float minx = 1000000;

            //go through all markers and set front/back positions to relevant marker positions
            for (int idx = 0; idx < markerStates.Count; idx++)
            {

                OptitrackMarkerState marker = markerStates[idx];
                markerPositions.Add(marker.Position);
                if (marker.Position.x > maxx)
                {
                    backPos = marker.Position; //back position has highest x value
                    maxx = backPos.x;
                }
                if (marker.Position.x < minx)
                {
                    frontPos = marker.Position; //front position has lowest x position
                    minx = frontPos.x;
                }
            }
            Opticuepos = CollectData.backWeight * backPos + CollectData.frontWeight * frontPos; //Set cue position (in Motiv env) to be weighted towards back marker

            //Transform cue position in Motiv to Unity environment by multiplying coordinates with transformation matrix
            float[,] OptiposM = new float[,] { { Opticuepos.x }, { Opticuepos.z }, { 1 } };
            float[,] transformed = CollectData.MultiplyMatrix(M, OptiposM);
            cuePos = new Vector3(transformed[0, 0], Opticuepos.y * yratio, transformed[1, 0]); //Unity cue position


            cueVelocity = (cuePos - prevPos) / Time.fixedDeltaTime;
            VelocityList.Add(cueVelocity);
            avgVelocity = AverageVelocity(VelocityList);
            //Debug.Log(avgVelocity);
            prevPos = cuePos;


            //move cue rigidbody to updated position
            this.transform.localPosition = cuePos;
            //get forward direction by lookng at forwrd position
            this.transform.localRotation = Quaternion.LookRotation(frontPos - backPos) * Quaternion.Euler(90f, 0f, 0f);

            //this.transform.localPosition = rbState.Pose.Position;
            //this.transform.localRotation = rbState.Pose.Orientation;

        }
    }

    void Calibration()
    {
        numCalSamples++;
        

        //Get marker positions
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId); //access rigidbody
        List<OptitrackMarkerState> markerStates = new List<OptitrackMarkerState>(); //initialize list of marker objects from rigidbody
        markerStates = StreamingClient.GetLatestMarkerStates(); //get objects of all markers from rigidbody
        if (numCalSamples < 2)
        {
            getMarkerId(markerStates);

        }

        for (int idx = 0; idx < markerStates.Count; idx++)
        {
            
            OptitrackMarkerState marker = markerStates[idx];
            int mID = marker.Id;
            if (mID == corner1ID)
            {
                C1.Add(marker.Position);
            }
            if (mID == corner2ID)
            {
                C2.Add(marker.Position);
            }
            if (mID == corner3ID)
            {
                C3.Add(marker.Position);
            }

        }

        if (numCalSamples > 1000)
        {
            Vector3 avgC1 = AverageVec(C1);
            Vector3 avgC2 = AverageVec(C2);
            Vector3 avgC3 = AverageVec(C3);

            CollectData.optiPocketPoints = new float[,] { { avgC1.x, avgC2.x, avgC3.x}, { avgC1.z, avgC2.z, avgC3.z }, { 1f, 1f, 1f } };

            if (test)
            {
                Debug.Log(corner1ID);
                Debug.Log(avgC1);
                Debug.Log(corner2ID);
                Debug.Log(avgC2);
                Debug.Log(corner3ID);
                Debug.Log(avgC3);
                test = false;

            }
        }
    }

    //Method that is called at any gameobject collision with the cue stick
    void OnCollisionEnter(Collision col)
    {

        //If cue ball doesn't collide with any rigidbody do nothing
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (!rb)
        {
            return;
        }

        //If cue stick collides with the cue ball
        if (col.gameObject.name == "CueBall")
        {

            //Get average of contact points
            int numContacts = col.contacts.Length;
            float x = 0f;
            float y = 0f;
            float z = 0f;
            foreach (ContactPoint contact in col.contacts)
            {
                x += contact.point.x;
                y += contact.point.y;
                z += contact.point.z;
            }
            Vector3 contactPos = new Vector3(x / numContacts, y / numContacts, z / numContacts);
            Vector3 forceDirection = (contactPos - transform.position).normalized;

            //if (CollectData.experiment == 3)
            //{
            //   forceDirection = forceDirection + CollectData.adaptationForce;
            //}
            float scaleForce = CollectData.scaleForce;
            Vector3 cueForce = forceDirection * Math.Abs(avgVelocity.magnitude) * scaleForce;
            rb.AddForce(cueForce);

        }

    }

    private void getMarkerId(List<OptitrackMarkerState> markers)
    {
        List<int> idList = new List<int>();
        float maxz = -1000f;
        int idz = -1;
        float minx = 1000f;
        int idx = -1;
        for (int j = 0; j < markers.Count; j++)
        {

            OptitrackMarkerState m = markers[j];
            idList.Add(m.Id);
            if (m.Position.z > maxz)
            {
                maxz = m.Position.z;
                idz = m.Id;
            }
            if (m.Position.x < minx)
            {
                minx = m.Position.x;
                idx = m.Id;
            }


        }
        idList.Remove(idz);
        idList.Remove(idx);
        corner1ID = idList[0];
        corner2ID = idx;
        corner3ID = idz;
    }

    private Vector3 AverageVelocity(List<Vector3> vlist)
    {
        Vector3 sumvel = new Vector3(0f, 0f, 0f);
        for (int idx = vlist.Count - 1; idx >= vlist.Count - CollectData.numVelocitiesAverage; idx--)
        {
            sumvel = sumvel + vlist[idx];
        }
        return sumvel / CollectData.numVelocitiesAverage;

    }

    private Vector3 AverageVec(List<Vector3> vlist)
    {
        Vector3 sumvec = new Vector3(0f, 0f, 0f);
        for (int idx = 0; idx < vlist.Count; idx++)
        {
            sumvec = sumvec + vlist[idx];
        }
        return (sumvec / vlist.Count);

    }

}
