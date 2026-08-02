using UnityEngine;
using UnityEngine.EventSystems;

namespace VaultsOfTheElixir.Player
{
    /// <summary>
    /// On-screen joystick for WebGL/touch builds. Drag on the background
    /// image moves the handle and outputs a Horizontal value in [-1, 1],
    /// same range as Input.GetAxisRaw("Horizontal") on desktop.
    ///
    /// Setup:
    /// 1. Under your HUD Canvas, create an empty GameObject "Joystick".
    /// 2. Add a child Image "Background" (a translucent circle sprite works well).
    /// 3. Add a child Image "Handle" under Background (a smaller circle).
    /// 4. Attach this script to "Background" (it needs to receive the drag events).
    /// 5. Assign the Handle's RectTransform to the "Handle" field below.
    /// 6. Only the vertical (up/down) drag is ignored — this is a side-scroller,
    ///    so the joystick only ever outputs horizontal movement.
    ///
    /// PlayerController reads VirtualJoystick.Instance.Horizontal automatically
    /// once this component exists in the scene — no other wiring needed for movement.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField] private RectTransform background;
        [Tooltip("How far (in local UI units) the handle can move from center before clamping.")]
        [SerializeField] private float handleRange = 60f;
        [Tooltip("Dead zone as a fraction of handleRange — small drags below this produce zero output, so accidental taps don't cause drift.")]
        [SerializeField] private float deadZone = 0.15f;

        public static VirtualJoystick Instance { get; private set; }

        public float Horizontal { get; private set; }

        private Vector2 _inputVector;

        private void Awake()
        {
            // Simple singleton — only one joystick expected per scene.
            // If one already exists (e.g. persisted across scenes), remove this duplicate.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (background == null)
            {
                background = GetComponent<RectTransform>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out localPoint);

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
            _inputVector = clamped / handleRange;

            if (handle != null)
            {
                handle.anchoredPosition = clamped;
            }

            ApplyDeadZone();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _inputVector = Vector2.zero;
            Horizontal = 0f;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }

        private void ApplyDeadZone()
        {
            // Only horizontal matters for this side-scroller — vertical drag is ignored.
            Horizontal = Mathf.Abs(_inputVector.x) < deadZone ? 0f : _inputVector.x;
        }
    }
}
