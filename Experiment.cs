using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;

//Class used to collect data from game objects and writes info to file
public class Experiment : MonoBehaviour
{

    //************************************************************************ */
    //Game Variables - physics, constants
    public static float angularDrag = 0.8f; //angular drag on the pool balls
    public static float scaleForce = 200f; //scale velocity of cue to force trasnmitted to cue ball
    public static int numVelocitiesAverage = 5; //how many velocities to average for the cue shot velocity measurement
    public static Rigidbody cueballRB; //cue ball rigidbody
    public static Rigidbody redballRB; //red ball rigidbody
    public static Rigidbody cueRB; //cue stick rigidbody
    public static Vector3 cueballstart; //cue ball starting position
    public static Vector3 redballstart; //red ball starting position
    public static Vector3 cuestart; //cue stick starting position
    public static bool cue_cueball; //cue - cueball collision
    public static bool cueball_redball; //cue ball - red ball collision
    public static bool madeShot; //true if red ball made in pocket
    public static bool scratch; //true if cue ball made in pocket
    public static bool outOfBounds; //true if cue ball is hit out of the pool table field
    public static MeshRenderer cueMesh; //cue ball visible mesh
    public static MeshRenderer redballMesh; //red ball visible mesh

    //************************************************************************ */
    //Optitrack --> Unity calibration
    public static float poolTableLength = 2f;
    public static float poolTableWidth = 1f;
    public static float unityTableHeight = 0.84f;
    public static float optitrackTableHeight = 1.4f;

    public static float unityCueLength = 1.3425f; //unity
    public static float realCueLength = 123f; //centimeters
    public static float tip_marker1 = 35f; //centimeters
    public static float marker1_marker2 = 33f; //centimeters
    public static float marker2_marker3 = 33f; //centimeters
    public static float marker3_base = 22f; //centimeters
    public static float unityMarker1_Marker3 = 1.3294f; //unity
    public static float real_unity_ratio = unityMarker1_Marker3 / (marker1_marker2 + marker2_marker3);
    public static float unityTip_marker1 = tip_marker1 / real_unity_ratio; //unity
    //public static float frontWeight = (unityCueLength - unityTip_marker1) / unityMarker1_Marker3;
    //public static float backWeight = 1 - frontWeight;

    public static float yratio = unityTableHeight / optitrackTableHeight; //height sclaing ratio between environemnts
    public static float frontWeight = 0.05f; //weight of cue towards front marker
    public static float backWeight = 0.95f; //weight of cue towards back marker
    public static float[,] unityPocketPoints = new float[,] { { 0f, -1f, 0f }, { -0.5f, -0.5f, 0.5f }, { 1f, 1f, 1f } }; //pocket positions in Unity
    //public static float[,] optiPocketPoints = new float[,] { { -0.4f, -1.7f, -0.4f }, { 0.1f, 0.1f, 1.4f }, { 1f, 1f, 1f } }; //actual table pocket position in Motiv
    public static float[,] optiPocketPoints = new float[,] { { -0.5f, -1.7f, -0.5f }, { 0f, 0f, 1.4f }, { 1f, 1f, 1f } }; //small table
    public static float[,] transformCoordinateMatrix = MultiplyMatrix(unityPocketPoints, inverseMat(optiPocketPoints)); //transformation matrix (Xu,Zu,1) = M * (Xo,Zo,1)

    //************************************************************************ */
    //Data Collection - trials, experiment type, number etc
    public static int experiment = 1; //0 = calibration, 1 = normal, 2 = no feedback, 3 = adaptation
    public static Vector3 adaptationForce = new Vector3(0, 0, 5f); //force applied to the cue ball for adaptation task
    public static float waitTime = 0.5f; //time to wait before feedback change
    public static int totalTrials = 50; //trials per block
    public static int trialNumber = 1;

    //************************************************************************ */
    private StringBuilder csv;
    private string dir;
    private string pth;

    private string timestamp;
    private string shottime;
    private string redballtime;
    private string madeshottime;
    private Vector3 pos;
    private int count = 0;

    //************************************************************************ */
    // Use this for initialization
    void Start()
    {
        //Initialize game variables
        cueRB = GameObject.Find("Cue").GetComponent<Rigidbody>(); //physics rigidbody of cue
        cueRB.constraints = RigidbodyConstraints.FreezeRotation; //freeze rotation of the cue stick (fixed bug of cue randomly moving)
        redballRB = GameObject.Find("RedBall").GetComponent<Rigidbody>(); //physics rigidbody of red ball
        cueballRB = GameObject.Find("CueBall").GetComponent<Rigidbody>(); //physics rigidbody of cue ball
        cueMesh = gameObject.GetComponent<MeshRenderer>(); //visible mesh of the cue object
        redballMesh = GameObject.Find("RedBall").GetComponent<MeshRenderer>(); //visible mesh of the red ball object

        cueballstart = cueballRB.position;
        redballstart = redballRB.position;
        cuestart = cueRB.position;

        cue_cueball = false;
        cueball_redball = false;
        madeShot = false;
        scratch = false;
        outOfBounds = false;
        cueMesh.enabled = true;
        redballMesh.enabled = true;

        //Increase angular drag to make ball stop rolling faster - initialized to 0.5
        cueballRB.angularDrag = angularDrag;
        redballRB.angularDrag = angularDrag;

        //Data stuff
        csv = new StringBuilder(); //file object to write to
        dir = @"C:\Users\iView\Documents\Guhan\PoolVR\Data\test.txt"; //path directory to write text file to
        startInfo(); //write subject data to file

    }

