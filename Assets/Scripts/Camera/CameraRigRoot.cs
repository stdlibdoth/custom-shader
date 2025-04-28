using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRigRoot : MonoBehaviour
{
    [SerializeField] private Camera m_camera;
    [SerializeField] private Transform m_rootTransform;
    [SerializeField] private Transform m_cameraTransform;

    [Space]
    [Header("Zoom")]
    [SerializeField] private float m_zoomSpeed;
    [SerializeField] private float m_initialZoom;
    [SerializeField] private Vector2 m_zoomRange;

    [Header("Pitch")]
    [SerializeField] private Vector2 m_pitchRange;
    [SerializeField] private float m_initialPitch;
    [SerializeField] private float m_pitchSpeed;

    [Header("Rotate")]
    [SerializeField] private float rotateSpeed;


    [Header("Edge Scroll")]
    [SerializeField] private bool m_isEdgeScroll;
    [SerializeField] private Vector2Int m_scrollEdgeWidth;
    [SerializeField] private float m_edgeScrollSpeed;


    [Header("Pan")]
    [SerializeField] private LayerMask m_panFloorMask;


    private InputSystem_Actions m_inputActions;

    private int m_rotateState;
    private int m_panState;
    private int m_pitchState;

    private Vector2Int m_screenScrollRangeX;
    private Vector2Int m_screenScrollRangeY;
    private bool m_isEdge;
    private Vector2 m_inputPosition;

    private Vector2 m_panStartPosition;


    private void Awake()
    {
        Application.targetFrameRate = 60;
        //Cursor.lockState = CursorLockMode.Confined;
        m_inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        m_inputActions.CameraControlMap.InputMoveAction.performed += InputMoveAction_performed;
        m_inputActions.CameraControlMap.InputPositionAction.performed += InputPositionAction_performed;
        m_inputActions.CameraControlMap.InputScrollAction.performed += InputScrollAction_performed;
        m_inputActions.CameraControlMap.InputPressAction2.performed += InputPressAction2_performed;
        m_inputActions.CameraControlMap.InputPressAction2.canceled += InputMoveAction2_canceled;
        m_inputActions.CameraControlMap.InputPressAction3.performed += InputPressAction3_performed;
        m_inputActions.CameraControlMap.InputPressAction3.canceled += InputMoveAction3_canceled;
        m_inputActions.CameraControlMap.Enable();

        m_cameraTransform.localPosition = new Vector3(0, m_initialZoom, 0);
        m_rootTransform.eulerAngles = new Vector3(m_initialPitch +270, 0, 0);
        m_rotateState = 0;
        m_panState = 0;
        m_pitchState = 0;
        m_screenScrollRangeX = new Vector2Int(m_scrollEdgeWidth.x, Screen.width - m_scrollEdgeWidth.x);
        m_screenScrollRangeY = new Vector2Int(m_scrollEdgeWidth.y, Screen.height - m_scrollEdgeWidth.y);
    }


    void Update()
    {
        if(m_isEdge)
        {
            Vector3 delta = Vector3.zero;
            if (m_inputPosition.x <= m_screenScrollRangeX.x)
            {
                delta += -m_rootTransform.right;
            }
            if (m_inputPosition.x >= m_screenScrollRangeX.y)
            {
                delta += m_rootTransform.right;
            }
            if (m_inputPosition.y <= m_screenScrollRangeY.x)
            {
                delta += -new Vector3(m_rootTransform.forward.x, 0, m_rootTransform.forward.z);
            }
            if (m_inputPosition.y >= m_screenScrollRangeY.y)
            {
                delta += new Vector3(m_rootTransform.forward.x, 0, m_rootTransform.forward.z);
            }
            delta = delta.normalized * Time.deltaTime * m_edgeScrollSpeed;
            m_rootTransform.position += delta;
        }
    }


    private void UpdatePichRotate(Vector2 inputDelta)
    {
        float dx = math.clamp(inputDelta.x, -10, 10);
        float dy = math.clamp(inputDelta.y, -10, 10);
        Vector3 euler = m_rootTransform.eulerAngles;
        euler += new Vector3(-m_pitchState * m_pitchSpeed * Time.deltaTime * dy, m_rotateState * rotateSpeed * Time.deltaTime * dx, 0);
        float pitch = math.clamp(euler.x, m_pitchRange.x + 270, m_pitchRange.y + 270);
        euler = new Vector3(pitch, euler.y, euler.z);
        m_rootTransform.rotation = Quaternion.Euler(euler);
    }


    private void UpdateZoom(Vector2 inputDelta)
    {
        float delta = -math.sign(inputDelta.y) * m_zoomSpeed;
        float pos = math.clamp(m_cameraTransform.localPosition.y + delta, m_zoomRange.x, m_zoomRange.y);
        m_cameraTransform.localPosition = new Vector3(0, pos, 0);
    }

    private void UpdateEdgeScroll()
    {
        if ((m_rotateState | m_pitchState | m_panState) == 1 || !m_isEdgeScroll)
            return;
        if (m_inputPosition.x < 0 || m_inputPosition.x > Screen.width || m_inputPosition.y < 0 || m_inputPosition.y > Screen.height)
            m_isEdge = false;
        else
        {
            m_isEdge = m_inputPosition.x <= m_screenScrollRangeX.x || m_inputPosition.x >= m_screenScrollRangeX.y
                || m_inputPosition.y <= m_screenScrollRangeY.x || m_inputPosition.y >= m_screenScrollRangeY.y;
        }
    }

    private void UpdatePan()
    {
        if(m_panState == 1 && m_rotateState == 0 && m_pitchState == 0)
        {
            var ray = m_camera.ScreenPointToRay(m_inputPosition);
            if(Physics.Raycast(ray, out RaycastHit hit, 1000, m_panFloorMask))
            {
                var currentPanPosition = new Vector2(hit.point.x, hit.point.z);
                var delta = m_panStartPosition -currentPanPosition;
                m_rootTransform.position += new Vector3(delta.x, 0, delta.y);
            }
        }
    }

    private void InputMoveAction_performed(InputAction.CallbackContext obj)
    {
        var moveDelta = obj.ReadValue<Vector2>();
        UpdatePichRotate(moveDelta);
    }
    private void InputPositionAction_performed(InputAction.CallbackContext obj)
    {
        m_inputPosition = obj.ReadValue<Vector2>();
        UpdateEdgeScroll();
        UpdatePan();
    }
    private void InputScrollAction_performed(InputAction.CallbackContext obj)
    {
        var val = obj.ReadValue<Vector2>();
        UpdateZoom(val);
    }

    private void InputPressAction2_performed(InputAction.CallbackContext obj)
    {
        m_rotateState = 1;
        m_pitchState = 1;
        m_panState = 0;
    }
    private void InputMoveAction2_canceled(InputAction.CallbackContext obj)
    {
        m_rotateState = 0;
        m_pitchState = 0;
        m_panState = 0;
    }

    private void InputPressAction3_performed(InputAction.CallbackContext obj)
    {
        var ray = m_camera.ScreenPointToRay(m_inputPosition);
        if(Physics.Raycast(ray, out RaycastHit hit, 1000, m_panFloorMask))
        {
            m_panState = 1;
            m_panStartPosition = new Vector2(hit.point.x, hit.point.z);
        }
    }
    private void InputMoveAction3_canceled(InputAction.CallbackContext obj)
    {
        m_panState = 0;
    }
}
