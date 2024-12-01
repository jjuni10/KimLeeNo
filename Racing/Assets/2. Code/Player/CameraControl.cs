using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Position")]
    public Transform target;
    public Vector3 offset;
    public Quaternion rotation;

    [Header("Zoom")]
    public float zoomSpeed;
    public float downZoom;
    public float upZoom;
    public float minZoom;
    public float maxZoom;
    public float boostZoom;

    [Header("Ray")]
    public LayerMask mask;
    public float rayRadiusValue;
    private List<Renderer> renderers = new List<Renderer>();
    private Dictionary<Renderer, float> originalAlphas = new Dictionary<Renderer, float>();

    [SerializeField] float _previousSpeed = 0f;

    void Update()
    {
        // ī�޶� ��ġ �̵� ó��
        CameraMove();

        // ī�޶� ���� ���� ó��
        CameraRotation();

        // Ÿ�� ���� ��ֹ� ó��
        GapObject();
    }

    void CameraMove()
    {
        Vector3 cameraPos = target.position + target.rotation * offset;

        transform.position = cameraPos;
    }

    void CameraRotation()
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);

        transform.rotation = targetRotation;
    }

    public void OffsetChange(float playerSpeed, bool isBoosting)
    {
        float basicTargetZoom = Mathf.Lerp(minZoom, maxZoom, playerSpeed / maxZoom);

        if (isBoosting)
        {
            basicTargetZoom += boostZoom;
        }

        float realTargetZoom = basicTargetZoom;

        if (playerSpeed > _previousSpeed)
        {
            zoomSpeed = upZoom;
        }
        else
        {
            zoomSpeed = downZoom;
        }

        float zoomFactor = Mathf.MoveTowards(offset.magnitude, realTargetZoom, zoomSpeed * Time.deltaTime);

        offset = offset.normalized * zoomFactor;

        _previousSpeed = playerSpeed;
    }

    void GapObject()
    {
        foreach (Renderer render in renderers)
        {
            AlphaObject(render, false); // ����
        }
        renderers.Clear();

        Vector3 dir = target.position - transform.position;
        float dis = dir.magnitude - target.localScale.x;

        float radius = target.localScale.x * rayRadiusValue;

        Ray ray = new Ray(transform.position, dir);
        RaycastHit[] hits = Physics.SphereCastAll(ray, radius, dis, mask);

        foreach (RaycastHit hit in hits)
        {
            Renderer render = hit.collider.GetComponent<Renderer>();

            if (render != null && !renderers.Contains(render))
            {
                AlphaObject(render, true); // ���� ó��
                renderers.Add(render);
            }
        }

        Debug.DrawRay(transform.position, dir.normalized * dis, Color.red);
    }

    void AlphaObject(Renderer render, bool isAlpha)
    {
        Material mat = render.material;

        if (isAlpha)
        {
            // �ʱ� ���� ���� ����
            if (!originalAlphas.ContainsKey(render))
            {
                originalAlphas[render] = mat.color.a;
            }

            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            Color color = mat.color;
            color.a = 0.1f; // ���� ����
            mat.color = color;
        }
        else
        {
            // ������ ���� �� ����
            mat.SetOverrideTag("RenderType", "Opaque");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 2000;

            Color color = mat.color;

            // ����� ���� �� ����
            if (originalAlphas.ContainsKey(render))
            {
                color.a = originalAlphas[render];
            }
            else
            {
                color.a = 1f; // �⺻�� (������ ������)
            }

            mat.color = color;
        }
    }
}
