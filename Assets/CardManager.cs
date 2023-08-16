namespace Core.Cards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CardManager : IManager
    {
        public static CardManager instance;

        public override void Awake() {
            base.Awake();

            if (instance == null) {
                instance = this;
            }
            else {
                DuplicateManagerError();
            }

            LoadStandard52();
        }

        public Dictionary<string, CardData> DeckData = new Dictionary<string, CardData>();

        public CardEntity CreateCard(CardEntity.CardType cardType, string cardSlug, Transform cardParent, bool faceDown = false) {
            GameObject prefab = Resources.Load<GameObject>("CardPrefab");
            CardEntity newCard = Instantiate(prefab, cardParent).GetComponent<CardEntity>();

            newCard.InitCard(cardSlug, cardType, faceDown);


            return newCard;
        }

        public void LoadStandard52() {
            for (int suitIndex = 0; suitIndex < 4; suitIndex++) {
                for (int cardIndex = 1; cardIndex < 14; cardIndex++) {
                    string cardFace = GetCardFaceFromIndex(cardIndex);
                    string cardSuit = GetSuitFromIndex(suitIndex);
                    string cardSlug = "card" + cardSuit + "_" + cardFace;
                    string spritePath = cardSuit.ToLower() + "/" + cardSlug;
                    CardData newCard = new CardData(cardSlug, cardIndex, Resources.Load<Sprite>(spritePath));
                    DeckData.Add(cardSlug, newCard);
                }
            }

        }

        public string GetSuitFromIndex(int index) {
            switch (index) {
                default:
                case 0:
                    return "Clubs";
                case 1:
                    return "Diamonds";
                case 2:
                    return "Hearts";
                case 3:
                    return "Spades";
            }
        }

        public string GetCardFaceFromIndex(int index) {
            switch (index) {
                case 1:
                    return "A";
                case 11:
                    return "J";
                case 12:
                    return "Q";
                case 13:
                    return "K";
                default:
                    return "" + index;
            }
        }
    }

}