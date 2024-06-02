using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace Object703.UI
{
	// Generate Id:eb722c50-c34c-4ca0-bf9e-de1be499e2b5
	public partial class FrontendNetworkPanel
	{
		public const string Name = "FrontendNetworkPanel";
		
		[SerializeField]
		public UnityEngine.UI.Button Button_CreatGame;
		[SerializeField]
		public UnityEngine.UI.Button Button_JoinGame;
		[SerializeField]
		public TMPro.TMP_InputField InputField_IP;
		[SerializeField]
		public TMPro.TMP_InputField InputField_Port;
		
		private FrontendNetworkPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Button_CreatGame = null;
			Button_JoinGame = null;
			InputField_IP = null;
			InputField_Port = null;
			
			mData = null;
		}
		
		public FrontendNetworkPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		FrontendNetworkPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new FrontendNetworkPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
