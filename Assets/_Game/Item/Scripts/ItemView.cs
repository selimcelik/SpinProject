using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ItemView : MonoBehaviour
{

    [Header("COMPONENTS")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _amountText;

    private ItemManager _manager;

    [Inject]
    private void Construct(ItemManager manager, ItemRepository repository)
    {
        _manager = manager;
    }

    private void OnEnable()
    {
        _manager?.AddView(this);
    }

    private void OnDisable()
    {
        _manager?.RemoveView(this);
    }

    public void SetItemType(SpinWaveItemData waveItemData)
    {
        if (waveItemData == null)
        {
            return;
        }

        ItemData itemData = _manager.GetData(waveItemData.type);

        if (itemData == null)
        {
            return;
        }

        _icon.sprite = itemData.icon;

        if (!waveItemData.multiple)
        {
            _amountText.gameObject.SetActive(false);
            return;
        }

        _amountText.gameObject.SetActive(true);
        _amountText.text = $"x{waveItemData.amount}";
    }
}
