using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimulationController : MonoBehaviour
{
    [Header("Tween Animation Property")]
    [SerializeField] private TweenProperty m_CameraTweenProperty;
    [Space(5)]
    [Header("GameObjects")]
    [SerializeField] private List<GameObject> m_ChemistryLabObjects;
    [SerializeField] private GameObject m_StepDetailsPanel;
    [SerializeField] private GameObject m_TutorialHand;
    [SerializeField] private GameObject m_TutorialHand_Bakar;
    [SerializeField] private GameObject m_TutorialHand_BakarHotState;
    [SerializeField] private GameObject m_TutorialHand_Acetate;
    [SerializeField] private GameObject m_Spoon;
    [SerializeField] private GameObject m_WaterObj;
    [SerializeField] private GameObject m_FinalWaterObj;
    [SerializeField] private GameObject m_Bakar;
    [SerializeField] private GameObject m_ResultBakar;
    [SerializeField] private GameObject m_Solt;
    [SerializeField] private GameObject m_HotWaterBubble;
    [SerializeField] private GameObject m_FlameEffect;
    [SerializeField] private GameObject m_FinalBakarWatar;
    [SerializeField] private GameObject m_FinalResultPanel;
    [Header("Matarials")]
    [SerializeField] private Material m_whiteWaterMaterial;
    [SerializeField] private Material m_AcetateMaterial;
    [Header("UI Objects")]
    [SerializeField] private Image m_FadeInOutLayer;
    [Header("UI Buttons")]
    [SerializeField] private Button m_StepDetailsPanelCrossButton;  
    [SerializeField] private Button m_RestartButton;  
    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI m_TextStepDetailsPanelTitle;
    [SerializeField] private TextMeshProUGUI m_TextStepDetailsPanelDiscription;
    [Header("Simulation Data")]
    [SerializeField] private List<SimulationData> m_SimulationSteps;
    [Header("Tween Extra Datas")]
    [SerializeField] private float m_SpoonMoveUpLimit;
    [SerializeField] private float m_BakarMoveUpLimit;

    #region Private_Variables
    private Camera _cameraMain;
    private int _currentStep;
    private int _totalStep;
    private Transform _TempTransformData;
    private bool _isTweeningStart, _isStepIntroShowDone;
    private List<Sequence> _runningTweenSequence;
    private int _TapCount;
    private Vector3 _TempBakarPos;
    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitVariables();
    }
    private void InitVariables()
    {
        _cameraMain = Camera.main;
        _currentStep = 0;
        _TapCount = 0;
        _isTweeningStart = false;
        _isStepIntroShowDone = false;
        _totalStep = m_SimulationSteps.Count-1;
        _runningTweenSequence = new List<Sequence>();
        m_StepDetailsPanelCrossButton.onClick.AddListener(() =>{ StartSimulation(_currentStep); });
        m_RestartButton.onClick.AddListener(() =>{ SceneManager.LoadScene(0); });
    }
    private void Update()
    {
        if(Input.GetMouseButton(0) && !_isTweeningStart && _isStepIntroShowDone) CheckRayTraching(new string[] { "Spoon","Bakar","Candle" });
    }
    private void CheckRayTraching(string[] objectsTag)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider != null)
            {
                if (hit.collider.transform.CompareTag(objectsTag[0]) && _currentStep == 0) // This logic for Step-1
                {
                    m_TutorialHand.SetActive(false);
                    Step1Initialization(_currentStep);
                    _isTweeningStart=true;
                    _isStepIntroShowDone=false;
                }            
                else if (hit.collider.transform.CompareTag(objectsTag[1]) && _currentStep == 1) // This logic for Step-2
                {
                    if(_TapCount == 0)
                    {
                        m_TutorialHand_Bakar.SetActive(false);
                        Step2Initialization(_currentStep);
                        _isTweeningStart = true;
                        _isStepIntroShowDone = false;
                        _TapCount++;
                    }
                    else // This will call when use click on hot Bakar
                    {

                        _isTweeningStart = true;
                        _isStepIntroShowDone = false;
                        m_TutorialHand_BakarHotState.SetActive(false);
                        Sequence sequence = DOTween.Sequence();
                        sequence.Append(m_Bakar.transform.DOMoveY(m_Bakar.transform.position.y + m_BakarMoveUpLimit/2, 0.25f));
                        sequence.Append(m_Bakar.transform.DOMoveY(m_Bakar.transform.position.y - m_BakarMoveUpLimit/2, 0.25f));
                        sequence.Join(m_Bakar.transform.DOMove(_TempBakarPos, 1));
                        sequence.AppendCallback(() => { m_FlameEffect.SetActive(false); m_HotWaterBubble.SetActive(false); });
                        sequence.AppendInterval(2f);
                        sequence.AppendCallback(() => { PrepareForNextStep(); _TapCount = 0; });
                        sequence.Play();
                    }

                }
                else if (hit.collider.transform.CompareTag(objectsTag[1]) && _currentStep == 2 || hit.collider.transform.CompareTag(objectsTag[0]) && _currentStep == 2) // This logic for Step-3
                {
                    if(_TapCount == 0)
                    {
                        _TapCount++;
                        m_TutorialHand_Bakar.SetActive(false);
                        Step3Initialization(_currentStep);
                        _isTweeningStart = true;
                        _isStepIntroShowDone = false;
                    }
                    else
                    {
                        _isTweeningStart = true;
                        _isStepIntroShowDone = false;
                        m_TutorialHand_Acetate.SetActive(false);
                        SimulationData simulationData = m_SimulationSteps[_currentStep];
                        Sequence sequence = DOTween.Sequence();
                        sequence.Append(
                            m_Spoon.transform.DOMove(simulationData.TweenPropertyList[3].TweenTransformData.position, 1f)
                            .OnComplete(() => { m_Spoon.transform.GetChild(1).gameObject.SetActive(true); })
                            );
                        sequence.Join(
                            m_Spoon.transform.DORotate(simulationData.TweenPropertyList[3].TweenTransformData.eulerAngles, 1f)
                            );
                        sequence.Append(
                            m_Spoon.transform.DOMove(simulationData.TweenPropertyList[4].TweenTransformData.position, 1f)
                            .OnComplete(() => { m_Spoon.transform.GetChild(1).gameObject.SetActive(false); })
                            );
                        sequence.Join(
                            m_Spoon.transform.DORotate(simulationData.TweenPropertyList[4].TweenTransformData.eulerAngles, 1f)
                            );
                        sequence.AppendCallback(() => 
                        {
                            m_FinalWaterObj.GetComponent<MeshRenderer>().material = m_AcetateMaterial;
                        });
                        sequence.AppendInterval(0.5f);
                        sequence.Append(m_ResultBakar.transform.DOMove(simulationData.TweenPropertyList[5].TweenTransformData.position, 1f));
                        sequence.AppendCallback(() => { m_FinalResultPanel.SetActive(true); });
                        sequence.Play();
                    }

                }
            }
        }
    }


    public void StartSimulation()
    {
        Sequence tweenSequence = DOTween.Sequence();
        tweenSequence.Append(
            _cameraMain.transform.DOMove(m_CameraTweenProperty.TweenTransformData.position, m_CameraTweenProperty.TweenDuration)
        .SetEase(m_CameraTweenProperty.TweenEase)
        ).
        Join(
            _cameraMain.transform.DORotate(m_CameraTweenProperty.TweenTransformData.eulerAngles, m_CameraTweenProperty.TweenDuration)
        .SetEase(m_CameraTweenProperty.TweenEase)
        );
        tweenSequence.Append(m_FadeInOutLayer.DOFade(1.0f,1f));
        tweenSequence.AppendCallback(() => 
        {
            foreach (var go in m_ChemistryLabObjects)
            {
                go.SetActive(true);
            }
        });
        tweenSequence.Append(m_FadeInOutLayer.DOFade(0.0f,1f));
        tweenSequence.AppendInterval(0.3f);
        tweenSequence.AppendCallback(() => { OpenStepPanel(_currentStep); });
        tweenSequence.Play();
        _runningTweenSequence.Add(tweenSequence);
    }

    private void OpenStepPanel(int index)
    {
        if(index == 1 || index == 2)
        {
            m_TutorialHand_Bakar.SetActive(true);
        }
        m_TextStepDetailsPanelTitle.text = $"Step - {index+1}";
        m_TextStepDetailsPanelDiscription.text = m_SimulationSteps[index].StepDescription;
        m_StepDetailsPanel.SetActive(true);
    } 
    private void StartSimulation(int index)
    {
        m_StepDetailsPanel.SetActive(false);
        if(_currentStep == 0) m_TutorialHand.SetActive(true);
        _isStepIntroShowDone = true;
    }

    #region Step-1(Animation)
    private void Step1Initialization(int index)
    {
        SimulationData simulationData = m_SimulationSteps[index];
        float duration = simulationData.TweenPropertyList[0].TweenDuration;
        _TempTransformData = m_Spoon.transform;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(
            m_Spoon.transform.DOMoveY(m_Spoon.transform.position.y + m_SpoonMoveUpLimit, duration / 2)
                             .SetEase(Ease.Linear)
            );
        sequence.Append(
        m_Spoon.transform.DOMoveY(m_Spoon.transform.position.y - m_SpoonMoveUpLimit, duration / 2)
                         .SetEase(Ease.Linear)
        );
        sequence.Join(m_Spoon.transform.DOMove(simulationData.TweenPropertyList[0].TweenTransformData.position, duration));    
        sequence.Join(m_Spoon.transform.DORotate(simulationData.TweenPropertyList[0].TweenTransformData.eulerAngles, duration));
        sequence.AppendCallback(() => { m_Spoon.transform.GetChild(0).gameObject.SetActive(true); });
        sequence.AppendInterval(0.5f);
        sequence.Append(m_Spoon.transform.DOMoveY(m_Spoon.transform.position.y + m_SpoonMoveUpLimit, duration));
        sequence.Append(m_Spoon.transform.DOMoveY(m_Spoon.transform.position.y - m_SpoonMoveUpLimit, duration));
        sequence.Join(m_Spoon.transform.DOMove(_TempTransformData.position, duration));
        sequence.Join(m_Spoon.transform.DORotate(_TempTransformData.eulerAngles, duration));
        sequence.Append(m_Spoon.transform.DORotate(new Vector3(0, 55, 0), 0.25f).SetEase(Ease.Linear).SetLoops(4, LoopType.Yoyo));
        sequence.AppendCallback(() =>
        {
            m_Spoon.transform.GetChild(0).gameObject.SetActive(false);
            m_WaterObj.GetComponent<MeshRenderer>().material = m_whiteWaterMaterial;
        });
        sequence.AppendCallback(() => 
        {
            m_Spoon.SetActive(false);
            m_Solt.SetActive(false);
            PrepareForNextStep();
        });
        sequence.Play();
        _runningTweenSequence.Add(sequence);
    }
    #endregion  
    #region Step-2(Animation)
    private void Step2Initialization(int index)
    {
        SimulationData simulationData = m_SimulationSteps[index];
        float duration = simulationData.TweenDuration;
        _TempTransformData = m_Bakar.transform;
        _TempBakarPos = m_Bakar.transform.position;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(m_Bakar.transform.DOMoveY(m_Bakar.transform.position.y + m_BakarMoveUpLimit, duration / 2));
        sequence.Append(m_Bakar.transform.DOMoveY(m_Bakar.transform.position.y - m_BakarMoveUpLimit, duration / 2));
        sequence.Join(m_Bakar.transform.DOMove(simulationData.TweenPropertyList[0].TweenTransformData.position, duration));
        sequence.AppendCallback(() => { m_FlameEffect.SetActive(true); });
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() => { m_HotWaterBubble.SetActive(true); });
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() => { m_TutorialHand_BakarHotState.SetActive(true); _isStepIntroShowDone = true; _isTweeningStart = false; });
        sequence.Play();
    }
    #endregion
    #region Step-3(Animation)
    private void Step3Initialization(int index)
    {
        SimulationData simulationData = m_SimulationSteps[index];
        float duration = simulationData.TweenDuration;
        _TempTransformData = m_Bakar.transform;
        _TempBakarPos = m_Bakar.transform.position;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(m_Bakar.transform.DOMove(simulationData.TweenPropertyList[0].TweenTransformData.position, duration));
        sequence.Join(m_Bakar.transform.DORotate(simulationData.TweenPropertyList[0].TweenTransformData.eulerAngles, duration));
        sequence.AppendInterval(1f);
        sequence.Append(m_Bakar.transform.DOMove(_TempBakarPos, duration));
        sequence.Join(m_Bakar.transform.DORotate(_TempTransformData.eulerAngles, duration));
        sequence.AppendCallback(() => 
        { 
            m_FinalBakarWatar.SetActive(true); 
            m_Solt.SetActive(true);
            m_Solt.transform.position = simulationData.TweenPropertyList[2].TweenTransformData.position;
            m_Spoon.SetActive(true);
            m_Spoon.transform.position = simulationData.TweenPropertyList[1].TweenTransformData.position;
            m_Spoon.transform.rotation = simulationData.TweenPropertyList[1].TweenTransformData.rotation;
            m_TutorialHand_Acetate.SetActive(true);
            _isTweeningStart = false;
            _isStepIntroShowDone = true;
        });
        sequence.Play();
    }
    #endregion
    private void PrepareForNextStep()
    {
        _currentStep++;
        _isTweeningStart = false;
        _runningTweenSequence.ForEach(seq => seq.Kill());
        _runningTweenSequence.Clear();
        m_TutorialHand.SetActive(false);    
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(1.5f);
        sequence.AppendCallback(() => 
        {
            OpenStepPanel(_currentStep);
        });
        sequence.Play();
        _runningTweenSequence.Add(sequence);

    }
}

[Serializable]
public class TweenProperty
{
    public string TweenName;
    public Transform TweenTransformData;
    public float TweenDuration;
    public Ease TweenEase;
}

[Serializable]
public class SimulationData
{
    public string StepName;
    public string StepDescription;
    public float TweenDuration;
    public List<TweenProperty> TweenPropertyList;
}
