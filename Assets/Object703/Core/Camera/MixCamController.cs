using Unity.Cinemachine;
using CinemachineAccessories;
using KittyHelpYouOut;

namespace Object703.Core
{
    public class MixCamController : KittyMonoSingletonManual<MixCamController>
    {
        private CinemachineMixingCamera _MixCam;
        public CinemachineMixingCamera MixCam
        {
            get
            {
                _MixCam ??= this.GetComponent<CinemachineMixingCamera>();
                return _MixCam;
            }
        }

        private MixCameraSwitcher _MixSwitcher;
        public MixCameraSwitcher MixSwitcher
        {
            get
            {
                _MixSwitcher ??= this.GetComponent<MixCameraSwitcher>();
                return _MixSwitcher;
            }
        }
                 
        
    }
}