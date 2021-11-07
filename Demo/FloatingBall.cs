using Butjok;
using UnityEngine;

public class FloatingBall : MonoBehaviour
{
    [Command]
    public Color Color {
        get => renderer.material.color;
        set => renderer.material.color = value;
    }
    [Command]
    public float amplitude;
    [Command]
    public float speed = 5;
    [Command]
    public float offset => transform.position.x;
    [Command]
    public float offsetMultiplier = .1f;

    private Renderer renderer;
    private Vector3 originalPosition;
    private void Awake() {
        originalPosition = transform.position;
        renderer = GetComponent<Renderer>();
        SetRandomColor();
    }
    private void Update() {
        transform.position = originalPosition + Vector3.up * amplitude * Mathf.Sin(speed * (offsetMultiplier * offset + Time.time));
    }

    [Command]
    public void SetRandomColor() {
        Color = Color.HSVToRGB(Random.value, 1, 1);
    }
}