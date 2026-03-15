using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelSelectController : MonoBehaviour
{
    [Header("Escenas de cada nivel")]
    public string escenaNivel1 = "Level_1";
    public string escenaNivel2 = "Level_2";
    public string escenaNivel3 = "Level_3";
    public string escenaNivel4 = "Level_4";
    public string escenaNivel5 = "Level_5";

    [Header("Menu principal")]
    public string escenaMenu = "Menu";

    [Header("Velocidad fade selector")]
    public float velocidadFade = 0.15f;

    private const string ID_SLOT1 = "SlotRow1";
    private const string ID_SLOT2 = "SlotRow2";
    private const string ID_SLOT3 = "SlotRow3";
    private const string ID_SLOT4 = "SlotRow4";
    private const string ID_SLOT5 = "SlotRow5";
    private const string ID_BACK  = "BackBtn";

    private bool[] bloqueados = { false, true, true, true, true };
    private List<VisualElement> slots = new List<VisualElement>();
    private Dictionary<VisualElement, Coroutine> fades = new Dictionary<VisualElement, Coroutine>();

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null) return;
        var root = doc.rootVisualElement;
        if (root == null) return;

        slots.Clear();
        fades.Clear();

        string[] ids     = { ID_SLOT1, ID_SLOT2, ID_SLOT3, ID_SLOT4, ID_SLOT5 };
        string[] escenas = { escenaNivel1, escenaNivel2, escenaNivel3, escenaNivel4, escenaNivel5 };

        for (int i = 0; i < 5; i++)
            RegistrarSlot(root, ids[i], i, escenas[i]);

        var back = root.Q<VisualElement>(ID_BACK);
        if (back != null)
            back.RegisterCallback<ClickEvent>(_ => SceneManager.LoadScene(escenaMenu));

        foreach (var s in slots)
            OcultarSelector(s);
    }

    void RegistrarSlot(VisualElement root, string slotId, int idx, string escena)
    {
        var slot = root.Q<VisualElement>(slotId);
        if (slot == null) { Debug.LogWarning("[LevelSelect] No encontre: " + slotId); return; }

        slots.Add(slot);
        bool bloqueado = bloqueados[idx];

        slot.RegisterCallback<MouseEnterEvent>(_ => {
            foreach (var s in slots) if (s != slot) IniciarFade(s, 0f);
            if (!bloqueado) IniciarFade(slot, 1f);
            else slot.style.opacity = 0.45f;
        });

        slot.RegisterCallback<MouseLeaveEvent>(_ => {
            IniciarFade(slot, 0f);
            if (bloqueado) slot.style.opacity = 1f;
        });

        slot.RegisterCallback<ClickEvent>(_ => {
            if (bloqueado) { Debug.Log("[LevelSelect] Nivel bloqueado"); return; }
            if (!string.IsNullOrEmpty(escena)) SceneManager.LoadScene(escena);
        });
    }

    void IniciarFade(VisualElement slot, float to)
    {
        if (fades.TryGetValue(slot, out var c) && c != null) StopCoroutine(c);
        fades[slot] = StartCoroutine(DoFade(slot, to));
    }

    IEnumerator DoFade(VisualElement slot, float to)
    {
        var sel = ObtenerSelector(slot);
        if (sel == null) yield break;

        float from = sel.resolvedStyle.opacity;
        float t = 0f;
        while (t < velocidadFade)
        {
            t += Time.deltaTime;
            sel.style.opacity = Mathf.Lerp(from, to, Mathf.Clamp01(t / velocidadFade));
            yield return null;
        }
        sel.style.opacity = to;
    }

    VisualElement ObtenerSelector(VisualElement slot)
    {
        foreach (var h in slot.Children())
        {
            string n = (h.name ?? "").ToLower();
            if (n.Contains("selector") || n.Contains("sel") || n.Contains("arrow"))
                return h;
            if (h.ClassListContains("slot-selector")) return h;
        }
        foreach (var h in slot.Children()) return h;
        return null;
    }

    void OcultarSelector(VisualElement slot)
    {
        var sel = ObtenerSelector(slot);
        if (sel != null) sel.style.opacity = 0f;
    }
}
