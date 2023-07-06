using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("factoryObjects", "warehouseObjects", "magazineObjects", "factoryShops")]
	public class ES3UserType_FactorySaves : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_FactorySaves() : base(typeof(FactorySaves)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (FactorySaves)obj;
			
			writer.WritePrivateField("factoryObjects", instance);
			writer.WritePrivateField("warehouseObjects", instance);
			writer.WritePrivateField("magazineObjects", instance);
			writer.WriteProperty("factoryShops", instance.factoryShops, ES3UserType_FactoryShopsDataArray.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (FactorySaves)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "factoryObjects":
					reader.SetPrivateField("factoryObjects", reader.Read<FactorySaves.FactoryObjectData[]>(), instance);
					break;
					case "warehouseObjects":
					reader.SetPrivateField("warehouseObjects", reader.Read<FactorySaves.WarehouseData[]>(), instance);
					break;
					case "magazineObjects":
					reader.SetPrivateField("magazineObjects", reader.Read<FactorySaves.MagazinesData[]>(), instance);
					break;
					case "factoryShops":
						instance.factoryShops = reader.Read<FactorySaves.FactoryShopsData[]>(ES3UserType_FactoryShopsDataArray.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_FactorySavesArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_FactorySavesArray() : base(typeof(FactorySaves[]), ES3UserType_FactorySaves.Instance)
		{
			Instance = this;
		}
	}
}