using UnityEngine;
using System.Collections;

public class SnookerBallController : MonoBehaviour {


	void Start() {

    }

    private void Update()
    {
        //Check if red ball is hit out of table
        OutOfBounds();
    }

    //Function to check if red ball leaves the pool table
    void OutOfBounds()
    {
        if (this.transform.position.x < Experiment.xmin || this.transform.position.x > Experiment.xmax || this.transform.position.z < Experiment.zmin || this.transform.position.z > Experiment.zmax)
        {
            Experiment.outOfBounds = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "walls")
        {
            Vector3 vel = Experiment.redballRB.velocity;
            Experiment.redballRB.velocity = new Vector3(vel.x, 0, vel.z);
        }
    }

    void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.tag == "LongEdge")
        {
            float dampingz = 0.9f;
            float dampingy = 0.01f;
            Vector3 vel = Experiment.redballRB.velocity;
            //Experiment.redballRB.velocity = new Vector3(vel.x, dampingy * vel.y, -dampingz * vel.z);
            
        }
        if (col.gameObject.tag == "SideEdge")
        {
            float dampingx = 0.9f;
            float dampingy = 0.01f;
            Vector3 vel = Experiment.redballRB.velocity;
            //Experiment.redballRB.velocity = new Vector3(-dampingx * vel.x, dampingy * vel.y, vel.z);

        }

    }
}
