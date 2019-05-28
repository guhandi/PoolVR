using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.VR;
using System.Text;
using System.IO;

/*
    The main game class.
    Contains methods to initialize all game variables, store & write game data, and perfrom calibrations
*/
public class Experiment : MonoBehaviour
{

    //Experiment variables to change
    public static int experiment = 1; //0 = calibration, 1 = normal, 2 = adaptation, 3 = reward
    private string dir = @"C:\Users\iView\Documents\Guhan\PoolVR\Data\test.txt"; //path directory to write text file to

    #region Game Variables
    //************************************************************************ */
    //Data Collection - trials, experiment type, etc
    public static Vector3 adaptationForce = new Vector3(-0.1f, 0, 0); //force applied to the cue ball for adaptation task
    public static float waitTime = 0.5f; //time to wait before feedback change
    public static int trialnum; //current trial number
    public static bool nextTrial;
    public Text trial;
    

    //************************************************************************ */
    //Calibration Variables to set between real-world, Optitrack, and Unity
    public static Vector3 shiftEnvironemnt; // offset of player position in environemnt 
    public static float envScale; //scaling VR environemnt to real object size
    public static float[,] M; // = MultiplyMatrix(tableUnityPoints, inverseMat(tableOptiPoints)) --> transformation matrix (Xu,Zu,1) = M * (Xo,Zo,1)
    public static float poolTableWidth; //unity
    public static float realWidth; //cm
    public static float yratio; //height sclaing ratio between environemnts (optiy * yratio = unityy)
    public static float cmToUnity; //cm * cmToUnity = Unity
    public static bool isEnvSet; //true if shiftEnvironment and envScale are calibrated


    //************************************************************************ */
    //Game Variables - physics, constants
    public static int numVelocitiesAverage;// = 5; //how many velocities to average for the cue shot velocity measurement
    public static Rigidbody cueballRB; //cue ball rigidbody
    public static Rigidbody redballRB; //red ball rigidbody
    //public static Rigidbody cueRB; //cue stick rigidbody
    public static Transform cue;
    public static Rigidbody cueFront; //reference to front of cue stick
    public static Transform cueTip; //cue tip
    public static Transform cueBack; //base of cue stick
    public static Transform table; //table object
    public static Transform env; //environment (Scaler) object
    public static Transform corner1; //unity pocket 1 - front left
    public static Transform corner2; //unity pocket 2 - middle left
    public static Transform corner3; //unity pocket 3 - front right
    public static Transform corner4; //unity pocket 4 - back right
    public static Transform corner5; //unity pocket 3 - front right
    public static Transform corner6; //unity pocket 4 - back right
    public static float xmin; //left edge postion
    public static float xmax; //right edge position
    public static float zmin; //back edge position
    public static float zmax; //front edge position
    public static Vector3 cueballstart; //cue ball starting position
    public static Vector3 redballstart; //red ball starting position
    public static Vector3 cuestart; //cue stick starting position
    public static MeshRenderer cueMesh; //cue ball visible mesh
    public static MeshRenderer redballMesh; //red ball visible mesh
    public static MeshRenderer cuestickMesh; //cue visible mesh
    public static SphereCollider cueballCollide;
    public SteamVR_TrackedObject ctrl1; //left vive controller
    public SteamVR_TrackedObject ctrl2; //right vive controller
    public static bool cue_cueball; //cue - cueball collision
    public static bool cueball_redball; //cue ball - red ball collision
    public static bool madeShot; //true if red ball made in pocket
    public static bool scratch; //true if cue ball made in pocket
    public static bool outOfBounds; //true if cue ball is hit out of the pool table field    

    //************************************************************************ */
    //data storage
    private StringBuilder csv;
    public float trialTime;
    private string timestamp;
    private string shottime;
    private string redballtime;
    private string madeshottime;
    private Vector3 pos;
    private int count = 0;

    //testing
    public static bool first = true;

    #endregion

    /*
        Method called at start of game
    */
    void Awake()
    {
        //set unique file path
        dir = MakeUnique(dir);

        //Initialize game variables
        initGameVariables();

        //Calibrate environments
        startCalibration();
        setTableBoundary();

        //Data stuff
        csv = new StringBuilder(); //file object to write to
        startInfo(); //write variable titles to file

    }

