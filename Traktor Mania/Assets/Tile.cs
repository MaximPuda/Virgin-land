using UnityEngine;

public class Tile : MonoBehaviour
{
    private Map map;
    public MeshRenderer renderer { set; get; }
    public Collider collider { set; get; }
    public bool isHighilited { set; get; }
    private int MaterialIndex { set;  get; }

   
    private void Awake()
    {
        map = FindObjectOfType<Map>();
        renderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        if (MaterialIndex == 0)
    //        {
    //            renderer.material = map.materials[1];
    //            MaterialIndex = 1;
    //            isHighilited = true;
    //        }
    //        else if (MaterialIndex == 1)
    //        {
    //            map.FillArea();
    //        }
    //    }
    //}
}
