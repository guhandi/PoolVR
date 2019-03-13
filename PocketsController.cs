using UnityEngine;
using System.Collections;

public class PocketsController : MonoBehaviour {

	void Start() {
		
    }

	void OnCollisionEnter(Collision collision) {
		
        if (collision.gameObject.name == "Redball")
        {
            Experiment.madeShot = true;
        }

		if (collision.gameObject.name == "CueBall")
        {
            Experiment.scratch = true;
		}
	}
}
