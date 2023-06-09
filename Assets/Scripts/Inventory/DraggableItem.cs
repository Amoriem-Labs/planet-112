using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    Transform parentAfterDrag;

    public void OnBeginDrag(){
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(){
        Debug.Log("position: " + parentAfterDrag.position + " , name: "+parentAfterDrag.name);
        transform.position = Input.mousePosition;// - transform.parent.parent.parent.position;
    }

    public void OnEndDrag(){
        transform.SetParent(parentAfterDrag);
    }
}
