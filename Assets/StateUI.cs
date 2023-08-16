namespace Core.UI
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class StateUI : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI headerText;
        public TMPro.TextMeshProUGUI subheaderText;
        public Button playButton;
        public Button quitButton;

        public void LoadVictoryUI() {
            SetUIText("Victory!", "Congratulations, Pyramid Cleared.");
            ToggleStateUI(true);
        }

        public void LoadDefeatUI() {
            SetUIText("Game Over!", "There is no available moves remaining");
            ToggleStateUI(true);
        }

        public void ToggleStateUI(bool state) {
            gameObject.SetActive(state);
        }

        public void SetUIText(string header, string subheader) {
            headerText.text = header;
            subheaderText.text = subheader;
        }

        public void PlayButtonPressed() {
            GameManager.instance.StartNewGame();
            ToggleStateUI(false);
        }

        public void QuitButtonPressed() {
            GameManager.QuitGame();
        }
    }

}