using UnityEngine;

namespace Object703.Authoring.Installer
{
    [RequireComponent(typeof(CanBeDestructAuthoring),
        typeof(CanHitTargetAuthoring),
        typeof(CanMoveAuthoring))]
    public class ShellInstaller : MonoBehaviour
    {
    }
}