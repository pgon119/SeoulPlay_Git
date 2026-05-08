using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParticleSystem))]
[CanEditMultipleObjects]
public class KoreanParticleSystemInspector : Editor
{
    private const string FoldoutPrefKey = "KoreanParticleSystemInspector.ShowGuide";

    private Editor unityParticleInspector;
    private bool showGuide;

    private static readonly (string Name, string Description)[] MainModuleTips =
    {
        ("Duration", "파티클 한 사이클이 몇 초 동안 재생되는지 정합니다. Looping이 켜져 있으면 이 시간마다 반복됩니다."),
        ("Looping", "파티클을 계속 반복 재생합니다. 불, 연기, 오라처럼 계속 나와야 하는 효과에 씁니다."),
        ("Prewarm", "Looping일 때 시작하자마자 이미 어느 정도 재생된 상태처럼 보이게 합니다. 켜면 연기나 불꽃이 처음부터 차 있는 느낌이 납니다."),
        ("Start Delay", "재생 버튼을 누른 뒤 파티클이 나오기까지 기다리는 시간입니다."),
        ("Start Lifetime", "파티클 하나가 살아 있는 시간입니다. 길수록 오래 남고, 짧을수록 빨리 사라집니다."),
        ("Start Speed", "파티클이 처음 튀어나가는 속도입니다. 0이면 거의 제자리에서 크기/색 변화만 보입니다."),
        ("3D Start Size", "X/Y/Z 크기를 따로 조절합니다. 꺼져 있으면 하나의 Start Size 값으로 전체 크기를 조절합니다."),
        ("Start Size", "파티클이 태어날 때의 크기입니다. 커질수록 한 알갱이가 크게 보입니다."),
        ("3D Start Rotation", "X/Y/Z 회전을 따로 조절합니다. 방향감 있는 조각, 균열, 잎사귀에 유용합니다."),
        ("Start Rotation", "파티클이 처음 생길 때의 회전값입니다."),
        ("Flip Rotation", "회전 방향을 일부 뒤집어 무작위 느낌을 더합니다. 0은 뒤집지 않고, 1은 많이 뒤집습니다."),
        ("Start Color", "파티클이 처음 생길 때의 색입니다. 그라디언트를 쓰면 생성 시점마다 색을 섞을 수 있습니다."),
        ("Gravity Source", "중력을 어디 기준으로 받을지 정합니다. 3D Physics는 물리 설정의 중력을 사용합니다."),
        ("Gravity Modifier", "중력 영향을 얼마나 받을지 정합니다. 0이면 중력 영향이 없고, 1이면 기본 중력만큼 떨어집니다. 음수면 위로 뜹니다."),
        ("Simulation Space", "파티클 위치를 어느 좌표계에서 계산할지 정합니다. Local은 부모를 따라가고, World는 생성된 뒤 월드에 남습니다."),
        ("Simulation Speed", "파티클 시뮬레이션 재생 속도입니다. 0.5는 느리게, 2는 빠르게 보입니다."),
        ("Delta Time", "시간 계산 방식을 정합니다. Scaled는 Time Scale 영향을 받고, Unscaled는 일시정지/슬로모션 영향을 덜 받습니다."),
        ("Scaling Mode", "부모 스케일이 파티클에 어떻게 적용될지 정합니다. Hierarchy는 부모 크기 영향을 함께 받습니다."),
        ("Play On Awake", "오브젝트가 켜질 때 자동으로 파티클을 재생합니다."),
        ("Emitter Velocity Mode", "이미터가 움직일 때 파티클 속도 계산에 어떤 기준을 쓸지 정합니다. Rigidbody가 있으면 Rigidbody가 자연스럽습니다."),
        ("Max Particles", "동시에 존재할 수 있는 파티클 최대 개수입니다. 너무 낮으면 중간에 끊기고, 너무 높으면 무거워질 수 있습니다."),
        ("Auto Random Seed", "매번 다른 난수로 재생합니다. 끄면 Random Seed 값으로 항상 같은 모양을 재현할 수 있습니다."),
        ("Random Seed", "파티클 랜덤 패턴의 기준값입니다. 같은 값이면 같은 식으로 재생됩니다."),
        ("Stop Action", "파티클 재생이 끝났을 때 할 일을 정합니다. 예를 들어 오브젝트 비활성화나 파괴에 쓸 수 있습니다."),
        ("Culling Mode", "카메라에 보이지 않을 때 시뮬레이션을 계속할지 정합니다. 성능과 정확도 사이의 선택입니다."),
        ("Ring Buffer Mode", "최대 파티클 수를 넘었을 때 오래된 파티클을 어떻게 재사용할지 정합니다.")
    };

