using UnityEngine;

public class InGameInputController : MonoBehaviour
{
    [SerializeField] private GameObject moveableObject;
    
    private IMoveableController moveableController;

    private void Awake()
    {
        moveableController = moveableObject.GetComponent<IMoveableController>();
        if (moveableController == null)
        {
            throw new System.NullReferenceException("No IMoveableController attached");
        }
    }
    
    void Update()
    {
        float direction = Input.GetAxis("Vertical");
        
        moveableController.Move(new Vector2(0, direction));
    }
}
