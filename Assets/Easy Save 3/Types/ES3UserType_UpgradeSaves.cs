using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("processingSpeedUpgrades", "playerChoppingSpeedUpgrades", "playerCapacityUpgrades", "playerSawScaleUpgrade", "playerSawFuel", "upgradeSaves")]
	public class ES3UserType_UpgradeSaves : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_UpgradeSaves() : base(typeof(UpgradeSaves)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (UpgradeSaves)obj;
			
			writer.WriteProperty("processingSpeedUpgrades", instance.processingSpeedUpgrades, ES3Type_int.Instance);
			writer.WriteProperty("playerChoppingSpeedUpgrades", instance.playerChoppingSpeedUpgrades, ES3Type_int.Instance);
			writer.WriteProperty("playerCapacityUpgrades", instance.playerCapacityUpgrades, ES3Type_int.Instance);
			writer.WriteProperty("playerSawScaleUpgrade", instance.playerSawScaleUpgrade, ES3Type_int.Instance);
			writer.WriteProperty("playerSawFuel", instance.playerSawFuel, ES3Type_int.Instance);
			writer.WriteProperty("upgradeSaves", instance.upgradeSaves);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (UpgradeSaves)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "processingSpeedUpgrades":
						instance.processingSpeedUpgrades = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "playerChoppingSpeedUpgrades":
						instance.playerChoppingSpeedUpgrades = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "playerCapacityUpgrades":
						instance.playerCapacityUpgrades = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "playerSawScaleUpgrade":
						instance.playerSawScaleUpgrade = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "playerSawFuel":
						instance.playerSawFuel = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "upgradeSaves":
						instance.upgradeSaves = reader.Read<System.Collections.Generic.List<UpgradeSaves.UpgradeSavesData>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_UpgradeSavesArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_UpgradeSavesArray() : base(typeof(UpgradeSaves[]), ES3UserType_UpgradeSaves.Instance)
		{
			Instance = this;
		}
	}
}