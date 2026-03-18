using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(UIDocument))]
public class ExtrasController : MonoBehaviour
{
    [Header("Escena de destino al hacer Back")]
    public string escenaMenu = "Menu";

    private const string ID_BTN_BACK = "BackBtn";

    private UIDocument doc;
    private bool       registrado = false;

    void Awake()
    {
        doc = GetComponent<UIDocument>();
    }

    void OnEnable() { StartCoroutine(IniciarDelay()); }

    IEnumerator IniciarDelay() { yield return null; Iniciar(); }

    void Iniciar()
    {
        if (doc?.rootVisualElement == null) return;
        var root = doc.rootVisualElement;

        if (!registrado)
        {
            var btnBack = BuscarBoton(root, ID_BTN_BACK, "back", "Back", "volver", "menu");

            if (btnBack != null)
            {
                btnBack.RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene(escenaMenu));
                btnBack.RegisterCallback<MouseEnterEvent>(_ => btnBack.style.opacity = 0.6f);
                btnBack.RegisterCallback<MouseLeaveEvent>(_ => btnBack.style.opacity = 1f);
                Debug.Log("[Extras] BtnBack registrado -> " + escenaMenu);
            }
            else
            {
                Debug.LogWarning("[Extras] No encontre el boton Back. ID buscado: " + ID_BTN_BACK);
            }

            registrado = true;
        }
    }

    VisualElement BuscarBoton(VisualElement root, params string[] ids)
    {
        foreach (var id in ids)
        {
            var el = root.Q<VisualElement>(id);
            if (el != null) return el;
        }
        foreach (var id in ids)
        {
            var el = root.Query<VisualElement>().Where(e =>
                (e.name ?? "").ToLower().Contains(id.ToLower())).First();
            if (el != null) return el;
        }
        return null;
    }
}
