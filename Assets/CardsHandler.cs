using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Spell{
    public int serial;

}

[System.Serializable]
public class Card{
    public Spell top = new Spell();
    public Spell bot = new Spell();
}

[System.Serializable]
public class ClientCard{
    public GameObject gmo;
    public int card;
    public int dir;
}
public class CardsHandler : MonoBehaviour {
    public GamePlayManager gamePlayManager;
    //[HideInInspector] public List<int> oldCards;
    public Transform cardsFolder;
    public Transform placedCardFolder;
    public GameObject gmoCard;

    public ClientCard[] oldCards;
    public ClientCard oldPlacedCard;

    public TextAsset cardsAsset;
    public Card[] cardsData;

	void Start(){
        cardsData = new Card[Constants.maxCards + 1];

        string[] c = cardsAsset.text.Split('\n');
        for (int i = 0; i < Constants.maxCards; i++){
            string[] tmp = c[i].Split(' ');
            cardsData[i + 1] = new Card();
            cardsData[i + 1].top.serial = int.Parse(tmp[0]);
            cardsData[i + 1].bot.serial = int.Parse(tmp[1]);
        }
	}

	public void UpdateCards(List<int> cards, int placedCard, int placedDir){
        for (int i = 0; i < oldCards.Length; i++) Destroy(oldCards[i].gmo);
        Destroy(oldPlacedCard.gmo);

        oldCards = new ClientCard[cards.Count];
        for (int i = 0; i < cards.Count; i++){
            oldCards[i] = new ClientCard();
            oldCards[i].gmo = Instantiate(gmoCard, new Vector3(), Quaternion.identity, cardsFolder);
            oldCards[i].gmo.transform.localPosition = new Vector3((i - 2) * 70, 0, 0);
            oldCards[i].card = cards[i];
            oldCards[i].dir = 1;

            //display more detail
            oldCards[i].gmo.transform.Find("top").Find("Text").GetComponent<Text>().text = cards[i].ToString();
        }
        if(placedCard != 0){
            oldPlacedCard = new ClientCard();
            oldPlacedCard.gmo = Instantiate(gmoCard, new Vector3(), Quaternion.identity, placedCardFolder);
            oldPlacedCard.gmo.transform.localPosition = new Vector3();
            oldPlacedCard.card = placedCard;
            oldPlacedCard.dir = placedDir;

            //display more detail
            oldPlacedCard.gmo.transform.Find("top").Find("Text").GetComponent<Text>().text = placedCard.ToString();
        }
    }
}
