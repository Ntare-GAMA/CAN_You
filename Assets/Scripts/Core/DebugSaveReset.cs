using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    public class DebugSaveReset : MonoBehaviour
    {
        private void Update()
{
    Debug.Log("[DebugSaveReset] Update is running");
    if (Input.GetKeyDown(KeyCode.F9))
    {
        SaveManager.Instance.ResetSave();
        Debug.Log("[DebugSaveReset] Save data wiped — F9 pressed.");
    }
}
    }
}