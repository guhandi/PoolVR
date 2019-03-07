using UnityEngine;
using System.Collections;

public class PocketsController : MonoBehaviour {
    public static bool redball_pocket;

	void Start() {
		
        redball_pocket = false;
    }

	void OnCollisionEnter(Collision collision) {
		
        if (collision.gameObject.name == "Redball")
        {
            redball_pocket = true;
        }

		if (collision.gameObject.name == "CueBall")
        {
            //CueBallController.madeShot = true;
		}
	}
}
