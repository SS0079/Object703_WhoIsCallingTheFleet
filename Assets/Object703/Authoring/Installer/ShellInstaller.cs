using KittyHelpYouOut;
using UnityEngine;

namespace Object703.Authoring.Installer
{
    [Installer(typeof(CanMoveAuthoring)
    ,typeof(CanHitTargetAuthoring)
    ,typeof(CanBeHitAuthoring))]
    public class ShellInstaller : InstallerBase
    {
    }
}