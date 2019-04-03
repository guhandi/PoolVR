using UnityEngine;
using System.Collections;

public class SnookerBallController : MonoBehaviour {


	void Start() {

    }

    private void Update()
    {

    }

    void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.name == "Wall1")
        {
            float dampingz = 0.9f;
            float dampingy = 0.5f;
            Vector3 vel = Experiment.redballRB.velocity;
            Experiment.redballRB.velocity = new Vector3(vel.x, dampingy * vel.y, -dampingz * vel.z);
            
        }

    }
}
