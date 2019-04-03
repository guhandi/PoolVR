using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//A class that tracks the cue ball to perfom necessary game functions and to keep track of various game events
public class CueBallController : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Check if cue ball is hit out of table
        OutOfBounds();

        //Adaptation
        if (Experiment.experiment == 2 && Experiment.cue_cueball)
        {
            Experiment.cueballRB.AddForce(Experiment.adaptationForce);
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
        }

        //If cue ball hits red ball
        if (col.gameObject.name == "RedBall")
        {
            Experiment.cueball_redball = true;

            //For second experimental condition, the visiual feedback is removed and the balls disappear from the environmnet after contact is made
            if (Experiment.experiment == 3)
            {
                StartCoroutine(Dissappear());
            }
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
