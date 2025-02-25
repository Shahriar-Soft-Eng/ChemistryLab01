using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
public class UiManager : MonoBehaviour
{
    [SerializeField] private GameObject m_SplashScreenPanel;
    [SerializeField] private GameObject m_HubPanel;
    [SerializeField] private Image m_ImageLoading;
    [SerializeField] private float m_LoadingDuration;
    [SerializeField] private UnityEvent m_SimulationStartEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_SplashScreenPanel.SetActive(true);
        m_ImageLoading.DOFillAmount(1.0f, m_LoadingDuration)
            .SetEase(Ease.Linear)
            .SetLoops(1,LoopType.Restart)
            .OnComplete(() =>
            {
                m_SplashScreenPanel.SetActive(false);
                m_HubPanel.SetActive(true);
                m_SimulationStartEvent?.Invoke();
            });
    }

}
