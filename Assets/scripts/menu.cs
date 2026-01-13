using Unity.VisualScripting;
using UnityEngine;

public class menu : MonoBehaviour
{
    GameObject pausemenu;
    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pausemenu.SetActive(!pausemenu.activeSelf);
        } 
    }
  
  
    public void gameexit()
    {
        Application.Quit();
    }
    
 
    
}
