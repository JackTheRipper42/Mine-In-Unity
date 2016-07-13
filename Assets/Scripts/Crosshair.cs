using UnityEngine;

namespace Assets.Scripts
{
    public class Crosshair : MonoBehaviour
    {
        public Texture2D CrosshairImage;

        protected virtual void OnGUI()
        {
            float xMin = (Screen.width/2) - (CrosshairImage.width/2);
            float yMin = (Screen.height/2) - (CrosshairImage.height/2);
            GUI.DrawTexture(new Rect(xMin, yMin, CrosshairImage.width, CrosshairImage.height), CrosshairImage);
        }
    }
}
