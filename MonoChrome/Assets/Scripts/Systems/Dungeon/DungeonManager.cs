using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// Minimal legacy bridge for old DungeonManager references.
    /// </summary>
    public class DungeonManager : MonoBehaviour
    {
        public static DungeonManager Instance => FindFirstObjectByType<DungeonManager>();

        public void GenerateNewDungeon(int stageIndex)
        {
            var controller = FindFirstObjectByType<DungeonController>();
            controller?.GenerateNewDungeon(stageIndex);
        }

        public void MoveToNode(int nodeIndex)
        {
            var controller = FindFirstObjectByType<DungeonController>();
            controller?.MoveToNode(nodeIndex);
        }
    }
}
