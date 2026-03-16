using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class VideoOptionsController : MonoBehaviour
{
    private const string KEY_RES = "video_res_index";
    private const string KEY_FS  = "video_fullscreen";

    private static readonly Color C_ACT  = new Color(0.94f, 0.95f, 1.00f, 1f);
    private static readonly Color C_INAC = new Color(0.75f, 0.77f, 0.88f, 0.7f);
    private static readonly Color I_VIS  = new Color(0.75f, 0.77f, 0.93f, 1.0f);
    private static readonly Color I_HID  = new Color(0.75f, 0.77f, 0.93f, 0.0f);

    private class Row
    {
        public string key, idL, idR, idLbl, idVal;
        public string[] opts;
        public int idx;
        public System.Action<int> onChange;
    }

    private List<Row>         rows;
    private int               activeRow = 0;
    private UIDocument        doc;
    private OptionsController optsCtrl;
    private bool              bound = false;

    void Awake()
    {
        doc      = GetComponent<UIDocument>();
        optsCtrl = FindFirstObjectByType<OptionsController>();
    }

    void OnEnable() { StartCoroutine(DelayInit()); }

    IEnumerator DelayInit() { yield return null; Init(); }

    void Init()
    {
        if (doc?.rootVisualElement == null) return;
        var r = doc.rootVisualElement;

        rows = new List<Row> {
            new Row { key=KEY_RES, idL="SelL_Resolution", idR="SelR_Resolution",
                idLbl="Label_Resolution", idVal="Value_Resolution",
                opts=new[]{"3840 X 2160 @ 60HZ","2560 X 1440 @ 60HZ","1920 X 1080 @ 60HZ","1280 X 720 @ 60HZ"},
                onChange=i=>ApplyRes(i) },
            new Row { key=KEY_FS, idL="SelL_FullScreen", idR="SelR_FullScreen",
                idLbl="Label_FullScreen", idVal="Value_FullScreen",
                opts=new[]{"ON","OFF"},
                onChange=i=>{ Screen.fullScreenMode=i==0?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed; } },
        };

        foreach (var row in rows)
        {
            row.idx = Mathf.Clamp(PlayerPrefs.GetInt(row.key, 0), 0, row.opts.Length - 1);
            SetLabel(r, row);
        }
        SelectRow(r, 0);

        if (!bound)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                int n = i; var row = rows[i];
                var re = r.Q<VisualElement>(row.idLbl)?.parent ?? r.Q<VisualElement>(row.idL)?.parent;
                if (re != null) re.RegisterCallback<MouseEnterEvent>(_ => SelectRow(r, n));
                var sl = r.Q<VisualElement>(row.idL);
                var sr = r.Q<VisualElement>(row.idR);
                if (sl != null) sl.RegisterCallback<ClickEvent>(_ => Cycle(r, n, -1));
                if (sr != null) sr.RegisterCallback<ClickEvent>(_ => Cycle(r, n, +1));
            }
            RegBtn(r, "BtnScreenScale",   ()=>Debug.Log("[Video] ScreenScale"));
            RegBtn(r, "BtnBrightness",    ()=>Debug.Log("[Video] Brightness"));
            RegBtn(r, "BtnResetDefaults", ()=>Reset(r));
            RegBtn(r, "BtnBack",          ()=>optsCtrl?.CerrarPanelActivo());
            bound = true;
        }

        ApplyRes(rows[0].idx);
        Screen.fullScreenMode = rows[1].idx == 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    void Update()
    {
        if (doc?.rootVisualElement == null) return;
        if (doc.rootVisualElement.style.display == DisplayStyle.None) return;
        if (Keyboard.current == null || rows == null) return;
        var r = doc.rootVisualElement;
        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
            SelectRow(r, (activeRow + 1) % rows.Count);
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
            SelectRow(r, (activeRow - 1 + rows.Count) % rows.Count);
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            Cycle(r, activeRow, +1);
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            Cycle(r, activeRow, -1);
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            optsCtrl?.CerrarPanelActivo();
    }

    void SelectRow(VisualElement r, int n)
    {
        activeRow = n;
        for (int i = 0; i < rows.Count; i++)
        {
            bool a = i == n; var row = rows[i];
            Col(r, row.idL,   a ? I_VIS  : I_HID);
            Col(r, row.idR,   a ? I_VIS  : I_HID);
            Col(r, row.idLbl, a ? C_ACT  : C_INAC);
            Col(r, row.idVal, a ? C_ACT  : C_INAC);
        }
    }

    void Cycle(VisualElement r, int n, int d)
    {
        var row = rows[n];
        row.idx = (row.idx + d + row.opts.Length) % row.opts.Length;
        SetLabel(r, row);
        PlayerPrefs.SetInt(row.key, row.idx);
        PlayerPrefs.Save();
        row.onChange?.Invoke(row.idx);
    }

    void SetLabel(VisualElement r, Row row)
    { var l = r.Q<Label>(row.idVal); if (l != null) l.text = row.opts[row.idx]; }

    void Reset(VisualElement r)
    {
        foreach (var row in rows)
        { row.idx = 0; SetLabel(r, row); PlayerPrefs.SetInt(row.key, 0); row.onChange?.Invoke(0); }
        PlayerPrefs.Save();
    }

    void ApplyRes(int i)
    {
        int[] W={3840,2560,1920,1280}; int[] H={2160,1440,1080,720};
        if (i < 0 || i >= W.Length) return;
        bool fs = rows != null && rows.Count > 1 && rows[1].idx == 0;
        Screen.SetResolution(W[i], H[i], fs ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
    }

    void RegBtn(VisualElement r, string id, System.Action a)
    {
        var el = r.Q<VisualElement>(id);
        if (el == null) return;
        el.RegisterCallback<ClickEvent>(_ => a());
        el.RegisterCallback<MouseEnterEvent>(_ => el.style.color = new StyleColor(C_ACT));
        el.RegisterCallback<MouseLeaveEvent>(_ => el.style.color = new StyleColor(C_INAC));
    }

    void Col(VisualElement r, string id, Color c)
    { var el = r.Q<VisualElement>(id); if (el != null) el.style.color = new StyleColor(c); }
}
