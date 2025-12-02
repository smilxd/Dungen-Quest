using UnityEngine;

public class AntiBugScript : MonoBehaviour
{
    public GameObject GameObjectToDisable;
    public float delay;
  
    void Start()
    {
       
    }

    private void FixedUpdate()
    {
        Delaystart();
    }
    private void Delaystart()
    {
       delay += Time.deltaTime;
        if (delay >= 3f)
        {
            GameObjectToDisable.SetActive(false);
        }   
    }

}
