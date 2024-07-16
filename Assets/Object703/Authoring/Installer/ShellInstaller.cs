using KittyHelpYouOut;

namespace Object703.Authoring.Installer
{
    [InstallComponent(typeof(CanMoveAuthoring)
    ,typeof(CanHitTargetAuthoring)
    ,typeof(CanBeHitAuthoring))]
    public class ShellInstaller : InstallerBase
    {
    }
}