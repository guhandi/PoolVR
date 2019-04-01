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
    public static Dictionary<int, Vector3> markerList;
    public int count;

    //public Transform front; //reference to the tip of the cue stick - used for cue stick positioning
    public Transform cueTip; //reference to the tip of the cue stick - used to apply force to the cue ball
    private Vector3 frontPos; //position of marker closest to front hand
    private Vector3 backPos; //position of marker closest to back hand
    private Vector3 avgPos;
    private Vector3 cuePos; //cue position at current timestep in Unity
    private Vector3 prevPos; //cue position at previous timestep
    private Vector3 Opticuepos; //cue position in Optitrack environment
    private Vector3 cueVelocity; //velocity of cue stick as it is being moved
    private Vector3 avgVelocity; //average velocity of the cue stick (for smoothing)
    private List<Vector3> VelocityList; //list of last 5 cue velocities
    private float[,] transformFront;
    private float[,] transformBack;

    private int backID;
    private int frontID;

    private int corner1ID; //marker ID at corner 1 (for calibration)
    private int corner2ID; //marker ID at corner 2 (for calibration)
    private int corner3ID; //marker ID at corner 3 (for calibration)
    private List<Vector3> C1; //list of corner 1 points (for calibration)
    private List<Vector3> C2; //list of corner 2 points (for calibration)
    private List<Vector3> C3; //list of corner 3 points (for calibration
    private int numCalSamples; //number of positions samples to collect for calibration

    private bool markerIDs = true;
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

        count = 0;
        prevPos = Experiment.cuestart; //initialize previous positons to cue starting position
        cueVelocity = new Vector3(0f, 0f, 0f);
        VelocityList = new List<Vector3>();
        for (int i = 0; i < Experiment.numVelocitiesAverage; i++)
        {
            VelocityList.Add(cueVelocity);
        }

        corner1ID = -1;
        corner2ID = -1;
        corner3ID = -1;
        C1 = new List<Vector3>();
        C2 = new List<Vector3>();
        C3 = new List<Vector3>();

        if (markerIDs)
        {
            getMarkerId();
        }

        test = true; //for calibration
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
        //UpdatePose();
    }
