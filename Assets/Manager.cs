using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    public float timeframe;
    public int populationSize;//creates population size
    public GameObject prefab;//holds bot prefab

    public int[] layers = new int[3] { 5, 3, 2 };//initializing network to the right size

    [Range(0.0001f, 1f)] public float MutationChance = 0.01f;

    [Range(0f, 1f)] public float MutationStrength = 0.5f;

    [Range(0.1f, 10f)] public float Gamespeed = 1f;

    //public List<Bot> Bots;
    public List<NeuralNetwork> networks;
    private List<Bot> cars;

    public Transform startPosition;

    public Checkpoint lastCheckpoint;

    void Start()// Start is called before the first frame update
    {
        // if population is odd, add 1 to make it even
        if (populationSize % 2 != 0)
        {
            populationSize++;
        }

        InitNetworks();
        InvokeRepeating("CreateBots", 0.1f, timeframe);//repeating function
    }

    void Update() {
        ColorBots();
    }

    public void InitNetworks()
    {
        networks = new List<NeuralNetwork>();
        for (int i = 0; i < populationSize; i++)
        {
            NeuralNetwork net = new NeuralNetwork(layers);
            net.Load("Assets/ModelSave.txt");//on start load the network save
            networks.Add(net);
        }
    }    

    public void CreateBots()
    {
        Time.timeScale = Gamespeed;//sets gamespeed, which will increase to speed up training
        if (cars != null)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                GameObject.Destroy(cars[i].gameObject);//if there are Prefabs in the scene this will get rid of them
            }
            SortNetworks();//this sorts networks and mutates them
        }

        cars = new List<Bot>();
        for (int i = 0; i < populationSize; i++)
        {
            Bot car = (Instantiate(prefab, startPosition.position, new Quaternion(0, 0, 1, 0))).GetComponent<Bot>();//create botes
            car.network = networks[i];//deploys network to each learner
            car.previousCheckpoint = lastCheckpoint;//sets the last checkpoint as the previous checkpoint
            cars.Add(car);
        }
    }

    // Color the bots based on their position, range from green to red
    public void ColorBots()
    {
        if (cars != null)
        {
            float lowestFitness = float.MaxValue;
            float highestFitness = float.MinValue;

            for (int i = 0; i < cars.Count; i++)
            {
                // Ignore cars that have already Collided
                /*if (cars[i].collided) {
                    continue;
                }*/
                if (cars[i].getCurrentFitness() > highestFitness)
                    highestFitness = cars[i].getCurrentFitness();
                if (cars[i].getCurrentFitness() < lowestFitness)
                    lowestFitness = cars[i].getCurrentFitness();
            }
            Debug.Log("Highest " + highestFitness + " Lowest " + lowestFitness);

            for (int i = 0; i < cars.Count; i++)
            {
                // Ignore cars that have already Collided
                if (cars[i].collided) {
                    cars[i].GetComponent<Renderer>().material.color = new Color(0,0,255);
                    continue;
                }
                float tempFitness = cars[i].getCurrentFitness();
                float upper = tempFitness - lowestFitness;
                float lower = highestFitness - lowestFitness;
                float percent = upper / lower;

                // if percent is nan, set it to 1
                if (float.IsNaN(percent)) {
                    percent = 1;
                }
                float g = percent * 255;
                float r = 255 - g;
                cars[i].GetComponent<Renderer>().material.color = new Color(r / 255, g / 255, 0);
            }
        }
    }

    public void SortNetworks()
    {
        for (int i = 0; i < populationSize; i++)
        {
            cars[i].UpdateFitness();//gets bots to set their corresponding networks fitness
        }
        networks.Sort();
        networks[populationSize - 1].Save("Assets/ModelSave.txt");//saves networks weights and biases to file, to preserve network performance
        for (int i = 0; i < populationSize / 2; i++)
        {
            networks[i] = networks[i + populationSize / 2].copy(new NeuralNetwork(layers));
            networks[i].Mutate((int)(1/MutationChance), MutationStrength);
        }
    }
}