    // Update is called once per frame
    void Update()
    {
        //write data to text file every ~1 second
        count++;
        storeData();
        if (count % 100 == 0)
        {
            File.AppendAllText(dir, csv.ToString());
        }

        //restart scene after shot is taken and balls are stationary
        if (cue_cueball && isSceneStill())
        {
            restartScene();
        }

    }

    //************************************************************************ */
    //function to access environment game object data
    void storeData()
    {
        timestamp = Time.time.ToString("F4");
        var cuepos = vecToStr(cueRB.position);
        var cuevel = vecToStr(cueRB.velocity);
        var cueballpos = vecToStr(cueballRB.position);
        var cueballvel = vecToStr(cueballRB.velocity);
        var redballpos = vecToStr(redballRB.position);
        var redballvel = vecToStr(redballRB.velocity);
        shottime = null;
        redballtime = null;
        madeshottime = null;
        if (cue_cueball)
        {
            shottime = Time.time.ToString("F4");
        }
        if (cueball_redball)
        {
            redballtime = Time.time.ToString("F4");
        }
        if (madeShot)
        {
            madeshottime = Time.time.ToString("F4");
        }
        var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", timestamp, cuepos, cuevel, cueballpos, cueballvel, redballpos, redballvel, shottime, redballtime, madeshottime);
        csv.AppendLine(newLine);
    }

    //function to return a single string representation of a vector
    private string vecToStr(Vector3 val)
    {
        return "(" + val[0] + ";" + val[1] + ";" + val[2] + ")";
    }

    //Function to write the start information
    void startInfo()
    {

        // This text is added only once to the file.
        if (!File.Exists(dir))
        {
            // Create a file to write to.
            string titles = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", "Timestamp", "cuepos", "cuevel", "CBpos", "CBvel", "RBpos", "RBvel", "C1", "C2", "C3" + System.Environment.NewLine);
            File.WriteAllText(dir, titles);
        }
        //string titles = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}", "Timestamp","cuepos","cuevel","CBpos","CBvel","RBpos","RBvel","C1","C2","C3");
        //string titles = string.Format("{0},{1},{2},{3},{4}", "Timestamp (s)", "CBpos", "CBvel", "RBpos", "RBvel");

        //csv.AppendLine(titles);
        //File.AppendAllText(dir, csv.ToString());
    }

    //Function to reset the game scene once the trial is over
    public static void restartScene()
    {
        //Set ball velocities to zero and move to starting positions
        setStill(cueballRB);
        setStill(redballRB);
        cueballRB.MovePosition(cueballstart);
        redballRB.MovePosition(redballstart);

        //reset booleans
        cueMesh.enabled = true;
        redballMesh.enabled = true;
        cue_cueball = false;
        cueball_redball = false;
        madeShot = false;
        scratch = false;
        outOfBounds = false;

        //Modify experimental data
        trialNumber++;

        //Load starting scene again
        SceneManager.LoadScene("Main");
    }

    //Function to set Rigidbody rb velocity to zero
    public static void setStill(Rigidbody rb)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public static bool isSceneStill()
    {
        if (outOfBounds)
        {
            setStill(cueballRB);
        }
        if (cueballRB.velocity.magnitude == 0 && redballRB.velocity.magnitude == 0)
        {
            return true;
        }
        return false;
    }

    //Function to multiply two 3x3 matrices with each other
    //newMat = a * b
    public static float[,] MultiplyMatrix(float[,] a, float[,] b)
    {
        if (a.GetLength(1) == b.GetLength(0))
        {
            float[,] newMat = new float[a.GetLength(0), b.GetLength(1)];
            for (int i = 0; i < newMat.GetLength(0); i++)
            {
                for (int j = 0; j < newMat.GetLength(1); j++)
                {
                    newMat[i, j] = 0;
                    for (int k = 0; k < a.GetLength(1); k++) // OR k<b.GetLength(0)
                        newMat[i, j] = newMat[i, j] + a[i, k] * b[k, j];
                }
            }
            return newMat;
        }
        else
        {
            Console.WriteLine("\n Number of columns in First Matrix should be equal to Number of rows in Second Matrix.");
            Console.WriteLine("\n Please re-enter correct dimensions.");
        }
        return null;
    }


    //Function to invert 3x3 matrix m
    //minv = m^-1
    public static float[,] inverseMat(float[,] m)
    {
        float det = m[0, 0] * (m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2]) -
             m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
             m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);

        float invdet = 1 / det;

        float[,] minv = new float[3, 3];
        minv[0, 0] = (m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2]) * invdet;
        minv[0, 1] = (m[0, 2] * m[2, 1] - m[0, 1] * m[2, 2]) * invdet;
        minv[0, 2] = (m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1]) * invdet;
        minv[1, 0] = (m[1, 2] * m[2, 0] - m[1, 0] * m[2, 2]) * invdet;
        minv[1, 1] = (m[0, 0] * m[2, 2] - m[0, 2] * m[2, 0]) * invdet;
        minv[1, 2] = (m[1, 0] * m[0, 2] - m[0, 0] * m[1, 2]) * invdet;
        minv[2, 0] = (m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1]) * invdet;
        minv[2, 1] = (m[2, 0] * m[0, 1] - m[0, 0] * m[2, 1]) * invdet;
        minv[2, 2] = (m[0, 0] * m[1, 1] - m[1, 0] * m[0, 1]) * invdet;

        return minv;

    }
}
