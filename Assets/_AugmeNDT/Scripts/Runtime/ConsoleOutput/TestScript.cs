using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Its a new Build - 05.02.2025!");
    }

    public void SayHello(string text)
    {
        Debug.Log("Hello i am " + this.name + " !" + "\n" + text);
    }

    public void ChangeColor()
    {
        //Changes the Object Color between Green and orange
        if (GetComponent<Renderer>().material.color == Color.green)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
    }
}