    /*
        Update method called once per frame (main game loop)
    */
    void FixedUpdate()
    {
        #region RestartScene
        //restart scene after shot is taken and balls are stationary
        if (experiment != 0 && cue_cueball && (isSceneStill() || outOfBounds))
        { 
            if (first)
            {
                first = false;
                return;
            }
            cue_cueball = false;
            first = true;
            StartCoroutine(wait());
        }
        #endregion

        #region Manage Experiment Info
        //store current frame variable data
        storeData();
        if (nextTrial)
        {
            trial.text = "Trial : " + trialnum;
        }
        #endregion

        #region Dynamic Calibration
        //DO CALIBRATION
        count++;
        if (experiment == 0)
        {
            if (count > 500 && !isEnvSet) //after 500 frames (to ensure equilibrium has been reached)
            {
                //set environment scale
                setScale(realWidth, ctrl1.transform.position, ctrl2.transform.position);

                //set calibration position to be the corner 3 pocket and match the environments
                Vector3 calpos = ctrl1.transform.position;
                if (ctrl2.transform.position.x > calpos.x)
                {
                    calpos = ctrl2.transform.position;
                }
                setEnvPosition(calpos);
                isEnvSet = true;
            }
        }
        #endregion

    }

    #region Method - Initialize variables
    /*
        Method to initialize all the game variables in the scene needed by the code
    */
    public void initGameVariables()
    {
        //Game Objects
        //cueRB = GameObject.FindGameObjectWithTag("cue").GetComponent<Rigidbody>(); //physics rigidbody of cue
        cue = GameObject.FindGameObjectWithTag("cue").GetComponent<Transform>(); //physics rigidbody of cue
        redballRB = GameObject.FindGameObjectWithTag("redball").GetComponent<Rigidbody>(); //physics rigidbody of red ball
        cueballRB = GameObject.FindGameObjectWithTag("cueball").GetComponent<Rigidbody>(); //physics rigidbody of cue ball
        cueTip = GameObject.FindGameObjectWithTag("cuetip").GetComponent<Transform>(); //child of cue stick - used for collision
        cueBack = GameObject.FindGameObjectWithTag("cueback").GetComponent<Transform>(); //child of cue stick - used for cue length
        cueFront = GameObject.FindGameObjectWithTag("cuefront").GetComponent<Rigidbody>(); //parent of cue stick - used to reference the front for position
        //cueRB.constraints = RigidbodyConstraints.FreezeRotation; //freeze rotation of the cue stick (fixed bug of cue randomly moving)
        cueFront.constraints = RigidbodyConstraints.FreezeRotation; //freeze rotation of the cue stick (fixed bug of cue randomly moving)
        cueFront.constraints = RigidbodyConstraints.FreezePosition; //freeze position of the cue stick (fixed bug of cue randomly moving)
        env = GameObject.FindGameObjectWithTag("scaler").GetComponent<Transform>(); //object in which all gameobjects are stored for scaling purposes
        table = GameObject.FindGameObjectWithTag("table").GetComponent<Transform>(); //pool table
        cueMesh = GameObject.FindGameObjectWithTag("cueball").GetComponent<MeshRenderer>(); //visible mesh of the cue ball object
        redballMesh = GameObject.FindGameObjectWithTag("redball").GetComponent<MeshRenderer>(); //visible mesh of the red ball object
        cuestickMesh = GameObject.FindGameObjectWithTag("cue").GetComponent<MeshRenderer>(); //visible mesh of the red ball object
        cueballCollide = GameObject.FindGameObjectWithTag("cueball").GetComponent<SphereCollider>(); //cue ball collider object

        //booleans
        cue_cueball = false;
        cueball_redball = false;
        madeShot = false;
        scratch = false;
        outOfBounds = false;
        cueMesh.enabled = true;
        redballMesh.enabled = true;
        isEnvSet = true;
        nextTrial = false;

        //Calibration Variables
        corner1 = GameObject.Find("Corner1").GetComponent<Transform>();
        corner2 = GameObject.Find("Corner2").GetComponent<Transform>();
        corner3 = GameObject.Find("Corner3").GetComponent<Transform>();
        corner4 = GameObject.Find("Corner4").GetComponent<Transform>();
        corner5 = GameObject.Find("Corner5").GetComponent<Transform>();
        corner6 = GameObject.Find("Corner6").GetComponent<Transform>();

        //trial variables
        trialnum = 1;
        trialTime = 0f;

    }
    #endregion

