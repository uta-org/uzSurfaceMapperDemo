using System;
using System.Linq;
using uzSurfaceMapper.Core.Attrs.CodeAnalysis;
using uzSurfaceMapper.Extensions;
using uzSurfaceMapper.Utils.Keyboard;
using UnityEngine;
using UnityEngine.Core;

namespace uzSurfaceMapper.Utils.Controllers
{
    /// <summary>
    ///     No Clip Controller
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    [RequireComponent(typeof(Camera))]
    //[ExecuteInEditMode]
    public class NoClipController : MonoSingleton<NoClipController>
    {
        public enum RotationAxes
        {
            MouseXAndY = 0,
            MouseX = 1,
            MouseY = 2
        }

        public RotationAxes axes = RotationAxes.MouseXAndY;

        /// <summary>
        ///     The default cam
        /// </summary>
        private Camera defaultCam;

        /// <summary>
        ///     The fly speed
        /// </summary>
        public float flySpeed = 0.5f;

        /// <summary>
        ///     The acceleration amount
        /// </summary>
        public float accelerationAmount = 3f;

        /// <summary>
        ///     The acceleration ratio
        /// </summary>
        public float accelerationRatio = 1f;

        /// <summary>
        ///     The slow down ratio
        /// </summary>
        public float slowDownRatio = 0.5f;

        /// <summary>
        ///     The move multiplier
        /// </summary>
        public float moveMultiplier = 2f;

        /// <summary>
        ///     The shift
        /// </summary>
        private bool shift;

        /// <summary>
        ///     The control
        /// </summary>
        private bool ctrl;

        /// <summary>
        ///     The alt
        /// </summary>
        private bool alt;

        /// <summary>
        ///     The quick move triggered
        /// </summary>
        private bool quickMoveTriggered;

        /// <summary>
        ///     The last alt
        /// </summary>
        private bool lastAlt;

        /// <summary>
        ///     The move quick
        /// </summary>
        private bool moveQuick;

        /// <summary>
        ///     The jump tapper
        /// </summary>
        private MultiTapper jumpTapper;

        /// <summary>
        ///     The move tapper
        /// </summary>
        private MultiTapper moveTapper;

        /// <summary>
        /// The maximum x
        /// </summary>
        public float maximumX = 360F;

        /// <summary>
        /// The maximum y
        /// </summary>
        public float maximumY = 60F;

        /// <summary>
        /// The minimum x
        /// </summary>
        public float minimumX = -360F;

        /// <summary>
        /// The minimum y
        /// </summary>
        public float minimumY = -60F;

        /// <summary>
        ///     The original position
        /// </summary>
        private Vector3 originalPosition;

        /// <summary>
        /// The original rotation
        /// </summary>
        private Quaternion originalRotation;

        /// <summary>
        /// The original name
        /// </summary>
        private string originalName;

        /// <summary>
        /// The original transform
        /// </summary>
        private Transform originalTransform;

        /// <summary>
        /// The rotation x
        /// </summary>
        private float rotationX;

        /// <summary>
        /// The rotation y
        /// </summary>
        private float rotationY;

        /// <summary>
        /// The sensitivity x
        /// </summary>
        public float sensitivityX = 2F;

        /// <summary>
        /// The sensitivity y
        /// </summary>
        public float sensitivityY = 2F;

        private float polar, elevation;

        /// <summary>
        /// Gets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [instance enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [instance enabled]; otherwise, <c>false</c>.
        /// </value>
        public static bool InstanceEnabled => Instance.IsEnabled;

        /// <summary>
        /// Gets the instance position.
        /// </summary>
        /// <value>
        /// The instance position.
        /// </value>
        public static Vector3 InstancePosition => Instance.transform.position;

        /// <summary>
        /// Gets the instace euler.
        /// </summary>
        /// <value>
        /// The instace euler.
        /// </value>
        public static Vector3 InstanceEuler => Instance.transform.eulerAngles;

        private static Transform playerTransform;

        public bool keepYAxis = true;

        // TODO: Add this to the axis manager (also to the pdf)
        public KeyCode enableKey = KeyCode.Space;

        public KeyCode upKey = KeyCode.LeftShift;

        public KeyCode downKey = KeyCode.Space;

        public KeyCode quickKey = KeyCode.W;

