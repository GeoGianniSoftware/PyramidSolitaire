using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Core.IManager
{

    public static GameManager instance;
    public enum GameState
    {   Menu,
        Setup,
        Ready,
        Over
    }

    private GameState currentGameState;
    private Sprite cardBackground;


    #region References
    [Header("References")]
    public Core.UI.StateUI stateUI;

    public Transform pyramidTransform;
    
    public Transform drawPileTransform;
    CardEntity drawPilePlaceholder;
    
    public Transform pairPileTransform;
    CardEntity pairPilePlaceholder;

    public Transform discardPileTransform;
    private CardEntity discardTop;
    #endregion

    #region Card Data
    private List<CardData> gameDeck = new List<CardData>();
    private List<CardEntity> pyramidCards = new List<CardEntity>();
    private List<CardData> discardPile = new List<CardData>();
    private List<CardData[]> pairPile = new List<CardData[]>();

    private CardEntity selectionA;
    private CardEntity selectionB;
    private List<CardEntity>[] cardRows;
    private int cardsInPyramid = 0;
    #endregion
    [Header("Cheat Mode")]
    public bool cheatMode = false;

    public Color cardOutline = Color.blue;
    public Color cheatOutline = Color.red;


    #region Base
    public void SetGameSettings(int cardBackground, bool cheatMode) {
        this.cheatMode = cheatMode;
        this.cardBackground = GetBackgroundByIndex(cardBackground);
    }

   
    private void Update() {
        CheatLogic();

        CheckForLoss();

    }

    public override void Awake() {


        currentGameState = GameState.Menu;
        base.Awake();

        if (instance == null) {
            instance = this;
        }
        else {
            DuplicateManagerError();
        }

        //StartNewGame();
    }

    public void StartNewGame() {
        currentGameState = GameState.Setup;

        ResetGameData();

        SetupGamePyramid();
        SetupPairPile();
        SetupDrawPile();

    }

    public void ResetGameData() {
        if(discardTop != null)
        Destroy(discardTop.gameObject);

        discardTop = null;

        if(pairPilePlaceholder != null)
        Destroy(pairPilePlaceholder.gameObject);

        pairPilePlaceholder = null;

        ClearPyramid();
        ClearSelection();

        gameDeck.Clear();
        gameDeck.AddRange(Core.Cards.CardManager.instance.DeckData.Values);

        pairPile = new List<CardData[]>();
        discardPile = new List<CardData>();
        pyramidCards = new List<CardEntity>();
    }

    public CardData DrawCard(bool discard = true) {
        int random = Random.Range(0, gameDeck.Count);
        CardData cache = gameDeck[random];
        gameDeck.Remove(cache);

        if (gameDeck.Count <= 0) {
            ToggleDrawPlaceholder(false);
        }

        if (discard) {
            discardPile.Add(cache);
        }

        return cache;
    }
    public static void QuitGame() {
#if UNITY_EDITOR
        if (Application.isEditor) {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif

        Application.Quit();
    }

    List<CardData> randomData = new List<CardData>();
    public CardData getRandomCard() {
        if(randomData.Count == 0) {
            randomData.AddRange(Core.Cards.CardManager.instance.DeckData.Values);
        }

        int index = Random.Range(0, randomData.Count);
        CardData cache = randomData[index];
        randomData.RemoveAt(index);

        return cache;
    }

    public void ReloadScene() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    #endregion


    #region Background
    public Sprite GetBackgroundByIndex(int index) {
        return Resources.LoadAll<Sprite>("Backgrounds/")[index];
    }

    public int GetBackgroundCount() {
        return Resources.LoadAll<Sprite>("Backgrounds/").Length;
    }

    public void SetBackground(int index) {
        cardBackground = GetBackgroundByIndex(index);
    }

    public Sprite getBackground() {
        return cardBackground;
    }

    #endregion

    #region Cheat mode
    public void CheatLogic() {
        if (cheatMode) {
            for (int x = 0; x < pyramidCards.Count; x++) {
                bool check = CheckCardCheat(selectionA, pyramidCards[x]);

                if (selectionA != null && check) {
                    pyramidCards[x].CheatOutline(true);
                }
                else if(!pyramidCards[x].isSelected) {
                    pyramidCards[x].CheatOutline(false);
                }
            }

            if (selectionA != null && selectionA.cardType == CardEntity.CardType.Pyramid) {
                bool check = CheckCardCheat(selectionA, discardTop);

                if(discardTop != null) {
                    if (check) {
                        discardTop.CheatOutline(true);
                    }
                    else {
                        discardTop.CheatOutline(false);
                    }
                }
            }

        }
    }

    public bool CheckCardCheat(CardEntity refCard, CardEntity cardToCheck) {
        if (refCard == null || cardToCheck == null)
            return false;

        if (refCard != null && cardToCheck.CanBePaired() && refCard.cardValue + cardToCheck.cardValue == 13) {
            return true;
        }
        else {
            return false;
        }
    }
    #endregion

    #region Events
    public void OnDrawPileClicked() {

        if (gameDeck.Count > 0) {
            SetDiscardPileTop(DrawCard(), true);
        }
    }

    public void OnPyramidCardClicked(CardEntity cardClicked) {
        if (cardClicked.isFaceUp) {
            SelectCard(cardClicked);
        }
    }

    public void OnDiscardPileClicked() {
        if (discardPile.Count > 0 && discardTop != null) {
            SelectCard(discardTop);
        }
    }
    #endregion

    #region Game States

    public void WinGame() {
        if (currentGameState != GameState.Over) {
            stateUI.LoadVictoryUI();

            currentGameState = GameState.Over;
        }
    }

    public void LoseGame() {
        if (currentGameState != GameState.Over) {
            stateUI.LoadDefeatUI();

            currentGameState = GameState.Over;
        }
    }

    public void CheckForLoss() {
        if (currentGameState != GameState.Ready || cardsInPyramid == 0)
            return;

        if (gameDeck.Count <= 0) {
            List<CardEntity> interactableCards = new List<CardEntity>();
            for (int i = 0; i < pyramidCards.Count; i++) {
                if (!pyramidCards[i].isRemoved && pyramidCards[i].CanBePaired()) {
                    interactableCards.Add(pyramidCards[i]);
                }
            }

            bool hasAPlay = false;
            for (int x = 0; x < interactableCards.Count; x++) {
                if (discardTop != null && interactableCards[x].cardValue + discardTop.cardValue == 13)
                    hasAPlay = true;

                for (int y = 0; y < interactableCards.Count; y++) {
                    if (x == y)
                        continue;

                    if (interactableCards[x].cardValue + interactableCards[y].cardValue == 13)
                        hasAPlay = true;

                }
            }

            if (!hasAPlay) {
                LoseGame();
            }

        }
    }

    #endregion

    #region Pairing & Selection

    public void SendToPair(List<CardEntity> cardsToPair) {
        List<CardData> tempData = new List<CardData>();
        for (int x = 0; x < cardsToPair.Count; x++) {
            if (cardsToPair[x] == null)
                continue;

            if (cardsToPair[x].cardType == CardEntity.CardType.Pyramid)
                PyramidCardRemoved(cardsToPair[x].getCardData());

            tempData.Add(cardsToPair[x].getCardData());

            if (cardsToPair[x].cardType == CardEntity.CardType.Pyramid)
                cardsToPair[x].HideCard(true);

        }



        pairPile.Add(tempData.ToArray());
    }

    public void SelectCard(CardEntity cardToSelect) {
        if (currentGameState != GameState.Ready)
            return;

        //Editor Cheat
        if(Application.isEditor && Input.GetKey(KeyCode.X) && cardToSelect.cardType == CardEntity.CardType.Pyramid) {
            SendToPair(new List<CardEntity>() { cardToSelect });
            ClearSelection();
            return;
        }

        if (!cardToSelect.CanBePaired())
            return;

        if (cardToSelect.isSelected) {
            ClearSelection();
            return;
        }



        if (selectionA == null) {
            selectionA = cardToSelect;
            selectionA.SetSelected(true);
        }
        else if (selectionB == null) {
            selectionB = cardToSelect;
            selectionB.SetSelected(true);
        }

        int value = CalculateSelection();
        print("Combo Value: " + value);

        if (selectionA && selectionB && value != 13) {
            ClearSelection();
            return;
        }

        if (value == 13) {
            if ((selectionA && selectionA.cardType == CardEntity.CardType.Draw) || (selectionB && selectionB.cardType == CardEntity.CardType.Draw)) {
                RemoveTopDiscardCard();
            }

            SendToPair(new List<CardEntity>() { selectionA, selectionB });
            ClearSelection();
        }
    }

    public void ClearSelection() {
        if (selectionA != null)
            selectionA.SetSelected(false);
        if (selectionB != null)
            selectionB.SetSelected(false);
        selectionA = null;
        selectionB = null;

        if (discardTop != null)
            discardTop.CheatOutline(false);
    }

    public int CalculateSelection() {
        int value = 0;
        if (selectionA != null)
            value += selectionA.cardValue;

        if (selectionB != null)
            value += selectionB.cardValue;

        return value;
    }

    #endregion

    #region Pyramid
    public void ClearPyramid() {
        int childCount = pyramidTransform.childCount;

        for (int x = 0; x < childCount; x++) {
            int x_childCount = pyramidTransform.GetChild(x).childCount;
            for (int y = 0; y < x_childCount; y++) {
                Destroy(pyramidTransform.GetChild(x).GetChild(y).gameObject);
            }
        }
    }

    public void SetupGamePyramid() {
        int pyramidCardCount = 28;
        int rowCount = 7;

        int cardPerRow = 1;
        int currentRowIndex = 0;
        int arrayIndex = 0;

        cardsInPyramid = pyramidCardCount;


        cardRows = new List<CardEntity>[rowCount];
        for (int i = 0; i < rowCount; i++) {
            cardRows[i] = new List<CardEntity>();
        }


        for (int x = 0; x < pyramidCardCount; x++) {
            CardData card = DrawCard(false);
            Transform currentRow = getPyramidRow(cardPerRow - 1);


            CardEntity newCard = Core.Cards.CardManager.instance.CreateCard(CardEntity.CardType.Pyramid, card.cardSlug, currentRow, true);
            cardRows[arrayIndex].Add(newCard);
            newCard.rowIndex = currentRowIndex;

            pyramidCards.Add(newCard);
            newCard.frontButton.onClick.AddListener(delegate { OnPyramidCardClicked(newCard); });

            currentRowIndex++;

            if (currentRowIndex == cardPerRow) {
                cardPerRow++;
                currentRowIndex = 0;
                arrayIndex++;
            }


        }

        SortPyramidParents();

        StartCoroutine(PyramidIntro());
    }

    public Transform getPyramidRow(int rowIndex) {
        return pyramidTransform.GetChild(rowIndex);
    }
    public void SortPyramidParents() {
        for (int x = 0; x < cardRows.Length - 1; x++) {
            int count = cardRows[x].Count;
            for (int y = 0; y < count; y++) {
                CardEntity a = null;
                CardEntity b = null;
                int parentAIndex = y;
                int parentBIndex = y + 1;
                if (x + 1 < cardRows.Length) {
                    a = cardRows[x + 1][parentAIndex];
                    if (parentBIndex < cardRows[x + 1].Count) {
                        b = cardRows[x + 1][parentBIndex];
                    }
                }

                if (a != null && b != null) {
                    cardRows[x][y].SetParents(a, b);
                }

            }
        }
    }

    IEnumerator PyramidIntro() {
        yield return new WaitForSeconds(1f);

        int cardPerRow = 1;
        int rowCount = 0;
        for (int x = 0; x < pyramidCards.Count; x++) {
            pyramidCards[x].ToggleFaceUp(true);
            rowCount++;

            if (rowCount == cardPerRow) {
                cardPerRow++;
                rowCount = 0;
                yield return new WaitForSeconds(.142f);

            }

        }


        currentGameState = GameState.Ready;
    }
    public void PyramidCardRemoved(CardData data) {

        cardsInPyramid--;

        if (cardsInPyramid <= 0) {
            WinGame();
        }
    }
    #endregion

    #region Draw Pile
    public void SetupDrawPile() {
        if (drawPilePlaceholder == null) {
            drawPilePlaceholder = Core.Cards.CardManager.instance.CreateCard(CardEntity.CardType.Placeholder, null, drawPileTransform);
            drawPilePlaceholder.backButton.onClick.AddListener(delegate { OnDrawPileClicked(); });
        }

        ToggleDrawPlaceholder(true);
    }
    public void ToggleDrawPlaceholder(bool state) {
        if (drawPilePlaceholder != null)
            drawPilePlaceholder.gameObject.SetActive(state);
    }


    #endregion

    #region Pair Pile
    public void SetupPairPile() {
        if (pairPilePlaceholder == null)
            pairPilePlaceholder = Core.Cards.CardManager.instance.CreateCard(CardEntity.CardType.Placeholder, null, pairPileTransform);

        pairPilePlaceholder.gameObject.SetActive(false);
    }

    public void TogglePairPlaceholder(bool state) {
        if (pairPilePlaceholder != null)
            pairPilePlaceholder.gameObject.SetActive(state);
    }


    #endregion

    #region Discard Pile
    public void SetDiscardPileTop(CardData cardToSet, bool clearSelection = false) {
        if(discardTop == null) {
            discardTop = Core.Cards.CardManager.instance.CreateCard(CardEntity.CardType.Draw, cardToSet.cardSlug,  discardPileTransform);
            discardTop.frontButton.onClick.AddListener(delegate { OnDiscardPileClicked(); });
        }
        else {
            if (clearSelection && discardTop.isSelected)
                ClearSelection();

            discardTop.InitCard(cardToSet.cardSlug, CardEntity.CardType.Draw);
        }

        ToggleDiscardPile(true);
    }
 
    public void RemoveTopDiscardCard() {
        print("removing top card");
        if (discardTop != null)
            discardTop.CloneCardToPair();

        discardPile.RemoveAt(discardPile.Count - 1);
        CardData nextCard = getLastDiscard();
        if(nextCard == null) {
            ToggleDiscardPile(false);
            print("next card null");
        }
        else {
            print("next card!");
            SetDiscardPileTop(nextCard);
        }
    }

    public CardData getLastDiscard() {
        if(discardPile.Count > 0) {
            return discardPile[discardPile.Count - 1];
        }

        return null;
    }

    public void ToggleDiscardPile(bool state) {
        discardTop.gameObject.SetActive(state);
    }

    #endregion





    

    

}
