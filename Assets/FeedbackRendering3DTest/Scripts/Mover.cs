using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;

public class Mover : MonoBehaviour {

    [SerializeField] protected float radius = 1f, speed = 1f;

    protected float offset;
    protected Vector3 prev, velocity;

    void Start()
    {
        offset = Random.Range(0f, 1000f);
    }

    void FixedUpdate()
    {
        var t = Time.timeSinceLevelLoad * speed;
        var tmp = transform.localPosition;
        transform.localPosition = new Vector3(
            Mathf.PerlinNoise(t, 0f) - 0.5f,
            Mathf.PerlinNoise(-offset, t) - 0.5f,
            Mathf.PerlinNoise(t, offset) - 0.5f
        ) * radius;
        prev = tmp;
        velocity = transform.localPosition - prev;
    }

    void OnDrawGizmos()
    {
        var nvel = velocity.normalized;
        Gizmos.color = new Color(
            Mathf.Abs(nvel.x), 
            Mathf.Abs(nvel.y), 
            Mathf.Abs(nvel.z)
       );
        Gizmos.DrawLine(transform.position, transform.position + nvel);
    }

}
