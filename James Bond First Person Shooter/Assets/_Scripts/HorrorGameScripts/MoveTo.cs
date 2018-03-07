using UnityEngine;
using System.Collections;

public class MoveTo : MonoBehaviour {

    public Transform goal;

	// Use this for initialization
	void Start () {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.destination = goal.position;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
