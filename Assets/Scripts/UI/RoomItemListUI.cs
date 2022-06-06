using UnityEngine;
using UnityEngine.UI;

public class RoomItemListUI : MonoBehaviour
{
    public Button button;
    public TMPro.TextMeshProUGUI idRoom;
    public TMPro.TextMeshProUGUI titleRoom;
    [SerializeField]
    private Image startedImage;

    private void Start()
    {
        startedImage.color = Color.green;
    }

    public void SetAsStarted()
    {
        startedImage.color = Color.red;
    }
}
