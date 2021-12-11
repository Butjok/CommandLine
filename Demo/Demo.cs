using System.Collections.Generic;
using Butjok;
using UnityEngine;

public class Demo : MonoBehaviour
{
    public FloatingBall prefab;
    public List<FloatingBall> balls = new List<FloatingBall>();
    
    private void Awake() {
        StartDemo();
    }
    
    [Command]
    public  void StartDemo(int cubesNumber = 100, float distance = 1) {
        foreach (var ball in balls)
            Destroy(ball.gameObject);
        balls.Clear();
        var overallDistance = (cubesNumber - 1) * distance;
        for (var i = 0; i < cubesNumber; i++) {
            var ball = Instantiate(prefab, (-overallDistance / 2 + i * distance) * Vector3.right, Quaternion.identity, transform);
            balls.Add(ball);
            ball.gameObject.SetActive(true);
        }
    }

    [Command]
    public static string PersistentDataPath => Application.persistentDataPath;
    [Command]
    public static float Random(float low, float high) => UnityEngine.Random.Range(low,high);
}