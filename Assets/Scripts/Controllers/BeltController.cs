using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltController : MonoBehaviour
{
    public static BeltController bcInstance = null;

    [SerializeField]
    private ConveyorBelt belt;

    public void Awake() {
        if ( null == bcInstance ) {
            bcInstance = this;
            DontDestroyOnLoad( this.gameObject );
        } else {
            Destroy( this.gameObject );
        }
    }

    public void UpdateSpeed()
    {
        belt.speed = belt.speed * 1.08f;
    }

    public float GetSpeed()
    {
        return belt.speed;
    }

    public void Reset()
    {
        belt.speed = belt.startSpeed;
    }
}
