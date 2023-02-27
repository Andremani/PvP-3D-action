using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Andremani.Pvp3DAction.PlayerRelated
{
    public class PlayerVisualsManager : MonoBehaviour
    {
        [SerializeField] private List<Renderer> playerRenderers;
        [SerializeField] private Material mainPlayerMaterial;
        [SerializeField] private Material damagedPlayerMaterial;

        public IEnumerator TriggerDamagedVisuals(float duration)
        {
            foreach (var renderer in playerRenderers)
            {
                renderer.material = damagedPlayerMaterial;
            }

            yield return new WaitForSeconds(duration);

            foreach (var renderer in playerRenderers)
            {
                renderer.material = mainPlayerMaterial;
            }
        }
    }
}