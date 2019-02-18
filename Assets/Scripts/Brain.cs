using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour 
{
    public GameObject paddle;
    public GameObject ball;

    Rigidbody2D b_RigidBod;
    float y_velocity;
    float paddleMinY = 8.6f;
    float paddleMaxY = 17.6f;
    float paddlesMaxSpeed = 15;

    public float numSaved = 0;
    public float numMissed = 0;

    ANN _artificialNeuronNetwork;

	// Use this for initialization
	void Start () 
    {
        _artificialNeuronNetwork = new ANN(6, 1, 1, 4, 0.11);
        b_RigidBod = ball.GetComponent<Rigidbody2D>();
	}

    List<double> Run(double ballX, double ballY, double ballVelocX, double ballVelocY, double paddleX, double paddleY, double paddleVelocity, bool train)
    {
        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();
        inputs.Add(ballX);
        inputs.Add(ballY);
        inputs.Add(ballVelocX);
        inputs.Add(ballVelocY);
        inputs.Add(paddleX);
        inputs.Add(paddleY);
        outputs.Add(paddleVelocity);

        if (train)
        {
            return (_artificialNeuronNetwork.Train(inputs, outputs));
        }
        else
        {
            return (_artificialNeuronNetwork.CalcOutput(inputs, outputs));
        }
    }
	
	// Update is called once per frame
	void Update () 
    {
        ProcessLearning();
	}

    void ProcessLearning()
    {
        float posY = Mathf.Clamp(paddle.transform.position.y + (y_velocity * Time.deltaTime * paddlesMaxSpeed), paddleMinY, paddleMaxY);
        paddle.transform.position = new Vector3(paddle.transform.position.x, posY, paddle.transform.position.z);

        List<double> output = new List<double>();
        int layerMask = 1 << 9;
        RaycastHit2D hit = Physics2D.Raycast(ball.transform.position, b_RigidBod.velocity, 1000, layerMask);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.tag == "tops")
            {
                Vector3 reflection = Vector3.Reflect(b_RigidBod.velocity, hit.normal);
                hit = Physics2D.Raycast(hit.point, reflection, 1000, layerMask);
            }

            else if (hit.collider.gameObject.tag == "backwall")
            {
                float distanceY = (hit.point.y - paddle.transform.position.y);

                output = Run(ball.transform.position.x,
                             ball.transform.position.y,
                             b_RigidBod.velocity.x,
                             b_RigidBod.velocity.y,
                             paddle.transform.position.x,
                             paddle.transform.position.y,
                             distanceY,
                             true);

                y_velocity = (float)output[0];
            }
        }
        else
            y_velocity = 0;
    }
}
