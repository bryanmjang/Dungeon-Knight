using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    // The hotbar stays in the bottom-left, and the extra slots expand above it.
    private const int HotbarSlotSize = 74;
    private const int InventorySlotSize = 74;
    private const int SlotSpacing = 15;

    // reference to the player's inventory data
    private PlayerInventory inventory;
    // bottom row that always stays on screen
    private GameObject hotbarRoot;
    // extra inventory rows that show when the inventory opens
    private GameObject inventoryPanel;
    // all visible slot UI objects, hotbar first and inventory slots after
    private readonly List<InventorySlotView> slotViews = new List<InventorySlotView>();
    // built-in font used by the text labels
    private Font builtInFont;

    public void Initialize(PlayerInventory targetInventory)
    {
        // Connect the UI to the player's inventory and listen for updates.
        inventory = targetInventory;
        builtInFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        EnsureEventSystem();
        BuildUi();

        inventory.InventoryChanged -= Refresh;
        inventory.InventoryChanged += Refresh;

        inventory.SelectedSlotChanged -= HandleSelectedSlotChanged;
        inventory.SelectedSlotChanged += HandleSelectedSlotChanged;

        Refresh();
        SetInventoryVisible(inventory.IsInventoryOpen);
    }

    public void SetInventoryVisible(bool visible)
    {
        // only show the extra rows when the inventory is opened
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(visible);
        }
    }

    private void BuildUi()
    {
        // Reuse an existing canvas if there is one, otherwise create a new one.
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            // create a runtime canvas if the scene does not already have one
            GameObject canvasObject = new GameObject("InventoryCanvas");
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
        }

        hotbarRoot = CreateHotbar(canvas.transform);
        inventoryPanel = CreateInventoryPanel(canvas.transform);
    }

    private GameObject CreateHotbar(Transform parent)
    {
        // This row is always visible, even when the full inventory is closed.
        // width is based on number of hotbar slots plus spacing and padding
        float width = (inventory.HotbarSize * HotbarSlotSize) + ((inventory.HotbarSize - 1) * SlotSpacing) + 28f;

        GameObject hotbarObject = CreatePanel(
            "Hotbar",
            parent,
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(22f, 22f),
            new Vector2(width, HotbarSlotSize + 26f),
            new Color(0f, 0f, 0f, 0f));

        HorizontalLayoutGroup layout = hotbarObject.AddComponent<HorizontalLayoutGroup>();
        // layout group keeps the hotbar slots aligned in one row automatically
        layout.padding = new RectOffset(14, 14, 12, 14);
        layout.spacing = SlotSpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        AddOutline(hotbarObject, new Color(0.02f, 0.09f, 0.32f, 0.95f), new Vector2(3f, -3f));

        for (int i = 0; i < inventory.HotbarSize; i++)
        {
            slotViews.Add(CreateSlotView(hotbarObject.transform, i, HotbarSlotSize, true));
        }

        return hotbarObject;
    }

    private GameObject CreateInventoryPanel(Transform parent)
    {
        // This panel holds the extra inventory rows that show above the hotbar.
        // total inventory slots minus the hotbar gives the hidden inventory section
        int inventorySlotCount = inventory.TotalSlotCount - inventory.HotbarSize;
        // rows are calculated from slot count and number of columns
        int rows = Mathf.CeilToInt(inventorySlotCount / (float)inventory.HotbarSize);
        float width = (inventory.HotbarSize * InventorySlotSize) + ((inventory.HotbarSize - 1) * SlotSpacing) + 28f;
        float gridHeight = (rows * InventorySlotSize) + ((rows - 1) * SlotSpacing) + 28f;

        GameObject panelObject = CreatePanel(
            "InventoryPanel",
            parent,
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(22f, HotbarSlotSize + 17f),
            new Vector2(width, gridHeight + 54f),
            new Color(0f, 0f, 0f, 0f));

        AddOutline(panelObject, new Color(0.02f, 0.09f, 0.32f, 0.95f), new Vector2(3f, -3f));

        CreateLabel(
            panelObject.transform,
            "InventoryTitle",
            "Inventory",
            28,
            new Vector2(22f, -6f),
            new Vector2(320f, 36f),
            TextAnchor.MiddleLeft,
            new Color(0.92f, 0.96f, 1f, 1f));

        GameObject gridObject = new GameObject("InventoryGrid");
        gridObject.transform.SetParent(panelObject.transform, false);

        RectTransform gridRect = gridObject.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(0f, 1f);
        gridRect.pivot = new Vector2(0f, 1f);
        gridRect.anchoredPosition = new Vector2(14f, -48f);
        gridRect.sizeDelta = new Vector2(width - 28f, gridHeight);

        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        // layout group keeps the hidden inventory section arranged as a grid
        grid.padding = new RectOffset(0, 0, 0, 0);
        grid.cellSize = new Vector2(InventorySlotSize, InventorySlotSize);
        grid.spacing = new Vector2(SlotSpacing, SlotSpacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = inventory.HotbarSize;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        for (int i = inventory.HotbarSize; i < inventory.TotalSlotCount; i++)
        {
            slotViews.Add(CreateSlotView(gridObject.transform, i, InventorySlotSize, false));
        }

        panelObject.SetActive(false);
        return panelObject;
    }

    private InventorySlotView CreateSlotView(Transform parent, int slotIndex, int slotSize, bool isHotbarSlot)
    {
        // Create one clickable UI slot and connect it to the matching inventory index.
        GameObject slotObject = new GameObject("Slot_" + slotIndex);
        slotObject.transform.SetParent(parent, false);

        Image slotImage = slotObject.AddComponent<Image>();
        // slot background color
        slotImage.color = new Color(0.25f, 0.31f, 0.67f, 0.66f);

        // button handles hover and click interaction on the slot
        Button button = slotObject.AddComponent<Button>();

        RectTransform slotRect = slotObject.GetComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(slotSize, slotSize);

        GameObject highlightObject = new GameObject("Highlight");
        highlightObject.transform.SetParent(slotObject.transform, false);

        Image highlightImage = highlightObject.AddComponent<Image>();
        // shown when the player selects this slot
        highlightImage.color = new Color(0.42f, 0.95f, 0.8f, 0.36f);
        highlightImage.enabled = false;

        RectTransform highlightRect = highlightObject.GetComponent<RectTransform>();
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.offsetMin = Vector2.zero;
        highlightRect.offsetMax = Vector2.zero;



        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(slotObject.transform, false);

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.preserveAspect = true;
        // icon image displays the item sprite inside the slot
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(slotSize - 20f, slotSize - 20f);

        GameObject quantityObject = new GameObject("Quantity");
        quantityObject.transform.SetParent(slotObject.transform, false);

        Text quantityText = quantityObject.AddComponent<Text>();
        // number text for stack counts
        quantityText.font = builtInFont;
        quantityText.fontSize = 15;
        quantityText.alignment = TextAnchor.LowerRight;
        quantityText.color = Color.white;

        RectTransform quantityRect = quantityObject.GetComponent<RectTransform>();
        quantityRect.anchorMin = Vector2.zero;
        quantityRect.anchorMax = Vector2.one;
        quantityRect.offsetMin = new Vector2(5f, 5f);
        quantityRect.offsetMax = new Vector2(-5f, -5f);

        AddOutline(quantityObject, new Color(0.08f, 0.12f, 0.3f, 1f), new Vector2(1f, -1f));

        if (isHotbarSlot)
        {
            // Hotbar slots get number labels so players know their quick-access order.
            CreateLabel(
                slotObject.transform,
                "HotbarNumber",
                GetHotbarLabel(slotIndex),
                16,
                new Vector2(9f, -9f),
                new Vector2(18f, 18f),
                TextAnchor.UpperLeft,
                new Color(0.88f, 0.93f, 1f, 0.96f));
        }

        InventorySlotView slotView = slotObject.AddComponent<InventorySlotView>();
        slotView.SetReferences(iconImage, quantityText, highlightImage, button);
        slotView.Initialize(inventory, slotIndex);

        return slotView;
    }

    private void Refresh()
    {
        // Update every visible slot to match the latest inventory data.
        if (inventory == null)
        {
            return;
        }

        for (int i = 0; i < slotViews.Count && i < inventory.Slots.Count; i++)
        {
            slotViews[i].Refresh(
                inventory.Slots[i],
                i == inventory.SelectedSlotIndex,
                i == inventory.DraggedSlotIndex);
        }
    }

    private void HandleSelectedSlotChanged(int _)
    {
        // selected slot changed, so redraw highlights
        Refresh();
    }

    private GameObject CreatePanel(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        // Helper for creating positioned UI containers in code.
        GameObject panelObject = new GameObject(objectName);
        panelObject.transform.SetParent(parent, false);

        Image image = panelObject.AddComponent<Image>();
        image.color = color;

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        return panelObject;
    }

    private void CreateLabel(Transform parent, string objectName, string textValue, int fontSize, Vector2 anchoredPosition, Vector2 sizeDelta, TextAnchor alignment, Color textColor)
    {
        // helper for creating text labels like the title and hotbar numbers
        GameObject labelObject = new GameObject(objectName);
        labelObject.transform.SetParent(parent, false);

        Text text = labelObject.AddComponent<Text>();
        text.font = builtInFont;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = textColor;
        text.text = textValue;

        RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        AddOutline(labelObject, new Color(0.05f, 0.12f, 0.3f, 1f), new Vector2(1f, -1f));
    }

    private void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        // helper for adding a quick outline effect to UI objects
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    private string GetHotbarLabel(int slotIndex)
    {
        // hotbar labels count 1-9, then 0 for the tenth slot
        int displayIndex = slotIndex + 1;
        return displayIndex == 10 ? "0" : displayIndex.ToString();
    }

    private void EnsureEventSystem()
    {
        // UI clicking only works if an EventSystem exists in the scene.
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }
}
