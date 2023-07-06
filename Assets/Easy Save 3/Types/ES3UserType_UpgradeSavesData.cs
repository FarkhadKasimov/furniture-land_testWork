using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("behaviour", "data", "currentUpgrade", "bought", "canBuy")]
	public class ES3UserType_UpgradeSavesData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_UpgradeSavesData() : base(typeof(UpgradeSaves.UpgradeSavesData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (UpgradeSaves.UpgradeSavesData)obj;
			
			writer.WritePropertyByRef("behaviour", instance.behaviour);
			writer.WritePropertyByRef("data", instance.data);
			writer.WriteProperty("currentUpgrade", instance.currentUpgrade, ES3Type_int.Instance);
			writer.WriteProperty("bought", instance.bought, ES3Type_bool.Instance);
			writer.WriteProperty("canBuy", instance.canBuy, ES3Type_bool.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (UpgradeSaves.UpgradeSavesData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "behaviour":
						instance.behaviour = reader.Read<WorkerBehaviour>();
						break;
					case "data":
						instance.data = reader.Read<CharacterData>();
						break;
					case "currentUpgrade":
						instance.currentUpgrade = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "bought":
						instance.bought = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "canBuy":
						instance.canBuy = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new UpgradeSaves.UpgradeSavesData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_UpgradeSavesDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_UpgradeSavesDataArray() : base(typeof(UpgradeSaves.UpgradeSavesData[]), ES3UserType_UpgradeSavesData.Instance)
		{
			Instance = this;
		}
	}
}