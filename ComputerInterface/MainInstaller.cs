using ComputerInterface.Interfaces;
using Zenject;

namespace AirJump.ComputerInterface
{
    class MainInstaller : Installer
    {
        public override void InstallBindings()
        {
            base.Container.Bind<IComputerModEntry>().To<AirJumpEntry>().AsSingle();
        }
    }
}
