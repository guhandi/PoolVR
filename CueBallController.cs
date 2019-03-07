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
        if (CollectData.experiment == 3 && CollectData.cue_cueball)
        {
            CollectData.cueballRB.AddForce(CollectData.adaptationForce);
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
            CollectData.cue_cueball = true;
        }

        //If cue ball hits red ball
        if (col.gameObject.name == "RedBall")
        {
            CollectData.cueball_redball = true;

            //For second experimental condition, the visiual feedback is removed and the balls disappear from the environmnet after contact is made
            if (CollectData.experiment == 2)
            {
                StartCoroutine(Dissappear());
            }

        }

    }

    //Function to make cue ball and red ball disappear from the scene in the environment
    IEnumerator Dissappear()
    {
        //Wait 0.5s after cue ball collides with redball before objects become invisible
        yield return new WaitForSeconds(CollectData.waitTime);
        CollectData.cueMesh.enabled = false;
        CollectData.redballMesh.enabled = false;
    }

    //Function to check if cue ball leaves the pool table
    void OutOfBounds()
    {
        float tableLimX = -(CollectData.poolTableLength + 0.1f); // Table x range = [-2 min, 0 max]
        float tableLimZ = CollectData.poolTableWidth + 0.1f; //Table z range = [-0.5 min, 0.5 max]
        if (transform.position.x < tableLimX || transform.position.x > 0.1 || Mathf.Abs(transform.position.z) > tableLimZ)
        {
            CollectData.outOfBounds = true;
        }

    }

}