        /// <summary>
        /// Called when [enable].
        /// </summary>
        /// <exception cref="System.Exception">You can't have more than two NoClipController scripts assigned to each camera.</exception>
        private void OnEnable()
        {
            if (Camera.main == null)
                throw new NullReferenceException("'Camera.main' is null!");

            defaultCam = Camera.main;

            // Check right NoClipController assignment
            if (Application.isEditor && !Application.isPlaying)
            {
                var cameras = FindObjectsOfType<Camera>();

                if (cameras.Length > 1 && cameras.Select(c => c.gameObject)
                        .All(g => g.GetComponent<NoClipController>() != null))
                    throw new Exception(
                        "You can't have more than two NoClipController scripts assigned to each camera.");
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        private void Start()
        {
            //if (Application.isEditor && !Application.isPlaying)
            //    return;

            jumpTapper = new MultiTapper(enableKey);
            moveTapper = new MultiTapper(quickKey);
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        private void Update()
        {
            //if (Application.isEditor && !Application.isPlaying)
            //    return;

            if (jumpTapper.CheckMultiTap())
                SwitchCamera();

            if (!IsEnabled)
                return;

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                shift = true;
                flySpeed *= accelerationRatio;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            {
                shift = false;
                flySpeed /= accelerationRatio;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                ctrl = true;
                flySpeed *= slowDownRatio;
            }

            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
            {
                ctrl = false;
                flySpeed /= slowDownRatio;
            }

            // Begin alt check

            alt = Input.GetKey(KeyCode.LeftAlt);

            if (lastAlt != alt)
                flySpeed = alt ? flySpeed / moveMultiplier : flySpeed * moveMultiplier;

            lastAlt = alt;

            // End alt check

            // Begin w check

            if (moveTapper.CheckMultiTap())
                moveQuick = true;

            if (moveQuick)
            {
                if (Input.GetKeyDown(quickKey))
                    flySpeed *= moveMultiplier;

                if (Input.GetKeyUp(quickKey))
                {
                    flySpeed /= moveMultiplier;
                    moveQuick = false;
                }
            }

            // End w check

            // Movement part

            if (Input.GetAxis("Vertical") != 0)
            {
                //transform.Translate(flySpeed * Input.GetAxis("Vertical") * Vector3.forward);

                var mov = flySpeed * Input.GetAxis("Vertical") * transform.forward;
                if (keepYAxis) mov.y = 0;
                transform.position += mov;
            }

            if (Input.GetAxis("Horizontal") != 0)
            {
                //transform.Translate(flySpeed * Input.GetAxis("Horizontal") * Vector3.right);

                var mov = flySpeed * Input.GetAxis("Horizontal") * transform.right;
                if (keepYAxis) mov.y = 0;
                transform.position += mov;
            }

            if (Input.GetKey(upKey))
                transform.Translate(flySpeed * 0.5f * defaultCam.transform.up);
            else if (Input.GetKey(downKey))
                transform.Translate(flySpeed * 0.5f * -defaultCam.transform.up);

            if (axes == RotationAxes.MouseXAndY)
            {
                // Read the mouse input axis
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

                rotationX = ClampAngle(rotationX, minimumX, maximumX);
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                var xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                var yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);

                transform.localRotation = xQuaternion * yQuaternion;
            }
            else if (axes == RotationAxes.MouseX)
            {
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationX = ClampAngle(rotationX, minimumX, maximumX);

                var xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                transform.localRotation = originalRotation * xQuaternion;
            }
            else
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = ClampAngle(rotationY, minimumY, maximumY);

                var yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
                transform.localRotation = originalRotation * yQuaternion;
            }

            // Teleport player to camera

            //if (Input.GetKeyDown(KeyCode.M))
            //    playerObject.transform.position = transform.position; //Moves the player to the flycam's position. Make sure not to just move the player's camera.
        }

        /// <summary>
        ///     Switches the camera.
        /// </summary>
        [WIP]
        private void SwitchCamera()
        {
            bool lastState = IsEnabled;

            IsEnabled = !IsEnabled;
            F.TogglePlayerComponents(lastState, out var playerObj);

            if (originalTransform == null)
                originalTransform = playerObj.transform;

            if (playerTransform == null)
                playerTransform = playerObj.transform.childCount == 0
                    ? throw new Exception("Expecting child")
                    : playerObj.transform.GetChild(0);

            if (!lastState) // means it is currently disabled. code will enable the flycam. you can NOT use 'enabled' as boolean's name.
            {
                // @TODO: Disable the player object from it's culling mask (if it has renderer) (WIP)

                originalPosition = playerTransform.localPosition;
                originalRotation = playerTransform.localRotation;
                var fwd = playerTransform.forward;

                playerTransform.parent = null;
                originalName = playerTransform.name;

                playerTransform.name = "FreeCam";
                playerTransform.rotation = Quaternion.LookRotation(fwd);
            }
            else // if it is not disabled, it must be enabled. the function will disable the freefly camera this time.
            {
                originalTransform.position = playerTransform.position;
                originalTransform.rotation = playerTransform.rotation;

                playerTransform.parent = originalTransform;

                playerTransform.localPosition = new Vector3(0, .8f, 0); // originalPosition;
                playerTransform.localRotation = originalRotation;
                playerTransform.name = originalName;

                originalTransform.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        /// <summary>
        ///     Clamps the angle.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;

            return Mathf.Clamp(angle, min, max);
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(5, 5, 200, 50), $"Fly Speed: x{flySpeed}\nAcceleration ratio: x{accelerationRatio}");
        }
    }
}