    private static readonly (string Name, string Description)[] ModuleTips =
    {
        ("Emission", "파티클이 얼마나 자주, 몇 개씩 나오는지 정합니다. Rate over Time은 초당 개수, Bursts는 특정 순간에 한 번에 터지는 개수입니다."),
        ("Shape", "파티클이 어디에서 어떤 모양으로 뿜어져 나오는지 정합니다. Cone, Sphere, Box 등 발사 형태를 고릅니다."),
        ("Velocity over Lifetime", "살아 있는 동안 속도를 추가로 바꿉니다. 바람에 밀리거나 옆으로 퍼지는 움직임을 만들 때 씁니다."),
        ("Limit Velocity over Lifetime", "파티클 속도가 너무 빨라지지 않도록 제한합니다. 감속, 공기 저항 같은 느낌에 좋습니다."),
        ("Inherit Velocity", "이미터나 부모 오브젝트의 움직임 속도를 파티클이 물려받게 합니다."),
        ("Lifetime by Emitter Speed", "이미터가 빠르게 움직일수록 파티클 수명에 변화를 줍니다."),
        ("Force over Lifetime", "살아 있는 동안 힘을 계속 가합니다. 바람, 끌림, 밀림 같은 효과를 만듭니다."),
        ("Color over Lifetime", "시간이 지나며 색과 알파를 바꿉니다. 불꽃이 식거나 연기가 사라지는 표현에 자주 씁니다."),
        ("Color by Speed", "파티클 속도에 따라 색을 바꿉니다. 빠른 조각만 밝게 만드는 식으로 씁니다."),
        ("Size over Lifetime", "시간이 지나며 크기를 바꿉니다. 점점 커지는 연기, 줄어드는 불꽃에 필수입니다."),
        ("Size by Speed", "속도에 따라 크기를 바꿉니다. 빠른 파편을 길거나 크게 보이게 할 수 있습니다."),
        ("Rotation over Lifetime", "살아 있는 동안 계속 회전시킵니다. 잎, 먼지, 파편에 생동감을 줍니다."),
        ("Rotation by Speed", "속도가 빠를수록 더 많이 회전하게 합니다."),
        ("External Forces", "Wind Zone 같은 외부 힘의 영향을 받게 합니다."),
        ("Noise", "불규칙한 흔들림을 추가합니다. 연기, 마법, 전기, 먼지에 자연스러운 움직임을 줍니다."),
        ("Collision", "파티클이 바닥이나 오브젝트에 부딪히게 합니다. 불꽃 튐, 빗방울, 파편에 씁니다."),
        ("Triggers", "특정 콜라이더 안/밖에 들어갈 때 스크립트 이벤트를 받을 수 있게 합니다."),
        ("Sub Emitters", "파티클이 태어나거나 죽거나 충돌할 때 다른 파티클을 추가로 발생시킵니다. 폭발 뒤 연기, 충돌 스파크에 좋습니다."),
        ("Texture Sheet Animation", "텍스처 시트의 프레임을 넘기며 애니메이션처럼 보이게 합니다. 불꽃, 폭발, 연기 flipbook에 씁니다."),
        ("Lights", "파티클마다 라이트를 붙입니다. 밝은 불꽃이나 마법 효과에 좋지만 성능 비용이 있습니다."),
        ("Trails", "파티클 뒤에 꼬리를 남깁니다. 유성, 검기, 마법 탄환에 유용합니다."),
        ("Custom Data", "스크립트나 셰이더에서 사용할 사용자 데이터를 파티클에 넣습니다."),
        ("Renderer", "파티클을 어떤 재질과 방식으로 그릴지 정합니다. Material, Render Mode, Sorting 등이 여기에 있습니다.")
    };

    private void OnEnable()
    {
        showGuide = EditorPrefs.GetBool(FoldoutPrefKey, true);
        CreateUnityParticleInspector();
    }

    private void OnDisable()
    {
        EditorPrefs.SetBool(FoldoutPrefKey, showGuide);

        if (unityParticleInspector != null)
        {
            DestroyImmediate(unityParticleInspector);
            unityParticleInspector = null;
        }
    }

    public override void OnInspectorGUI()
    {
        if (unityParticleInspector == null)
        {
            CreateUnityParticleInspector();
        }

        if (unityParticleInspector != null)
        {
            unityParticleInspector.OnInspectorGUI();
        }
        else
        {
            DrawDefaultInspector();
        }

        DrawKoreanGuide();
    }

    private void CreateUnityParticleInspector()
    {
        if (unityParticleInspector != null)
        {
            return;
        }

        var inspectorType = Type.GetType("UnityEditor.ParticleSystemInspector, UnityEditor");
        if (inspectorType == null)
        {
            return;
        }

        try
        {
            unityParticleInspector = CreateEditor(targets, inspectorType);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not create Unity ParticleSystem inspector. Falling back to default inspector.\n" + exception.Message);
            unityParticleInspector = null;
        }
    }

    private void DrawKoreanGuide()
    {
        EditorGUILayout.Space(8f);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showGuide = EditorGUILayout.Foldout(showGuide, "Particle System 한국어 빠른 설명", true);
            if (!showGuide)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                "Unity 기본 파티클 인스펙터의 hover 툴팁은 에디터 내부에 있어서 직접 번역 교체가 어렵습니다. 대신 자주 쓰는 항목을 바로 아래에 한국어로 정리했습니다.",
                MessageType.Info);

            DrawSection("Main", MainModuleTips);
            DrawSection("Modules", ModuleTips);
        }
    }

    private static void DrawSection(string title, IReadOnlyList<(string Name, string Description)> tips)
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        foreach (var tip in tips)
        {
            EditorGUILayout.LabelField(new GUIContent(tip.Name, tip.Description), EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(tip.Description, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(2f);
        }
    }
}
