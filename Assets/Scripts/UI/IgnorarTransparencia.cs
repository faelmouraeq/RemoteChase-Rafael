using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IgnorarTransparencia : MonoBehaviour
{
    void Start()
    {
        // Apenas regista o clique se o pixel tiver mais de 10% de opacidade (visível)
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }
}
