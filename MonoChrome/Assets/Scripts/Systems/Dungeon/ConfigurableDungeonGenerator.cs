using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// Facade that exposes a single entry point for the available dungeon
    /// generation algorithms. Select the desired algorithm in the inspector.
    /// </summary>
    public class ConfigurableDungeonGenerator : MonoBehaviour
    {
        public enum GenerationAlgorithm
        {
            Procedural,
            Improved,
            Advanced
        }

        [Header("Generator Selection")]
        [SerializeField] private GenerationAlgorithm algorithm = GenerationAlgorithm.Procedural;
        [SerializeField] private ProceduralDungeonGenerator proceduralGenerator;
        [SerializeField] private ImprovedDungeonGenerator improvedGenerator;
        [SerializeField] private AdvancedDungeonGenerator advancedGenerator;

        /// <summary>
        /// Generate a dungeon using the selected algorithm.
        /// </summary>
        public List<DungeonNode> GenerateDungeon(int stageIndex = 0)
        {
            switch (algorithm)
            {
                case GenerationAlgorithm.Procedural:
                    if (proceduralGenerator != null)
                    {
                        return proceduralGenerator.GenerateProceduralDungeon(stageIndex);
                    }
                    break;
                case GenerationAlgorithm.Improved:
                    if (improvedGenerator != null)
                    {
                        return improvedGenerator.GenerateImprovedDungeon();
                    }
                    break;
                case GenerationAlgorithm.Advanced:
                    if (advancedGenerator != null)
                    {
                        return advancedGenerator.GenerateAdvancedDungeon();
                    }
                    break;
            }

            Debug.LogWarning("ConfigurableDungeonGenerator: Selected generator is missing.");
            return new List<DungeonNode>();
        }
    }
}
