using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] spots = new GameObject[3];
    [SerializeField]
    private GameObject PlayerObj;
    public static CHARATER_TYPE CharacterType;

    protected virtual void Awake()
    {
        CharacterType = (CHARATER_TYPE)Random.Range(0, 3);
        PlayerObj.transform.position = spots[(int)CharacterType].transform.position;
    }
}