#endif


    void Update()
    {
        count++;
        if (Experiment.experiment == 0 && Experiment.isEnvSet)
        {
            calibrateTable();
        }
        if (Experiment.experiment != 0)
        {
            UpdatePose();
        }

    }

    //Update the position of the cue rigidbody every frame
    void UpdatePose()
    {
        float optiY = 0f;
        int nummark = 0;
        Vector3 OfrontPos = new Vector3();
        Vector3 ObackPos = new Vector3();
        Vector3 OavgPosition = new Vector3();
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId); //access rigidbody
        List<OptitrackMarkerState> markerStates = new List<OptitrackMarkerState>(); //initialize list of marker objects from rigidbody
        List<Vector3> markerPositions = new List<Vector3>(); //initialize list of marker positions

        if (rbState != null)
        {
            //Get marker positions
            markerStates = StreamingClient.GetLatestMarkerStates(); //get objects of all markers from rigidbody
            nummark = markerStates.Count;
            //go through all markers and set front/back positions to relevant marker positions
            for (int idx = 0; idx < markerStates.Count; idx++)
            {
                OptitrackMarkerState marker = markerStates[idx];
                markerPositions.Add(marker.Position);

                if (marker.Id == backID)
                {
                    ObackPos = marker.Position;
                    
                }
                if (marker.Id == frontID)
                {
                    OfrontPos = marker.Position;
                    optiY = frontPos.y;
                }
            }
            OavgPosition = Experiment.AverageVec(markerPositions); //average position of all markers
            markerPositions.Clear();

            float[,] frontMatrix = new float[,] { { OfrontPos.x }, { OfrontPos.z }, { 1 } };
            float[,] backMatrix = new float[,] { { ObackPos.x }, { ObackPos.z }, { 1 } };
            float[,] avgMatrix = new float[,] { { OavgPosition.x }, { OavgPosition.z }, { 1 } };
            float[,] transformFront = Experiment.MultiplyMatrix(Experiment.M, frontMatrix);
            float[,] transformBack = Experiment.MultiplyMatrix(Experiment.M, backMatrix);
            float[,] avgU = Experiment.MultiplyMatrix(Experiment.M, avgMatrix);
            backPos = new Vector3(transformBack[0, 0], ObackPos.y * Experiment.yratio, transformBack[1, 0]);
            frontPos = new Vector3(transformFront[0, 0], OfrontPos.y * Experiment.yratio, transformFront[1, 0]);

            avgPos = new Vector3(avgU[0, 0], OavgPosition.y * Experiment.yratio, avgU[1, 0]);
            avgPos = new Vector3((float)System.Math.Round(avgPos.x, 1), (float)System.Math.Round(avgPos.y, 1), (float)System.Math.Round(avgPos.z, 1));
            Vector3 direction = frontPos - backPos;

            //cuePos = OfrontPos;
            //cuePos = ObackPos;
            //cuePos = backPos - Experiment.cuePositionWeight * direction;
            if (ObackPos == new Vector3())
            {
                //cuePos = prevPos;
                Debug.Log("zeeeeeerrroo");
            }

            cuePos = markerToPosition(OfrontPos, ObackPos, OavgPosition);
            
            
            //calculate velocity
            cueVelocity = (cuePos - prevPos) / Time.fixedDeltaTime;
            VelocityList.Add(cueVelocity);
            avgVelocity = AverageVelocity(VelocityList, Experiment.numVelocitiesAverage);
            prevPos = cuePos;

            //move cue rigidbody to updated position
            this.transform.position = cuePos;
            //Experiment.front.position = cuePos;
            //get forward direction by lookng at forwrd position
            this.transform.rotation = Quaternion.LookRotation(OfrontPos - ObackPos) * Quaternion.Euler(90f, 0f, 0f);
            //Experiment.front.rotation = Quaternion.LookRotation(frontPos - backPos) * Quaternion.Euler(90f, 0f, 0f);
            //Experiment.cueRB.transform.rotation = Quaternion.LookRotation(cuePos-backPos) * Quaternion.Euler(90f, 0f, 0f);

            Debug.Log("back   : " + ObackPos.ToString("f4"));
            //Debug.Log("front   : " + OfrontPos.ToString("f4"));
            //Debug.Log("cuepos   : " + cuePos.ToString("f4"));
            //Debug.Log(Experiment.corner3.position.ToString("f4"));
            //Debug.Log("offset  : " + (cuePos - Experiment.corner3.position).ToString("f4"));

        }
    }

    public static Vector3 markerToPosition(Vector3 front, Vector3 back, Vector3 avg)
    {
        float[,] frontMatrix = new float[,] { { front.x }, { front.z }, { 1 } };
        float[,] backMatrix = new float[,] { { back.x }, { back.z }, { 1 } };
        float[,] avgMatrix = new float[,] { { avg.x }, { avg.z }, { 1 } };
        float[,] frontU = Experiment.MultiplyMatrix(Experiment.M, frontMatrix);
        float[,] backU = Experiment.MultiplyMatrix(Experiment.M, backMatrix);
        float[,] avgU = Experiment.MultiplyMatrix(Experiment.M, avgMatrix);
        Vector3 backPos = new Vector3(backU[0, 0], back.y * Experiment.yratio, backU[1, 0]);
        Vector3 frontPos = new Vector3(frontU[0, 0], front.y * Experiment.yratio, frontU[1, 0]);

        Vector3 avgPos = new Vector3(avgU[0, 0], avg.y * Experiment.yratio, avgU[1, 0]);
        avgPos = new Vector3((float)System.Math.Round(avgPos.x, 2), (float)System.Math.Round(avgPos.y, 2), (float)System.Math.Round(avgPos.z, 2));

        //For average position
        //Vector3 direction = (frontPos - backPos).normalized;
        //float alpha = PlayerPrefs.GetFloat("tip_marker1") * PlayerPrefs.GetFloat("cmToUnity") + (frontPos - avgPos).magnitude; //from average position
        //Vector3 cuePosition = avgPos + alpha * direction;

        //For back Marker
        Vector3 direction = (front - back).normalized;
        Vector3 OcuePos = back - (direction * PlayerPrefs.GetFloat("marker3_base") * PlayerPrefs.GetFloat("cmToUnity"));
        float[,] OposMatrix = new float[,] { { OcuePos.x }, { OcuePos.z }, { 1 } };
        float[,] posMatrix = Experiment.MultiplyMatrix(Experiment.M, OposMatrix);
        Vector3 cuePosition = new Vector3(posMatrix[0, 0], OcuePos.y * Experiment.yratio, posMatrix[1, 0]);

        //For front Marker
        //Vector3 direction = (front - back).normalized;
        //Vector3 OcuePos = front - (direction * (PlayerPrefs.GetFloat("marker3_base") + PlayerPrefs.GetFloat("marker2_marker3") + PlayerPrefs.GetFloat("marker1_marker2")) * PlayerPrefs.GetFloat("cmToUnity"));
        //Vector3 OcuePos = front - (direction * 123f * PlayerPrefs.GetFloat("cmToUnity"));
        //float[,] OposMatrix = new float[,] { { OcuePos.x }, { OcuePos.z }, { 1 } };
        //float[,] posMatrix = Experiment.MultiplyMatrix(Experiment.M, OposMatrix);
        //Vector3 cuePosition = new Vector3(posMatrix[0, 0], OcuePos.y * Experiment.yratio, posMatrix[1, 0]);

        return cuePosition;
    }

    void calibrateTable()
    {
        numCalSamples++;

        //Get marker positions
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId); //access rigidbody
        List<OptitrackMarkerState> markerStates = new List<OptitrackMarkerState>(); //initialize list of marker objects from rigidbody
        markerStates = StreamingClient.GetLatestMarkerStates(); //get objects of all markers from rigidbody

        //loop through all markers
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

        //stop calibratiing after 100 samples and print results
        if (numCalSamples > 100)
        {

            Vector3 avgC1 = Experiment.AverageVec(C1);
            Vector3 avgC2 = Experiment.AverageVec(C2);
            Vector3 avgC3 = Experiment.AverageVec(C3);

            //derive C2
            float length = (avgC3 - avgC1).magnitude;
            Vector3 direction = Quaternion.Euler(0, -90f, 0) * (avgC3 - avgC1).normalized;
            Vector3 derivedC2 = avgC1 + length * direction;

            //Unity Corner Positions
            PlayerPrefs.SetFloat("Ucorner1x", Experiment.corner1.position.x);
            PlayerPrefs.SetFloat("Ucorner1y", Experiment.corner1.position.y);
            PlayerPrefs.SetFloat("Ucorner1z", Experiment.corner1.position.z);
            PlayerPrefs.SetFloat("Ucorner2x", Experiment.corner2.position.x);
            PlayerPrefs.SetFloat("Ucorner2y", Experiment.corner2.position.y);
            PlayerPrefs.SetFloat("Ucorner2z", Experiment.corner2.position.z);
            PlayerPrefs.SetFloat("Ucorner3x", Experiment.corner3.position.x);
            PlayerPrefs.SetFloat("Ucorner3y", Experiment.corner3.position.y);
            PlayerPrefs.SetFloat("Ucorner3z", Experiment.corner3.position.z);

            //Optitrack Corner Positions
            PlayerPrefs.SetFloat("Ocorner1x", avgC1.x);
            PlayerPrefs.SetFloat("Ocorner1y", avgC1.y);
            PlayerPrefs.SetFloat("Ocorner1z", avgC1.z);
            PlayerPrefs.SetFloat("Ocorner2x", derivedC2.x);
            PlayerPrefs.SetFloat("Ocorner2y", derivedC2.y);
            PlayerPrefs.SetFloat("Ocorner2z", derivedC2.z);
            PlayerPrefs.SetFloat("Ocorner3x", avgC3.x);
            PlayerPrefs.SetFloat("Ocorner3y", avgC3.y);
            PlayerPrefs.SetFloat("Ocorner3z", avgC3.z);

            //Cue ball starting position
            PlayerPrefs.SetFloat("Ocuex", avgC2.x);
            PlayerPrefs.SetFloat("Ocuey", avgC2.y);
            PlayerPrefs.SetFloat("Ocuez", avgC2.z);
            //Experiment.optiPocketPoints = new float[,] { { avgC1.x, avgC2.x, avgC3.x }, { avgC1.z, avgC2.z, avgC3.z }, { 1f, 1f, 1f } };

            if (test)
            {
                Debug.Log("corner1ID = " + corner1ID);
                Debug.Log("corner1 position = " + avgC1.ToString("f4"));
                Debug.Log("corner2ID = " + corner2ID);
                Debug.Log("corner2 position = " + avgC2.ToString("f4"));
                Debug.Log("derived C2    " + derivedC2.ToString("f4"));
                Debug.Log("corner3ID = " + corner3ID);
                Debug.Log("corner3 position = " + avgC3.ToString("f4"));
                Debug.Log("Unity positions");
                Debug.Log("corner1" + Experiment.corner1.position.ToString("f4"));
                Debug.Log("corner2" + Experiment.corner2.position.ToString("f4"));
                Debug.Log("corner3" + Experiment.corner3.position.ToString("f4"));

                float optiPocketDist = (avgC1 - avgC2).magnitude;
                float unityPocketDist = (Experiment.corner1.position - Experiment.corner2.position).magnitude;

                //Debug.Log("opti pocket dist : " + optiPocketDist.ToString("f4"));
                //Debug.Log("unity pocket dist : " + unityPocketDist.ToString("f4"));
                //Debug.Log("hmd pos : " + Experiment.hmd.position.ToString("f4"));
                //Experiment.setEnvPosition();

                test = false;
            }
        }
    }

    //Function to get IDs of morkers in corresponding pockets for calibration
    private void getMarkerId()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId); //access rigidbody
        List<OptitrackMarkerState> markers = new List<OptitrackMarkerState>(); //initialize list of marker objects from rigidbody
        markers = StreamingClient.GetLatestMarkerStates(); //get objects of all markers from rigidbody
        List<int> idList = new List<int>();

        //For Calibrattion
        if (Experiment.experiment == 0)
        {

            float maxz = -1000f;
            int idz = -1;
            float maxx = -1000f;
            int idx = -1;
            for (int j = 0; j < markers.Count; j++)
            {

                OptitrackMarkerState m = markers[j];
                int mID = m.Id;
                Vector3 pos = m.Position;
                idList.Add(mID);

                // one corner is placed further to the positive z (right)
                if (pos.x > maxx)
                {
                    maxx = pos.x;
                    idx = mID;
                }
                // one marker is placed further to the negative x (toward end of table)
                if (pos.z > maxz)
                {
                    maxz = pos.z;
                    idz = mID;
                }
            }
            idList.Remove(idz);
            idList.Remove(idx);
            //store marker IDs
            corner1ID = idList[0];
            corner2ID = idz;
            corner3ID = idx;
        }

        //For cue stick
        else
        {
            float maxz = -1000f;
            int idback = -1;
            float minz = 1000f;
            int idfront = -1;
            for (int j = 0; j < markers.Count; j++)
            {

                OptitrackMarkerState m = markers[j];
                int mID = m.Id;
                Vector3 pos = m.Position;
                idList.Add(mID);

                // one corner is placed further to the positive z (front)
                if (pos.z > maxz)
                {
                    maxz = pos.z;
                    idfront = mID;
                }
                // one marker is placed further to the negative z (back)
                if (pos.z < minz)
                {
                    minz = pos.z;
                    idback = mID;
                }

            }
            frontID = idfront;
            backID = idback;
            markerIDs = false;

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
            Vector3 contactPos = new Vector3(x / numContacts, y / numContacts, z / numContacts); //average position of contact between cue stick and cue ball
            Vector3 forceDirection = (contactPos - transform.position).normalized; //direction of applied force = contact position - cue stick position
            float scaleForce = Experiment.scaleForce; //scaling factor of force direction
            Vector3 cueForce = forceDirection * Math.Abs(avgVelocity.magnitude) * scaleForce; //applied force to cue weighted by scaling factor & velocity and applied in forceDirection
            //Vector3 f = new Vector3(5f, 0f, 200f);
            //rb.AddForce(f);
            rb.AddForce(cueForce); //apply force to cue ball

        }
    }

    //Calculate average of last n vectors in list
    private Vector3 AverageVelocity(List<Vector3> vlist, int numavg)
    {
        Vector3 sumvel = new Vector3(0f, 0f, 0f);
        for (int idx = vlist.Count - 1; idx >= vlist.Count - numavg; idx--)
        {
            sumvel = sumvel + vlist[idx];
        }
        return sumvel / numavg;

    }

}