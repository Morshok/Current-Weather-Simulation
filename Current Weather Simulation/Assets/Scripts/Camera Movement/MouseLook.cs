using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100.0f;

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;

    private bool shouldRotate;

    private void Start()
    {
        shouldRotate = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && shouldRotate == true)
        {
            shouldRotate = false;
        }
        if(Input.GetKeyDown(KeyCode.Mouse0) && shouldRotate == false)
        {
            shouldRotate = true;
        }

        if(shouldRotate)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            yRotation += mouseX;

            xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);

            transform.localRotation = Quaternion.Euler(new Vector3(xRotation, yRotation, 0.0f));
        }
    }
}