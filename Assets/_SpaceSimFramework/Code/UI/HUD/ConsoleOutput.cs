using TMPro;
using UnityEngine;

namespace SpaceSimFramework.Code.UI.HUD
{
    public class ConsoleOutput : Singleton<ConsoleOutput>
    {
        public GameObject holder;
        private TMP_Text[] _textFields;
        private int _maxNumberOfMessages;
        private int _numberOfMessages = 0;

        private void Awake()
        {
            _textFields = holder.GetComponentsInChildren<TMP_Text>();
            _maxNumberOfMessages = _textFields.Length;
        }

        /// <summary>
        /// Posts a message to the console output which is then displayed on the UI.
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="color">Message color</param>
        public static void PostMessage(string message, Color color)
        {
            if (Instance == null)
            {
                Debug.LogWarning("Trying to write to ConsoleOutput, but instance is null");
                return;
            }

            if (Instance._textFields.Length == 0)
                return;

            if (Instance._numberOfMessages < Instance._maxNumberOfMessages)
            {
                Instance._numberOfMessages++;
                for (int i = Instance._numberOfMessages - 1; i > 0; i--)
                {
                    Instance._textFields[i].text = Instance._textFields[i - 1].text;
                    Instance._textFields[i].color = Instance._textFields[i - 1].color;
                }
                Instance._textFields[0].text = message;
                Instance._textFields[0].color = color;
            }
            else
            {
                // Circular buffer imitation
                for (int i = Instance._maxNumberOfMessages - 1; i > 0; i--)
                {
                    Instance._textFields[i].text = Instance._textFields[i - 1].text;
                    Instance._textFields[i].color = Instance._textFields[i - 1].color;
                }
                Instance._textFields[0].text = message;
                Instance._textFields[0].color = color;
            }

            Instance.UpdateTextFieldAlpha();
        }

        /// <summary>
        /// Posts a message to the console output which is then displayed on the UI.
        /// Message color is white by default.
        /// </summary>
        /// <param name="message">Message text</param>
        public static void PostMessage(string message)
        {
            PostMessage(message, Color.white);
        }

        /// <summary>
        /// Updates the text field alpha channel so that the oldest message fades the most.
        /// </summary>
        private void UpdateTextFieldAlpha()
        {
            for (int i = 0; i < _maxNumberOfMessages; i++)
            {
                var textColor = _textFields[i].color;

                textColor.a = (_maxNumberOfMessages - i + 1.0f) / (float) _maxNumberOfMessages;

                _textFields[i].color = textColor;
            }
        }

        public static void ClearConsole()
        {
            for (int i = Instance._numberOfMessages - 1; i > 0; i--)
            {
                Instance._textFields[i].text = "";
            }
        }
    }
}
