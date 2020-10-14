using UnityEngine;

namespace uzSurfaceMapper.Utils.Keyboard
{
    /// <summary>
    ///     Checks multi key tapping from Keyboard
    /// </summary>
    public class MultiTapper
    {
        /// <summary>
        ///     The key tap delay
        /// </summary>
        private const float keyTapDelay = .5f;

        /// <summary>
        ///     The button cooler
        /// </summary>
        private float ButtonCooler;

        /// <summary>
        ///     The button count
        /// </summary>
        private int ButtonCount;

        /// <summary>
        ///     Prevents a default instance of the <see cref="MultiTapper" /> class from being created.
        /// </summary>
        private MultiTapper()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultiTapper" /> class.
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <param name="count">The count.</param>
        public MultiTapper(KeyCode keyCode, int count = 2)
        {
            KeyCode = keyCode;
            Count = count;
        }

        /// <summary>
        ///     Gets or sets the key code.
        /// </summary>
        /// <value>
        ///     The key code.
        /// </value>
        public KeyCode KeyCode { private get; set; }

        public int Count { private get; set; }

        /// <summary>
        ///     Checks the multi tap.
        /// </summary>
        /// <returns></returns>
        public bool CheckMultiTap()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                if (ButtonCooler > 0 && ButtonCount == Count - 1)
                {
                    return true;
                }

                ButtonCooler = keyTapDelay;
                ButtonCount += 1;
            }

            if (ButtonCooler > 0)
                ButtonCooler -= 1 * Time.deltaTime;
            else
                ButtonCount = 0;

            return false;
        }
    }
}