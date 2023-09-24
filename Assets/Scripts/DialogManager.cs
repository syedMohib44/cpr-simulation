using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI textDialog;
    private int textIndex = 0;
    private int dialogIndex = 0;

    string[] dialog = new string[4];
    void Start()
    {
        dialog[0] = "Ooooo! Looks like there is an accident";
        dialog[1] = "Lets get closer to take a look";
        dialog[2] = "This person seems to be seriously injured";
        dialog[3] = "What should I do!";
        StartDialog();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (textDialog.text == dialog[dialogIndex])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textDialog.text = dialog[dialogIndex];
            }
        }
    }

    void StartDialog()
    {
        StartCoroutine(TypeLine());
    }

    // Update is called once per frame
    IEnumerator TypeLine()
    {
        for (int i = 0; i < dialog[dialogIndex].Length; i++)
        {
            textDialog.text += dialog[dialogIndex][i];
            yield return new WaitForSeconds(0.1f);
        }
    }

    void NextLine()
    {
        if (dialogIndex < dialog.Length - 1)
        {
            dialogIndex++;
            textDialog.text = "";
            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
