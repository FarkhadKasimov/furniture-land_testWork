using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("shop", "bought", "canBuy", "moneyAdded")]
	public class ES3UserType_FactoryShopsData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_FactoryShopsData() : base(typeof(FactorySaves.FactoryShopsData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (FactorySaves.FactoryShopsData)obj;
			
			writer.WritePropertyByRef("shop", instance.shop);
			writer.WriteProperty("bought", instance.bought, ES3Type_bool.Instance);
			writer.WriteProperty("canBuy", instance.canBuy, ES3Type_bool.Instance);
			writer.WriteProperty("moneyAdded", instance.moneyAdded, ES3Type_int.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (FactorySaves.FactoryShopsData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "shop":
						instance.shop = reader.Read<FactoryShop>();
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
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new FactorySaves.FactoryShopsData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_FactoryShopsDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_FactoryShopsDataArray() : base(typeof(FactorySaves.FactoryShopsData[]), ES3UserType_FactoryShopsData.Instance)
		{
			Instance = this;
		}
	}
}