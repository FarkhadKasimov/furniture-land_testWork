using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("magazine", "bought", "canBuy", "moneyAdded", "currentObjectsCount")]
	public class ES3UserType_MagazinesData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_MagazinesData() : base(typeof(FactorySaves.MagazinesData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (FactorySaves.MagazinesData)obj;
			
			writer.WritePropertyByRef("magazine", instance.magazine);
			writer.WriteProperty("bought", instance.bought, ES3Type_bool.Instance);
			writer.WriteProperty("canBuy", instance.canBuy, ES3Type_bool.Instance);
			writer.WriteProperty("moneyAdded", instance.moneyAdded, ES3Type_int.Instance);
			writer.WriteProperty("currentObjectsCount", instance.currentObjectsCount, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (FactorySaves.MagazinesData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "magazine":
						instance.magazine = reader.Read<MagazineObjects>();
						break;
					case "bought":
						instance.bought = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "canBuy":
						instance.canBuy = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					case "moneyAdded":
						instance.moneyAdded = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "currentObjectsCount":
						instance.currentObjectsCount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new FactorySaves.MagazinesData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_MagazinesDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MagazinesDataArray() : base(typeof(FactorySaves.MagazinesData[]), ES3UserType_MagazinesData.Instance)
		{
			Instance = this;
		}
	}
}