using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Interact.Papers
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggablePaper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,IBeginDragHandler, IDragHandler
    {
        [Header("Juice Settings (DOTween)")]
        [SerializeField]
        private float _grabDuration = 0.2f;
        
        [SerializeField] 
        private Ease _grabEase = Ease.OutBack;

        [SerializeField] 
        private float _dropDuration = 0.15f;
        
        [SerializeField] 
        private Ease _dropEase = Ease.OutSine;
        
        [SerializeField] 
        private float _straightenDuration = 0.2f;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector3 _dragOffset;

        private readonly Vector3 _normalScale = Vector3.one;
        private readonly Vector3 _liftedScale = new Vector3(1.05f, 1.05f, 1.05f);

        public Action<bool> OnPaperPointedChanged;
        
        public bool isAcceptedByZone = false;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDisable()
        {
            _rectTransform.DOKill();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOLocalRotate(Vector3.zero, _straightenDuration).SetEase(Ease.OutBack);
            
            _rectTransform.SetAsLastSibling();

            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 globalMousePos);

            _dragOffset = _rectTransform.position - globalMousePos;
            
            OnPaperPointedChanged?.Invoke(true);
            
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _rectTransform.DOKill();
            _rectTransform.DOScale(_liftedScale, _grabDuration).SetEase(_grabEase);

            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    _rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector3 globalMousePos))
            {
                _rectTransform.position = globalMousePos + _dragOffset;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isAcceptedByZone) return;
            _rectTransform.DOKill();
        
            _rectTransform.DOScale(_normalScale, _dropDuration).SetEase(_dropEase);
        
            float randomZ = Random.Range(-4f, 4f);
            transform.DOLocalRotate(new Vector3(0, 0, randomZ), _straightenDuration).SetEase(Ease.OutCubic);

            _canvasGroup.blocksRaycasts = true;
        
            OnPaperPointedChanged?.Invoke(false); 
        }
    }
}
