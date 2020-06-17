using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Класс указателя на объекты на сцене.
    /// </summary>
    public class Pointer : MonoBehaviour
    {
        /// <summary>
        /// Длина указателя по умолчанию.
        /// </summary>
        public float DefaultLength = 5.0f;

        /// <summary>
        /// Объект точки на конце указателя.
        /// </summary>
        public GameObject Dot;

        /// <summary>
        /// Модуль ввода для обработки событий взаимодействия с окружением.
        /// </summary>
        public VRInputModule InputModule;

        /// <summary>
        /// Линия указателя.
        /// </summary>
        private LineRenderer LineRenderer = null;

        /// <summary>
        /// Настройка указаеля.
        /// </summary>
        private void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();
        }

        /// <summary>
        /// Обновление указателя.
        /// </summary>
        private void Update()
        {
            UpdateLine();
        }

        /// <summary>
        /// Обновить линию указателя.
        /// </summary>
        private void UpdateLine()
        {
            // Получаем данные из обработчика событий
            var eventData = InputModule.GetEventData();

            // Задать длину указателя по умолчанию или по расстоянию до объекта
            var targetLength = eventData.pointerCurrentRaycast.distance == 0 ? DefaultLength : eventData.pointerCurrentRaycast.distance;

            // Отлавливаем попадание лазера на объект
            var hit = CreateRaycast(targetLength);

            // Координата конца лазера по умолчанию
            var endPosition = transform.position + transform.forward * targetLength;

            if (hit.collider != null)       // Координата конца лазера на объекте, если он есть
                endPosition = hit.point;

            // Размещаем точку на конце лазера
            Dot.transform.position = endPosition;

            // Отрисовываем линию лазера
            LineRenderer.SetPosition(0, transform.position);
            LineRenderer.SetPosition(1, endPosition);
        }

        /// <summary>
        /// Определить попадание луча в объект.
        /// </summary>
        /// <param name="length">Длина луча лазера.</param>
        /// <returns>Информация о попадании линии указателя в какой-либо объект.</returns>
        private RaycastHit CreateRaycast(float length)
        {
            var ray = new Ray(transform.position, transform.forward);

            Physics.Raycast(ray, out RaycastHit hit, length);

            return hit;
        }
    }
}