    #region Methods - Static Calibration
    /*
        Method to set the scale and position of the environment and game objects
    */
    public static void startCalibration()
    {

        //If calibration, reset calibration variables (for troubleshooting uncomment gameinfo() to reset values)
        if (experiment == 0)
        {
            isEnvSet = false;
            //GameInfo();
        }

        //Actually set calibration variables to respective values
        envScale = PlayerPrefs.GetFloat("scalingRatio");
        float yoffset_cm = 3.8f;
        shiftEnvironemnt = getEnvShift() - new Vector3(0, yoffset_cm * PlayerPrefs.GetFloat("cmToUnity"), 0); //shift VR environemnt to match real life objects
        M = getTransformationMatrix();
        env.localScale = envScale * (new Vector3(1, 1, 1));
        env.position = shiftEnvironemnt;
        setCueSize(); //scale cue stick size
        setBallSize(); //scale ball size

        //Game physics
        numVelocitiesAverage = 5;
        poolTableWidth = PlayerPrefs.GetFloat("poolTableWidth"); //unity
        realWidth = PlayerPrefs.GetFloat("realWidth"); //unity
        yratio = corner1.position.y / PlayerPrefs.GetFloat("Ocuey");

        //store object start positions
        cueballstart = cueballRB.position;
        redballstart = redballRB.position;
        //cuestart = cueRB.position;
        cuestart = cue.position;

    }

    //place unity object at corner 3 (close right) position to calibrate real pool table position with unity table position
    public static void setEnvPosition(Vector3 objectPos)
    {
        Vector3 cornerpos = corner2.position;
        Vector3 envOffset = objectPos - cornerpos;
        env.position = env.position + envOffset;

        PlayerPrefs.SetFloat("ShiftEnvironemnt_x", env.position.x);
        PlayerPrefs.SetFloat("ShiftEnvironemnt_y", env.position.y);
        PlayerPrefs.SetFloat("ShiftEnvironemnt_z", env.position.z);

    }

    //Method to calibrate real life pocket pool table width with unity table width
    public static void setScale(float tableWidth_cm, Vector3 p1, Vector3 p2)
    {
        float currentTableWidth = (corner1.position - corner2.position).magnitude;
        float desiredWidth = (p1 - p2).magnitude;
        float cmLength = PlayerPrefs.GetFloat("realWidth");
        float scale = desiredWidth / currentTableWidth;

        env.localScale = env.localScale * scale;
        PlayerPrefs.SetFloat("scalingRatio", env.localScale.x);
        PlayerPrefs.SetFloat("cmToUnity", desiredWidth / cmLength);
        //Debug.Log("Scaling = " + scale);
    }

    //Method to scale the length of the cue stick using a cm-Unity calibration value
    public static void setCueSize()
    {
        float realLength = PlayerPrefs.GetFloat("realCueLength");
        float unityLength = (cueBack.transform.position - cueTip.position).magnitude;
        float desiredLength = realLength * PlayerPrefs.GetFloat("cmToUnity");
        float cueScale = desiredLength / unityLength;

        cueFront.transform.localScale = cueFront.transform.localScale * cueScale;
        PlayerPrefs.SetFloat("unityCueLength", (cueBack.transform.position - cueTip.position).magnitude);
    }

    //Method to set raidus of pool balls
    public static void setBallSize()
    {
        float d = 1.25f;
        float r = d / 2;
        cueballRB.transform.localScale = d * new Vector3(1f, 1f, 1f);
        redballRB.transform.localScale = d * new Vector3(1f, 1f, 1f);
        //cueballCollide.radius = r;
    }

    //Get table limits using pocket positions and error
    public void setTableBoundary()
    {
        float d = 0.2f;
        xmin = corner1.transform.position.x - d;
        xmax = corner2.transform.position.x + d;
        zmin = corner1.transform.position.z - d;
        zmax = corner6.transform.position.z + d;
    }

    #endregion

    #region Methods - Restarting Scene
    //Method to reset the game scene once the trial is over
    public void restartScene()
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
        CueBallController.onAdapt = true;
        nextTrial = true;

        //other
        //cueRB.constraints = RigidbodyConstraints.FreezeRotation; //freeze rotation of the cue stick (fixed bug of cue randomly moving)
        cueFront.constraints = RigidbodyConstraints.FreezeRotation;
        cueFront.constraints = RigidbodyConstraints.FreezePosition;

