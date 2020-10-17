using System;
using UnityEngine;

// Container for a simple debug entry
namespace IngameDebugConsole
{
    public class DebugLogEntry : IEquatable<DebugLogEntry>
    {
        private const int HASH_NOT_CALCULATED = -623218;

        private string completeLog;

        // Collapsed count
        public int count;

        private int hashValue = HASH_NOT_CALCULATED;

        public string logString;

        // Sprite to show with this entry
        public Sprite logTypeSpriteRepresentation;
        public string stackTrace;

        public DebugLogEntry(string logString, string stackTrace, Sprite sprite)
        {
            this.logString = logString;
            this.stackTrace = stackTrace;

            logTypeSpriteRepresentation = sprite;

            count = 1;
        }

        // Check if two entries have the same origin
        public bool Equals(DebugLogEntry other)
        {
            return logString == other.logString && stackTrace == other.stackTrace;
        }

        // Return a string containing complete information about this debug entry
        public override string ToString()
        {
            if (completeLog == null)
                completeLog = string.Concat(logString, "\n", stackTrace);

            return completeLog;
        }

        // Credit: https://stackoverflow.com/a/19250516/2373034
        public override int GetHashCode()
        {
            if (hashValue == HASH_NOT_CALCULATED)
                unchecked
                {
                    hashValue = 17;
                    hashValue = hashValue * 23 + logString == null ? 0 : logString.GetHashCode();
                    hashValue = hashValue * 23 + stackTrace == null ? 0 : stackTrace.GetHashCode();
                }

            return hashValue;
        }
    }
}