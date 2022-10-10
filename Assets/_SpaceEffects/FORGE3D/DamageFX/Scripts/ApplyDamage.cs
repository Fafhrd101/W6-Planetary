using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyDamage : MonoBehaviour
{
    public bool DebugMode;
    public bool ShowOnGuiInfo = true;
    public float HitRadius = 0.1f;
    public float Dirt = 1f;
    public float Burn = 1f;
    public float Heat = 1f;
    public float Clip = 0.7f;
    private RaycastHit _hitInfo;
    public Transform ImpactFX;
    public float ImpactSize = 0.3f;

    private void DealDamage()
    {
        var dfx = _hitInfo.collider.GetComponent<DamageFX>();
        if (dfx != null)
            dfx.Hit(dfx.transform.InverseTransformPoint(_hitInfo.point), HitRadius, Dirt, Burn, Heat, Clip);
    }

    private void OnGUI()
    {
        if (!ShowOnGuiInfo) return;
        GUILayout.Label("  HitRadius: " + HitRadius + "\n" +
                        "  Dirt: " + Dirt + "\n" +
                        "  Burn: " + Burn + "\n" +
                        "  Heat: " + Heat + "\n" +
                        "  Clip: " + Clip);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W)) HitRadius += 0.01f;
        if (Input.GetKey(KeyCode.S)) HitRadius -= 0.01f;
        if (Input.GetKey(KeyCode.Q)) Dirt -= 0.01f;
        if (Input.GetKey(KeyCode.E)) Dirt += 0.01f;
        if (Input.GetKey(KeyCode.A)) Heat -= 0.01f;
        if (Input.GetKey(KeyCode.D)) Heat += 0.01f;
        if (Input.GetKey(KeyCode.Alpha1)) Clip -= 0.01f;
        if (Input.GetKey(KeyCode.Alpha2)) Clip += 0.01f;
        if (Input.GetKey(KeyCode.Z)) Burn -= 0.01f;
        if (Input.GetKey(KeyCode.X)) Burn += 0.01f;
        Dirt = Mathf.Clamp01(Dirt);
        Heat = Mathf.Clamp01(Heat);
        Clip = Mathf.Clamp01(Clip);
        Burn = Mathf.Clamp01(Burn);
        HitRadius = Mathf.Max(0, HitRadius);
        if (DebugMode)
        {
            var overlapp = Physics.OverlapSphere(transform.position, transform.lossyScale.x);
            if (overlapp == null) return;
            for (var i = 0; i < overlapp.Length; i++)
            {
                var dfx = overlapp[i].GetComponent<DamageFX>();
                if (dfx)
                    dfx.Hit(dfx.transform.InverseTransformPoint(transform.position), HitRadius, Dirt, Burn, Heat,
                        Clip);
            }
            return;
        }
        var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetKeyDown(KeyCode.Space))
            if (Physics.Raycast(screenRay, out _hitInfo, Mathf.Infinity))
            {
                var dfx = _hitInfo.transform.root.GetComponentsInChildren<DamageFX>();
                if (dfx != null && dfx.Length > 0)
                    foreach (var d in dfx)
                        d.Clear();
            }
        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1)) return;
        if (!Physics.Raycast(screenRay, out _hitInfo, Mathf.Infinity)) return;
        DealDamage();
        if (!ImpactFX) return;
        var fx = Instantiate(ImpactFX, _hitInfo.point, Quaternion.LookRotation(_hitInfo.normal));
        fx.localScale = Vector3.one * HitRadius + Vector3.one * ImpactSize;
    }
}