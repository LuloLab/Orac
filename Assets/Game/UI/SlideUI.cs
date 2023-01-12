using UnityEngine;
using UnityEngine.EventSystems;
using static Global;

public class SlideUI : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public float speed;
    public float tolerence;

    private bool draging = false;
    private int dir = 0, target = 0;
    private Transform scroll;
    private float X
    {
        get => -scroll.transform.localPosition.x;
        set => scroll.transform.localPosition = new Vector3(-value, 0, 0);
    }
    private void Start()
    {
        scroll = transform.Find("scroll");
    }
    void Update()
    {
        if (!draging && dir != 0)
        {
            X += dir * speed * Time.deltaTime;
            if ((X - target * Screen.width) * dir > 0)
            {
                X = target * Screen.width;
                dir = 0;
            }
        }
    }
    public void OnDrag(PointerEventData e)
    {
        draging = true;
        dir = e.delta.x < 0 ? 1 : -1;
        X = Mathf.Clamp(X - e.delta.x, 
            -tolerence * Screen.width, (NLevel.Length - 1 + tolerence) * Screen.width);
    }
    public void OnEndDrag(PointerEventData e)
    {
        draging = false;
        if (X < 0)
            dir = 1;
        else if (X > (NLevel.Length - 1) * Screen.width)
            dir = -1;

        if (dir == 1)
            target = Mathf.CeilToInt(X / Screen.width);
        else if (dir == -1)
            target = Mathf.FloorToInt(X / Screen.width);
    }
    public void GoTo(int index)
    {
        target = index;
        X = Screen.width * target;
    }
}