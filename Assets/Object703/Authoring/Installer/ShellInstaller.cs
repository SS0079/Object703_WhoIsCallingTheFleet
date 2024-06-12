using UnityEngine;

namespace Object703.Authoring.Installer
{
    [RequireComponent(typeof(CanSelfDestructAuthoring),
        typeof(CanHitTargetAuthoring),
        typeof(CanMoveAuthoring))]
    public class ShellInstaller : MonoBehaviour
    {
    }
}