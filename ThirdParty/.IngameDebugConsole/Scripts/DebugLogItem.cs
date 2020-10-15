#if UNITY_EDITOR
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#endif

// A UI element to show information about a debug entry
namespace IngameDebugConsole
{
    public class DebugLogItem : MonoBehaviour, IPointerClickHandler
    {
        // Index of the entry in the list of entries

        // Objects related to the collapsed count of the debug entry
        [SerializeField] private GameObject logCountParent;

        [SerializeField] private Text logCountText;

        // Debug entry to show with this log item
        private DebugLogEntry logEntry;

        [SerializeField] private Text logText;

        [SerializeField] private Image logTypeImage;

        private DebugLogRecycledListView manager;

        // Cached components

        [field: SerializeField] public RectTransform Transform { get; }

        [field: SerializeField] public Image Image { get; }

        public int Index { get; private set; }

        // This log item is clicked, show the debug entry's stack trace
        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_EDITOR
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                var regex = Regex.Match(logEntry.stackTrace, @"\(at .*\.cs:[0-9]+\)$", RegexOptions.Multiline);
                if (regex.Success)
                {
                    var line = logEntry.stackTrace.Substring(regex.Index + 4, regex.Length - 5);
                    var lineSeparator = line.IndexOf(':');
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(line.Substring(0, lineSeparator));
                    if (script != null)
                        AssetDatabase.OpenAsset(script, int.Parse(line.Substring(lineSeparator + 1)));
                }
            }
            else
            {
                manager.OnLogItemClicked(this);
            }
#else
			manager.OnLogItemClicked( this );
#endif
        }

        public void Initialize(DebugLogRecycledListView manager)
        {
            this.manager = manager;
        }

        public void SetContent(DebugLogEntry logEntry, int entryIndex, bool isExpanded)
        {
            this.logEntry = logEntry;
            Index = entryIndex;

            var size = Transform.sizeDelta;
            if (isExpanded)
            {
                logText.horizontalOverflow = HorizontalWrapMode.Wrap;
                size.y = manager.SelectedItemHeight;
            }
            else
            {
                logText.horizontalOverflow = HorizontalWrapMode.Overflow;
                size.y = manager.ItemHeight;
            }

            Transform.sizeDelta = size;

            logText.text = isExpanded ? logEntry.ToString() : logEntry.logString;
            logTypeImage.sprite = logEntry.logTypeSpriteRepresentation;
        }

        // Show the collapsed count of the debug entry
        public void ShowCount()
        {
            logCountText.text = logEntry.count.ToString();
            logCountParent.SetActive(true);
        }

        // Hide the collapsed count of the debug entry
        public void HideCount()
        {
            logCountParent.SetActive(false);
        }

        public float CalculateExpandedHeight(string content)
        {
            var text = logText.text;
            var wrapMode = logText.horizontalOverflow;

            logText.text = content;
            logText.horizontalOverflow = HorizontalWrapMode.Wrap;

            var result = logText.preferredHeight;

            logText.text = text;
            logText.horizontalOverflow = wrapMode;

            return Mathf.Max(manager.ItemHeight, result);
        }

        // Return a string containing complete information about the debug entry
        public override string ToString()
        {
            return logEntry.ToString();
        }
    }
}