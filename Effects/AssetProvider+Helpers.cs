using Coffee.UIExtensions;
using UnityEngine;

namespace Projects.Scripts.Effects
{
    public partial class AssetProvider
    {
        #region Private Helper Methods
        
        private RectTransform _currentRectTransform;
        
        
        private static Vector2 GetScreenPosition()
        {
            RectTransform canvasRect = FindAnyObjectByType<UIParticle>().transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                null,
                out var localPoint
            );
        
            return localPoint;
        }
        
        private static Vector2 ScreenToLocalIn(RectTransform parentRect, Vector2 screenPos)
        {
            var canvas = parentRect.GetComponentInParent<Canvas>();
            var cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out var localPoint);
            return localPoint;
        }
        
        private static Vector2 GetPointerScreenPos()
        {
            if (Input.touchCount > 0) return Input.GetTouch(0).position;
            return Input.mousePosition;
        }
        
        private static Vector3 ScreenToWorldPointOnCanvas(Canvas canvas, Vector2 screenPos)
        {
            var canvasRect = canvas.GetComponent<RectTransform>();
            var cam = (canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPos, cam, out var worldPoint);
            return worldPoint;
        }
        
        #endregion
    }
}