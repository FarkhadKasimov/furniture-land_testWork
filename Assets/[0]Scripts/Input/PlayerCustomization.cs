using MoreMountains.Tools;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
    [SerializeField] private GameObject body = null;
    [SerializeField] private GameObject head = null;
    [SerializeField] private GameObject face = null;
    [SerializeField] private GameObject hair = null;
    [SerializeField] private GameObject headdress = null;

    [FoldoutGroup("Other settings")]
    [SerializeField] private Transform objectsPositionBase;
#if UNITY_EDITOR
    [FoldoutGroup("Other settings")]
    [SerializeField] private AnimatorController animatorBase;
    #endif
    [FoldoutGroup("Other settings")]
    [SerializeField] private Transform modelParent;
    [FoldoutGroup("Other settings")] 
    [SerializeField] private string headObjectName = "+ Head";
    [FoldoutGroup("Other settings")] 
    [SerializeField] private float modelScale = 0.8f;
    
    private GameObject _body = null;
    private GameObject _head = null;
    private GameObject _face = null;
    private GameObject _hair = null;
    private GameObject _headdress = null;
    private Transform _objectsInHandsPosition = null;

    private PlayerController _playerController;
    private ObjectsHolder _objectsHolder;

    [Button]
    private void Customize()
    {
        if (_playerController == null)
        {
            _playerController = GetComponent<PlayerController>();
        }

        if (_objectsHolder == null)
        {
            _objectsHolder = GetComponent<ObjectsHolder>();
        }
        
        if (body == null || head == null)
        {
            return;
        }

        ClearCurrentModel();

        _body = Instantiate(body, modelParent);
        _body.name = "player_model_female";
        _body.transform.localScale = Vector3.one * modelScale;

        var headParent = _body.transform.MMFindDeepChildDepthFirst(headObjectName);

        if (headParent == null)
        {
            ShowError("Cannot find head object with name " + headObjectName + ", in the " + _body.name + " object.");
            return;
        }
        
        _head = Instantiate(head, headParent);
        
        if (face != null)
        {
            _face = Instantiate(face, headParent);
        }

        if (hair != null)
        {
            _hair = Instantiate(hair, headParent);
        }

        if (headdress != null)
        {
            _headdress = Instantiate(headdress, headParent);
        }
        
        _objectsInHandsPosition = Instantiate(objectsPositionBase, _body.transform);

        var animator = _body.GetComponent<Animator>();
        animator.applyRootMotion = false;

        if (animator == null)
        {
            ShowError("Cannot find Animator component, on the " + _body.name + " object!");
            return;
        }

#if UNITY_EDITOR
        animator.runtimeAnimatorController = animatorBase;

        
        _playerController.AddAnimator(animator);
#endif
        _objectsHolder.UpdateBeginPosition(_objectsInHandsPosition);
    }

    private void ShowError(string error)
    {
        Debug.LogError(error);
        ClearCurrentModel();
    }
    
    private void ClearCurrentModel()
    {
        if (_body != null)
        {
            DestroyImmediate(_body);
        }

        var model = modelParent.MMFindDeepChildDepthFirst("player_model_female");
        if (model != null)
        {
            DestroyImmediate(model.gameObject);
        }

        _body = null;
        _head = null;
        _face = null;
        _hair = null;
        _headdress = null;
        _objectsInHandsPosition = null;
    }
}
