using UnityEngine;

// 이 스크립트를 Rim2D 셰이더를 쓰는 SpriteRenderer가 달린 오브젝트에 붙이세요.
// SpriteRenderer.flipX 값이 바뀔 때마다 자동으로 셰이더에 반영해줍니다.
// 추가로, 스프라이트마다 다른 노멀맵도 MaterialPropertyBlock으로 개별 적용
[RequireComponent(typeof(SpriteRenderer))]
public class RimFlipSync : MonoBehaviour
{
    private SpriteRenderer sr;
    private MaterialPropertyBlock mpb;

    private static readonly int FlipXID = Shader.PropertyToID("_FlipX");

    // 문자열로 매번 "_Bump"를 찾는 것보다 빠르고 안전
    private static readonly int BumpID = Shader.PropertyToID("_Bump");

    // 같은 머티리얼을 공유하더라도, 이 값은 오브젝트마다 다르게 넣을 수 있습니다.
    [SerializeField] private Texture2D normalMap;

    private bool lastFlipX;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();

        // 기존 코드처럼 flipX가 바뀔 때만 적용하면,
        // 처음 상태의 _FlipX 또는 normalMap이 셰이더에 바로 안 들어갈 수 있습니다.
        ApplyProperties();
    }

    void LateUpdate()
    {
        // 값이 바뀌었을 때만 갱신 (매 프레임 불필요한 갱신 방지)
        if (sr.flipX != lastFlipX)
        {
            ApplyProperties();
        }
    }

    // SpriteRenderer에 적용할 셰이더 프로퍼티들을 한 번에 갱신하는 함수
    // 현재는 _FlipX와 _Bump를 MaterialPropertyBlock으로 적용
    private void ApplyProperties()
    {
        lastFlipX = sr.flipX;

        // 기존 PropertyBlock 값을 먼저 가져옴
        // 다른 스크립트나 시스템이 이미 넣어둔 MPB 값이 있다면 덮어쓰지 않기 위해 필요
        sr.GetPropertyBlock(mpb);

        // flipX가 true면 좌우 반전 상태이므로 -1,
        // false면 기본 방향이므로 1을 전달
        mpb.SetFloat(FlipXID, sr.flipX ? -1f : 1f);

        // 이 값은 머티리얼 자체가 아니라 해당 SpriteRenderer에만 적용
        // 그래서 여러 스프라이트가 같은 머티리얼을 써도 각자 다른 노멀맵을 가질 수 있음
        if (normalMap != null)
        {
            mpb.SetTexture(BumpID, normalMap);
        }

        // 변경된 MaterialPropertyBlock을 SpriteRenderer에 다시 적용
        // 여기서 적용된 값들은 공유 머티리얼을 바꾸지 않고, 이 Renderer에만 적용
        sr.SetPropertyBlock(mpb);
    }
}