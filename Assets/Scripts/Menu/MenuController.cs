using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    [Header("Escenas")]
    public string escenaJuego    = "GlobalLevels";
    public string escenaOpciones = "Options";
    public string escenaExtras   = "Extras";

    [Header("Velocidad fade icono en segundos")]
    public float velocidadFade = 0.18f;

    private const string ID_START   = "StartGameRow";
    private const string ID_OPTIONS = "OptionsRow";
    private const string ID_EXTRAS  = "ExtrasRow";
    private const string ID_QUIT    = "QuitGameRow";

    private List<VisualElement> rows = new List<VisualElement>();
    private Dictionary<VisualElement, Coroutine> fades = new Dictionary<VisualElement, Coroutine>();

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null) return;
        var root = doc.rootVisualElement;
        rows.Clear();
        fades.Clear();

        Bind(root, ID_START,   () => IrAJugar());
        Bind(root, ID_OPTIONS, () => IrAOpciones());
        Bind(root, ID_EXTRAS,  () => IrAExtras());
        Bind(root, ID_QUIT,    () => Salir());

        foreach (var r in rows) PintarIconos(r, 0f);
    }

    void Bind(VisualElement root, string id, System.Action click)
    {
        var row = root.Q<VisualElement>(id);
        if (row == null) { Debug.LogWarning("[Menu] No encontre: " + id); return; }
        rows.Add(row);
        row.RegisterCallback<MouseEnterEvent>(_ => {
            foreach (var r in rows) if (r != row) Fade(r, 0f);
            Fade(row, 1f);
        });
        row.RegisterCallback<MouseLeaveEvent>(_ => Fade(row, 0f));
        row.RegisterCallback<ClickEvent>(_ => click());
    }

    void Fade(VisualElement row, float to)
    {
        if (fades.TryGetValue(row, out var c) && c != null) StopCoroutine(c);
        fades[row] = StartCoroutine(DoFade(row, to));
    }

    IEnumerator DoFade(VisualElement row, float to)
    {
        var hijos = new List<VisualElement>();
        foreach (var h in row.Children()) hijos.Add(h);
        VisualElement izq = null, der = null;

        foreach (var h in hijos)
        {
            string n = (h.name ?? "").ToLower();
            if (n.Contains("left")  || n.Contains("izq"))  izq = h;
            if (n.Contains("right") || n.Contains("der"))  der = h;
            if (h.ClassListContains("menu-icon-left"))  izq = h;
            if (h.ClassListContains("menu-icon-right")) der = h;
        }

        if (izq == null && hijos.Count >= 1) izq = hijos[0];
        if (der == null && hijos.Count >= 2) der = hijos[hijos.Count - 1];

        float from = izq != null ? izq.resolvedStyle.opacity : 0f;
        float t = 0f;
        while (t < velocidadFade)
        {
            t += Time.deltaTime;
            float op = Mathf.Lerp(from, to, Mathf.Clamp01(t / velocidadFade));
            if (izq != null) izq.style.opacity = op;
            if (der != null) der.style.opacity = op;
            yield return null;
        }
        if (izq != null) izq.style.opacity = to;
        if (der != null) der.style.opacity = to;
    }

    void PintarIconos(VisualElement row, float op)
    {
        var hijos = new List<VisualElement>();
        foreach (var h in row.Children()) hijos.Add(h);
        if (hijos.Count >= 1) hijos[0].style.opacity = op;
        if (hijos.Count >= 2) hijos[hijos.Count - 1].style.opacity = op;
    }

    void IrAJugar()
    {
        if (string.IsNullOrEmpty(escenaJuego)) { Debug.LogError("[Menu] escenaJuego vacio."); return; }
        SceneManager.LoadScene(escenaJuego);
    }

    void IrAOpciones()
    {
        if (string.IsNullOrEmpty(escenaOpciones)) { Debug.LogError("[Menu] escenaOpciones vacio."); return; }
        SceneManager.LoadScene(escenaOpciones);
    }

    void IrAExtras()
    {
        if (string.IsNullOrEmpty(escenaExtras)) { Debug.LogError("[Menu] escenaExtras vacio."); return; }
        SceneManager.LoadScene(escenaExtras);
    }

    void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
