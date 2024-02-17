using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Components.Agents;

public class UI_AgentInfo : MonoBehaviour
{
    public static UI_AgentInfo Instance {  get; private set; }

    [SerializeField] private GameObject uiPanel;
    [SerializeField] private TextMeshProUGUI text_AgentName;
    [SerializeField] private Slider slider_HpValue;
    [SerializeField] private Image image_HpSliderFill;
    [SerializeField] private TextMeshProUGUI text_HpValue;
    [SerializeField] private Color[] colors_HpSlider = new Color[2];
    IAgent agent;

    private void Awake() 
    {
        Instance = this;
        uiPanel.SetActive(false);
    }

    public void SetAgent(IAgent agentP)
    {
        if(agentP != null)
        {
            agent = agentP;

            text_AgentName.text = agent.GetAgentName();
            text_HpValue.text = agent.GetAgentCurrentHp().ToString();

            slider_HpValue.maxValue = agent.GetAgentMaxHp();
            slider_HpValue.value = agent.GetAgentCurrentHp();
            UpdateSliderColor();

            agent.Event_OnHpValueChange += OnHpChange;
            agent.Event_OnAgentRelease += OnAgentRelease;

            uiPanel.SetActive(true);
        }
        else
        {
            uiPanel.SetActive(false);
            RemoveAgentReferences();
        }
    }
    private void RemoveAgentReferences()
    {
        agent.Event_OnHpValueChange -= OnHpChange;
        agent.Event_OnAgentRelease -= OnAgentRelease;
        agent = null;
    }
    private void UpdateSliderColor()
    {
        if (slider_HpValue.value <= 1) image_HpSliderFill.color = colors_HpSlider[1];
        else image_HpSliderFill.color = colors_HpSlider[0];
    }

    // Agent events //
    public void OnHpChange(int currentHp)
    {
        text_HpValue.text = currentHp.ToString();
        slider_HpValue.value = currentHp;
        UpdateSliderColor();
    }
    public void OnAgentRelease()
    {
        RemoveAgentReferences();
        uiPanel.SetActive(false);
    }
}
