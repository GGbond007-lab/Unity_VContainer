namespace UniVCon
{
    using UnityEngine;
    public class SceneManagerBinder : MonoBehaviour {
        [SerializeField] private GameObject cube;
        public GameObject Cube => cube;
        public BoxCollider CubeCollider {
            get;
            private set;
        }
        private void Awake() {
            if (cube == null) {
                Debug.LogWarning("[SceneManagerBinder] Cube reference is not assigned.");
                return;
            }
            CubeCollider = cube.GetComponent<BoxCollider>();
            if (CubeCollider == null) Debug.LogWarning("[SceneManagerBinder] Cube does not have a BoxCollider.");
        }
    }
}