        //trial stuff
        trialTime = 0f;
        trialnum = trialnum + 1;

    }

    IEnumerator wait()
    {
        //Wait 0.5s after cue ball collides with redball before objects become invisible
        yield return new WaitForSeconds(1);
        restartScene();

    }
   
    //Method to set Rigidbody rb velocity to zero
    public static void setStill(Rigidbody rb)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    //Method to check if trial scene has no more movement
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
    #endregion

    #region Methods - Matrices

    //Method to do a 2D coordinate transformation given a vector pos (x,z,1) and 3x3 transformation matrix m
    public static Vector3 transformCoordinates(Vector3 pos, float[,] m)
    {
        float[,] A = new float[,] { { pos.x }, { pos.z }, { 1 } }; //optitrack position
        float[,] transformed = MultiplyMatrix(m, A);
        Vector3 B = new Vector3(transformed[0, 0], pos.y * yratio, transformed[1, 0]); //Unity cue position
        return B;

    }

    //Method to multiply two 3x3 matrices with each other
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


    //Method to invert 3x3 matrix m
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
    #endregion

    #region Methods - List operations

    //Method to return average of all vectors in list
    public static Vector3 AverageVec(List<Vector3> vlist)
    {
        Vector3 sumvec = new Vector3(0f, 0f, 0f);
        for (int idx = 0; idx < vlist.Count; idx++)
        {
            sumvec = sumvec + vlist[idx];
        }
        return (sumvec / vlist.Count);

    }

    //Method to return a single string representation of a vector
    private string vecToStr(Vector3 val)
    {
        return ("" + val[0] + ";" + val[1] + ";" + val[2] + ";");
    }

    #endregion

    #region Methods - Data writing
    //Method to store environment game object data to csv object
    void storeData()
    {
        trialTime += Time.deltaTime;
        var cueposfront = vecToStr(cueFront.position);
        var cueposback = vecToStr(cueBack.position);
        var cuevel = vecToStr(cueFront.velocity);
        var cueballpos = vecToStr(cueballRB.position);
        var cueballvel = vecToStr(cueballRB.velocity);
        var redballpos = vecToStr(redballRB.position);
        var redballvel = vecToStr(redballRB.velocity);
        shottime = null;
        redballtime = null;
        madeshottime = null;
        if (cue_cueball)
        {
            shottime = "x";
        }
        if (cueball_redball)
        {
            redballtime = "x";
        }
        if (madeShot)
        {
            madeshottime = "x";
        }
        var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}", trialnum, trialTime.ToString("f4"), cueposfront, cueposback, cuevel, cueballpos, cueballvel, redballpos, redballvel, shottime, redballtime, madeshottime);
        csv.AppendLine(newLine);
    }

    //Method to write the start information
    void startInfo()
    {

        // This text is added only once to the file.
        if (!File.Exists(dir))
        {
            // Create a file to write to.
            string titles = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}", "TrialNumber", "Timestamp", "cueposfront", "cueposback", "cuevel", "CBpos", "CBvel", "RBpos", "RBvel", "C1", "C2", "C3", corner1.position.ToString("f4"), corner2.position.ToString("f4"), corner5.position.ToString("f4"), corner6.position.ToString("f4") + System.Environment.NewLine);
            File.WriteAllText(dir, titles);
        }
        //File.AppendAllText(dir, csv.ToString());
    }

    //Method to make file path name unique if it aready exists
    public string MakeUnique(string path)
    {
        string dir = Path.GetDirectoryName(path);
        string fileName = Path.GetFileNameWithoutExtension(path);
        string fileExt = Path.GetExtension(path);

        for (int i = 1; ; ++i)
        {
            if (!File.Exists(path))
            {
                Debug.Log(path);
                return path;
            }

            path = Path.Combine(dir, fileName + "" + i + fileExt);
        }
    }

    //Method called when application closed to write data to text file
    private void OnApplicationQuit()
    {
        File.AppendAllText(dir, csv.ToString());
    }

    #endregion

    #region Methods - PlayerPrefs

    //Method to get calibrated start position for environment
    public static Vector3 getEnvShift()
    {
        float offx = PlayerPrefs.GetFloat("ShiftEnvironemnt_x"); //cm : unity
        float offy = PlayerPrefs.GetFloat("ShiftEnvironemnt_y") + (3f * PlayerPrefs.GetFloat("cmToUnity")); //3
        float offz = PlayerPrefs.GetFloat("ShiftEnvironemnt_z");
        return new Vector3(offx, offy, offz);
    }

    //Method to get stored calibrated transformation matrix
    public static float[,] getTransformationMatrix()
    {
        //Unity Corner Positions
        float u1x = PlayerPrefs.GetFloat("Ucorner1x");
        float u1y = PlayerPrefs.GetFloat("Ucorner1y");
        float u1z = PlayerPrefs.GetFloat("Ucorner1z");
        float u2x = PlayerPrefs.GetFloat("Ucorner2x");
        float u2y = PlayerPrefs.GetFloat("Ucorner2y");
        float u2z = PlayerPrefs.GetFloat("Ucorner2z");
        float u3x = PlayerPrefs.GetFloat("Ucorner3x");
        float u3y = PlayerPrefs.GetFloat("Ucorner3y");
        float u3z = PlayerPrefs.GetFloat("Ucorner3z");
        //Optitrack Corner Positions
        float o1x = PlayerPrefs.GetFloat("Ocorner1x");
        float o1y = PlayerPrefs.GetFloat("Ocorner1y");
        float o1z = PlayerPrefs.GetFloat("Ocorner1z");
        float o2x = PlayerPrefs.GetFloat("Ocorner2x");
        float o2y = PlayerPrefs.GetFloat("Ocorner2y");
        float o2z = PlayerPrefs.GetFloat("Ocorner2z");
        float o3x = PlayerPrefs.GetFloat("Ocorner3x");
        float o3y = PlayerPrefs.GetFloat("Ocorner3y");
        float o3z = PlayerPrefs.GetFloat("Ocorner3z");


        float[,] tableOptiPoints = new float[,] { { o1x, o2x, o3x }, { o1z, o2z, o3z }, { 1f, 1f, 1f } }; //matrix of points from the Optitrack tracked markers from the pool table pockets
        float[,] tableUnityPoints = new float[,] { { u1x, u2x, u3x }, { u1z, u2z, u3z }, { 1f, 1f, 1f } }; //pocket positions in Unity
        float[,] trans = MultiplyMatrix(tableUnityPoints, inverseMat(tableOptiPoints)); //transformation matrix (Xu,Zu,1) = M * (Xo,Zo,1)

        return trans;

    }

    //Method to get reset stroed game variables to default
    public static void GameInfo()
    {

        //CALIBRATION VARIABLES TO SET
        PlayerPrefs.SetFloat("ShiftEnvironemnt_x", 1); //cm : unity
        PlayerPrefs.SetFloat("ShiftEnvironemnt_y", 1);
        PlayerPrefs.SetFloat("ShiftEnvironemnt_z", 1);
        PlayerPrefs.SetFloat("M", 1); //transformation matrix (Xu,Zu,1) = M * (Xo,Zo,1)


        //************************************************************************ */
        //Game Variables - physics, constants
        PlayerPrefs.SetFloat("angularDrag", 0.8f);
        PlayerPrefs.SetFloat("scaleForce", 200f);
        PlayerPrefs.SetFloat("numVelocitiesAverage", 5f);


        //************************************************************************ */
        //Real life and virtual environment measurements
        PlayerPrefs.SetFloat("poolTableWidth", 1f); //unity
        PlayerPrefs.SetFloat("realWidth", 68f); //cm
        PlayerPrefs.SetFloat("unityTableHeight", 0.86f);
        PlayerPrefs.SetFloat("optitrackTableHeight", 1.585f);
        PlayerPrefs.SetFloat("unityCueLength", 1.3425f);
        PlayerPrefs.SetFloat("realCueLength", 122f); //centimeters
        PlayerPrefs.SetFloat("tip_marker1", 2.5f); //centimeters
        PlayerPrefs.SetFloat("tip_marker2", 40f); //centimeters
        PlayerPrefs.SetFloat("marker3_base", 92.5f); //centimeters
        PlayerPrefs.SetFloat("yratio", 1f);
        PlayerPrefs.SetFloat("cmToUnity", 1f);
        PlayerPrefs.SetFloat("ballRadius", 2.5f); //radius of balls in cm

        //************************************************************************ */
        //Data Collection - trials, experiment type, number etc
        PlayerPrefs.SetFloat("adaptationForce_x", 5f);
        PlayerPrefs.SetFloat("adaptationForce_y", 0f);
        PlayerPrefs.SetFloat("adaptationForce_z", 0f);
        PlayerPrefs.SetFloat("waitTime", 0.5f);

        //************************************************************************ */
    }

    #endregion

}