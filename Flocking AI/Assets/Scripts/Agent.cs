using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {
    [SerializeField]
    private float VelocityWeight;
    [SerializeField]
    private float CohesionWeight;
    [SerializeField]
    private float SeparationWeight;

    private uint neighbourCount;
    public Vector3 velocity { get; private set; }
    private List<Agent> neighbours;

	void Awake () {
        neighbourCount = 0;
        velocity = Vector3.up;
        neighbours = new List<Agent>();
	}
	

	void Update () {
        if(neighbourCount != 0) {
            velocity += CalculateVelocity() * VelocityWeight + CalculateCohesion() * CohesionWeight + CalculateSeparation() * SeparationWeight;
        } else {
            velocity = Vector3.up;
        }
	}

    #region Triggers
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Agent")
        {
            CalculateVelocity(other.GetComponent<Agent>());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Agent")
        {
            neighbours.Remove(other.GetComponent<Agent>());
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
        } else {
            velocity += other.velocity;
            neighbours.Add(other);
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
        } else {
            vel += other.transform.position;
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
        } else {
            vel += other.transform.position - this.transform.position;
        }
        
        vel /= neighbourCount;
        vel *= -1;
        vel.Normalize();
        return vel;
    }
    #endregion

}
