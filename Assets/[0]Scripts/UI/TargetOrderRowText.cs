using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetOrderRowText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI orderText;
    [SerializeField] private Image orderImage;
    public void RemoveRow()
    {
        Destroy(gameObject);
    }

    public void ShowText(int total, int current, Sprite sprite)
    {
        orderText.text = current + "/" + total;
        orderImage.sprite = sprite;
    }
}
