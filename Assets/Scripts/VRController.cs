using UnityEngine;
using Valve.VR;

namespace Assets.Scripts
{
    /// <summary>
    /// Класс для управления передвижением игрока с помощью тачпада.
    /// https://youtube.com/watch?v=QREKO1sf8b8
    /// </summary>
    public class VRController : MonoBehaviour
    {
        /// <summary>
        /// Сила гравитации для игрока.
        /// </summary>
        public float Gravity = 30.0f;

        /// <summary>
        /// Насколько значение силы нажатия на тачпад будет увеличивать скорость игрока [-1..1].
        /// </summary>
        public float Sensitivity = 0.1f;

        /// <summary>
        /// Максимальная скорость игрока.
        /// </summary>
        public float MaxSpeed = 1.0f;

        /// <summary>
        /// Направление движения игрока, задаваемое касанием тачпада.
        /// </summary>
        public SteamVR_Action_Vector2 MoveValue = null;

        /// <summary>
        /// Скорость передвижения игрока.
        /// </summary>
        private float speed = 0.0f;

        /// <summary>
        /// Объект управления игроком.
        /// </summary>
        private CharacterController characterController = null;

        /// <summary>
        /// Объект VR-камеры игрока.
        /// </summary>
        private Transform cameraRig = null;

        /// <summary>
        /// Объект головы игрока в VR.
        /// </summary>
        private Transform head = null;

        /// <summary>
        /// Метод, вызываемый до Start.
        /// </summary>
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        void Start()
        {
            cameraRig = SteamVR_Render.Top().origin;
            head = SteamVR_Render.Top().head;
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            HandleHead();
            HandleHeight();
            CalculateMovement();
        }

        /// <summary>
        /// Обработка поворота головы игроком.
        /// </summary>
        private void HandleHead()
        {
            // Сохранить текущее положение камеры игрока
            var oldPosition = cameraRig.position;
            var oldRotation = cameraRig.rotation;

            // Поворачиваем объект игрока туда, куда повёрнута камеры
            transform.eulerAngles = new Vector3(0.0f, head.rotation.eulerAngles.y, 0.0f);

            // Восстанавливаем положение камеры, т.к. она тоже двигается с объектом игрока
            cameraRig.position = oldPosition;
            cameraRig.rotation = oldRotation;
        }

        /// <summary>
        /// Изменить высоту объекта игрока в соответствии с высотой расположения шлема.
        /// </summary>
        private void HandleHeight()
        {
            // Задать положение головы в локальном пространстве по высоте от 1 до 2 метров
            var headHeight = Mathf.Clamp(head.position.y, 1, 2);
            characterController.height = headHeight;

            // Задать значение центра объекта игрока, чтобы ось повторота игрока была в центре
            var newCenter = Vector3.zero;
            newCenter.y = characterController.height / 2;
            newCenter.y += characterController.skinWidth; // Если не добавить, будет небольшая тряска

            // Движение капсулы в локальном пространстве
            newCenter.x = head.localPosition.x;
            newCenter.z = head.localPosition.z;

            // Поворот капсулы объекта игрока
            newCenter = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * newCenter;

            // Применить
            characterController.center = newCenter;
        }

        /// <summary>
        /// Приведение объекта игрока в движение.
        /// </summary>
        private void CalculateMovement()
        {
            // Определяем направление движения
            var orientation = CalculateOrientation();
            var moveDirection = Vector3.zero;

            // Если не двигаемся, обнуляем скорость (можно замедлять, чтобы убрать инерцию)
            // Если касание тачпада во все стороны нулевая
            if (MoveValue.axis.magnitude == 0)
                speed = 0;

            // Увеличиваем скорость игрока по направлению касания тачпада
            speed += MoveValue.axis.magnitude * Sensitivity;
            speed = Mathf.Clamp(speed, -MaxSpeed, MaxSpeed);    // Проверяем, что скорость не выходит за границы

            // Указываем направление движения с заданной скоростью
            // Идём в ту сторону, в которую смотрим
            moveDirection += orientation * (speed * Vector3.forward);

            // Применение гравитации, пояснения см. по ссылке
            // https://docs.unity3d.com/ScriptReference/CharacterController.Move.html
            moveDirection.y -= Gravity * Time.deltaTime;

            // Увеличить скорость передвижения
            characterController.Move(moveDirection * Time.deltaTime);
        }

        /// <summary>
        /// Определить направление движения игрока по месту нажатия на тачпад.
        /// </summary>
        /// <returns>Направление движения игрока.</returns>
        private Quaternion CalculateOrientation()
        {
            // Значение в радианах направления касания тачпада или джойстика
            var rotation = Mathf.Atan2(MoveValue.axis.x, MoveValue.axis.y);
            rotation *= Mathf.Rad2Deg;  // Конвертируем в градусы

            // Поскольку объект игрока вращается вместе с камерой, поворачиваем сам объект
            var orientationEuler = new Vector3(0, transform.eulerAngles.y + rotation, 0);

            return Quaternion.Euler(orientationEuler);
        }
    }
}
