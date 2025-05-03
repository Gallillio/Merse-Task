using UnityEngine;
using System;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for inventory management
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Attach an item to a socket
        /// </summary>
        /// <param name="item">The item transform</param>
        /// <param name="socket">The socket transform</param>
        void AttachItem(Transform item, Transform socket);

        /// <summary>
        /// Detach an item from its socket
        /// </summary>
        /// <param name="item">The item transform</param>
        void DetachItem(Transform item);

        /// <summary>
        /// Play a sound effect for item pickup
        /// </summary>
        /// <param name="item">The item being picked up</param>
        void PlayPickupSound(Transform item);

        /// <summary>
        /// Event triggered when an item is attached to a socket
        /// </summary>
        event Action<Transform, Transform> OnItemAttached;
    }
}