using UnityEngine;
using MonoChrome.Events;
using MonoChrome.Core;

namespace MonoChrome.Systems.Combat
{
    /// <summary>
    /// Minimal legacy bridge for old CombatManager references.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance => FindFirstObjectByType<CombatManager>();

        /// <summary>
        /// Legacy initialization entry point. Current system triggers combat via events.
        /// </summary>
        public void InitializeCombat()
        {
            // Actual combat is handled by CombatSystem through CombatEvents.
            // This method is kept for backward compatibility.
        }
    }
}
