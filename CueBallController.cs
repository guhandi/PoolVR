using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//A class that tracks the cue ball to perfom necessary game functions and to keep track of various game events
public class CueBallController : MonoBehaviour
{
    public AudioClip myClip;
    public AudioSource mySource;
    public static bool onAdapt;
    // Use this for initialization
    void Start()
    {
        mySource.clip = myClip;
        onAdapt = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //automatic cueball movement
        //moveBall();

        //Check if cue ball is hit out of table
        OutOfBounds();

        //Adaptation
        if (Experiment.cueball_redball || this.transform.position.z > Experiment.redballRB.position.z)
        {
            onAdapt = false;
        }
        if (Experiment.experiment == 2 && Experiment.cue_cueball)
        {
            if (onAdapt)
            {
                Experiment.cueballRB.AddForce(Experiment.adaptationForce);
            }
            
        }
    }

    //No cue control
    void moveBall()
    {
        float scale = PlayerPrefs.GetFloat("scalingRatio");
        Vector3 vel = new Vector3(0.05f, 0, 1.5f);
        Vector3 impulseForce = scale * vel;
        if (Input.GetMouseButtonDown(0))
        {
            Experiment.cueballRB.AddForce(impulseForce, ForceMode.Impulse);
        }
    }

    //Method that is called at any game object collision with cue ball
    void OnCollisionEnter(Collision col)
    {
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        //If cue ball doesn't collide with any rigidbody do nothing
        if (!rb)
        {
            return;
        }

        //If cue ball collides with cue stick
        if (col.gameObject.name == "Cue")
        {
            Experiment.cue_cueball = true;
            //Experiment.cueballRB.AddForce(PlayerPrefs.GetFloat("scalingRatio") * OptitrackRigidBody.avgVelocity, ForceMode.Impulse);

        }

        //If cue ball hits red ball
        if (col.gameObject.name == "RedBall")
        {
            Experiment.cueball_redball = true;
            mySource.PlayOneShot(myClip);

            //For third experimental condition, the visiual feedback is removed and the balls disappear from the environmnet after contact is made
            if (Experiment.experiment == 3)
            {
                StartCoroutine(Dissappear());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "walls")
        {
            Vector3 vel = Experiment.cueballRB.velocity;
            Experiment.cueballRB.velocity = new Vector3(vel.x, 0, vel.z);
            onAdapt = false;
        }
    }

    //Function to make cue ball and red ball disappear from the scene in the environment
    IEnumerator Dissappear()
    {
        //Wait 0.5s after cue ball collides with redball before objects become invisible
        yield return new WaitForSeconds(Experiment.waitTime);
        Experiment.cueMesh.enabled = false;
        Experiment.redballMesh.enabled = false;
    }


    //Function to check if cue ball leaves the pool table
    void OutOfBounds()
    {
        if (this.transform.position.x < Experiment.xmin || this.transform.position.x > Experiment.xmax || this.transform.position.z < Experiment.zmin || this.transform.position.z > Experiment.zmax)
        {
            Experiment.outOfBounds = true;
        }
    }
}
