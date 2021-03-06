﻿using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerWalkScript : MonoBehaviour
    {
        public float Speed = 1;
        public Quaternion OriginalRotationValue; // declare this as a Quaternion

        private const float RotationResetSpeed = 1.0f;

        protected virtual void Start()
        {
            OriginalRotationValue = transform.rotation; // save the initial rotation
        }

        protected virtual void Update()
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            {
                transform.Rotate(Vector3.down, Speed*2);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, OriginalRotationValue,
                    Time.time*RotationResetSpeed);
                transform.rotation = Quaternion.Euler(transform.parent.root.transform.rotation.eulerAngles.x,
                    transform.parent.root.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            }
        }
    }
}