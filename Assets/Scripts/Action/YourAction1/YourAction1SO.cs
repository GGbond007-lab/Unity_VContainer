using UnityEngine;

[CreateAssetMenu(fileName = "SceneSO", menuName = "Action System/YourAction1 Config")]
public class YourAction1SO : ScriptableObject
{
    [SerializeField] private GameObject cubePrefab;

    public GameObject CubePrefab => cubePrefab;
}
