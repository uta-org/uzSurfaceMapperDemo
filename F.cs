using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace uzSurfaceMapper.Extensions
{
    public static class F
    {
        public static void TogglePlayerComponents(bool enabled)
        {
            TogglePlayerComponents(enabled, out _);
        }

        public static void TogglePlayerComponents(bool enabled, out GameObject player)
        {
            player = Object.FindObjectOfType<CharacterController>()?.gameObject;

            if (player == null)
                throw new NullReferenceException("No player found!");

            foreach (var child in player.GetComponents<MonoBehaviour>())
                child.enabled = enabled;

            player.GetComponent<CharacterController>().enabled = enabled;
            player.GetComponent<Rigidbody>().isKinematic = !enabled;
        }

        public static void Clear<T>(this ConcurrentBag<T> bag)
        {
            while (!bag.IsEmpty)
            {
                bag.TryTake(out _);
            }
        }

        public static void AddRange<T>(this ConcurrentBag<T> bag, List<T> list)
        {
            foreach (var item in list)
            {
                bag.Add(item);
            }
        }
    }
}