using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers.Network;
using UnityEngine;

namespace Managers.CoreLogic
{
    public class GuestObject : MonoBehaviour
    {
        [SerializeField] private float _walkDuration = 2f;
        [SerializeField] private float _turnDuration = 0.5f;
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SkinnedMeshRenderer _faceMesh;

        [Header("Face Settings")]
        [SerializeField] private float _blendDuration = 0.3f;

        private Dictionary<string, int> _blendShapeIndexMap = new Dictionary<string, int>();

        private bool _isDangerous = false;
        public bool IsDangerous => _isDangerous;

        private void Awake()
        {
            if (_faceMesh != null && _faceMesh.sharedMesh != null)
            {
                int shapeCount = _faceMesh.sharedMesh.blendShapeCount;
                for (int i = 0; i < shapeCount; i++)
                {
                    string shapeName = _faceMesh.sharedMesh.GetBlendShapeName(i);
                    _blendShapeIndexMap.Add(shapeName, i); 
                }
            }
        }

        private void Start()
        {
            CoopSyncManager.OnBroadcastReceived += _handleBroadcast;
        }

        private void OnDestroy()
        {
            CoopSyncManager.OnBroadcastReceived -= _handleBroadcast;
        }

        public void Initialize()
        {
            _animator.SetBool("Walk",false);
        }

        public void MoveToCounter(Transform targetPosition, Action onGuestArrivedAtCounter)
        {
            _animator.SetBool("Walk",true);

            Vector3 lookPosition = new Vector3(targetPosition.position.x, transform.position.y, targetPosition.position.z);
            transform.DOLookAt(lookPosition, 0.3f); 
            
            Sequence moveSeq = DOTween.Sequence();

            moveSeq.Append(transform.DOMove(targetPosition.position, _walkDuration).SetEase(Ease.Linear));

            float turnStartTime = Mathf.Max(0, _walkDuration - _turnDuration);
            moveSeq.Insert(turnStartTime, transform.DORotate(targetPosition.rotation.eulerAngles, _turnDuration).SetEase(Ease.InOutSine));

            moveSeq.OnComplete(() =>
            {
                _animator.SetBool("Walk", false);
                _animator.SetTrigger("GiveDocument");
                onGuestArrivedAtCounter?.Invoke();
            });
        }

        public void PlayReaction(bool isApproved)
        {
            if (isApproved)
            {
                SetFaceShapeWeight("Smile", 100f, _blendDuration);
            }
            else
            {
                SetFaceShapeWeight("Angry", 100f, _blendDuration);
            }
        }
        
        private void SetFaceShapeWeight(string shapeName, float targetWeight, float duration)
        {
            if (_blendShapeIndexMap.TryGetValue(shapeName, out int shapeIndex))
            {
                DOTween.To(() => _faceMesh.GetBlendShapeWeight(shapeIndex), 
                        x => _faceMesh.SetBlendShapeWeight(shapeIndex, x), 
                        targetWeight, duration)
                    .SetEase(Ease.InOutSine);
            }
        }

        public void ResetFace()
        {
            foreach (var shapeName in _blendShapeIndexMap.Keys)
            {
                SetFaceShapeWeight(shapeName, 0f, _blendDuration);
            }
        }

        public void Leave(Transform exitPoint, Action onGuestExited)
        {
            ResetFace();
            
            _animator.SetBool("Walk",true);

            Vector3 lookPosition = new Vector3(exitPoint.position.x, transform.position.y, exitPoint.position.z);
            transform.DOLookAt(lookPosition, 0.3f);

            transform.DOMove(exitPoint.position, 2f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    onGuestExited?.Invoke();
                    Destroy(gameObject);
                });
        }

        public void ReleaseBomb()
        {
            _animator.SetBool("Bomb",true);
        }
        
        private void _handleBroadcast(string eventName)
        {
            switch (eventName)
            {
                case CoopEvents.BombExplode:
                    _animator.SetBool("Bomb",false);
                    _animator.SetTrigger("BombExplosion");
                    break;
                case CoopEvents.BombButtonPressed:
                    _animator.SetBool("Bomb",false);
                    _animator.SetTrigger("Die");
                    break;
            }
        }

        public void SetActiveFalse()
        {
            //TODO:Afford SetActiveFalse animation when BombExplosion and Die animation ended
        }

        public void SetIsDangerous(bool isDangerous)
        {
            _isDangerous = isDangerous;
        }
    }
}