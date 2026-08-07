using UnityEngine;

public class CharacterNetworkSync : MonoBehaviour
{
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private float _interpolationSpeed = 10.0f;

    public void OnServerTransformReceived(Vector3 newPos, Quaternion newRot)
    {
        _targetPosition = newPos;
        _targetRotation = newRot;
    }

    private void Update()
    {
        // Smooth dead-reckoning movement interpolation
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _interpolationSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * _interpolationSpeed);
    }
}
