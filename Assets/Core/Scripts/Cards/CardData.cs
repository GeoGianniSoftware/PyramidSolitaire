using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData
{
    public string cardSlug;
    public int cardValue;
    public Sprite cardSprite;

    public CardData(string cardSlug, int cardValue, Sprite cardSprite) {
        this.cardSlug = cardSlug;
        this.cardValue = cardValue;
        this.cardSprite = cardSprite;
    }
}
