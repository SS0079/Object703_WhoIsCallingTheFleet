using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace KittyHelpYouOut
{
	public class JsonFileHandler
	{

		// 读取文件
		public static T Read<T>(string path)
		{
			if (!File.Exists(path))
			{
				Debug.LogError("读取的文件不存在！");
				return default;
			}
			string json = File.ReadAllText(path);
			return JsonUtility.FromJson<T>(json);
		}

		public static bool TryReadSafe<T>(string path, out T result,T defaultValue)
		{
			result = defaultValue;
			if (!File.Exists(path))
			{
				Debug.LogError("读取的文件不存在！");
				return false;
			}
			try
			{
				string json = File.ReadAllText(path);
				result = JsonUtility.FromJson<T>(json);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
				return false;
			}
		}

		// 保存文件，没什么好说的
		public static void Rewrite(object obj, string path)
		{
			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}
			string json = JsonUtility.ToJson(obj, true);
			File.WriteAllText(path, json);
			Debug.Log("保存成功");
		}

	}
}