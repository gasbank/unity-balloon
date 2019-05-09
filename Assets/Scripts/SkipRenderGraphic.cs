using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler 이벤트를
// 받기 위해서는 오브젝트에 기본적으로 Raycast Target 속성이 있어야하고, 이를 위해 지금까지는 Image 컴포넌트를 써 왔다.
// 그러나 경량화 프리팹에서 (A) Raycast Target 속성을 위한 Image 컴포넌트 하나(알파가 0인 눈에 보이지 않는 이미지),
// 실제 완전한 초밥 이미지를 위한 (B) Image 컴포넌트 하나 이렇게 해서 총 두 개의 이미지를 씀에 따라,
// 경량화 프리팹의 배치 렌더링이 되지 않는다. (1레벨 초밥을 256개 생성해서 Stats의 Batches 수를 보면 확인 가능)
// 경량화 프리팹의 핵심은 아주 많이 생성되어 있을 때 배치 렌더링을 반드시 활용할 수 있어야 하는 점에 있으므로
// 본 컴포넌트를 (A) 부분 대신에 쓰도록 한다.
// 이 컴포넌트는 화면에 그리는 것은 아무 것도 없으면서, Raycast Target 목적만을 이루는 것이다.
// 앞으로 Raycast Target 목적으로 보이지 않는 영역을 지정할 때는 알파가 0인 눈에 보이지 않는 이미지를 쓰지 말고
// 이 컴포넌트를 대신 쓰도록 하자.
//
// 참고: https://docs.unity3d.com/ScriptReference/UI.Graphic.html
public class SkipRenderGraphic : Graphic {
    protected override void OnPopulateMesh(VertexHelper vh) {
        // Vector2 corner1 = Vector2.zero;
        // Vector2 corner2 = Vector2.zero;

        // corner1.x = 0f;
        // corner1.y = 0f;
        // corner2.x = 0.5f;
        // corner2.y = 0.5f;

        // corner1.x -= rectTransform.pivot.x;
        // corner1.y -= rectTransform.pivot.y;
        // corner2.x -= rectTransform.pivot.x;
        // corner2.y -= rectTransform.pivot.y;

        // corner1.x *= rectTransform.rect.width;
        // corner1.y *= rectTransform.rect.height;
        // corner2.x *= rectTransform.rect.width;
        // corner2.y *= rectTransform.rect.height;

        vh.Clear();

        // UIVertex vert = UIVertex.simpleVert;

        // vert.position = new Vector2(corner1.x, corner1.y);
        // vert.color = color;
        // vh.AddVert(vert);

        // vert.position = new Vector2(corner1.x, corner2.y);
        // vert.color = color;
        // vh.AddVert(vert);

        // vert.position = new Vector2(corner2.x, corner2.y);
        // vert.color = color;
        // vh.AddVert(vert);

        // vert.position = new Vector2(corner2.x, corner1.y);
        // vert.color = color;
        // vh.AddVert(vert);

        // vh.AddTriangle(0, 1, 2);
        // vh.AddTriangle(2, 3, 0);
    }
}
