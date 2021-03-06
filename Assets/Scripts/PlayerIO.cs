﻿using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerIO : MonoBehaviour
    {

        public static PlayerIO CurrentPlayerIo;
        public float MaxInteractDistance = 8;
        public int SelectedInventory;
        public bool ResetCamera;

        protected virtual void Start()
        {
            CurrentPlayerIo = this;
        }

        protected virtual void Update()
        {
            if (GameObject.FindWithTag("FPSController").transform.position.y < -20)
            {
                Debug.Log("Test");
                GameObject.FindWithTag("FPSController").transform.position =
                    new Vector3(GameObject.FindWithTag("FPSController").transform.position.x, 60,
                        GameObject.FindWithTag("FPSController").transform.position.z);
            }
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                var ray = GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, MaxInteractDistance))
                {
                    var chunk = hit.transform.GetComponent<Chunk>();
                    if (chunk == null)
                    {
                        return;
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        var worldPosition = hit.point;
                        worldPosition -= hit.normal/4;
                        chunk.SetBlockIdGlobal(Position3.From(worldPosition), Block.Air.Id);
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        var worldPosition = hit.point;
                        if (SelectedInventory != 0)
                        {
                            worldPosition += hit.normal/4;

                            chunk.SetBlockIdGlobal(Position3.From(worldPosition), SelectedInventory);
                        }
                    }

                    if (Input.GetMouseButtonDown(2))
                    {
                        var worldPosition = hit.point;
                        worldPosition -= hit.normal/4;
                        SelectedInventory = chunk.GetBlockIdGlobal(Position3.From(worldPosition));
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (ResetCamera == false)
                {
                    transform.localPosition -= Vector3.forward*2;
                    GameObject.FindWithTag("MinecraftPlayer").layer = 1;
                    ResetCamera = true;
                }
                else
                {
                    transform.position = transform.parent.root.transform.localPosition + new Vector3(0f, 0.7f, 0.06f);
                    transform.rotation = transform.parent.root.transform.rotation;
                    GameObject.FindWithTag("MinecraftPlayer").layer = 8;
                    ResetCamera = false;
                }
            }

            if (Input.GetKey(KeyCode.Escape) && Input.GetKey(KeyCode.F1))
            {
                Application.Quit();
            }
        }
    }
}