using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    public float increment = .05f;
    //Script ensuring breath indicators are destroyed at the 
    //correct place and movement is smooth and in time with 
    //graph updating

	void Start () {
		
	}
	
	// Move is called by GraphR for the breath indicators 
	public void Move () {
        transform.position = new Vector2(transform.position.x - Time.deltaTime - increment, transform.position.y);
        if (transform.position.x < -13)
        {
            DestroyObject(gameObject);
        }
	}
}
