using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsHandler : MonoBehaviour {
    public GamePlayManager gamePlayManager;
    //[HideInInspector] public List<int> oldCards;
    public Transform cardsFolder;
    public GameObject gmoCard;

    public GameObject[] oldCards;

    public void UpdateCards(List<int> cards){
        foreach (GameObject gmo in oldCards) Destroy(gmo);

        oldCards = new GameObject[cards.Count];
        for (int i = 0; i < cards.Count; i++){
            oldCards[i] = Instantiate(gmoCard, new Vector3((i - 2) * 70, 0, 0), Quaternion.identity, cardsFolder);

            //display more detail
            oldCards[i].transform.Find("top").Find("Text").GetComponent<Text>().text = cards[i].ToString();
        }
    }
}
