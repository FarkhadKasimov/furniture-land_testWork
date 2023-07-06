using System.Collections.Generic;
using UnityEngine;

public class CustomerGenerationFabrique : MonoBehaviour
{
    [SerializeField] private Customer customerPrefab;

    [SerializeField] private Color[] customerBodyMaterialColor;
    [SerializeField] private GameObject[] customerHead;
    
    public Customer GenerateRandomCustomer(Vector3 position, List<WantedResource> order, float wrappingChance, FactoryShop shop)
    {
        Color randomColor = Color.black;
        GameObject randomHead = null;

        bool colorChange = false;
        bool headChange = false;
        
        if (customerBodyMaterialColor.Length > 0)
        {
            randomColor = customerBodyMaterialColor[Random.Range(0, customerBodyMaterialColor.Length)];
            colorChange = true;
        }
        
        if (customerHead.Length > 0)
        {
            randomHead = customerHead[Random.Range(0, customerHead.Length)];
            headChange = true;
        }

        var customer = Instantiate(customerPrefab);
        
        if (colorChange && headChange)
        {
            customer.SetupCustomer(position, order, randomColor, randomHead, shop);
        }
        else if (colorChange)
        {
            customer.SetupCustomer(position, order, randomColor, shop);
        }
        else if (headChange)
        {
            customer.SetupCustomer(position, order, randomHead, shop);
        }
        else
        {
            customer.SetupCustomer(position, order, shop);
        }

        customer.CheckForWrapping(wrappingChance);
        
        return customer;
    }
}
