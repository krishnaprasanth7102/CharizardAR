using UnityEngine;

public class CharizardController : MonoBehaviour
{
    [SerializeField] private float speed = 1.0f;

    private FixedJoystick fixedJoystick;
    private Rigidbody rigidBody;
    private Animator animationController;
    private bool isFlying = false;

    // Name of the Bool parameter in the CharizardController Animator Controller
    private const string FLY_PARAM = "FlyParam";

    private void OnEnable()
    {
        fixedJoystick = FindFirstObjectByType<FixedJoystick>();
        rigidBody = gameObject.GetComponent<Rigidbody>();
        animationController = gameObject.GetComponent<Animator>();

        // Start in Hover (idle) state
        animationController?.SetBool(FLY_PARAM, false);
    }

    [SerializeField] private float movementBoundary = 2.0f; // max distance from image anchor in local space

    private void FixedUpdate()
    {
        if (fixedJoystick == null)
        {
            Debug.LogWarning("[CharizardController] FixedJoystick not found in scene.");
            return;
        }

        // Read joystick input
        float xVal = fixedJoystick.Horizontal;
        float yVal = fixedJoystick.Vertical;
        Vector3 input = new Vector3(xVal, 0f, yVal);

        if (input.sqrMagnitude > 0.01f)
        {
            // Normalize to keep consistent speed when moving diagonally
            Vector3 direction = input.normalized;

            // Move in local space relative to the tracked image anchor
            Vector3 delta = direction * speed * Time.fixedDeltaTime;
            Vector3 newLocalPos = transform.localPosition + delta;

            // Clamp to boundary radius
            transform.localPosition = Vector3.ClampMagnitude(newLocalPos, movementBoundary);

            // Rotate to face movement direction (yaw only)
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);

            // Switch to Fly (looping fly animation)
            if (!isFlying)
            {
                animationController?.SetBool(FLY_PARAM, true);
                isFlying = true;
            }
        }
        else
        {
            // No joystick input – stop any physics velocity
            if (rigidBody != null)
            {
                rigidBody.linearVelocity = Vector3.zero;
            }

            // Switch back to Hover (idle wing-flap animation)
            if (isFlying)
            {
                animationController?.SetBool(FLY_PARAM, false);
                isFlying = false;
            }
        }
    }
}