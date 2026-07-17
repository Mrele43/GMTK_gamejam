using System;
using UnityEngine;
using Random = System.Random;
public class SharedLevelData : MonoBehaviour
{
    public static SharedLevelData Instance;
    [SerializeField] int seed = Environment.TickCount;
    public Random RandomGen { get; set; }
    public int Seed => seed;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            RandomGen = new Random(seed);
        }
        else
        {
            Destroy(this);
        }
    }
    public void resetRandom()
    {
        RandomGen = new Random(seed);
    }
}
