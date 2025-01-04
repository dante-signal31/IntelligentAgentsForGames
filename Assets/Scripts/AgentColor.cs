using UnityEngine;

/// <summary>
/// This component changes its GameObject's sprite renderer color.
/// </summary>
public class AgentColor : MonoBehaviour
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Color for this agent.")] 
    [SerializeField] private Color agentColor;

    [Header("WIRING:")]
    [Tooltip("Agent sprite renderer to change color.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    public Color Color
    {
        get => agentColor;
        set 
        {
            if (agentColor == value) return;
            agentColor = value;
            spriteRenderer.color = agentColor;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer.color = agentColor;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        spriteRenderer.color = agentColor;
    }
    
#endif
}
