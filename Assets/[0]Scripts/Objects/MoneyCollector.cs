using System;
using UnityEngine;

public class MoneyCollector : MonoBehaviour
{
    private CassaObject casa;
    private ExportMachineObject export;
    private bool collecting = false;
    
    public void Init(CassaObject cassaObject, ExportMachineObject exportMachineObject)
    {
        casa = cassaObject;
        export = exportMachineObject;
        collecting = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            collecting = true;
            
            if (export != null)
            {
                export.StartCollecting(playerController.transform);
            }

            if (casa != null)
            {
                casa.StartCollecting(playerController.transform);
                print("Start collecting");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            collecting = false;
            if (casa != null)
            {
                casa.StopCollecting();
                print("Stop collecting");
            }
            
            if (export != null)
            {
                export.StopCollecting();
            }
        }
    }
}
