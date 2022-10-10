using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerViewController : Singleton<ComputerViewController>
{
    public InterfaceAnimManager leftConsole;
    public InterfaceAnimManager rightConsole;

    private bool computersOn = false;
    
    public void OnComputersClicked()
    {
        //print("Computer status changing");
        if (computersOn)
        {
            leftConsole.startDisappear();
            rightConsole.startDisappear();
            //testConsole.startDisappear();
        }
        else
        {
            leftConsole.startAppear();
            rightConsole.startAppear();
            //testConsole.startAppear();
        }
        computersOn = !computersOn;
    }
}
