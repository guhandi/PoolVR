//======================================================================================================
// 2016, NaturalPoint Inc.
//======================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
    A class that streams data from an Optitrack Motiv rigidbody object to the Unity environment
    Has methods to do Optritrack -> Unity calibration, cue stick control, and cue ball ccollision phsyics
*/
public class OptitrackRigidBody : MonoBehaviour
{
    public OptitrackStreamingClient StreamingClient;
    public Int32 RigidBodyId; //Id of rigidbody object being streamed from Motiv
    public int count; //frame count

    //public Transform front; //reference to the tip of the cue stick - used for cue stick positioning
    public Transform cueTip; //reference to the tip of the cue stick - used to apply force to the cue ball
    private Vector3 frontPos; //position of marker closest to front hand
    private Vector3 backPos; //position of marker closest to back hand
    private Vector3 cuePos; //cue position at current timestep in Unity
    private Vector3 prevPos; //cue position at previous timestep
    private Vector3 Opticuepos; //cue position in Optitrack environment
    private Vector3 cueVelocity; //velocity of cue stick as it is being moved
    public static Vector3 avgVelocity; //average velocity of the cue stick (for smoothing)
    private List<Vector3> VelocityList; //list of last 5 cue velocities

    private int backID1;
    private int backID2;
    private int frontID1;
    private int frontID2;

    //CALIBRATION STUFF
    private int corner1ID; //marker ID at corner 1 (for calibration)
    private int corner2ID; //marker ID at corner 2 (for calibration)
    private int corner3ID; //marker ID at corner 3 (for calibration)
    private List<Vector3> C1; //list of corner 1 points (for calibration)
    private List<Vector3> C2; //list of corner 2 points (for calibration)
    private List<Vector3> C3; //list of corner 3 points (for calibration
    private int numCalSamples; //number of positions samples to collect for calibration

    private bool markerIDs = true;
    private bool test;

    /*
        Method called at start of game
    */
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

        count = 0; //frame number
        prevPos = Experiment.cuestart; //initialize previous positons to cue starting position
        
        //initialize cue velocities
        cueVelocity = new Vector3(0f, 0f, 0f);
        VelocityList = new List<Vector3>();
        for (int i = 0; i < Experiment.numVelocitiesAverage; i++)
        {
            VelocityList.Add(cueVelocity);
        }

        //initialize IDs and positions of optitrack markers placed on the pool table
        corner1ID = -1;
        corner2ID = -1;
        corner3ID = -1;
        C1 = new List<Vector3>();
        C2 = new List<Vector3>();
        C3 = new List<Vector3>();

