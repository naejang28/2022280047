using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class BGMDropdown : MonoBehaviour
{
    private TMP_Dropdown dd;

    void Start()
    {
        dd = GetComponent<TMP_Dropdown>();

        var bgm = BGMManager.Instance;
        if (bgm == null) return;

        dd.ClearOptions();
        var names = new System.Collections.Generic.List<string>();
        foreach (var t in bgm.tracks) names.Add(t.title);   // 목록에는 음표 없이
        dd.AddOptions(names);

        dd.SetValueWithoutNotify(Mathf.Max(bgm.CurrentIndex, 0));

        dd.onValueChanged.AddListener(i =>
        {
            BGMManager.Instance.Play(i);
            if (DropdownTooltip.Instance != null) DropdownTooltip.Instance.Hide();
        });

        // 곡 목록을 채운 뒤 크기 갱신 (DropdownAutoSize를 쓰는 경우)
        var auto = GetComponent<DropdownAutoSize>();
        if (auto != null) auto.Resize();
    }

    // 닫혀 있는 Label(현재 곡 이름)에만 음표를 붙임
    void LateUpdate()
    {
        if (dd == null || dd.captionText == null || dd.options.Count == 0) return;

        string wanted = "♪ " + dd.options[dd.value].text;
        if (dd.captionText.text != wanted)
            dd.captionText.text = wanted;
    }
}