using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bot : MonoBehaviour
{
    public float speed;//Speed Multiplier
    public float rotation;//Rotation multiplier
    public LayerMask raycastMask;//Mask for the sensors
    public LayerMask raycastCoinMask;//Mask for the sensors

    private float[] input = new float[10];//input to the neural network, first 5 are ranges to walls, second 5 are ranges to coins
    public NeuralNetwork network;

    public int numCheckpoints;//Checkpoint number on the course
    public int coinsCollected;//Coins collected
    public bool collided;//To tell if the car has crashed

    public Checkpoint previousCheckpoint;

    void FixedUpdate()//FixedUpdate is called at a constant interval
    {
        if (!collided)//if the car has not collided with the wall, it uses the neural network to get an output
        {
            for (int i = 0; i < 5; i++)//draws five debug rays as inputs
            {
                Vector3 newVector = Quaternion.AngleAxis(i * 45 - 90, new Vector3(0, 1, 0)) * transform.right;//calculating angle of raycast
                RaycastHit hit;
                Ray Ray = new Ray(transform.position, newVector);

                if (Physics.Raycast(Ray, out hit, 10, raycastMask))
                {
                    input[i] = (10 - hit.distance) / 10;//return distance, 1 being close
                }
                else if(Physics.Raycast(Ray, out hit, 10, raycastCoinMask))
                {
                    input[i + 5] = (10 - hit.distance) / 10;//return distance, 1 being close
                }
                else
                {
                    input[i] = 0;//if nothing is detected, will return 0 to network
                    input[i+5] = 0;//if nothing is detected, will return 0 to network
                }
            }

            float[] output = network.FeedForward(input);//Call to network to feedforward
        
            transform.Rotate(0, output[0] * rotation, 0, Space.World);//controls the cars movement
            transform.position += this.transform.right * output[1] * speed;//controls the cars turning
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("CheckPoint"))//check if the car passes a gate
        {
            Checkpoint checkpoint = collision.collider.gameObject.GetComponent<Checkpoint>();//get the checkpoint script
            if (previousCheckpoint.nextCheckpoints.Contains(checkpoint))//check if the checkpoint is the next checkpoint
            {
                numCheckpoints++;//increase the checkpoint number
                previousCheckpoint = checkpoint;//set the previous checkpoint to the current checkpoint
            } else  {
                numCheckpoints--;
            }
        }
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Coin")) {
            coinsCollected++;//if the car collides with a coin, it increments the coins collected
        }
        else if(collision.collider.gameObject.layer != LayerMask.NameToLayer("Learner"))
        {
            collided = true;//stop operation if car has collided
        }
    }


    public int getCurrentFitness()//returns the current value of the car
    {
        if (numCheckpoints > 0) {
            return numCheckpoints;
        } else {
            return 0;
        }
    }

    public void UpdateFitness()
    {
        network.fitness = getCurrentFitness();//updates fitness of network for sorting
    }
}
