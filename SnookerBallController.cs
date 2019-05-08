using UnityEngine;
using System.Collections;

public class SnookerBallController : MonoBehaviour {

    private Vector3 cueballVelocity;


	void Start() {
        //cueballVelocity = Experiment.cueballRB.velocity;
    }

    private void FixedUpdate()
    {
        cueballVelocity = Experiment.cueballRB.velocity;
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


    void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.tag == "cueball")
        {
            //Initial
            Vector3 xaxis = new Vector3(1, 0, 0);
            float theta = Vector3.Angle(cueballVelocity, xaxis) * Mathf.Deg2Rad;
            float Px = cueballVelocity.magnitude * Mathf.Cos(theta);
            float Py = cueballVelocity.magnitude * Mathf.Sin(theta);


            Vector3 contactPoint = col.GetContact(0).point;
            Vector3 direction = this.transform.position - contactPoint;
            
            float alpha = Vector3.Angle(direction, xaxis) * Mathf.Deg2Rad;
            float beta = alpha - Mathf.PI / 2;
            //float beta = Mathf.Atan(Mathf.Tan(theta) - Mathf.Tan(alpha));

            float a1 = Mathf.Cos(alpha); float a2 = Mathf.Sin(alpha); float b1 = Mathf.Cos(beta);  float b2 = Mathf.Sin(beta);
            float D = (b2 * a1) - (a2 * b1); 
            float Dx = (b2 * Px) - (Py * b1);
            float Dy = (Py * a1) - (a2 * Px);

            float X = Dx / D;
            float Y = Dy / D;

            Vector3 rbVel = X* (new Vector3(a1, 0, a2));
            Vector3 cbVel = Y * (new Vector3(b1, 0, b2));

            Experiment.redballRB.velocity = rbVel;
            Experiment.cueballRB.velocity = cbVel;


            float thetad = theta / Mathf.Deg2Rad; float alphad = alpha / Mathf.Deg2Rad; float betad = beta / Mathf.Deg2Rad;

            Debug.Log("init V  :" + cueballVelocity.ToString("f4"));
            Debug.Log(" X  : " + X.ToString("f4"));
            Debug.Log(" Y  : " + Y.ToString("f4"));
            Debug.Log("theta      :  " + thetad.ToString("f4"));
            Debug.Log("Alpha      :  " + alphad.ToString("f4"));
            Debug.Log("Beta      :  " + betad.ToString("f4"));
        }

    }
}
