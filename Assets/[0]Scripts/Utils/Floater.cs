using System;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class Floater : MonoBehaviour {
    
    public float amplitude = 0.5f;
    public float frequency = 1f;
    public Direction direction = Direction.Z;
    public bool rotate = false;
    [ShowIf(nameof(rotate))] [SerializeField] private float degreesPerSecond = 0;
    
    float tempPos = 0;

    public bool Animate;

    void Start ()
    {
        switch (direction)
        {
            case Direction.X:
                tempPos = transform.localPosition.x;
                break;
            case Direction.Y:
                tempPos = transform.localPosition.y;
                break;
            case Direction.Z:
                tempPos = transform.localPosition.z;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    void Update () {

        if (Animate)
        {
            if (rotate)
            {
                transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);
            }
            
            var pos = tempPos + Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
            var position = transform.localPosition;
            
            switch (direction)
            {
                case Direction.X:
                    position.x = pos;
                    break;
                case Direction.Y:
                    position.y = pos;
                    break;
                case Direction.Z:
                    position.z = pos;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            transform.localPosition = position;
        }
    }
    
    public enum Direction
    {
        X,
        Y,
        Z
    }
}