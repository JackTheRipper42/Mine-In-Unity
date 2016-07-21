using UnityEngine;

namespace Assets.Scripts
{
    public class Test : MonoBehaviour
    {
        private World _world;

        protected virtual void Start()
        {
            _world = FindObjectOfType<World>();
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _world.Save();
            }
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                _world.Load();
            }
        }
    }
}
