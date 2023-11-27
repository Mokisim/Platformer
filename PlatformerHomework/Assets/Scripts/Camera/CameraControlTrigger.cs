using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;

[RequireComponent(typeof(Collider2D))]
public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObjects CustomInspectorObjects;

    private Collider2D _collider2D;

    private void Start()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (CustomInspectorObjects.PanCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(CustomInspectorObjects.PanDistance, CustomInspectorObjects.PanTime, CustomInspectorObjects.PanDirection, false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 exitDirection = (collision.transform.position - _collider2D.bounds.center).normalized;

            if(CustomInspectorObjects.SwapCameras && CustomInspectorObjects.CameraOnLeft != null && CustomInspectorObjects.CameraOnRight != null)
            {
                CameraManager.instance.SwapCamera(CustomInspectorObjects.CameraOnLeft, CustomInspectorObjects.CameraOnRight, exitDirection);
            }

            if (CustomInspectorObjects.PanCameraOnContact)
            {
                CameraManager.instance.PanCameraOnContact(CustomInspectorObjects.PanDistance, CustomInspectorObjects.PanTime, CustomInspectorObjects.PanDirection, true);
            }
        }
    }
}

[System.Serializable]
public class CustomInspectorObjects
{
    public bool SwapCameras = false;
    public bool PanCameraOnContact = false;

    [HideInInspector] public CinemachineVirtualCamera CameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera CameraOnRight;

    [HideInInspector] public PanDirection PanDirection;
    [HideInInspector] public float PanDistance = 3f;
    [HideInInspector] public float PanTime = 0.35f;
}

public enum PanDirection
{
    Up,
    Down, 
    Left, 
    Right
}

[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor: Editor
{
    CameraControlTrigger cameraControlTrigger;

    private void OnEnable()
    {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (cameraControlTrigger.CustomInspectorObjects.SwapCameras)
        {
            cameraControlTrigger.CustomInspectorObjects.CameraOnLeft = EditorGUILayout.ObjectField("Camera on Left", cameraControlTrigger.CustomInspectorObjects.CameraOnLeft,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

            cameraControlTrigger.CustomInspectorObjects.CameraOnRight = EditorGUILayout.ObjectField("Camera on Right", cameraControlTrigger.CustomInspectorObjects.CameraOnRight,
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        }

        if (cameraControlTrigger.CustomInspectorObjects.PanCameraOnContact)
        {
            cameraControlTrigger.CustomInspectorObjects.PanDirection = (PanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
                cameraControlTrigger.CustomInspectorObjects.PanDirection);

            cameraControlTrigger.CustomInspectorObjects.PanDistance = EditorGUILayout.FloatField("Pan Distance", cameraControlTrigger.CustomInspectorObjects.PanDistance);
            cameraControlTrigger.CustomInspectorObjects.PanTime = EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.CustomInspectorObjects.PanTime);
        }

        if(GUI.changed)
        {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}
