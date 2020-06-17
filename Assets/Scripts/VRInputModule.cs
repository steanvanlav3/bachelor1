using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

namespace Assets.Scripts
{
    /// <summary>
    /// Класс обработки событий взаимодействия с окружением.
    /// </summary>
    public class VRInputModule : BaseInputModule
    {
        /// <summary>
        /// Камера на в начале луча на геймпаде.
        /// </summary>
        public Camera Camera;

        /// <summary>
        /// Источник событий - геймпад.
        /// </summary>
        public SteamVR_Input_Sources TargetSource;

        /// <summary>
        /// Действие на геймпаде, которое будем отслеживать.
        /// </summary>
        public SteamVR_Action_Boolean ClickAction;

        /// <summary>
        /// Текущий объект, на который направлен луч.
        /// </summary>
        private GameObject currentObject = null;

        /// <summary>
        /// Объект данных, передаваемый вместе с событием.
        /// </summary>
        private PointerEventData eventData = null;

        /// <summary>
        /// Инициализация используемых данных.
        /// Переобределяем Awake класса BaseInputModule.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            eventData = new PointerEventData(eventSystem);
        }

        /// <summary>
        /// Получить объект данных события.
        /// </summary>
        /// <returns>Объект данных события.</returns>
        public PointerEventData GetEventData()
        {
            return eventData;
        }

        /// <summary>
        /// Обработка события. Этот метод ведёт себя как Update.
        /// </summary>
        public override void Process()
        {
            // Обновляем данные события
            eventData.Reset();

            // Позиция луча - в середине того, что видит камера
            eventData.position = new Vector2(Camera.pixelWidth / 2, Camera.pixelHeight / 2);

            // Определяем объект, на который наведён луч
            eventSystem.RaycastAll(eventData, m_RaycastResultCache);
            eventData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            currentObject = eventData.pointerCurrentRaycast.gameObject;

            // Очищаем кеш Raycast
            m_RaycastResultCache.Clear();

            // Обработка события наведения луча
            HandlePointerExitAndEnter(eventData, currentObject);

            // Обработка события нажатия кнопки
            if (ClickAction.GetStateDown(TargetSource))
                ProcessPress(eventData);

            // Обработка события отпускания кнопки
            if (ClickAction.GetStateUp(TargetSource))
                ProcessRelease(eventData);
        }

        /// <summary>
        /// Обработка события нажатия на отслеживаемую кнопку на геймпаде.
        /// </summary>
        /// <param name="data">Объект данных события.</param>
        private void ProcessPress(PointerEventData data)
        {
            // Задаём данные 
            data.pointerPressRaycast = data.pointerCurrentRaycast;

            // Вызываем событие нажатия кнопки
            var newPointerPress = ExecuteEvents.ExecuteHierarchy(currentObject, data, ExecuteEvents.pointerDownHandler);

            // Нажатие на кнопку - не всегда полный клик
            if (newPointerPress == null)
                newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);

            data.pressPosition = data.position;
            data.pointerPress = newPointerPress;
            data.rawPointerPress = currentObject;
        }

        /// <summary>
        /// Обработка события отпускания отслеживаемой кнопки на геймпаде.
        /// </summary>
        /// <param name="data">Объект данных события.</param>
        private void ProcessRelease(PointerEventData data)
        {
            // Вызываем событие поднимания кнопки
            ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

            var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObject);

            if (data.pointerPress == pointerUpHandler)
                ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);

            // Очищаем данные
            eventSystem.SetSelectedGameObject(null);

            data.pressPosition = Vector2.zero;
            data.pointerPress = null;
            data.rawPointerPress = null;
        }
    }
}
