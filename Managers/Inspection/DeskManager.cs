using System;
using System.Collections.Generic;
using Interactables;
using Interactables.Papers;
using UnityEngine;
using SO;
using Managers.Inspection;
using TMPro;
using Random = UnityEngine.Random;

public class DeskManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _weightText;
    
    [SerializeField]
    private Transform _deskContainer;
    
    [SerializeField]
    private List<DocumentTemplateSo> _allTemplates;
    
    private List<PaperVisualizer> _activeDocuments = new List<PaperVisualizer>();
    
    public static Action OnDeskCleared;

    private void Awake()
    { 
        GivePassportZone.OnDocumentGiven += _handleDocumentGiven;
    }

    public void SpawnGuestDocuments(string guestId)
    {
        GuestEntry guest = GuestManager.GetGuest(guestId);
        if (guest == null) return;

        foreach (string docId in guest.documents)
        {
            SpawnDocumentOnDesk(docId, guest);
        }
        _setWeightText(guest.globalData);
    }

    public void SpawnDocumentOnDesk(string documentId, GuestEntry guest)
    {
        BookieEntry dataEntry = BookieManager.GetEntry(documentId);
        if (dataEntry == null) return;

        DocumentTemplateSo correctTemplate = _allTemplates.Find(t => t.documentType.ToString() == dataEntry.type);
        if (correctTemplate == null) return;

        PaperVisualizer newDoc = Instantiate(correctTemplate.documentPrefab, _deskContainer);
        _activeDocuments.Add(newDoc);
        
        newDoc.transform.localScale = correctTemplate.documentPrefab.transform.localScale;
        
        newDoc.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-4f, 4f));
        newDoc.GetComponent<RectTransform>().anchoredPosition += new Vector2(Random.Range(-30f, 30f), Random.Range(-30f, 30f));

        newDoc.Setup(correctTemplate, dataEntry.title, guest, documentId);
    }
    
    private void _setWeightText(DocumentFieldData[] fields)
    {
        string safeKey = FieldType.Weight.ToString();
        DocumentFieldData matchedData = System.Array.Find(fields, f => f.key == safeKey);
        _weightText.text=$"WT: {(matchedData != null ? matchedData.value : "N/A")}";
    }
    
    public void ClearDesk()
    {
        foreach (Transform child in _deskContainer)
        {
            Destroy(child.gameObject);
        }
        _activeDocuments.Clear();
    }

    private void _handleDocumentGiven(PaperVisualizer paperVisualizer)
    {
        if (_activeDocuments.Contains(paperVisualizer))
            _activeDocuments.Remove(paperVisualizer);
        if (_activeDocuments.Count <= 0)
        {
            OnDeskCleared?.Invoke();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            foreach (Transform child in _deskContainer)
            {
                Destroy(child.gameObject);
            }
            SpawnGuestDocuments("guest_jorji_01");
        }
    }
}