        //get IDs and positions of optitrack markers
        if (Experiment.experiment == 0 && markerIDs)
        {
            getCalMarkerId();
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

    /*
        Update method called once per frame 
    */
    void FixedUpdate()
    {
        //CALIBRATION
        count++;
        if (Experiment.experiment == 0 && Experiment.isEnvSet)
        {
            calibrateTable();
        }

        //EXPERIMENT - get marker positions and translate cue position from Optitrack to Unity environemnt
        if (Experiment.experiment != 0)
        {
            UpdatePose();
        }

    }

    /*
        Method to update the position of the cue rigidbody every by reading the data from the Optitrack client
    */
    void UpdatePose()
    {
        
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId); //access rigidbody
        Vector3 OfrontPos = new Vector3(); //optitrack front position
        Vector3 ObackPos = new Vector3(); //optitrack back position
        List<OptitrackMarkerState> markerStates = new List<OptitrackMarkerState>(); //initialize list of marker objects from rigidbody
        List<Vector3> markerPositions = new List<Vector3>(); //list of all cue marker positions


        if (rbState != null)
        {
            //Get marker positions
            markerStates = StreamingClient.GetLatestMarkerStates(); //get objects of all markers from rigidbody
            
            //if not all markers are tracked, make cue stick invisible
            if (markerStates.Count != 4)
            {
                Experiment.cuestickMesh.enabled = false;
                //Debug.Log(markerStates.Count);
                cuePos = prevPos;
                return;
            }
            else
            {
                Experiment.cuestickMesh.enabled = true;
            }
            
            //Loop through markers
            for (int idx = 0; idx < markerStates.Count; idx++)
            {
                OptitrackMarkerState marker = markerStates[idx];
                int markerID = marker.Id;
                Vector3 pos = marker.Position;
                markerPositions.Add(marker.Position);
            }
            
            IEnumerable<Vector3> sorted = markerPositions.OrderBy(v => v.z); //sort marker positions by z position
            OfrontPos = (sorted.ElementAt(3) + sorted.ElementAt(2)) / 2; //front position is average of front two markers
            ObackPos = (sorted.ElementAt(1) + sorted.ElementAt(0)) / 2; //back position is average of back two markers

            //Translate optitrack positions to unity using helper methods ( [Xo, Zo, 1] * M = [Xu, Zu, 1])
            float[,] frontMatrix = new float[,] { { OfrontPos.x }, { OfrontPos.z }, { 1 } };
            float[,] backMatrix = new float[,] { { ObackPos.x }, { ObackPos.z }, { 1 } };
            float[,] frontU = Experiment.MultiplyMatrix(Experiment.M, frontMatrix);
            float[,] backU = Experiment.MultiplyMatrix(Experiment.M, backMatrix);
            Vector3 backPos = new Vector3(backU[0, 0], ObackPos.y * Experiment.yratio, backU[1, 0]);
            Vector3 frontPos = new Vector3(frontU[0, 0], OfrontPos.y * Experiment.yratio, frontU[1, 0]);

            //cue position is front position plus projected vector
            Vector3 direction = (frontPos - backPos).normalized;
            cuePos = frontPos + (direction * PlayerPrefs.GetFloat("tip_marker1") * PlayerPrefs.GetFloat("cmToUnity")); 
            
            //limit cuetip height to table surface height
            if (cuePos.y < Experiment.corner1.position.y)
            {
                cuePos = new Vector3(cuePos.x, Experiment.corner1.position.y, cuePos.z);
            }

            //calculate velocity
            cueVelocity = (cuePos - prevPos) / Time.fixedDeltaTime;
            VelocityList.Add(cueVelocity);
            avgVelocity = AverageVelocity(VelocityList, Experiment.numVelocitiesAverage);
            prevPos = cuePos;

            //move cue rigidbody to updated position
            Experiment.cueFront.transform.position = cuePos;
            //get forward direction by lookng at forwrd position from actual optitrack position (could also do with transformed positions)
            Experiment.cueFront.transform.rotation = Quaternion.LookRotation(frontPos - backPos) * Quaternion.Euler(0f, 0f, 0f);

        }
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

            //derive C2 based on equalateral right triangle 
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

            //Cue ball starting position in unity
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

                float optiPocketDist = (avgC1 - avgC3).magnitude;
                float unityPocketDist = (Experiment.corner1.position - Experiment.corner3.position).magnitude;
                PlayerPrefs.SetFloat("optiToUnity", unityPocketDist / optiPocketDist);

                test = false;
            }
        }
    }

    //Function to get IDs of markers in corresponding pockets for calibration
    private void getCalMarkerId()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId); //access rigidbody
        List<OptitrackMarkerState> markers = new List<OptitrackMarkerState>(); //initialize list of marker objects from rigidbody
        markers = StreamingClient.GetLatestMarkerStates(); //get objects of all markers from rigidbody
        List<int> idList = new List<int>();

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
        corner1ID = idList[0]; //by elimination we get the 3rd corner 
        corner2ID = idz;
        corner3ID = idx;
    }

    //Get marker IDs for cue for cue stick
    private void getCueMarkers()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId); //access rigidbody
        List<OptitrackMarkerState> markers = new List<OptitrackMarkerState>(); //initialize list of marker objects from rigidbody
        markers = StreamingClient.GetLatestMarkerStates(); //get objects of all markers from rigidbody

        SortedList<float, int> sortedMarkers = new SortedList<float, int>();
        for (int j = 0; j < markers.Count; j++)
        {
            OptitrackMarkerState m = markers[j];
            int mID = m.Id;
            Vector3 pos = m.Position;
            Debug.Log(pos.ToString("f4"));
            float zpos = pos.z;

            sortedMarkers.Add(zpos, mID);

        }
        backID1 = sortedMarkers.Values[0];
        backID2 = sortedMarkers.Values[1];
        frontID1 = sortedMarkers.Values[2];
        frontID2 = sortedMarkers.Values[3];
        markerIDs = false;

    }

    //When cue stick comes into contact with other objects
    private void OnCollisionEnter(Collision col)
    {
        //If cue ball doesn't collide with any rigidbody do nothing
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (!rb)
        {
            //return;
        }

        //If cue stick collides with the cue ball
        if (col.gameObject.name == "CueBall")
        {
            
            //for contact quality
            int minpoints = 1;
            int maxpoints = 5;
            int numcontacts = col.contacts.Length;
            float contactScale = (numcontacts - minpoints) / ((maxpoints - minpoints)/2);


            //for cue force and velocity
            float minvel = 0.01f;
            float maxvel = 2f;
            float minforce = 0f;
            float maxforce = 0.1f;

            Vector3 direction = avgVelocity.normalized; //force direction is norm of cue velocity
            float mag = avgVelocity.magnitude;
            float scale = (mag - minvel) / (maxvel - minvel);
            Vector3 forceVec = maxforce * scale * direction;

            Vector3 contactPoint = new Vector3();
            List<Vector3> contactList = new List<Vector3>(); //list of all points of contact between stick and ball
            foreach (ContactPoint contact in col.contacts)
            {
                Vector3 pt = contact.point;
                contactList.Add(contact.point);
                //rb.AddForceAtPosition(forceVec, pt, ForceMode.Impulse);
            }

            contactPoint = Experiment.AverageVec(contactList);
            //contactPoint = col.contacts[0].point;
            Debug.Log("numpoints  :" + numcontacts);
            
            Vector3 t = new Vector3(0.1f, 0, 5f);
            //rb.AddForce(0.5f * avgVelocity.normalized, ForceMode.Impulse);
            //rb.AddForce(contactScale * forceVec, ForceMode.Impulse);
            //rb.AddForceAtPosition(forceVec, contactPoint, ForceMode.Impulse);
            //rb.AddTorque(forceVec);
            Vector3 momentumVec = cueVelocity * Experiment.cueRB.mass / Experiment.cueballRB.mass; //elastic collision velocity between cue stick and ball
            rb.velocity = momentumVec; //add velocity to cue ball

            Debug.Log("force app  :  " + forceVec.ToString("f4"));
            Debug.Log("velocity   :  " + avgVelocity.ToString("f4")); //min = 0.5    max = 5
            Debug.Log("mag   :  " + mag.ToString("f4"));
            Debug.Log("cuevelocity   :  " + cueVelocity.ToString("f4"));
            Debug.Log("contactpoint   : " + contactPoint.ToString("f4"));
            Debug.Log("frontpos   : " + cueTip.position.ToString("f4"));
        }
    }


    //Method that is called at any gameobject collision with the cue stick
    void OnCollisionStay(Collision col)
    {
        return;
        //float scale = 0.05f;
        //If cue ball doesn't collide with any rigidbody do nothing
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (!rb)
        {
            //return;
        }

        //If cue stick collides with the cue ball
        if (col.gameObject.name == "CueBall")
        {
            float minvel = 0.1f;
            float maxvel = 5f;
            float minforce = 0f;
            float maxforce = 0.25f;


            foreach (ContactPoint contact in col.contacts)
            {
                Vector3 contactPoint = contact.point;
                Vector3 tforce = new Vector3(0f, 0f, 0.3f);
                Vector3 direction = avgVelocity.normalized;
                float mag = avgVelocity.magnitude;
                float scale = (mag - minvel) / (maxvel - minvel);
                Vector3 forceVec = scale * maxforce * direction;

                //rb.AddForceAtPosition(scale * forceVec, contactPoint, ForceMode.Impulse);
                rb.AddForce(scale * forceVec, ForceMode.Force);
                //rb.AddForceAtPosition(tforce, contactPoint, ForceMode.Impulse);
                //rb.AddForce(numContacts * scale*forceVec, ForceMode.Impulse);
                Debug.Log("contact point  = " + contactPoint.ToString("f4"));
                Debug.Log("force app  :  " + (scale * forceVec).ToString("f4"));
                Debug.Log("velocity   :  " + avgVelocity.ToString("f4")); //min = 0.5    max = 5

            }
        }
    }

    //Helper method to calculate average of last n vectors in list
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