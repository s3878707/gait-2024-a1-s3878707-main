using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{   
    public float lifetime = 2f;
    // Start is called before the first frame update
    void Start()
    {   
        Destroy(gameObject, lifetime);

    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
        {   
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Snake"))
        {
            Destroy(gameObject);
        }
        }
}
