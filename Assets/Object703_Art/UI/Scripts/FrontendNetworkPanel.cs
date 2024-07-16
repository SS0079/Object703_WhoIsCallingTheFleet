using Object703.Core.NetCode;
using QFramework;

namespace Object703.UI
{
	public class FrontendNetworkPanelData : UIPanelData
	{
	}
	public partial class FrontendNetworkPanel : UIPanel
	{
		
		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as FrontendNetworkPanelData ?? new FrontendNetworkPanelData();
			// please add init code here
			Button_CreatGame.onClick.AddListener(Wrapper_CreatGame);
			Button_JoinGame.onClick.AddListener(Wrapper_JoinGame);
		}

		private void Wrapper_CreatGame()
		{
			var port =(ushort)InputField_Port.text.ParseToInt();
			// UIKit.HidePanel<FrontendNetworkPanel>();
			FrontendConnectionManager.Instance.StartClientServer(port);
		}

		private void Wrapper_JoinGame()
		{
			var ip = InputField_IP.text;
			var port = (ushort)InputField_Port.text.ParseToInt();
			// UIKit.HidePanel<FrontendNetworkPanel>();
			FrontendConnectionManager.Instance.ConnectToServer(ip,port);
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
			Button_CreatGame.onClick.RemoveAllListeners();
			Button_JoinGame.onClick.RemoveAllListeners();
		}
	}
}
