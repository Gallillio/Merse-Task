using UnityEngine;
using Core.Interfaces;
using Core.Services;

namespace Inventory
{
    /// <summary>
    /// Extension methods for ItemSocketInteractor
    /// </summary>
    public static class ItemSocketInteractorExtensions
    {
        /// <summary>
        /// Configures hover visual feedback on an ItemSocketInteractor
        /// </summary>
        /// <param name="socket">The socket to configure hover feedback for</param>
        /// <param name="hoverColor">Optional color to use for hover state (default: red)</param>
        /// <param name="defaultColor">Optional color to use for default state (default: white)</param>
        /// <returns>The socket with configured hover visuals</returns>
        public static ItemSocketInteractor ConfigureHoverVisual(this ItemSocketInteractor socket, Color? hoverColor = null, Color? defaultColor = null)
        {
            if (socket == null)
                return null;

            if (hoverColor.HasValue && defaultColor.HasValue)
            {
                socket.SetHoverColors(hoverColor.Value, defaultColor.Value);
            }
            else if (hoverColor.HasValue)
            {
                socket.SetHoverColors(hoverColor.Value, Color.white);
            }
            else if (defaultColor.HasValue)
            {
                socket.SetHoverColors(Color.red, defaultColor.Value);
            }

            // Enable hover visuals
            socket.SetHoverVisualEnabled(true);

            // Log using service locator if available
            var logger = ServiceLocator.Get<ILoggingService>();
            logger?.Log($"Configured hover visual on socket {socket.name}");

            return socket;
        }

        /// <summary>
        /// Configures hover visual feedback for all ItemSocketInteractors in the scene
        /// </summary>
        /// <param name="hoverColor">Optional color to use for hover state (default: red)</param>
        /// <param name="defaultColor">Optional color to use for default state (default: white)</param>
        public static void ConfigureHoverVisualForAllSockets(Color? hoverColor = null, Color? defaultColor = null)
        {
            var sockets = UnityEngine.Object.FindObjectsOfType<ItemSocketInteractor>();
            var logger = ServiceLocator.Get<ILoggingService>();

            int count = 0;
            foreach (var socket in sockets)
            {
                socket.ConfigureHoverVisual(hoverColor, defaultColor);
                count++;
            }

            logger?.Log($"Configured hover visual on {count} sockets in the scene");
        }
    }
}