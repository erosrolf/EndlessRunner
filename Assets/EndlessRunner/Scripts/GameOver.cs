using System.Collections;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

namespace EndlessRunner.Scripts
{
    public class GameOver : MonoBehaviour
    {
        [SerializeField] private GameObject _gameOverCanvas;
        [SerializeField] private TMP_InputField _playerNameInputField;
        [SerializeField] private TextMeshProUGUI _leaderboardScoreText;
        [SerializeField] private TextMeshProUGUI _leaderboardNameText;

        private int _score;
        private string _leaderboardID = "24927";
        private int _leaderboardTopCount = 10;
        
        public void StopGame(int score)
        {
            _score = score;
            _gameOverCanvas.SetActive(true);
            SubmitScore();
        }

        public void RestartLevel()
        {
            
        }

        public void SubmitScore()
        {
            StartCoroutine(SubmitScoreToLeaderboard());
        }
        
        public void AddXP(int score)
        {
            
        }

        private IEnumerator SubmitScoreToLeaderboard()
        {
            bool? nameSet = null;
            LootLockerSDKManager.SetPlayerName(_playerNameInputField.text, (response) =>
            {
                if (response.success)
                {
                    Debug.Log("Successfully set the player name");
                    nameSet = true;
                }
                else
                {
                    Debug.Log("Was not able to add the player name");
                    nameSet = false;
                }
            });
            yield return new WaitUntil(() => nameSet.HasValue);
            if (!nameSet.Value) yield break;
            bool? scoreSubmitted = null;
            LootLockerSDKManager.SubmitScore("", _score, _leaderboardID, (response) =>
            {
                if (response.success)
                {
                    Debug.Log("Successfully submitted the score to the leaderboard");
                    scoreSubmitted = true;
                }
                else
                {
                    Debug.Log("Was not able to submit the score to ther leaderboard");
                    scoreSubmitted = false;
                }
            });
            yield return new WaitUntil(() => scoreSubmitted.HasValue);
            if (!scoreSubmitted.Value) yield break;
            GetLeaderboard();
        }

        private void GetLeaderboard()
        {
            LootLockerSDKManager.GetScoreList(_leaderboardID, _leaderboardTopCount, (response) =>
            {
                if (response.success)
                {
                    Debug.Log("Successfully retrieved scores from the leaderboard");
                    string leaderboardName = "";
                    string leaderboardScore = "";
                    LootLockerLeaderboardMember[] members = response.items;
                    for (int i = 0; i < members.Length; i++)
                    {
                        LootLockerPlayer player = members[i].player;
                        if (player == null) continue;

                        if (player.name != "")
                        {
                            leaderboardName += members[i].player.name + '\n';
                        }
                        else
                        {
                            leaderboardName += members[i].player.id + '\n';
                        }
                        leaderboardScore += members[i].score + '\n';
                    }
                    _leaderboardNameText.SetText(leaderboardName);
                    _leaderboardScoreText.SetText(leaderboardScore);
                }
                else
                {
                    Debug.Log("Failed to get scores from the leaderboard");
                }
            });
        }
    }
}