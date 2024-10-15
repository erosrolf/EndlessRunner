using System.Collections;
using UnityEngine;
using LootLocker.Requests;
using UnityEngine.Events;

namespace EndlessRunner.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private UnityEvent _playerConnected;
        
        private IEnumerator Start()
        {
            bool connected = false;
            LootLockerSDKManager.StartGuestSession((response) =>
            {
                if (!response.success)
                {
                    Debug.Log("Error starting LootLocker session");
                    return;
                }
                Debug.Log("Successfully LootLocker session");
                connected = true;
            });
            yield return new WaitUntil(() => connected);
            _playerConnected?.Invoke();
        }
    }
}