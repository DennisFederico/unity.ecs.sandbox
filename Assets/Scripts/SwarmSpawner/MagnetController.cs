using SwarmSpawner.Components;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace SwarmSpawner {
    public class MagnetController : MonoBehaviour {
        [SerializeField] private float speed;
        [SerializeField] private Vector3 gizmoSize;

        private InputControl _inputControl;
        private Camera _mainCamera;
        private Vector3 _moveVector;
        private float _speed;
        private bool _boost;
        private World _world;
        private EntityManager _entityManager;
        private Entity _magnetAreaEntity;

        private void Awake() {
            _mainCamera = Camera.main;
            _inputControl = new InputControl();
            _inputControl.MagnetControl.Move.started += ctx => ProcessMoveInput(ctx.ReadValue<Vector3>());
            _inputControl.MagnetControl.Move.canceled += ctx => ProcessMoveInput(ctx.ReadValue<Vector3>());
            _inputControl.MagnetControl.Move.performed += ctx => ProcessMoveInput(ctx.ReadValue<Vector3>());
            _inputControl.MagnetControl.Boost.performed += ctx => Boost(ctx.ReadValue<float>());
            _inputControl.MagnetControl.Boost.canceled += ctx => Boost(ctx.ReadValue<float>());
            _inputControl.MagnetControl.Enable();
        }

        private void OnEnable() {
            _world = World.DefaultGameObjectInjectionWorld;
            _entityManager = _world.EntityManager;
        }

        private void OnDisable() {
            Cursor.lockState = CursorLockMode.None;
        }

        void OnApplicationFocus(bool hasFocus) {
            if (hasFocus) Cursor.lockState = CursorLockMode.Locked;
        }

        private void Start() {
            _speed = speed;
            //Create the area entity here since the query for the entity is not reliable
            if (_world.IsCreated) {
                var areaTagComponentType = ComponentType.ReadOnly(typeof(FloatTargetAreaTag));
                var areaDataComponentType = ComponentType.ReadOnly(typeof(AreaComponentData));
                var areaArchetype = _entityManager.CreateArchetype(areaTagComponentType, areaDataComponentType);
                _magnetAreaEntity = _entityManager.CreateEntity(areaArchetype);
                _entityManager.SetComponentData(_magnetAreaEntity, new AreaComponentData { area = gizmoSize });
                _entityManager.AddComponentData(_magnetAreaEntity, LocalTransform.FromPosition(transform.position));
            }
        }

        private void ProcessMoveInput(Vector3 value) {
            _moveVector = value;
        }
        
        private void Boost(float activated) {
            _speed = speed * (activated != 0 ? 2 : 1);
        }

        private void Update() {
            if (Vector3.zero == _moveVector) return;
            var direction = DirectionOnCameraSpace(new Vector3(_moveVector.x, 0, _moveVector.z).normalized);
            HandleRotation(direction);
            HandleMovement(direction);
            HandleElevation(_moveVector.y);
            SyncEcsMagnetPosition();
        }

        private Vector3 DirectionOnCameraSpace(Vector3 moveVector) {
            var cameraTransform = _mainCamera.transform;
            var forwardVector = cameraTransform.forward * moveVector.z;
            var rightVector = cameraTransform.right * moveVector.x;
            var relativeToCamera = forwardVector + rightVector;

            var position = transform.position;
            var translatedVector = position + relativeToCamera;
            return translatedVector - position;
        }

        private void HandleRotation(Vector3 direction) {
            //NO TWEEN - MAKE IT SNAPPY
            transform.rotation = Vector3.zero == direction ? Quaternion.identity : Quaternion.LookRotation(direction);
        }

        private void HandleMovement(Vector3 direction) {
            var moveVector = direction * (_speed * Time.deltaTime);
            transform.position += moveVector;
        }

        private void HandleElevation(float value) {
            if (value == 0f) return;
            var moveVector = Vector3.up * (_speed * value * Time.deltaTime);
            transform.position += moveVector;
        }

        private void SyncEcsMagnetPosition() {
            //Does this check impacts the performance??
            if (!_world.IsCreated || !_entityManager.Exists(_magnetAreaEntity)) {
                Debug.LogError($"Cannot sync magnet position. World is not created or magnet area entity does not exist.");
                return;
            }
            _entityManager.SetComponentData(_magnetAreaEntity, LocalTransform.FromPosition(transform.position));
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, gizmoSize);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, gizmoSize);
        }
    }
}