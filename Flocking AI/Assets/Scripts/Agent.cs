using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Flocking AI designed to herd a group from point A to B while avoiding obstacles
/// Referenced web page only: 
/// http://gamedevelopment.tutsplus.com/tutorials/the-three-simple-rules-of-flocking-behaviors-alignment-cohesion-and-separation--gamedev-3444
/// </summary>

public class Agent : MonoBehaviour {
    [SerializeField]
    private float VelocityWeight;
    [SerializeField]
    private float CohesionWeight;
    [SerializeField]
    private float SeparationWeight;
    [SerializeField]
    private float NavigationWeight;

    private uint neighbourCount;
    public Vector3 velocity { get; private set; }
    private List<Agent> neighbours;
    private List<Transform> walls;

	void Awake () {
        neighbourCount = 0;
        velocity = Vector3.up;
        neighbours = new List<Agent>();
	}
	
    void Start() {
        StartCoroutine(update_cr());
    }

	private IEnumerator update_cr () {
        if(neighbourCount != 0) {
            velocity += CalculateVelocity() * VelocityWeight + CalculateCohesion() * CohesionWeight + CalculateSeparation() * SeparationWeight;
        } else {
            velocity = Vector3.up;
        }
        yield return null;
	}

    #region Triggers
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Agent")
        {
            CalculateVelocity(other.GetComponent<Agent>());
        }
        else if(other.tag == "Wall")
        {
            walls.Add(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Agent")
        {
            neighbours.Remove(other.GetComponent<Agent>());
        }
        else if(other.tag == "Wall")
        {
            walls.Add(other.transform);
        }
    }
    #endregion

    #region Calculations
    private Vector3 CalculateVelocity(Agent other = null){
        Vector3 vel = velocity;
        if (other == null) {
            vel = Vector3.zero;
            foreach (Agent agent in neighbours) {
                vel += agent.velocity;
            }
        }
        return vel;
    }

    private Vector3 CalculateCohesion(Agent other = null){
        Vector3 vel = velocity;
        if(other == null) {
            vel = Vector3.zero;
            foreach(Agent agent in neighbours) {
                vel += agent.transform.position;
            }
        }

        vel /= neighbourCount;
        vel = vel - this.transform.position;
        vel.Normalize();
        return vel;
    }

    private Vector3 CalculateSeparation(Agent other = null) {
        Vector3 vel = velocity;
        if(other == null) {
            vel = Vector3.zero;
            foreach(Agent agent in neighbours) {
                vel += agent.transform.position - this.transform.position;
            }
        }
        
        vel /= neighbourCount;
        vel *= -1;
        vel.Normalize();
        return vel;
    }
    #endregion

}
