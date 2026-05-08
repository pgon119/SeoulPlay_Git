//RealToonGUI URP
//MJQStudioWorks
//©2026

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace RealToon.GUIInspector
{

    public class RealToonShaderGUI_URP_SRP : ShaderGUI
    {

        #region foldout bools variable

        static bool ShowTextureColor;
        static bool ShowNormalMap;
        static bool ShowTransparency;
        static bool ShowMatCap;
        static bool ShowCutout;
        static bool ShowColorAdjustment;
        static bool ShowOutline;
        static bool ShowSelfLit;
        static bool ShowGloss;
        static bool ShowShadow;
        static bool ShowLighting;
        static bool ShowReflection;
        static bool ShowRimLight;
        static bool ShowSeeThrough;
        static bool NearFadeDithering;
        static bool Triplanar;
        static bool ShowPerspecAdju;
        static bool ShowSmeEff;
        static bool ShowTessellation;
        static bool ShowDisableEnable;
        static bool ShowSettings;
        //static bool ShowFReflection; //remove later
        //static bool ShowDepth; //remove later

        static bool ShowUI = true;

        string LightBlendString = "Anime/Cartoon";
        static string ShowUIString = "Hide Settings";


        #endregion

        #region Variables

string shader_type = "Default";
string srp_mode = "URP";
bool del_skw = false;
static bool aruskw = false;

static bool UseSSOL = true;
static string UseSSOLStat = "Use Screen Space Outline";
static string OLType = "Traditional";

static bool remoout = true;
static string remooutstat = "Remove Outline";

static bool twofourfive_target = false;
static string twofourfive_target_string = "Change shader compilation target to 4.5";

static bool dots_lbs_cd = false;
static string dots_lbs_cd_string = "DOTS|HR - Use Compute Deformation";

static bool add_st = true;
static string add_st_string = "Add 'See Through' feature";

static bool tess_supp = false;
static string tess_supp_string = "Enable Tessellation";

        static int MatRenQue = 0;

        #endregion

        #region Material Properties Variables

        MaterialProperty _Culling = null;
        MaterialProperty _TRANSMODE = null;
        MaterialProperty _UVSet = null;

        MaterialProperty _MainTex = null;
        MaterialProperty _TexturePatternStyle = null;
        MaterialProperty _MainColor = null;
        MaterialProperty _MaiColPo = null;
        MaterialProperty _MVCOL = null;
        MaterialProperty _MCIALO = null;

        MaterialProperty _MCapIntensity = null;
        MaterialProperty _MCap = null;
        MaterialProperty _SPECMODE = null;
        MaterialProperty _SPECIN = null;
        MaterialProperty _MCapMask = null;

        MaterialProperty _Cutout = null;
        MaterialProperty _UseSecondaryCutout = null;
        MaterialProperty _SecondaryCutout = null;
        MaterialProperty _AlphaBaseCutout = null;
        MaterialProperty _N_F_SCO = null;
        MaterialProperty _AlpToCov = null;
        MaterialProperty _AAS = null;
        MaterialProperty _N_F_COEDGL = null;
        MaterialProperty _Glow_Color = null;
        MaterialProperty _Glow_Edge_Width = null;

        MaterialProperty _Opacity = null;
        MaterialProperty _TransparentThreshold = null;
        MaterialProperty _MaskTransparency = null;
        MaterialProperty _BleModSour = null;
        MaterialProperty _BleModDest = null;

        MaterialProperty _SimTrans = null;
        MaterialProperty _TransAffSha = null;

        MaterialProperty _NormalMap = null;
        MaterialProperty _NormalMapIntensity = null;

        MaterialProperty _Saturation = null;

        MaterialProperty _OutlineWidth = null;
        MaterialProperty _OutlineWidthControl = null;
        MaterialProperty _OutlineExtrudeMethod = null;
        MaterialProperty _OutlineOffset = null;
        MaterialProperty _OutResi = null;
        MaterialProperty _OutlineZPostionInCamera = null;
        MaterialProperty _DoubleSidedOutline = null;
        MaterialProperty _OutlineColor = null;
        MaterialProperty _MixMainTexToOutline = null;
        MaterialProperty _NoisyOutlineIntensity = null;
        MaterialProperty _DynamicNoisyOutline = null;
        MaterialProperty _LightAffectOutlineColor = null;
        MaterialProperty _OutlineWidthAffectedByViewDistance = null;
        MaterialProperty _FarDistanceMaxWidth = null;
        MaterialProperty _VertexColorBlueAffectOutlineWitdh = null;
        MaterialProperty _OutStenPass = null;
        MaterialProperty _OutZWrite = null;
        MaterialProperty _OutZTest = null;

        MaterialProperty _N_F_MSSOLTFO = null;
        MaterialProperty _DepthThreshold = null;

        MaterialProperty _SelfLitIntensity = null;
        MaterialProperty _SelfLitColor = null;
        MaterialProperty _SelfLitPower = null;
        MaterialProperty _TEXMCOLINT = null;
        MaterialProperty _SelfLitHighContrast = null;
        MaterialProperty _N_F_SLMM = null;
        MaterialProperty _MaskSelfLit = null;

        MaterialProperty _GlossIntensity = null;
        MaterialProperty _Glossiness = null;
        MaterialProperty _GlossSoftness = null;
        MaterialProperty _GlossColor = null;
        MaterialProperty _GlossColorPower = null;
        MaterialProperty _MaskGloss = null;

        MaterialProperty _GlossTexture = null;
        MaterialProperty _GlossTextureSoftness = null;
        MaterialProperty _PSGLOTEX = null;
        MaterialProperty _GlossTextureRotate = null;
        MaterialProperty _GlossTextureFollowObjectRotation = null;
        MaterialProperty _N_F_ANIS = null;
        MaterialProperty _NoisTexInten = null;
        MaterialProperty _StraWidt = null;
        MaterialProperty _NoiTexAffStraWidt;
        MaterialProperty _ShifAnis = null;
        MaterialProperty _GlossTextureFollowLight = null;

        MaterialProperty _OverallShadowColor = null;
        MaterialProperty _OverallShadowColorPower = null;
        MaterialProperty _SelfShadowShadowTAtViewDirection = null;

        MaterialProperty _ReduSha = null;
        MaterialProperty _ShadowHardness = null;

        MaterialProperty _HighlightColor = null;
        MaterialProperty _HighlightColorPower = null;

        MaterialProperty _SelfShadowRealtimeShadowIntensity = null;
        MaterialProperty _SelfShadowThreshold = null;
        MaterialProperty _VertexColorGreenControlSelfShadowThreshold = null;
        MaterialProperty _SelfShadowHardness = null;
        MaterialProperty _SelfShadowRealTimeShadowColor = null;
        MaterialProperty _SelfShadowRealTimeShadowColorPower = null;
        MaterialProperty _LigIgnoYNorDir = null;
        MaterialProperty _SelfShadowAffectedByLightShadowStrength = null;

        MaterialProperty _SmoothObjectNormal = null;
        MaterialProperty _VertexColorRedControlSmoothObjectNormal = null;
        MaterialProperty _XYZPosition = null;
        MaterialProperty _ShowNormal = null;

        MaterialProperty _ShadowColorTexture = null;
        MaterialProperty _ShadowColorTexturePower = null;

        MaterialProperty _ShadowTIntensity = null;
        MaterialProperty _ShadowT = null;
        MaterialProperty _ShadowTLightThreshold = null;
        MaterialProperty _ShadowTShadowThreshold = null;
        MaterialProperty _ShadowTColor = null;
        MaterialProperty _ShadowTColorPower = null;
        MaterialProperty _ShadowTHardness = null;
        MaterialProperty _STIL = null;
        MaterialProperty _N_F_STIS = null;
        MaterialProperty _N_F_STIAL = null;
        MaterialProperty _ShowInAmbientLightShadowIntensity = null;
        MaterialProperty _ShowInAmbientLightShadowThreshold = null;
        MaterialProperty _LightFalloffAffectShadowT = null;
        MaterialProperty _N_F_STSDFM = null;

        MaterialProperty _PTexture = null;
        MaterialProperty _PTCol = null;
        MaterialProperty _PTexturePower = null;

        MaterialProperty _RELG = null;
        MaterialProperty _EnvironmentalLightingIntensity = null;

        MaterialProperty _GIFlatShade = null;
        MaterialProperty _GIShadeThreshold = null;
        MaterialProperty _LightAffectShadow = null;
        MaterialProperty _LightIntensity = null;

        MaterialProperty _UseTLB = null;
        MaterialProperty _N_F_EAL = null;

        MaterialProperty _DirectionalLightIntensity = null;
        MaterialProperty _PointSpotlightIntensity = null;
        MaterialProperty _LightFalloffSoftness = null;

        MaterialProperty _N_F_LLI = null;
        MaterialProperty _LLI_Min = null;
        MaterialProperty _LLI_Max = null;

        MaterialProperty _CustomLightDirectionIntensity = null;
        MaterialProperty _CustomLightDirectionFollowObjectRotation = null;
        MaterialProperty _CustomLightDirection = null;

        MaterialProperty _ReflectionIntensity = null;
        MaterialProperty _Smoothness = null;
        MaterialProperty _RefMetallic = null;
        MaterialProperty _MaskReflection = null;
        MaterialProperty _FReflection = null;

        MaterialProperty _RimLigInt = null;
        MaterialProperty _RimLightUnfill = null;
        MaterialProperty _RimLightColor = null;
        MaterialProperty _RimLightColorPower = null;
        MaterialProperty _RimLightSoftness = null;
        MaterialProperty _RimLigPosi = null;
        MaterialProperty _RimLightInLight = null;
        MaterialProperty _LightAffectRimLightColor = null;
        MaterialProperty _N_F_RLIS = null;

        MaterialProperty _MinFadDistance = null;
        MaterialProperty _MaxFadDistance = null;

        MaterialProperty _TriPlaTile = null;
        MaterialProperty _TriPlaBlend = null;

        MaterialProperty _PresAdju = null;
        MaterialProperty _ClipAdju = null;
        MaterialProperty _PASize = null;
        MaterialProperty _PASmooTrans = null;
        MaterialProperty _PADist = null;

        MaterialProperty _TessellationSmoothness = null;
        MaterialProperty _TessellationTransition = null;
        MaterialProperty _TessellationNear = null;
        MaterialProperty _TessellationFar = null;
        MaterialProperty _NorMapAsDis = null;

        MaterialProperty _RefVal = null;
        MaterialProperty _Oper = null;
        MaterialProperty _Compa = null;

        MaterialProperty _N_F_ESSAO = null;
        MaterialProperty _SSAOColor = null;

        MaterialProperty _N_F_MC = null;
        MaterialProperty _N_F_NM = null;
        MaterialProperty _N_F_CO = null;
        MaterialProperty _N_F_O = null;
        MaterialProperty _N_F_CA = null;
        MaterialProperty _N_F_SL = null;
        MaterialProperty _N_F_GLO = null;
        MaterialProperty _N_F_GLOT = null;
        MaterialProperty _N_F_SS = null;
        MaterialProperty _N_F_SON = null;
        MaterialProperty _N_F_SCT = null;
        MaterialProperty _N_F_ST = null;
        MaterialProperty _N_F_PT = null;
        MaterialProperty _N_F_CLD = null;
        MaterialProperty _N_F_R = null;
        MaterialProperty _N_F_FR = null;
        MaterialProperty _N_F_RL = null;
        MaterialProperty _N_F_HDLS = null;
        MaterialProperty _N_F_HPSS = null;
        MaterialProperty _N_F_DCS = null;
        MaterialProperty _N_F_NLASOBF = null;
        MaterialProperty _N_F_RDC = null;
        MaterialProperty _N_F_DDMD = null;
        MaterialProperty _N_F_NFD = null;
        MaterialProperty _N_F_TP = null;
        MaterialProperty _N_F_PA = null;
        MaterialProperty _N_F_SE = null;

        MaterialProperty _ObjePosiZCS = null;
        MaterialProperty _ZWrite = null;
        MaterialProperty _ZTest = null;
        MaterialProperty _N_F_OFLMB = null;

        MaterialProperty _RQSO = null;

        #endregion

        #region List of Toggle Keywords

        enum SFKW
        {
            N_F_USETLB_ON,
            N_F_STIS_ON,
            N_F_STIAL_ON,
            N_F_EAL_ON,
            N_F_MC_ON,
            N_F_NM_ON,
            N_F_CO_ON,
            N_F_O_ON,
            N_F_CA_ON,
            N_F_SL_ON,
            N_F_GLO_ON,
            N_F_GLOT_ON,
            N_F_SS_ON,
            N_F_SON_ON,
            N_F_SCT_ON,
            N_F_ST_ON,
            N_F_PT_ON,
            N_F_RELGI_ON,
            N_F_CLD_ON,
            N_F_R_ON,
            N_F_FR_ON,
            N_F_RL_ON,
            N_F_HDLS_ON,
            N_F_HPSS_ON,
            N_F_DCS_ON,
            N_F_NLASOBF_ON,
            N_F_DNO_ON,
            N_F_TRANS_ON,
            N_F_TRANSAFFSHA_ON,
            N_F_OFLMB_ON,
            N_F_ESSAO_ON,
            N_F_RDC_ON,
            N_F_COEDGL_ON,
            N_F_DDMD_ON,
            N_F_SIMTRANS_ON,
            N_F_NFD_ON,
            N_F_TP_ON,
            N_F_PA_ON,
            N_F_SE_ON,
            N_F_SCO_ON,
            N_F_STSDFM_ON,
            N_F_ATC_ON,
            N_F_ANIS_ON,
            N_F_LLI_ON,
            N_F_SLMM_ON,
            N_F_RLIS_ON,
            N_F_TESS_ON,
            _UVSET_UV0,
            _UVSET_UV1,
        }

        #endregion

        #region TOTIPS

        string[] TOTIPS =
        {

        //Culling [0]
        "Controls which sides of polygons should be culled (not drawn).\n\n\nBack: Don’t render polygons that are facing away from the viewer.\n\nFront: Don’t render polygons that are facing towards the viewer, Used for turning objects inside-out.\n\nOff: Disables culling - all faces are drawn, This also called Double Sided." ,

        //Texture [1]
        "Main or base texture." , 

        //Texture Pattern Style [2]
        "Turn the 'Main/Base Texture' into pattern style." ,

        //Main Color [3]
        "Main or base color." ,

        //Mix Vertex Color [4]
        "Mix or show vertex color." ,

        //Main Color in Ambient Light Only [5]
        "Put the 'Main/Base Color' into ambient light." ,

        //Highlight Color [6]
        "Highlight color." ,

        //Highlight Color Power [7]
        "'Highlight Color' power or intensity." ,

        //Main Color Power [8]
        "'Main Color' power or intensity." ,

        //Blend - Source [9] [Transparent Mode]
        "Blending source.\n\n-Default Value: ScrAlpha" ,

        //Blend - Destination [10] [Transparent Mode]
        "Blending Destination.\n\n-Default Value: OneMinusScrAlpha" ,

        //Transparent Mode [11]
        "Setting the current mode from Opaque to Transparent.\n\nThis will allow you to use 'Fade Transparency' and 'Cutout' feature.",

        //Intensity [12] [MatCap]
        "MatCap intensity." ,

        //MatCap [13] [MatCap]
        "MatCap texture." ,

        //Specualar Mode [14] [MatCap]
        "Turn MatCap into specular." ,

        //Specular Power [15] [MatCap]
        "Specular intensity or power." ,

        //Mask MatCap [16] [MatCap]
        "Mask MatCap.\n\nUse a Black and White texture map.\nWhite means visible matcap while Black is not." ,

        //Cutout [17]
        "Cutout value or threshold." ,

        //Alpha Base Cutout [18] 
        "It will use the alpha/transparent channel of the 'Main/Base Texture' to cutout." ,

        //Use Secondary Cutout Only [19]
        "Use only the 'Secondary Cutout' to do the cutout." ,

        //Secondary Cutout [20]
        "Secondary texture cutout.\n\nUse a Black and White texture map.\nWhite means not cut out while Black is cutout." ,

        //Opacity [21]
        "Adjust the Transparency - Opacity of the object" ,

        //Transparent Threshold [22]
        "'Main/Base Texture' transparency threshold." ,

        //Mask Transparency [23]
        "Mask Transparency.\n\nWhite means opaque while Black means transparent." ,

        //Normal Map [24]
        "Normal Map." ,

        //Normal Map Intensity [25]
        "'Normal Map' intensity." ,

        //Saturation [26] [Color Adjustment]
        "Color saturation of the object." ,

        //Width [27] [Outline]
        "Outline main width." ,

        //Width Control [28] [Outline]
        "Controls the 'Outline Width' using texture Map.\n\nUse a Black and White texture map.\nWhite means 1 while Black means 0.\nThis will not work if the Outline main width value is 0." ,

        //Outline Extrude Method [29] [Outline]
        "Outline Extrude Methods.\n\nNormal - The outline extrusion will be based on normal direction.\n\nOrigin - The outline extrusion will be based on the center of the object." ,

        //Outline Offset [30] [Outline]
        "Outline XYZ position." ,

        //Double Sided Outline [31] [Outline]
        "Show the front side of the outline.\n\nUseful for plane object.\n'Outline Z Position In Camera' option is needed to be adjust to show the object." ,

        //Color [32] [Outline] [Outline]
        "Outline color." ,

        //Mix Main Texture To Outline [33] [Outline]
        "Mix 'Main/Base Texture' to oultine." ,

        //Noisy Outline Intensity [34] [Outline]
        "The power/intensity of the outline distortion or noise." ,

        //Dynamic Noisy Outline [35] [Outline]
        "Moving noisy or distort outline." ,

        //Light Affect Outline Color [36] [Outline]
        "Light (Brightness and Color) affect Outline color." ,

        //Outline Width Affected By View Distance [37] [Outline]
        "'Outline Width' affected by view distance." ,

        //Far Distance Max Width [38] [Outline]
        "The maximum 'Outline Width' limit when moving far from the object." ,

        //Vertex Color Blue Affect Outline Width [39] [Outline]
        "'Vertex Color Blue will affect the Outline Width.\n\nThis will not work if the Outline main width value is 0." ,

        //Intensity [40] [SelfLit]
        "How visible or strong the 'Self Lit' is." ,

        //Color [41] [SelfLit]
        "Self Lit color" ,

        //Power [42] [SelfLit]
        "'Self Lit Color' power or intensity." ,

        //Texture and Main Color Intensity [43] [SelfLit]
        "'Main/Base Texture' and 'Main/Base Color' intensity.\n\nAdjust this if the 'Main/Base Texture' and 'Main/Base Color' is too strong or too bright for Self Lit." ,

        //High Contrast [44] [SelfLit]
        "Turn Self Lit into high contrast colors and mix 'Base/Main Texture' twice." ,

        //Mask Self Lit [45] [SelfLit]
        "Mask Self Lit.\n\nUse a Black and White texture map.\nWhite means visible Self Lit while Black is not." ,

        //Gloss Intensity [46] [Gloss]
        "How visible or strong the 'Gloss' is." ,

        //Glossiness [47] [Gloss]
        "Glossiness." ,

        //Softness [48] [Gloss]
        "How soft the 'Gloss' is." ,

        //Color [49] [Gloss]
        "Gloss color" ,

        //Power [50] [Gloss]
        "'Gloss Color' power or intensity." ,

        //Mask Gloss [51] [Gloss]
        "Mask Gloss.\n\nWhite means visible Gloss while black is not." ,

        //Gloss Texture [52] [Gloss Texture]
        "A Black and White texture map to be used as gloss.\n\nWhite means gloss while Black is not." ,

        //Softness [53] [Gloss Texture]
        "The softness of the 'Gloss Texture'." ,

        //Pattern Style [54] [Gloss Texture]
        "Turn 'Gloss Texture' into pattern style." ,

        //Rotate [55] [Gloss Texture]
        "Rotate 'Gloss Texture'." ,

        //Follow Object Rotation [56] [Gloss Texture]
        "'Gloss Texture' will follow the object local rotation." ,

        //Follow Light [57] [Gloss Texture]
        "'Gloss Texture' will follow the light direction or position." ,

        //Overall Shadow Color [58]
        "Overall shadow color.\n\nThis will affect Realtime Shadow, Self Shadow/Shade and ShadowT." ,

        //Overall Shadow Color Power [59]
        "'Overall shadow Color' power or intensity." ,

        //Self Shadow & ShadowT At View Direction [60]
        "'Self Shadow' and 'ShadowT' follow your view or camera view direction." ,

        //Reduce Shadow (Point Light) [61]
        "The amount of reduce self cast shadow.\n\nThis option will only take effect when there's a Point Light." ,

        //Refresh Settings [62]
        "This will refresh and re-apply the settings properly.\n\nClick this if there are some problem, after you update, after material reset or re-import RealToon.",

        //Reduce Shadow [63]
        "The amount of reduce self cast shadow.\n\nThis option will only take effect when there's a 'Directional Light', 'Point' or 'Spot Light'." ,

        //Shadow Hardness [64] [RealTime Shadow]
        "Real time shadow hardness" ,

        //Threshold [65] [Self Shadow]
        "The amount of 'Self Shadow/Shade' on the object." ,

        //Vertex Color Green Control Self Shadow Threshold [66]
        "Controls 'Self Shadow Threshold' by using vertex color Green." ,

        //Hardness [67] [Self Shadow]
        "'Self Shadow/Shade' hardness." ,

        //Self Shadow & Real Time Shadow Color [68]
        "'Self Shadow and Real Time Shadow Color'.\n\nBefore you set/change this, Set 'Overall Shadow Color' to White." ,

        //Self Shadow & Real Time Shadow Color Power [69]
        "'Self Shadow and Real Time Shadow Color' power or intensity." ,

        //Self Shadow Affected By Light Shadow Strength [70]
        "Light shadow strength will affect self shadow visibility." ,

        //Smooth Object Normal [71]
        "The amount of smooth object normal." ,

        //Vertex Color Red Control Smooth Object Normal [72]
        "'Vertex color Red controls the amount of smooth object normal." ,

        //XYZ Position [73] [Smooth Object Normal]
        "Normal's XYZ positions." ,

        //Affect Shadow [74]
        "Transparency affect shadow." ,

        //Show Normal [75] [Smooth Object Normal]
        "Show the normal of the object." ,

        //Shadow Color Texture [76]
        "A texture to color shadow.\n\nThis includes (RealTime Shadow, Self Shadow/Shade and ShadowT.\nYou can also use your 'Main/Base Texture' and adjust 'Power' to make it dark." ,

        //Power [77] [Shadow Color Texture]
        "How strong or dark the 'Shadow Color Texture'." ,

        //Intensity [78] [ShadowT]
        "How visitble or strong the 'ShadowT' is." ,

        //ShadowT [79]
        "ShadowT or Shadow Texture, shadows in texture form.\n\nUse Black or Gray and White Flat, Gradient and Smooth texture map.\nGray and White affected by light while Black is not.\n\nFor more info and how to use and make ShadowT texture maps, see 'Video Tutorials' and 'User Guide.pdf' at the bottom of this RealToon inspector.",

        //Light Threshold [80] [ShadowT]
        "The amount of light." ,

        //Shadow Threshold [81] [ShadowT]
        "The amount of ShadowT." ,

        //Hardness [82] [ShadowT]
        "'ShadowT' hardness." ,

        //Show In Shadow [83] [ShadowT]
        "Show 'ShadowT' in shadow.\n\nThis will only be visible if realtime shadow and self shadow/shade color is not Black." ,

        //Show In Ambient Light [84] [ShadowT]
        "Show 'ShadowT' in Ambient Light.\n\nThis will only be visible if there's an Ambient Light present or GI." ,

        //Show In Ambient Light & Shadow Intensity [85] [ShadowT]
        "'ShadowT' intensity or visibility in shadow and ambient light." ,

        //Show In Ambient Light & Shadow Threshold [86] [ShadowT]
        "'ShadowT' threshold in Ambient Light and shadow." ,

        //Light Falloff Affect ShadowT [87]
        "'Point light' and 'Spot Light' light falloff affect 'ShadowT'." ,

        //PTexture [88]
        "A Black and White texture to be used as pattern for shadow.\n\nBlack means pattern while White is nothing.\nThis will not be visible if the shadow color is Black." ,

        //Power [89] [PTexture]
        "How strong or dark the pattern is." ,

        //Receive Environmental Ligthing and GI [90] [Lighting]
        "Turn on or off receive 'Environmental Ligthing' or 'GI'." ,

        //Environmental Ligthing Intensity [91] [Lighting]
        "Ambient Light, GI or Environmental Ligthing intensity on the object." ,

        //GI Flat Shade [92] [Lighting]
        "Turn GI or SH lighting shade into flat shade." ,

        //GI Shade Threshold [93] [Lighting]
        "The amount of GI Shade on the object." ,

        //Light affect Shadow [94] [Lighting]
        "Light intensity, color and light falloff affect shadows.\n\nThis will affect (RealTime shadow, Self Shadow and ShadowT)." ,

        //Directional Light Intensity [95] [Lighting]
        "Directional Light intensity received on the object." ,

        //Point and Spot Light Intensity [96] [Lighting]
        "Point and Spot light intensity received on the object." ,

        //Light Falloff Softness [97] [Lighting]
        "How soft is the point and spot light light falloff." ,

        //Intensity [98] [Custom Light Direction]
        "The amount of custom light direction." ,

        //Custom Light Direction [99] [Custom Light Direction]
        "XYZ light direction." ,

        //Follow Object Rotation [100] [Custom Light Direction]
        "'Custom Light Direction' follow object rotation." ,

        //Intensity [101] [Reflection]
        "The amount reflection visibility." ,

        //Roughness [102] [Reflection]
        "'Reflection' roughness." ,
        
        //Metallic [103] [Reflection]
        "The amount of reflection metallic look." ,
        
        //Mask Reflection [104]
        "Mask Reflection.\n\nWhite means visible relfection while Black means reflection not visible." ,

        //FReflection [105]
        "A texture or image to be used as reflection." ,

        //Unfill [106] [Rim Light]
        "Unfill the 'Rim Light' on the object." ,

        //Softness [107] [Rim Light]
        "'Rim Light' softness." ,

        //Light Affect Rim Light [108] [Rim Light]
        "Light (Brightness and Color) affect 'Rim Light'." ,

        //Color [109] [Rim Light]
        "'Rim Light' color." ,

        //Color Power [110] [Rim Light]
        "'Rim Light Color' power or intensity." ,

        //Rim Light In Light [111]
        "'Rim Light' will be visible in light only." ,

        //ID [112] [See Through]
        "ID or reference value.\n\n-Default Value: 0" ,

        //Set A [113] [See Through]
        "'A' The see through object while 'B' is the object to be seen through 'A'.\n\n-Default Value: A" ,

        //Set B [114] [See Through]
        "'A' The see through object while 'B' is the object to be seen through 'A'.\n\n-Default Value: None" ,

        //No Light and Shadow On Backface [115]
        "No light and shadow will be visible on a back of a plane/flat object or face.\n\nThis will only be take effect or visible if 'Culling' is turned 'Off' or 'Front'." ,

        //Change Shader Compilation Target To 2.0/4.5. [116]
        "This will change the Shader Compilation Target of the RealToon Shader file to '2.0' or '4.5'.\n\n*If the shader compilation target is changed to 4.5, the shader will support DOTS/DOTS Hybrid Renderer and Tessellation.",

        //Hide Directional Light Shadow [117]
        "Hide received 'Directional Light' shadows on the object." ,

        //Hide Point & Spot Light Shadow [118]
        "Hide received 'Point and Spot Light' shadows on the object." ,

        //Disable Cast Shadow [119]
        "Disable object cast shadow." ,

        //ZWrite [120]
        "Turn on or off ZWrite.\n\n*Does not affect outline, there is a dedicated ZWrite option for outline, it is under the 'Outline' category.",

        //Automatic Remove Unused Shader Keywords [121]
        "Remove unused shader keywords automatically in all materials with Realtoon Shader. This will take effect once this enabled and when the RealToon Inspector shown. Disable this if you experience too slow Inspector.\n\n(Warning: This will also remove stored previous shaders shader keywords.)",

        //Color[122] [PTexture]
        "'PTexture' color." ,

        //Outline Z Position In Camera [123] [Outline]
        "Adjust the outline Z position in camera space." ,

        //RealTime Shadow Intensity [124] [RealTime Shadow]
        "Adjust the realtime shadow intensity." ,

        //Rim Light Intensity [125] [RimLight]
        "'Rim Light' intensity.",

        //Self Shadow & RealTime Shadow Intensity [126]
        "Adjust the 'Self Shadow' and realtime shadow intensity." ,

        //Self Shadow Color [127] [Shadow]
        "'Self Shadow' color." ,

        //Self Shadow Color Power [128] [Shadow]
        "'Self Shadow' color power or intensity." ,

        //Color [129] [ShadowT]
        "'ShadowT' color." ,

        //Color Power [130] [ShadowT]
        "'ShadowT' color power or intensity.",

        //Ignore Light [131] [ShadowT]
        "'ShadowT' ignore direction light or light position.",

        //Light Intensity [132] [Lighting]
        "How strong is the Light in the shadow.",

        //Enable Additional Lights [133] [Lighting]
        "Enable additional lights like Point and Spot lights.",

        //Use Traditional Light Blend [134] [Lighting]
        "Use traditional light blend.\n\nIf enabled light blending will be in add mode, if not enabled the light blending will based on high or maximum light intensity and the blending will be similar to Anime or Cartoon.",

        //Remove Outline/Add Outline (On Shader) [135]
        "This will remove the Outline feature completely on the shader file or Add back the Outline feature on the shader file.\n\nThis is not per material.",

        //Video Tutorials [136]
        "RealToon's video tutorial playlist.",

        //RealToon (User Guide).pdf [137]
        "RealToon's user guide or documentation.",

        //Hide/Show UI [138]
        "This will hide or show RealToon's Inspector UI.\n\nThis is global and not per material.",

        //Depth Threshold [139] [outline]
        "This will adjust the depth based outline threshold.",

        //Mix Outline To The Shader Output [140] [outline]
        "This will mix the outline to the shader output",

        //Optimize for [Light Mode:Baked] [141]
        "If enabled, it will disable all realtime features on the shader and optimize it for [Light Mode:Baked].\n\nDisable or uncheck this for [Light Mode: RealTime or Mixed] use.",

        //Use Screen Space Outline/Use Traditional Outline [142] [outline]
        "This will enable you to use 'Screen Space Outline' or 'Traditional Outline'.\n\n'Depth Texture' needs to be turn 'On' if you use the 'Screen Space Outline'.\n\nThis is not per material.",

        //Use Linear Blend Skinning/Compute Deformation [143]
        "This will enable you to use 'Linear Blend Skinning' or 'Compute Deformation'.\n\nThis will modify the RealToon shader file.\nCurrently it does not support Tessellation.",

        //Light Ignore Y Normal Direcion [144]
        "Light will ignore Object Normal Y direction.",

        //Enable Screen Space Ambient Occlusion [145]
        "Enable SSAO or Screen Space Ambient Occlusion." ,

        //Ambient Occlusion Color [146]
        "Ambient Occlusion color or tint.",

        //Receive Decal [147]
        "The object will Receive Decal.",

        //Glow Color [148]
        "Glow edge color.",

        //Glow Edge Width [149]
        "The width of the glow.",

        //Simple Transparency Mode[150]
        "Common simple transparency.\nOnly 'Opacity', 'Blend Modes' and 'Affect Shadow' are available.\n\n'Transparent Threshold' and 'Mask Transparency' not available on this mode.",

        //Disable DOTS Mesh Deformation[151]
        "Disable DOTS Mesh Deformation: 'Linear Blend Skinning and Compute Deformation'.\n\n*For Static Objects, enabled this.",

        //Near Fade Dithering - Min Distance[152]
        "The minimum near distance.",

        //Near Fade Dithering - Max Distance[153]
        "The maximum near distance.",

        //Soft Cutout [154]
        "Dithering/Dot style cutout.\n\nFor a soft edge cutout.",

        //Tile (Triplanar) [155]
        "Tiling scale of the texture.",

        //Blend (Triplanar) [156]
        "Blending of the triplanar texture.",

        //Perspective (Perspective Adjustment) [157]
        "This will change the perspective of an object to 2D or 3D or FOV stretch look.\nFor 2d toon/anime look, set it to 0.5 or 0.",

        //Clip (Perspective Adjustment) [158]
        "This will change the clipping on the object.\nChange this if the object is overlapping front or back.\nHigher value will slice the object.",

        //Close-Up Size (Perspective Adjustment) [159]
        "This will adjust the size of the object when the camera is closer.",

        //Close-Up Size Smooth Transition (Perspective Adjustment) [160]
        "How smooth the transition of the sizing.",

        //Close-Up Size Distance (Perspective Adjustment) [161]
        "Distance transition from the camera to the object.",

        //SDF Mode (ShadowT) [162]
        "SDF Style Shadowing.\nNote: This only affect 3D Space X & Z axis.",

        //Add/Remove 'See Through' feature [163]
        "This will add or remove 'See Through' feature on the RealToon Shader.\n\nUse this if you don't need the 'See Through' feature.\n\nThis will modify the RealToon shader file.",

         //Anti-Aliasing (Cutout) [164]
        "Anti-Aliasing/MSAA affects cutout.\n\n*This is Alpha To Coverage and it will only work if Forward/Forward+ Path Rendering is use.\n*If you turn off Cutout feature, this will revert to disable/off.",

        //Anisotropic Mode (Gloss Texture) [165]
        "Setting the Gloss Texture to Anisotropic Mode.\n\nNote: This will use Gloss Texture texture input as noise.",

        //Noise Texture Intensity (Gloss Texture) [166]
        "How strong the Noise Texture distortion.",

        //Width (Gloss Texture) [167]
        "Width of the Anisotropic.",

        //Shift (Gloss Texture) [168]
        "Shift the Anisotropic to Up or Down.",
        
        //Noise Texture Affect Width (Gloss Texture) [169]
        "Noise Texture affect Anisotropic Width.\n\nNote: White means 1 while Black is 0.",

        //Stencil: Pass (Outline) [170]
        "Use for fixing outline overlapping issues to other assets, objects and UI.",

        //Limit Light Intensity [171]
        "This will limit the light intensity.\nMinimum and maximum light intensity.",

        //Minimum (Limit Light Intensity) [172]
        "Minimum Light Intensity value.",

        //Maximum (Limit Light Intensity) [173]
        "Maximum Light Intensity value.",

        //Anti-Aliasing Softness (Anti-Aliasing Affects Cutout) (Cutout) [174]
        "How soft the Anti-Aliasing.",

        //Map Mode (SelfLit) [175]
        "Use SelfLit Mask as SelfLit Map/Emission Map",

        //SelfLit Map (SelfLit) [176]
        "A SelfLit Map/Emission Map.\nYou can use Grayscale/alpha or RGB/Colored Map.",

        //Outline Resize (Outline) [177]
        "Resizing the outline XYZ.",

        //Object Position Z (CS) [178]
        "Adjust object position z axis in clip space.\n\n*Can also use it to move the object infront or back.\n*Can also be use for adjusting 'Prespective Adjustment - Clip'.",

        //Rimlight In Shadow (Rimlight) [179]
        "Rim Light in shadow only.",

        //Position (Rimlight) [180]
        "Rim Light position.",

        //Render Order [181]
        "This will change the object's Render Order, infront or behind.\nThis will affect the Render Queue.\n\n*Mostly useful when using Transparent Mode or if the ZWrite is Off.",

        //ZTest [182]
        "Determines whether a pixel should be rendered based on its distance from the camera compared to what is already drawn in the depth buffer (Z-buffer).\n\n*Does not affect outline, there is a dedicated ZTest option for outline, it is under the 'Outline' category",

        //ZTest (Outline) [183]
        "Determines whether a pixel should be rendered based on its distance from the camera compared to what is already drawn in the depth buffer (Z-buffer).\n\n*This only affect Outline.",

        //ZWrite (Outline) [184]
        "Turn off or on.\n\n*This only affect Outline.",

        //UV Set [185]
        "UV Set/UV Channel.\n\n*This affect all texture/map slots.",

        //Enable/Disable Tessellation [186]
        "This will Enable/Disable 'Tessellation' feature on the RealToon Shader.\n\nThis will modify the RealToon shader file.",

        //Smoothness (Tessellation) [187]
        "Smooth tessellated faces.",

        //Tessellation Transition (Tessellation) [188]
        "Transition distance between Near and Far.\n\n*0 means mostly near tessellation value while 1 means mostly far tessellation value.",

        //Tessellation Near (Tessellation) [189]
        "The amount of Tessellation when Near.",

        //Tessellation Far (Tessellation) [190]
        "The amount of Tessellation when Far.",

        //Normal Map As Displacement (Tessellation) [191]
        "Normal Map as a displacement.\n\n*You need to enable Normal Map feature.",


        };

        #endregion

        #region TOTIPS for EnDisFeatures

        string[] TOTIPSEDF =
        {
        //MatCap [0]
        "MatCap or Material Capture.",

        //Normal Map [1]
        "Normal Map.",

        //Outline [2]
        "Outline.",

        //Cutout [3]
        "Cutout.",

        //Color Adjustment [4]
        "Adjust the color of the object.",

        //SelfLit [5]
        "Own light or Emission.",

        //Gloss [6]
        "Gloss.",

        //Gloss Texture [7]
        "Gloss in texture form.\n\nUse a Black and White texture map.\nWhite means gloss while Black is not.",

        //Self Shadow [8]
        "Self Shadow or Shade.",

        //Smooth Object Normal [9]
        "Smooth object normal or ignore object normal.",

        //Shadow Color Texture [10]
        "Color shadow using texture.",

        //ShadowT [11]
        "ShadowT or Shadow Texture, shadows in texture form.\n\nUse Black or Gray and White Flat, Gradient and Smooth texture map.\nGray and White affected by light while Black is not.\n\nFor more info and how to use and make ShadowT texture maps, see 'Video Tutorials' and 'User Guide.pdf' at the bottom of this RealToon inspector.",

        //PTexture [12]
        "PTexture or Pattern Texture.\n\nA Black and White texture to be used as pattern for shadow.\n\nBlack means pattern while White is nothing.\nThis will not be visible if the shadow color is Black.",

        //Custom Light Direction [13]
        "Custom light direction.",

        //Reflection [14]
        "Reflection.",

        //FReflection [15]
        "FReflection or Fake Reflection.\n\nUse any texture or image as reflection.",

        //Rim Light [16]
        "Rim light or fresnel effect.",

        //Near Fade Dithering [17]
        "Object fades when the camera near.",

        //Triplanar [18]
        "For a uniform texture scale and tiles.\n\nUseful for static objects and environment.",

        //Perspective Adjustment [19]
        "This will adjust the perspective of your object to look 2D Toon/Anime or Default 3D.",

        //Smear Effect [20]
        "Trail lines or Line noise effect when an object move fast, like the Anime/Cartoon."

        };

        #endregion

        bool koreanTooltipsApplied;

        void ApplyKoreanTooltips()
        {
            if (koreanTooltipsApplied)
            {
                return;
            }

            string[] koreanTooltips =
            {
                "어느 면을 그릴지 정합니다.\n\nBack: 뒤쪽 면은 그리지 않습니다. 일반적인 캐릭터/오브젝트에 많이 씁니다.\n\nFront: 앞쪽 면은 그리지 않습니다. 오브젝트를 뒤집어 보이게 할 때 씁니다.\n\nOff: 양면을 모두 그립니다. 얇은 천, 종이, 머리카락 같은 평면에 유용하지만 조금 더 무거울 수 있습니다.",
                "가장 기본이 되는 색 텍스처입니다. 캐릭터 피부, 옷 무늬, 오브젝트 표면 이미지가 여기에 들어갑니다.",
                "기본 텍스처를 일반 이미지가 아니라 반복 무늬처럼 보이게 합니다.",
                "기본 색입니다. 텍스처 위에 전체적으로 덧입히는 색이라고 생각하면 됩니다.",
                "모델의 버텍스 컬러를 섞거나 표시합니다. 모델에 칠해진 정점 색 정보가 있을 때만 효과가 잘 보입니다.",
                "기본 색을 주변광 영역에만 적용합니다. 그림자나 어두운 영역의 색감을 맞출 때 씁니다.",
                "밝게 강조되는 부분의 색입니다. 빛이 닿는 느낌을 더 강하게 만들 때 씁니다.",
                "강조 색의 세기입니다. 값을 올리면 하이라이트 색이 더 진하고 밝게 보입니다.",
                "기본 색의 세기입니다. 값을 올리면 텍스처/색이 더 강하게 보입니다.",
                "투명 모드에서 원본 색을 얼마나 섞을지 정하는 블렌드 값입니다.\n\n보통 기본값 그대로 두면 됩니다.",
                "투명 모드에서 배경 색을 얼마나 섞을지 정하는 블렌드 값입니다.\n\n보통 기본값 그대로 두면 됩니다.",
                "머티리얼을 불투명에서 투명 처리 가능한 상태로 바꿉니다.\n\n켜면 페이드 투명도와 컷아웃 기능을 사용할 수 있습니다.",
                "MatCap 효과의 세기입니다. 값을 올리면 MatCap 질감이 더 뚜렷해집니다.",
                "MatCap 텍스처입니다. 조명 대신 특정 질감/광택 이미지를 입혀 스타일을 만들 때 씁니다.",
                "MatCap을 반짝임처럼 사용합니다. 금속/눈동자/장식처럼 빛나는 표현에 유용합니다.",
                "반짝임의 세기입니다. 값을 올리면 Specular 효과가 강해집니다.",
                "MatCap이 보일 영역을 정하는 마스크입니다.\n\n흰색은 보임, 검은색은 안 보임입니다.",
                "컷아웃 기준값입니다. 값보다 어두운 알파 영역은 잘려 나갑니다.",
                "기본 텍스처의 알파 채널을 사용해서 잘라냅니다. 머리카락, 속눈썹, 잎사귀처럼 투명 부분이 있는 텍스처에 씁니다.",
                "기본 텍스처 알파 대신 보조 컷아웃 텍스처만 사용합니다.",
                "보조 컷아웃 텍스처입니다.\n\n흰색은 남고, 검은색은 잘려 나갑니다.",
                "오브젝트의 투명도를 조절합니다. 낮추면 더 투명해집니다.",
                "기본 텍스처의 투명 기준값입니다. 어느 정도 알파부터 투명하게 볼지 정합니다.",
                "투명도를 제한하는 마스크입니다.\n\n흰색은 불투명, 검은색은 투명입니다.",
                "표면의 작은 굴곡을 표현하는 노멀맵입니다. 실제 모델을 깎지 않고 입체감을 줍니다.",
                "노멀맵의 세기입니다. 값을 올리면 표면 굴곡이 더 강해 보입니다.",
                "색의 채도입니다. 낮추면 회색에 가까워지고, 올리면 색이 선명해집니다.",
                "외곽선의 기본 두께입니다.",
                "텍스처로 외곽선 두께를 조절합니다.\n\n흰색은 두껍게, 검은색은 얇게 또는 없음입니다.",
                "외곽선을 어느 방향으로 밀어낼지 정합니다.\n\nNormal은 표면 방향 기준, Origin은 오브젝트 중심 기준입니다.",
                "외곽선의 XYZ 위치를 미세하게 옮깁니다.",
                "외곽선의 앞면도 보이게 합니다.\n\n평면 오브젝트에서 외곽선이 안 보일 때 유용합니다.",
                "외곽선 색입니다.",
                "기본 텍스처 색을 외곽선에도 섞습니다.",
                "외곽선이 흔들리거나 거칠게 보이는 정도입니다.",
                "움직이는 노이즈 외곽선을 사용합니다.",
                "조명의 밝기와 색이 외곽선 색에도 영향을 주게 합니다.",
                "카메라와의 거리에 따라 외곽선 두께가 달라지게 합니다.",
                "멀리 떨어졌을 때 외곽선이 너무 두꺼워지지 않도록 최대 두께를 제한합니다.",
                "버텍스 컬러의 파란 채널로 외곽선 두께를 조절합니다.\n\n기본 외곽선 두께가 0이면 효과가 보이지 않습니다.",
                "자체 발광(Self Lit)이 얼마나 잘 보일지 정합니다.",
                "자체 발광 색입니다.",
                "자체 발광 색의 세기입니다.",
                "기본 텍스처와 기본 색이 자체 발광에 들어가는 세기입니다.\n\n너무 밝거나 강하면 낮춰보세요.",
                "자체 발광을 더 선명하고 대비 강한 색으로 만듭니다.",
                "자체 발광 영역을 정하는 마스크입니다.\n\n흰색은 발광, 검은색은 발광 안 함입니다.",
                "광택(Gloss)이 얼마나 강하게 보일지 정합니다.",
                "광택의 날카로움입니다. 값이 높을수록 작고 또렷한 빛이 됩니다.",
                "광택의 부드러움입니다. 값이 높을수록 경계가 부드러워집니다.",
                "광택 색입니다.",
                "광택 색의 세기입니다.",
                "광택 영역을 정하는 마스크입니다.\n\n흰색은 광택 보임, 검은색은 안 보임입니다.",
                "텍스처 형태의 광택입니다.\n\n흰색은 광택, 검은색은 광택 없음입니다.",
                "광택 텍스처의 부드러움입니다.",
                "광택 텍스처를 패턴처럼 보이게 합니다.",
                "광택 텍스처를 회전합니다.",
                "광택 텍스처가 오브젝트의 로컬 회전을 따라가게 합니다.",
                "광택 텍스처가 조명의 방향이나 위치를 따라가게 합니다.",
                "전체 그림자 색입니다.\n\n실시간 그림자, 셀프 그림자, ShadowT에 함께 영향을 줍니다.",
                "전체 그림자 색의 세기입니다.",
                "셀프 그림자와 ShadowT가 카메라 방향을 따라가게 합니다.",
                "포인트 라이트에서 생기는 자체 그림자를 줄이는 양입니다.",
                "RealToon 설정을 다시 적용합니다.\n\n업데이트, 머티리얼 리셋, 재임포트 뒤 표시가 이상하면 눌러보세요.",
                "자체 그림자를 줄이는 양입니다.\n\nDirectional, Point, Spot Light에서 효과가 납니다.",
                "실시간 그림자의 경계가 얼마나 딱딱한지 정합니다.",
                "오브젝트에 생기는 셀프 그림자의 양입니다.",
                "버텍스 컬러의 초록 채널로 셀프 그림자 기준값을 조절합니다.",
                "셀프 그림자의 경계가 얼마나 딱딱한지 정합니다.",
                "셀프 그림자와 실시간 그림자 색입니다.\n\n이 값을 조절하기 전에는 Overall Shadow Color를 흰색으로 두는 것이 좋습니다.",
                "셀프 그림자와 실시간 그림자 색의 세기입니다.",
                "라이트의 그림자 강도가 셀프 그림자의 보이는 정도에 영향을 주게 합니다.",
                "오브젝트 노멀을 부드럽게 처리하는 양입니다. 툰 셰이딩의 그림자 경계를 정리할 때 씁니다.",
                "버텍스 컬러의 빨간 채널로 Smooth Object Normal 값을 조절합니다.",
                "노멀의 XYZ 위치 값입니다.",
                "투명도가 그림자에도 영향을 주게 합니다.",
                "오브젝트의 노멀 방향을 화면에 표시합니다. 디버그용입니다.",
                "그림자에 색을 입히는 텍스처입니다.\n\n실시간 그림자, 셀프 그림자, ShadowT에 영향을 줍니다. 기본 텍스처를 넣고 Power를 낮춰 어둡게 만드는 식으로도 쓸 수 있습니다.",
                "Shadow Color Texture가 얼마나 강하고 어둡게 보일지 정합니다.",
                "ShadowT가 얼마나 강하게 보일지 정합니다.",
                "ShadowT는 텍스처로 만드는 그림자입니다.\n\n검정/회색/흰색 텍스처를 사용합니다. 회색과 흰색은 빛 영향을 받고, 검정은 빛 영향을 거의 받지 않습니다.",
                "ShadowT에서 빛으로 판단되는 양입니다.",
                "ShadowT 그림자의 양입니다.",
                "ShadowT 그림자 경계의 딱딱함입니다.",
                "그림자 안에서도 ShadowT를 보이게 합니다.\n\n실시간 그림자/셀프 그림자 색이 완전 검정이면 잘 보이지 않을 수 있습니다.",
                "주변광이나 GI 영역에서도 ShadowT를 보이게 합니다.",
                "그림자와 주변광 안에서 ShadowT가 보이는 세기입니다.",
                "그림자와 주변광 안에서 ShadowT가 나타나는 기준값입니다.",
                "포인트/스팟 라이트의 거리 감쇠가 ShadowT에 영향을 주게 합니다.",
                "그림자에 패턴을 넣는 흑백 텍스처입니다.\n\n검은색은 패턴, 흰색은 없음입니다. 그림자 색이 검정이면 잘 보이지 않습니다.",
                "패턴이 얼마나 강하고 어둡게 보일지 정합니다.",
                "환경광 또는 GI를 받을지 켜고 끕니다.",
                "주변광, GI, 환경광이 오브젝트에 들어오는 세기입니다.",
                "GI/구면조화 조명도 셀 애니메이션처럼 평평한 명암으로 만듭니다.",
                "GI 그림자가 오브젝트에 생기는 양입니다.",
                "조명의 세기, 색, 거리 감쇠가 그림자에 영향을 주게 합니다.\n\n실시간 그림자, 셀프 그림자, ShadowT에 적용됩니다.",
                "Directional Light를 받는 세기입니다.",
                "Point/Spot Light를 받는 세기입니다.",
                "Point/Spot Light의 거리 감쇠가 얼마나 부드러운지 정합니다.",
                "커스텀 라이트 방향의 영향량입니다.",
                "직접 지정하는 XYZ 조명 방향입니다.",
                "커스텀 라이트 방향이 오브젝트 회전을 따라가게 합니다.",
                "반사가 얼마나 보일지 정합니다.",
                "반사의 거칠기입니다. 높을수록 반사가 흐려집니다.",
                "금속처럼 반사되는 느낌의 양입니다.",
                "반사 영역을 정하는 마스크입니다.\n\n흰색은 반사 보임, 검은색은 반사 안 보임입니다.",
                "가짜 반사로 사용할 텍스처 또는 이미지입니다.",
                "림라이트를 덜 채웁니다. 가장자리 빛의 폭을 줄이는 느낌입니다.",
                "림라이트 경계의 부드러움입니다.",
                "조명의 밝기와 색이 림라이트에 영향을 주게 합니다.",
                "림라이트 색입니다.",
                "림라이트 색의 세기입니다.",
                "림라이트가 밝은 영역에서만 보이게 합니다.",
                "See Through 기능에서 대상을 구분하는 ID 값입니다.\n\n기본값: 0",
                "See Through에서 A는 가리는 물체, B는 A를 통해 보일 물체입니다.\n\n기본값: A",
                "See Through에서 A는 가리는 물체, B는 A를 통해 보일 물체입니다.\n\n기본값: None",
                "평면이나 얇은 면의 뒷면에는 빛과 그림자가 보이지 않게 합니다.\n\nCulling이 Off 또는 Front일 때만 의미가 있습니다.",
                "RealToon 셰이더 파일의 컴파일 타깃을 2.0 또는 4.5로 바꿉니다.\n\n4.5로 바꾸면 DOTS/Hybrid Renderer와 Tessellation 지원에 필요할 수 있습니다.",
                "오브젝트가 받는 Directional Light 그림자를 숨깁니다.",
                "오브젝트가 받는 Point/Spot Light 그림자를 숨깁니다.",
                "이 오브젝트가 다른 곳에 그림자를 드리우지 않게 합니다.",
                "ZWrite를 켜거나 끕니다.\n\n외곽선에는 별도의 ZWrite 옵션이 있습니다.",
                "RealToon 머티리얼의 사용하지 않는 셰이더 키워드를 자동으로 지웁니다.\n\n인스펙터가 느려지면 꺼보세요. 이전 셰이더 키워드 기록도 지워질 수 있습니다.",
                "PTexture 패턴의 색입니다.",
                "카메라 공간에서 외곽선의 Z 위치를 조절합니다.",
                "실시간 그림자의 세기입니다.",
                "림라이트의 세기입니다.",
                "셀프 그림자와 실시간 그림자의 세기를 함께 조절합니다.",
                "셀프 그림자 색입니다.",
                "셀프 그림자 색의 세기입니다.",
                "ShadowT 색입니다.",
                "ShadowT 색의 세기입니다.",
                "ShadowT가 조명 방향이나 위치를 무시하게 합니다.",
                "그림자 안에서 빛이 얼마나 강하게 들어올지 정합니다.",
                "Point/Spot 같은 추가 조명을 사용합니다.",
                "전통적인 조명 블렌드를 사용합니다.\n\n켜면 조명이 더해지는 방식이고, 끄면 가장 강한 빛 기준으로 애니/카툰 느낌에 가깝게 섞입니다.",
                "셰이더 파일에서 외곽선 기능을 완전히 제거하거나 다시 추가합니다.\n\n머티리얼별 설정이 아니라 셰이더 전체 설정입니다.",
                "RealToon 비디오 튜토리얼 재생목록입니다.",
                "RealToon 사용자 가이드 문서입니다.",
                "RealToon 인스펙터 UI를 숨기거나 표시합니다.\n\n머티리얼별이 아니라 전체 설정입니다.",
                "깊이 기반 외곽선의 기준값을 조절합니다.",
                "외곽선을 셰이더 결과에 섞어 출력합니다.",
                "Baked 조명용으로 최적화합니다.\n\n켜면 실시간 기능 일부를 끕니다. RealTime 또는 Mixed 조명을 쓸 때는 끄는 것이 좋습니다.",
                "스크린 스페이스 외곽선과 전통 외곽선 중 선택합니다.\n\n스크린 스페이스 외곽선을 쓰려면 Depth Texture가 켜져 있어야 합니다. 셰이더 전체 설정입니다.",
                "Linear Blend Skinning 또는 Compute Deformation을 사용할 수 있게 합니다.\n\n셰이더 파일을 수정하며, 현재 Tessellation과는 함께 지원되지 않습니다.",
                "조명이 오브젝트 노멀의 Y 방향을 무시하게 합니다.",
                "SSAO, 즉 화면 공간 주변 폐색을 사용합니다.",
                "Ambient Occlusion의 색 또는 틴트입니다.",
                "오브젝트가 데칼을 받을 수 있게 합니다.",
                "가장자리 글로우 색입니다.",
                "글로우 가장자리의 폭입니다.",
                "간단한 투명 모드입니다.\n\nOpacity, Blend Modes, Affect Shadow만 사용합니다. Transparent Threshold와 Mask Transparency는 이 모드에서 쓰지 않습니다.",
                "DOTS 메시 변형 기능을 끕니다.\n\n움직이지 않는 Static 오브젝트라면 켜두는 것이 좋습니다.",
                "카메라 가까이에서 페이드가 시작되는 최소 거리입니다.",
                "카메라 가까이에서 페이드가 끝나는 최대 거리입니다.",
                "점무늬/디더링 방식의 부드러운 컷아웃입니다.",
                "Triplanar 텍스처의 타일 크기입니다.",
                "Triplanar 텍스처가 서로 섞이는 정도입니다.",
                "오브젝트의 원근감을 2D, 3D, FOV 늘림처럼 조절합니다.\n\n툰/애니 느낌의 2D 표현은 0.5 또는 0 근처를 써보세요.",
                "오브젝트의 클리핑을 조절합니다.\n\n앞뒤로 겹쳐 보일 때 조절합니다. 값을 올리면 더 많이 잘려 보입니다.",
                "카메라가 가까울 때 오브젝트 크기를 조절합니다.",
                "가까워질 때 크기 변화가 얼마나 부드럽게 이어질지 정합니다.",
                "카메라와 오브젝트 사이의 크기 전환 거리입니다.",
                "SDF 방식의 ShadowT입니다.\n\n3D 공간의 X/Z 축 기준으로만 영향을 줍니다.",
                "RealToon 셰이더에 See Through 기능을 추가하거나 제거합니다.\n\n기능이 필요 없다면 제거해도 됩니다. 셰이더 파일을 수정합니다.",
                "컷아웃 가장자리에 안티앨리어싱/MSAA를 적용합니다.\n\nForward/Forward+ 렌더링에서만 동작합니다. 컷아웃 기능을 끄면 이 옵션도 꺼집니다.",
                "Gloss Texture를 이방성 모드로 사용합니다.\n\n이 경우 Gloss Texture 입력을 노이즈처럼 사용합니다.",
                "노이즈 텍스처가 광택을 왜곡하는 세기입니다.",
                "이방성 광택의 폭입니다.",
                "이방성 광택을 위/아래로 이동합니다.",
                "노이즈 텍스처가 이방성 광택 폭에 영향을 주게 합니다.\n\n흰색은 1, 검은색은 0입니다.",
                "다른 오브젝트나 UI와 외곽선이 겹쳐 보이는 문제를 고칠 때 쓰는 스텐실 Pass 값입니다.",
                "조명의 최소/최대 세기를 제한합니다.",
                "조명 세기의 최소값입니다.",
                "조명 세기의 최대값입니다.",
                "컷아웃 안티앨리어싱의 부드러움입니다.",
                "SelfLit 마스크를 SelfLit Map 또는 Emission Map처럼 사용합니다.",
                "SelfLit/Emission 맵입니다.\n\n흑백/알파 맵 또는 RGB 컬러 맵을 사용할 수 있습니다.",
                "외곽선의 XYZ 크기를 조절합니다.",
                "클립 공간에서 오브젝트의 Z 위치를 조절합니다.\n\n앞뒤로 살짝 옮기거나 Perspective Adjustment의 Clip 보정에도 쓸 수 있습니다.",
                "림라이트가 그림자 영역에서만 보이게 합니다.",
                "림라이트의 위치입니다.",
                "오브젝트의 렌더 순서를 앞/뒤로 바꿉니다.\n\nRender Queue에 영향을 줍니다. 투명 모드나 ZWrite Off일 때 특히 유용합니다.",
                "이미 그려진 깊이값과 비교해서 픽셀을 그릴지 정합니다.\n\n외곽선에는 별도의 ZTest 옵션이 있습니다.",
                "외곽선만 적용되는 ZTest입니다.",
                "외곽선만 적용되는 ZWrite입니다.",
                "사용할 UV 세트/UV 채널입니다.\n\n모든 텍스처/맵 슬롯에 영향을 줍니다.",
                "Tessellation 기능을 켜거나 끕니다.\n\n셰이더 파일을 수정합니다.",
                "테셀레이션으로 나뉜 면을 부드럽게 합니다.",
                "가까운 거리와 먼 거리 테셀레이션 값 사이의 전환입니다.\n\n0은 가까운 값 위주, 1은 먼 값 위주입니다.",
                "카메라에 가까울 때 테셀레이션 양입니다.",
                "카메라에서 멀 때 테셀레이션 양입니다.",
                "노멀맵을 변위(Displacement)처럼 사용합니다.\n\nNormal Map 기능이 켜져 있어야 합니다."
            };

            string[] koreanFeatureTooltips =
            {
                "MatCap, 즉 머티리얼 캡처 효과입니다. 조명 대신 텍스처로 질감/광택을 입힙니다.",
                "노멀맵 기능입니다. 표면 굴곡 느낌을 추가합니다.",
                "외곽선 기능입니다.",
                "컷아웃 기능입니다. 알파 기준으로 일부 영역을 잘라냅니다.",
                "오브젝트 색을 보정합니다.",
                "자체 발광 또는 Emission 기능입니다.",
                "광택 기능입니다.",
                "텍스처로 광택을 표현합니다.\n\n흰색은 광택, 검은색은 광택 없음입니다.",
                "셀프 그림자 또는 음영 기능입니다.",
                "오브젝트 노멀을 부드럽게 하거나 무시해 툰 느낌의 명암을 정리합니다.",
                "텍스처로 그림자 색을 입힙니다.",
                "ShadowT는 텍스처로 만드는 그림자입니다.\n\n검정/회색/흰색 맵을 사용하며, 회색과 흰색은 빛 영향을 받고 검정은 거의 받지 않습니다.",
                "PTexture는 그림자에 패턴을 넣는 텍스처입니다.\n\n검은색은 패턴, 흰색은 없음입니다.",
                "직접 조명 방향을 지정합니다.",
                "반사 기능입니다.",
                "가짜 반사 기능입니다.\n\n원하는 텍스처나 이미지를 반사처럼 사용합니다.",
                "림라이트 또는 프레넬 효과입니다. 가장자리 빛을 만듭니다.",
                "카메라가 가까워지면 오브젝트가 서서히 사라지는 기능입니다.",
                "Triplanar 텍스처 기능입니다.\n\n환경 오브젝트처럼 균일한 텍스처 스케일이 필요할 때 유용합니다.",
                "오브젝트 원근감을 2D 툰/애니 느낌 또는 기본 3D 느낌으로 조절합니다.",
                "빠르게 움직일 때 생기는 애니메이션식 잔상/속도선 효과입니다."
            };

            for (int i = 0; i < koreanTooltips.Length && i < TOTIPS.Length; i++)
            {
                TOTIPS[i] = koreanTooltips[i];
            }

            for (int i = 0; i < koreanFeatureTooltips.Length && i < TOTIPSEDF.Length; i++)
            {
                TOTIPSEDF[i] = koreanFeatureTooltips[i];
            }

            koreanTooltipsApplied = true;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            ApplyKoreanTooltips();

            //This Material
            Material targetMat = materialEditor.target as Material;

            //Settings
            materialEditor.SetDefaultGUIWidths();

            if (MatRenQue == 0)
            {
                MatRenQue = targetMat.renderQueue;
            }

            //Content

            #region Shader Name Switch

            //switch (targetMat.shader.name)
            //{
            //case "Universal Render Pipeline/RealToon/Version 5/Default/Default":
            //    shader_name = "default_d";
            //    shader_type = "URP - Default";
            //    break;
            //case "Universal Render Pipeline/RealToon/Version 5/Default/Fade Transparency":
            //    shader_name = "default_ft";
            //    shader_type = "URP - Fade Transperancy"; 
            //    break;
            //case "RealToon/Version 5/Default/Refraction": //Temporarily Removed
            //shader_name = "default_ref"; //Temporarily Removed
            //shader_type = "Refraction";
            //break;
            //case "RealToon/Version 5/Tessellation/Default": //Temporarily Removed
            //shader_name = "tessellation_d"; //Temporarily Removed
            //shader_type = "Tessellation - Default"; //Temporarily Removed
            //break; //Temporarily Removed
            //case "RealToon/Version 5/Tessellation/Fade Transparency": //Temporarily Removed
            //shader_name = "tessellation_ft"; //Temporarily Removed
            //shader_type = "Tessellation - Fade Transparency"; //Temporarily Removed
            //break; //Temporarily Removed
            //case "RealToon/Version 5/Tessellation/Refraction": //Temporarily Removed
            //shader_name = "tessellation_ref"; //Temporarily Removed
            //shader_type = "Tessellation - Refraction"; //Temporarily Removed
            //break; //Temporarily Removed
            //default:
            //    shader_name = string.Empty;
            //    shader_type = string.Empty;
            //    break;
            //}


            #endregion

            #region Material Properties

            _UVSet = ShaderGUI.FindProperty("_UVSet", properties);
            _UseTLB = ShaderGUI.FindProperty("_UseTLB", properties);
            _Culling = ShaderGUI.FindProperty("_Culling", properties);
            _TRANSMODE = ShaderGUI.FindProperty("_TRANSMODE", properties);

            _MainTex = ShaderGUI.FindProperty("_MainTex", properties);
            _TexturePatternStyle = ShaderGUI.FindProperty("_TexturePatternStyle", properties);

            _MainColor = ShaderGUI.FindProperty("_MainColor", properties);
            _MaiColPo = ShaderGUI.FindProperty("_MaiColPo", properties);

            _MVCOL = ShaderGUI.FindProperty("_MVCOL", properties);
            _MCIALO = ShaderGUI.FindProperty("_MCIALO", properties);

            _MCapIntensity = ShaderGUI.FindProperty("_MCapIntensity", properties);
            _MCap = ShaderGUI.FindProperty("_MCap", properties);
            _SPECMODE = ShaderGUI.FindProperty("_SPECMODE", properties);
            _SPECIN = ShaderGUI.FindProperty("_SPECIN", properties);
            _MCapMask = ShaderGUI.FindProperty("_MCapMask", properties);

            _Cutout = ShaderGUI.FindProperty("_Cutout", properties);
            _UseSecondaryCutout = ShaderGUI.FindProperty("_UseSecondaryCutout", properties);
            _SecondaryCutout = ShaderGUI.FindProperty("_SecondaryCutout", properties);
            _AlphaBaseCutout = ShaderGUI.FindProperty("_AlphaBaseCutout", properties);
            _AAS = ShaderGUI.FindProperty("_AAS", properties);
            _N_F_SCO = ShaderGUI.FindProperty("_N_F_SCO", properties);
            _AlpToCov = ShaderGUI.FindProperty("_AlpToCov", properties);

            _N_F_COEDGL = ShaderGUI.FindProperty("_N_F_COEDGL", properties);
            _Glow_Color = ShaderGUI.FindProperty("_Glow_Color", properties);
            _Glow_Edge_Width = ShaderGUI.FindProperty("_Glow_Edge_Width", properties);

            _Opacity = ShaderGUI.FindProperty("_Opacity", properties);
            _TransparentThreshold = ShaderGUI.FindProperty("_TransparentThreshold", properties);
            _MaskTransparency = ShaderGUI.FindProperty("_MaskTransparency", properties);
            _BleModSour = ShaderGUI.FindProperty("_BleModSour", properties);
            _BleModDest = ShaderGUI.FindProperty("_BleModDest", properties);

            _SimTrans = ShaderGUI.FindProperty("_SimTrans", properties);
            _TransAffSha = ShaderGUI.FindProperty("_TransAffSha", properties);

            _NormalMap = ShaderGUI.FindProperty("_NormalMap", properties);
            _NormalMapIntensity = ShaderGUI.FindProperty("_NormalMapIntensity", properties);

            _Saturation = ShaderGUI.FindProperty("_Saturation", properties);

            _OutlineWidth = ShaderGUI.FindProperty("_OutlineWidth", properties);
            _OutlineWidthControl = ShaderGUI.FindProperty("_OutlineWidthControl", properties);
            _OutlineExtrudeMethod = ShaderGUI.FindProperty("_OutlineExtrudeMethod", properties);
            _OutlineOffset = ShaderGUI.FindProperty("_OutlineOffset", properties);
            _OutResi = ShaderGUI.FindProperty("_OutResi", properties);
            _OutlineZPostionInCamera = ShaderGUI.FindProperty("_OutlineZPostionInCamera", properties);
            _DoubleSidedOutline = ShaderGUI.FindProperty("_DoubleSidedOutline", properties);
            _OutlineColor = ShaderGUI.FindProperty("_OutlineColor", properties);
            _MixMainTexToOutline = ShaderGUI.FindProperty("_MixMainTexToOutline", properties);
            _NoisyOutlineIntensity = ShaderGUI.FindProperty("_NoisyOutlineIntensity", properties);
            _DynamicNoisyOutline = ShaderGUI.FindProperty("_DynamicNoisyOutline", properties);
            _LightAffectOutlineColor = ShaderGUI.FindProperty("_LightAffectOutlineColor", properties);
            _OutlineWidthAffectedByViewDistance = ShaderGUI.FindProperty("_OutlineWidthAffectedByViewDistance", properties);
            _FarDistanceMaxWidth = ShaderGUI.FindProperty("_FarDistanceMaxWidth", properties);
            _VertexColorBlueAffectOutlineWitdh = ShaderGUI.FindProperty("_VertexColorBlueAffectOutlineWitdh", properties);
            _OutStenPass = ShaderGUI.FindProperty("_OutStenPass", properties);
            _OutZWrite = ShaderGUI.FindProperty("_OutZWrite", properties);
            _OutZTest = ShaderGUI.FindProperty("_OutZTest", properties);

            _DepthThreshold = ShaderGUI.FindProperty("_DepthThreshold", properties);
            _N_F_MSSOLTFO = ShaderGUI.FindProperty("_N_F_MSSOLTFO", properties);

            _SelfLitIntensity = ShaderGUI.FindProperty("_SelfLitIntensity", properties);
            _SelfLitColor = ShaderGUI.FindProperty("_SelfLitColor", properties);
            _SelfLitPower = ShaderGUI.FindProperty("_SelfLitPower", properties);
            _TEXMCOLINT = ShaderGUI.FindProperty("_TEXMCOLINT", properties);
            _SelfLitHighContrast = ShaderGUI.FindProperty("_SelfLitHighContrast", properties);
            _N_F_SLMM = ShaderGUI.FindProperty("_N_F_SLMM", properties);
            _MaskSelfLit = ShaderGUI.FindProperty("_MaskSelfLit", properties);

            _GlossIntensity = ShaderGUI.FindProperty("_GlossIntensity", properties);
            _Glossiness = ShaderGUI.FindProperty("_Glossiness", properties);
            _GlossSoftness = ShaderGUI.FindProperty("_GlossSoftness", properties);
            _GlossColor = ShaderGUI.FindProperty("_GlossColor", properties);
            _GlossColorPower = ShaderGUI.FindProperty("_GlossColorPower", properties);
            _MaskGloss = ShaderGUI.FindProperty("_MaskGloss", properties);

            _GlossTexture = ShaderGUI.FindProperty("_GlossTexture", properties);
            _GlossTextureSoftness = ShaderGUI.FindProperty("_GlossTextureSoftness", properties);
            _PSGLOTEX = ShaderGUI.FindProperty("_PSGLOTEX", properties);
            _GlossTextureRotate = ShaderGUI.FindProperty("_GlossTextureRotate", properties);
            _GlossTextureFollowObjectRotation = ShaderGUI.FindProperty("_GlossTextureFollowObjectRotation", properties);
            _N_F_ANIS = ShaderGUI.FindProperty("_N_F_ANIS", properties);
            _NoisTexInten = ShaderGUI.FindProperty("_NoisTexInten", properties);
            _StraWidt = ShaderGUI.FindProperty("_StraWidt", properties);
            _NoiTexAffStraWidt = ShaderGUI.FindProperty("_NoiTexAffStraWidt", properties);
            _ShifAnis = ShaderGUI.FindProperty("_ShifAnis", properties);
            _GlossTextureFollowLight = ShaderGUI.FindProperty("_GlossTextureFollowLight", properties);

            _OverallShadowColor = ShaderGUI.FindProperty("_OverallShadowColor", properties);
            _OverallShadowColorPower = ShaderGUI.FindProperty("_OverallShadowColorPower", properties);
            _SelfShadowShadowTAtViewDirection = ShaderGUI.FindProperty("_SelfShadowShadowTAtViewDirection", properties);

            _HighlightColor = ShaderGUI.FindProperty("_HighlightColor", properties);
            _HighlightColorPower = ShaderGUI.FindProperty("_HighlightColorPower", properties);

            _SelfShadowThreshold = ShaderGUI.FindProperty("_SelfShadowThreshold", properties);
            _VertexColorGreenControlSelfShadowThreshold = ShaderGUI.FindProperty("_VertexColorGreenControlSelfShadowThreshold", properties);
            _SelfShadowHardness = ShaderGUI.FindProperty("_SelfShadowHardness", properties);

            _SelfShadowRealtimeShadowIntensity = ShaderGUI.FindProperty("_SelfShadowRealtimeShadowIntensity", properties);

            _SelfShadowRealTimeShadowColor = ShaderGUI.FindProperty("_SelfShadowRealTimeShadowColor", properties);
            _SelfShadowRealTimeShadowColorPower = ShaderGUI.FindProperty("_SelfShadowRealTimeShadowColorPower", properties);

            _LigIgnoYNorDir = ShaderGUI.FindProperty("_LigIgnoYNorDir", properties);
            _SelfShadowAffectedByLightShadowStrength = ShaderGUI.FindProperty("_SelfShadowAffectedByLightShadowStrength", properties);

            _SmoothObjectNormal = ShaderGUI.FindProperty("_SmoothObjectNormal", properties);
            _VertexColorRedControlSmoothObjectNormal = ShaderGUI.FindProperty("_VertexColorRedControlSmoothObjectNormal", properties);
            _XYZPosition = ShaderGUI.FindProperty("_XYZPosition", properties);
            _ShowNormal = ShaderGUI.FindProperty("_ShowNormal", properties);

            _ShadowColorTexture = ShaderGUI.FindProperty("_ShadowColorTexture", properties);
            _ShadowColorTexturePower = ShaderGUI.FindProperty("_ShadowColorTexturePower", properties);

            _ShadowTIntensity = ShaderGUI.FindProperty("_ShadowTIntensity", properties);
            _ShadowT = ShaderGUI.FindProperty("_ShadowT", properties);
            _ShadowTLightThreshold = ShaderGUI.FindProperty("_ShadowTLightThreshold", properties);
            _ShadowTShadowThreshold = ShaderGUI.FindProperty("_ShadowTShadowThreshold", properties);
            _ShadowTColor = ShaderGUI.FindProperty("_ShadowTColor", properties);
            _ShadowTColorPower = ShaderGUI.FindProperty("_ShadowTColorPower", properties);
            _ShadowTHardness = ShaderGUI.FindProperty("_ShadowTHardness", properties);
            _STIL = ShaderGUI.FindProperty("_STIL", properties);
            _N_F_STIS = ShaderGUI.FindProperty("_N_F_STIS", properties);
            _N_F_STIAL = ShaderGUI.FindProperty("_N_F_STIAL", properties);
            _ShowInAmbientLightShadowIntensity = ShaderGUI.FindProperty("_ShowInAmbientLightShadowIntensity", properties);
            _ShowInAmbientLightShadowThreshold = ShaderGUI.FindProperty("_ShowInAmbientLightShadowThreshold", properties);

            _LightFalloffAffectShadowT = ShaderGUI.FindProperty("_LightFalloffAffectShadowT", properties);

            _N_F_STSDFM = ShaderGUI.FindProperty("_N_F_STSDFM", properties);

            _PTexture = ShaderGUI.FindProperty("_PTexture", properties);
            _PTCol = ShaderGUI.FindProperty("_PTCol", properties);
            _PTexturePower = ShaderGUI.FindProperty("_PTexturePower", properties);

            _EnvironmentalLightingIntensity = ShaderGUI.FindProperty("_EnvironmentalLightingIntensity", properties);
            _RELG = ShaderGUI.FindProperty("_RELG", properties);

            _GIFlatShade = ShaderGUI.FindProperty("_GIFlatShade", properties);
            _GIShadeThreshold = ShaderGUI.FindProperty("_GIShadeThreshold", properties);
            _LightAffectShadow = ShaderGUI.FindProperty("_LightAffectShadow", properties);
            _LightIntensity = ShaderGUI.FindProperty("_LightIntensity", properties);

            _N_F_EAL = ShaderGUI.FindProperty("_N_F_EAL", properties);

            _DirectionalLightIntensity = ShaderGUI.FindProperty("_DirectionalLightIntensity", properties);
            _PointSpotlightIntensity = ShaderGUI.FindProperty("_PointSpotlightIntensity", properties);
            _LightFalloffSoftness = ShaderGUI.FindProperty("_LightFalloffSoftness", properties);

            _N_F_LLI = ShaderGUI.FindProperty("_N_F_LLI", properties);
            _LLI_Min = ShaderGUI.FindProperty("_LLI_Min", properties);
            _LLI_Max = ShaderGUI.FindProperty("_LLI_Max", properties);

            _ReduSha = ShaderGUI.FindProperty("_ReduSha", properties);
            _ShadowHardness = ShaderGUI.FindProperty("_ShadowHardness", properties);

            _CustomLightDirectionIntensity = ShaderGUI.FindProperty("_CustomLightDirectionIntensity", properties);
            _CustomLightDirectionFollowObjectRotation = ShaderGUI.FindProperty("_CustomLightDirectionFollowObjectRotation", properties);
            _CustomLightDirection = ShaderGUI.FindProperty("_CustomLightDirection", properties);

            _ReflectionIntensity = ShaderGUI.FindProperty("_ReflectionIntensity", properties);
            _Smoothness = ShaderGUI.FindProperty("_Smoothness", properties);
            _RefMetallic = ShaderGUI.FindProperty("_RefMetallic", properties);
            _MaskReflection = ShaderGUI.FindProperty("_MaskReflection", properties);
            _FReflection = ShaderGUI.FindProperty("_FReflection", properties);

            _RimLigInt = ShaderGUI.FindProperty("_RimLigInt", properties);
            _RimLightUnfill = ShaderGUI.FindProperty("_RimLightUnfill", properties);
            _RimLightColor = ShaderGUI.FindProperty("_RimLightColor", properties);
            _RimLightColorPower = ShaderGUI.FindProperty("_RimLightColorPower", properties);
            _RimLightSoftness = ShaderGUI.FindProperty("_RimLightSoftness", properties);
            _RimLigPosi = ShaderGUI.FindProperty("_RimLigPosi", properties);
            _RimLightInLight = ShaderGUI.FindProperty("_RimLightInLight", properties);
            _LightAffectRimLightColor = ShaderGUI.FindProperty("_LightAffectRimLightColor", properties);
            _N_F_RLIS = ShaderGUI.FindProperty("_N_F_RLIS", properties);

            _MinFadDistance = ShaderGUI.FindProperty("_MinFadDistance", properties);
            _MaxFadDistance = ShaderGUI.FindProperty("_MaxFadDistance", properties);

            _TriPlaTile = ShaderGUI.FindProperty("_TriPlaTile", properties);
            _TriPlaBlend = ShaderGUI.FindProperty("_TriPlaBlend", properties);

            _PresAdju = ShaderGUI.FindProperty("_PresAdju", properties);
            _ClipAdju = ShaderGUI.FindProperty("_ClipAdju", properties);
            _PASize = ShaderGUI.FindProperty("_PASize", properties);
            _PASmooTrans = ShaderGUI.FindProperty("_PASmooTrans", properties);
            _PADist = ShaderGUI.FindProperty("_PADist", properties);

            _TessellationSmoothness = ShaderGUI.FindProperty("_TessellationSmoothness", properties);
            _TessellationTransition = ShaderGUI.FindProperty("_TessellationTransition", properties);
            _TessellationNear = ShaderGUI.FindProperty("_TessellationNear", properties);
            _TessellationFar = ShaderGUI.FindProperty("_TessellationFar", properties);
            _NorMapAsDis = ShaderGUI.FindProperty("_NorMapAsDis", properties);

            _RefVal = ShaderGUI.FindProperty("_RefVal", properties);
            _Oper = ShaderGUI.FindProperty("_Oper", properties);
            _Compa = ShaderGUI.FindProperty("_Compa", properties);

            _N_F_MC = ShaderGUI.FindProperty("_N_F_MC", properties);
            _N_F_NM = ShaderGUI.FindProperty("_N_F_NM", properties);
            _N_F_CO = ShaderGUI.FindProperty("_N_F_CO", properties);
            _N_F_O = ShaderGUI.FindProperty("_N_F_O", properties);
            _N_F_CA = ShaderGUI.FindProperty("_N_F_CA", properties);
            _N_F_SL = ShaderGUI.FindProperty("_N_F_SL", properties);
            _N_F_GLO = ShaderGUI.FindProperty("_N_F_GLO", properties);
            _N_F_GLOT = ShaderGUI.FindProperty("_N_F_GLOT", properties);
            _N_F_SS = ShaderGUI.FindProperty("_N_F_SS", properties);
            _N_F_SON = ShaderGUI.FindProperty("_N_F_SON", properties);
            _N_F_SCT = ShaderGUI.FindProperty("_N_F_SCT", properties);
            _N_F_ST = ShaderGUI.FindProperty("_N_F_ST", properties);
            _N_F_PT = ShaderGUI.FindProperty("_N_F_PT", properties);
            _N_F_CLD = ShaderGUI.FindProperty("_N_F_CLD", properties);
            _N_F_R = ShaderGUI.FindProperty("_N_F_R", properties);
            _N_F_FR = ShaderGUI.FindProperty("_N_F_FR", properties);
            _N_F_RL = ShaderGUI.FindProperty("_N_F_RL", properties);
            _N_F_NFD = ShaderGUI.FindProperty("_N_F_NFD", properties);
            _N_F_TP = ShaderGUI.FindProperty("_N_F_TP", properties);
            _N_F_PA = ShaderGUI.FindProperty("_N_F_PA", properties);
            _N_F_SE = ShaderGUI.FindProperty("_N_F_SE", properties);

            _N_F_HDLS = ShaderGUI.FindProperty("_N_F_HDLS", properties);
            _N_F_HPSS = ShaderGUI.FindProperty("_N_F_HPSS", properties);

            _N_F_DCS = ShaderGUI.FindProperty("_N_F_DCS", properties);

            _N_F_HDLS = ShaderGUI.FindProperty("_N_F_HDLS", properties);
            _N_F_HPSS = ShaderGUI.FindProperty("_N_F_HPSS", properties);
            _N_F_DCS = ShaderGUI.FindProperty("_N_F_DCS", properties);
            _ObjePosiZCS = ShaderGUI.FindProperty("_ObjePosiZCS", properties);
            _ZWrite = ShaderGUI.FindProperty("_ZWrite", properties);
            _ZTest = ShaderGUI.FindProperty("_ZTest", properties);

            _N_F_NLASOBF = ShaderGUI.FindProperty("_N_F_NLASOBF", properties);

            _N_F_OFLMB = ShaderGUI.FindProperty("_N_F_OFLMB", properties);

            _N_F_ESSAO = ShaderGUI.FindProperty("_N_F_ESSAO", properties);
            _SSAOColor = ShaderGUI.FindProperty("_SSAOColor", properties);

            _N_F_RDC = ShaderGUI.FindProperty("_N_F_RDC", properties);

            _N_F_DDMD = ShaderGUI.FindProperty("_N_F_DDMD", properties);

            _RQSO = ShaderGUI.FindProperty("_RQSO", properties);

            #endregion

            //UI

            #region UI

            //Header
            Rect r_header = EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("RealToon 5.0.15", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("(" + srp_mode + " - " + shader_type + ")", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            if (ShowUI == true)
            {

                GUILayout.Space(20);

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                //Light Blend

                #region Light Blend

                Rect r_lightblend = EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Light Blend Style: " + LightBlendString);
                EditorGUILayout.EndVertical();

                switch ((int)_UseTLB.floatValue)
                {
                    case 0:
                        LightBlendString = "Anime/Cartoon";
                        break;
                    case 1:
                        LightBlendString = "Traditional";
                        break;
                    default:
                        break;
                }

                #endregion

                //Double Sided

                #region Culling

                Rect r_culling = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_Culling, new GUIContent(_Culling.displayName, TOTIPS[0]));
                EditorGUILayout.EndVertical();

                #endregion

                //UV Set

                #region UV Set

                Rect r_uvset = EditorGUILayout.BeginVertical("HelpBox");
                materialEditor.ShaderProperty(_UVSet, new GUIContent(_UVSet.displayName, TOTIPS[185]));
                EditorGUILayout.EndVertical();

                #endregion

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                //Transparent Mode

                #region Transparent Mode

                Rect r_renderqueue = EditorGUILayout.BeginVertical("HelpBox");

                EditorGUI.BeginChangeCheck();

                materialEditor.ShaderProperty(_TRANSMODE, new GUIContent(_TRANSMODE.displayName, TOTIPS[11]));

                if (EditorGUI.EndChangeCheck())
                {
                    foreach (Material m in materialEditor.targets)
                    {
                        switch (_TRANSMODE.floatValue)
                        {
                            case 0:

                                m.renderQueue = -1;
                                MatRenQue = 2000;
                                m.SetOverrideTag("RenderType", "Opaque");
                                m.SetInt("_BleModSour", 1);
                                m.SetInt("_BleModDest", 0);
                                shader_type = "Default";
                                break;

                            case 1:

                                m.SetInt("_BleModSour", 5);
                                m.SetInt("_BleModDest", 10);

                                if (m.IsKeywordEnabled("N_F_CO_ON") || m.GetFloat("_N_F_CO") == 1.0f)
                                {
                                    m.renderQueue = 2450;
                                    MatRenQue = m.renderQueue;
                                    m.SetOverrideTag("RenderType", "TransparentCutout");
                                }
                                else
                                {
                                    m.renderQueue = 3000;
                                    MatRenQue = m.renderQueue;
                                    m.SetOverrideTag("RenderType", "Transparent");
                                }

                                shader_type = "Fade Transperancy";
                                break;

                            default:
                                break;
                        }

                    }

                    materialEditor.PropertiesChanged();

                }

                EditorGUILayout.EndVertical();

                #endregion

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.Space(20);


                //Texture - Color

                #region Texture - Color

                Rect r_texturecolor = EditorGUILayout.BeginVertical("Button");

                ShowTextureColor = EditorGUILayout.Foldout(ShowTextureColor, "(Texture - Color)", true, EditorStyles.foldout);

                if (ShowTextureColor)
                {

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MainTex, new GUIContent(_MainTex.displayName, TOTIPS[1]));

                    EditorGUI.BeginDisabledGroup(_MainTex.textureValue == null);
                    materialEditor.ShaderProperty(_TexturePatternStyle, new GUIContent(_TexturePatternStyle.displayName, TOTIPS[2]));
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_MainColor, new GUIContent(_MainColor.displayName, TOTIPS[3]));
                    materialEditor.ShaderProperty(_MaiColPo, new GUIContent(_MaiColPo.displayName, TOTIPS[8]));

                    GUILayout.Space(10);
                    materialEditor.ShaderProperty(_MVCOL, new GUIContent(_MVCOL.displayName, TOTIPS[4]));

                    GUILayout.Space(10);
                    materialEditor.ShaderProperty(_MCIALO, new GUIContent(_MCIALO.displayName, TOTIPS[5]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_HighlightColor, new GUIContent(_HighlightColor.displayName, TOTIPS[6]));
                    materialEditor.ShaderProperty(_HighlightColorPower, new GUIContent(_HighlightColorPower.displayName, TOTIPS[7]));

                    GUILayout.Space(10);

                }

                EditorGUILayout.EndVertical();

                #endregion

                //MatCap

                #region MatCap

                if (_N_F_MC.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_matcap = EditorGUILayout.BeginVertical("Button");
                    ShowMatCap = EditorGUILayout.Foldout(ShowMatCap, "(MatCap)", true, EditorStyles.foldout);

                    if (ShowMatCap)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_MCapIntensity, new GUIContent(_MCapIntensity.displayName, TOTIPS[13]));
                        materialEditor.ShaderProperty(_MCap, _MCap.displayName);

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_SPECMODE, new GUIContent(_SPECMODE.displayName, TOTIPS[14]));
                        EditorGUI.BeginDisabledGroup(_SPECMODE.floatValue == 0);
                        materialEditor.ShaderProperty(_SPECIN, new GUIContent(_SPECIN.displayName, TOTIPS[15]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_MCapMask, new GUIContent(_MCapMask.displayName, TOTIPS[16]));

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();
                }

                #endregion

                //Cutout

                #region Cutout

                if (_TRANSMODE.floatValue == 1)
                {
                    if (_N_F_CO.floatValue == 1)
                    {
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        EditorGUI.BeginDisabledGroup(_N_F_CO.floatValue == 0);

                        Rect r_cutout = EditorGUILayout.BeginVertical("Button");
                        ShowCutout = EditorGUILayout.Foldout(ShowCutout, "(Cutout)", true, EditorStyles.foldout);

                        if (ShowCutout)
                        {

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_Cutout, new GUIContent(_Cutout.displayName, TOTIPS[17]));
                            materialEditor.ShaderProperty(_AlphaBaseCutout, new GUIContent(_AlphaBaseCutout.displayName, TOTIPS[18]));
                            materialEditor.ShaderProperty(_N_F_SCO, new GUIContent(_N_F_SCO.displayName, TOTIPS[154]));

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_AlpToCov, new GUIContent(_AlpToCov.displayName, TOTIPS[164]));

                            EditorGUI.BeginDisabledGroup(_AlpToCov.floatValue == 0.0f);
                            materialEditor.ShaderProperty(_AAS, new GUIContent(_AAS.displayName, TOTIPS[174]));
                            EditorGUI.EndDisabledGroup();

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_UseSecondaryCutout, new GUIContent(_UseSecondaryCutout.displayName, TOTIPS[19]));
                            materialEditor.ShaderProperty(_SecondaryCutout, new GUIContent(_SecondaryCutout.displayName, TOTIPS[20]));

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_N_F_COEDGL, _N_F_COEDGL.displayName);
                            EditorGUI.BeginDisabledGroup(_N_F_COEDGL.floatValue == 0.0f);
                            materialEditor.ShaderProperty(_Glow_Color, new GUIContent(_Glow_Color.displayName, TOTIPS[148]));
                            materialEditor.ShaderProperty(_Glow_Edge_Width, new GUIContent(_Glow_Edge_Width.displayName, TOTIPS[149]));
                            EditorGUI.EndDisabledGroup();

                            GUILayout.Space(10);

                        }

                        EditorGUILayout.EndVertical();

                        EditorGUI.EndDisabledGroup();
                    }
                }

                #endregion

                //Transperancy

                #region Transperancy

                if (_TRANSMODE.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    EditorGUI.BeginDisabledGroup(_N_F_CO.floatValue == 1);

                    Rect r_transparency = EditorGUILayout.BeginVertical("Button");
                    ShowTransparency = EditorGUILayout.Foldout(ShowTransparency, "(Transparency)", true, EditorStyles.foldout);

                    if (ShowTransparency)
                    {

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_SimTrans, new GUIContent(_SimTrans.displayName, TOTIPS[150]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_Opacity, new GUIContent(_Opacity.displayName, TOTIPS[21]));

                        EditorGUI.BeginDisabledGroup(_SimTrans.floatValue == 1);
                        materialEditor.ShaderProperty(_TransparentThreshold, new GUIContent(_TransparentThreshold.displayName, TOTIPS[22]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_BleModSour, new GUIContent(_BleModSour.displayName, TOTIPS[9]));
                        materialEditor.ShaderProperty(_BleModDest, new GUIContent(_BleModDest.displayName, TOTIPS[10]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_TransAffSha, new GUIContent(_TransAffSha.displayName, TOTIPS[74]));

                        GUILayout.Space(10);

                        EditorGUI.BeginDisabledGroup(_SimTrans.floatValue == 1);
                        materialEditor.ShaderProperty(_MaskTransparency, new GUIContent(_MaskTransparency.displayName, TOTIPS[23]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);

                    }

                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndVertical();
                }

                #endregion

                //Normal Map

                #region Normal Map

                if (_N_F_NM.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_normalmap = EditorGUILayout.BeginVertical("Button");
                    ShowNormalMap = EditorGUILayout.Foldout(ShowNormalMap, "(Normal Map)", true, EditorStyles.foldout);

                    if (ShowNormalMap)
                    {
                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_NormalMap, new GUIContent(_NormalMap.displayName, TOTIPS[24]));

                        EditorGUI.BeginDisabledGroup(_NormalMap.textureValue == null);
                        materialEditor.ShaderProperty(_NormalMapIntensity, new GUIContent(_NormalMapIntensity.displayName, TOTIPS[25]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();

                }
                #endregion

                //Color Adjustment

                #region Color Adjustment

                if (_N_F_CA.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_cadjustment = EditorGUILayout.BeginVertical("Button");
                    ShowColorAdjustment = EditorGUILayout.Foldout(ShowColorAdjustment, "Color Adjustment", true, EditorStyles.foldout);

                    if (ShowColorAdjustment)
                    {

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_Saturation, new GUIContent(_Saturation.displayName, TOTIPS[26]));

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical(); ;

                }

                #endregion

                //Outline

                #region Outline

                if (remoout == true)
                {

                    if (_N_F_O.floatValue == 1)
                    {

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        Rect r_outline = EditorGUILayout.BeginVertical("Button");
                        ShowOutline = EditorGUILayout.Foldout(ShowOutline, "(Outline - " + OLType + ")", true, EditorStyles.foldout);


                        if (ShowOutline)
                        {

                            GUILayout.Space(10);

                            EditorGUI.BeginDisabledGroup(_TRANSMODE.floatValue == 1 && _N_F_CO.floatValue == 0 && UseSSOL == false);
                            materialEditor.ShaderProperty(_OutlineWidth, new GUIContent(_OutlineWidth.displayName, TOTIPS[8]));
                            EditorGUI.EndDisabledGroup();

                            if (UseSSOL == true)
                            {

                                materialEditor.ShaderProperty(_OutlineWidthControl, new GUIContent(_OutlineWidthControl.displayName, TOTIPS[28]));

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_OutlineExtrudeMethod, new GUIContent(_OutlineExtrudeMethod.displayName, TOTIPS[29]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_OutlineOffset, new GUIContent(_OutlineOffset.displayName, TOTIPS[30]));

                                materialEditor.ShaderProperty(_OutResi, new GUIContent(_OutResi.displayName, TOTIPS[177]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_OutlineZPostionInCamera, new GUIContent(_OutlineZPostionInCamera.displayName, TOTIPS[123]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_DoubleSidedOutline, new GUIContent(_DoubleSidedOutline.displayName, TOTIPS[31]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_OutlineColor, new GUIContent(_OutlineColor.displayName, TOTIPS[32]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_MixMainTexToOutline, new GUIContent(_MixMainTexToOutline.displayName, TOTIPS[33]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_NoisyOutlineIntensity, new GUIContent(_NoisyOutlineIntensity.displayName, TOTIPS[34]));
                                materialEditor.ShaderProperty(_DynamicNoisyOutline, new GUIContent(_DynamicNoisyOutline.displayName, TOTIPS[35]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_LightAffectOutlineColor, new GUIContent(_LightAffectOutlineColor.displayName, TOTIPS[36]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_OutlineWidthAffectedByViewDistance, new GUIContent(_OutlineWidthAffectedByViewDistance.displayName, TOTIPS[37]));
                                EditorGUI.BeginDisabledGroup(_OutlineWidthAffectedByViewDistance.floatValue == 0);
                                materialEditor.ShaderProperty(_FarDistanceMaxWidth, new GUIContent(_FarDistanceMaxWidth.displayName, TOTIPS[38]));
                                EditorGUI.EndDisabledGroup();

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_VertexColorBlueAffectOutlineWitdh, new GUIContent(_VertexColorBlueAffectOutlineWitdh.displayName, TOTIPS[39]));

                            }
                            else
                            {
                                EditorGUI.BeginDisabledGroup(_TRANSMODE.floatValue == 1 && _N_F_CO.floatValue == 0);
                                materialEditor.ShaderProperty(_OutlineColor, new GUIContent(_OutlineColor.displayName, TOTIPS[28]));

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_N_F_MSSOLTFO, new GUIContent(_N_F_MSSOLTFO.displayName, TOTIPS[140]));

                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                                materialEditor.ShaderProperty(_DepthThreshold, new GUIContent(_DepthThreshold.displayName, TOTIPS[122]));
                                EditorGUI.EndDisabledGroup();

                            }

                            if (add_st == true)
                            {
                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_OutStenPass, new GUIContent(_OutStenPass.displayName, TOTIPS[170]));
                            }

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_OutZWrite, new GUIContent(_OutZWrite.displayName, TOTIPS[184]));
                            materialEditor.ShaderProperty(_OutZTest, new GUIContent(_OutZTest.displayName, TOTIPS[183]));

                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            if (GUILayout.Button(new GUIContent(UseSSOLStat, TOTIPS[142]), "Button"))
                            {
                                USSOL_OR_TOL();
                            }

                            GUILayout.Space(10);

                        }

                        EditorGUILayout.EndVertical();

                    }

                }

                #endregion

                //Self Lit

                #region SelfLit

                if (_N_F_SL.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_selflit = EditorGUILayout.BeginVertical("Button");
                    ShowSelfLit = EditorGUILayout.Foldout(ShowSelfLit, "(Self Lit)", true, EditorStyles.foldout);

                    if (ShowSelfLit)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_SelfLitIntensity, new GUIContent(_SelfLitIntensity.displayName, TOTIPS[40]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_SelfLitColor, new GUIContent(_SelfLitColor.displayName, TOTIPS[41]));
                        materialEditor.ShaderProperty(_SelfLitPower, new GUIContent(_SelfLitPower.displayName, TOTIPS[42]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_TEXMCOLINT, new GUIContent(_TEXMCOLINT.displayName, TOTIPS[43]));

                        GUILayout.Space(10);

                        EditorGUI.BeginDisabledGroup(_N_F_SLMM.floatValue == 1);
                            materialEditor.ShaderProperty(_SelfLitHighContrast, new GUIContent(_SelfLitHighContrast.displayName, TOTIPS[44]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_N_F_SLMM, new GUIContent(_N_F_SLMM.displayName, TOTIPS[175]));

                        GUILayout.Space(1);

                        if (_N_F_SLMM.floatValue == 1)
                        {
                            materialEditor.ShaderProperty(_MaskSelfLit, new GUIContent("Self Lit Map", TOTIPS[176]));
                        }
                        else
                        {
                            materialEditor.ShaderProperty(_MaskSelfLit, new GUIContent(_MaskSelfLit.displayName, TOTIPS[45]));
                        }

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();

                }
                #endregion

                //Gloss

                #region Gloss

                if (_N_F_OFLMB.floatValue == 0)
                {

                    if (_N_F_GLO.floatValue == 1)
                    {
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        Rect r_gloss = EditorGUILayout.BeginVertical("Button");
                        ShowGloss = EditorGUILayout.Foldout(ShowGloss, "(Gloss)", true, EditorStyles.foldout);

                        if (ShowGloss)
                        {

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_GlossIntensity, new GUIContent(_GlossIntensity.displayName, TOTIPS[46]));

                            EditorGUI.BeginDisabledGroup(_N_F_GLOT.floatValue == 1);
                            materialEditor.ShaderProperty(_Glossiness, new GUIContent(_Glossiness.displayName, TOTIPS[47]));
                            EditorGUI.EndDisabledGroup();

                            materialEditor.ShaderProperty(_GlossSoftness, new GUIContent(_GlossSoftness.displayName, TOTIPS[48]));

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_GlossColor, new GUIContent(_GlossColor.displayName, TOTIPS[49]));
                            materialEditor.ShaderProperty(_GlossColorPower, new GUIContent(_GlossColorPower.displayName, TOTIPS[50]));

                            GUILayout.Space(10);

                            materialEditor.ShaderProperty(_MaskGloss, new GUIContent(_MaskGloss.displayName, TOTIPS[51]));

                            GUILayout.Space(10);

                            //Gloss Texture

                            #region Gloss Texture

                            if (_N_F_GLOT.floatValue == 1)
                            {

                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                                Rect r_glosstexture = EditorGUILayout.BeginVertical("Button");
                                GUILayout.Label("Gloss Texture", EditorStyles.boldLabel);
                                EditorGUILayout.EndVertical();

                                if (_N_F_GLOT.floatValue == 1)
                                {

                                    GUILayout.Space(10);

                                    materialEditor.ShaderProperty(_GlossTexture, new GUIContent(_GlossTexture.displayName, TOTIPS[52]));

                                    GUILayout.Space(10);

                                    materialEditor.ShaderProperty(_N_F_ANIS, new GUIContent(_N_F_ANIS.displayName, TOTIPS[165]));

                                    GUILayout.Space(10);


                                    if (_N_F_ANIS.floatValue == 1)
                                    {

                                        materialEditor.ShaderProperty(_NoisTexInten, new GUIContent(_NoisTexInten.displayName, TOTIPS[166]));

                                        GUILayout.Space(10);

                                        materialEditor.ShaderProperty(_StraWidt, new GUIContent(_StraWidt.displayName, TOTIPS[167]));
                                        materialEditor.ShaderProperty(_NoiTexAffStraWidt, new GUIContent(_NoiTexAffStraWidt.displayName, TOTIPS[169]));

                                        GUILayout.Space(10);

                                        materialEditor.ShaderProperty(_ShifAnis, new GUIContent(_ShifAnis.displayName, TOTIPS[168]));
                                        materialEditor.ShaderProperty(_GlossTextureFollowLight, new GUIContent(_GlossTextureFollowLight.displayName, TOTIPS[57]));

                                    }
                                    else if (_N_F_ANIS.floatValue == 0)
                                    {

                                        EditorGUI.BeginDisabledGroup(_GlossTexture.textureValue == null);
                                        materialEditor.ShaderProperty(_GlossTextureSoftness, new GUIContent(_GlossTextureSoftness.displayName, TOTIPS[53]));

                                        GUILayout.Space(10);

                                        materialEditor.ShaderProperty(_PSGLOTEX, new GUIContent(_PSGLOTEX.displayName, TOTIPS[54]));

                                        GUILayout.Space(10);

                                        EditorGUI.BeginDisabledGroup(_PSGLOTEX.floatValue == 1);
                                        materialEditor.ShaderProperty(_GlossTextureRotate, new GUIContent(_GlossTextureRotate.displayName, TOTIPS[55]));
                                        materialEditor.ShaderProperty(_GlossTextureFollowObjectRotation, new GUIContent(_GlossTextureFollowObjectRotation.displayName, TOTIPS[56]));
                                        materialEditor.ShaderProperty(_GlossTextureFollowLight, new GUIContent(_GlossTextureFollowLight.displayName, TOTIPS[57]));
                                        EditorGUI.EndDisabledGroup();

                                        EditorGUI.EndDisabledGroup();
                                    }

                                }

                                GUILayout.Space(10);

                            }
                            #endregion

                        }

                        EditorGUILayout.EndVertical();

                    }

                }

                #endregion

                //Shadow

                #region Shadow

                if (_N_F_OFLMB.floatValue == 0)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_shadow = EditorGUILayout.BeginVertical("Button");
                    ShowShadow = EditorGUILayout.Foldout(ShowShadow, "(Shadow)", true, EditorStyles.foldout);

                    if (ShowShadow)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_OverallShadowColor, new GUIContent(_OverallShadowColor.displayName, TOTIPS[58]));
                        materialEditor.ShaderProperty(_OverallShadowColorPower, new GUIContent(_OverallShadowColorPower.displayName, TOTIPS[59]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_SelfShadowShadowTAtViewDirection, new GUIContent(_SelfShadowShadowTAtViewDirection.displayName, TOTIPS[60]));
                        materialEditor.ShaderProperty(_LigIgnoYNorDir, new GUIContent(_LigIgnoYNorDir.displayName, TOTIPS[144]));

                        GUILayout.Space(10);

                        //materialEditor.ShaderProperty(_ReduceShadowPointLight, _ReduceShadowPointLight.displayName);
                        //materialEditor.ShaderProperty(_PointLightSVD, _PointLightSVD.displayName);

                        materialEditor.ShaderProperty(_ReduSha, new GUIContent(_ReduSha.displayName, TOTIPS[63]));

                        if (_N_F_HDLS.floatValue == 0 || _N_F_HPSS.floatValue == 0)
                        {
                            GUILayout.Space(10);
                            materialEditor.ShaderProperty(_ShadowHardness, new GUIContent(_ShadowHardness.displayName, TOTIPS[64]));
                        }

                        switch ((int)_N_F_SS.floatValue)
                        {
                            case 0:
                                materialEditor.ShaderProperty(_SelfShadowRealtimeShadowIntensity, new GUIContent("Realtime Shadow Intensity", TOTIPS[124]));
                                break;
                            case 1:
                                materialEditor.ShaderProperty(_SelfShadowRealtimeShadowIntensity, new GUIContent(_SelfShadowRealtimeShadowIntensity.displayName, TOTIPS[126]));
                                break;
                            default:
                                break;
                        }

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_N_F_ESSAO, new GUIContent(_N_F_ESSAO.displayName, TOTIPS[145]));
                        EditorGUI.BeginDisabledGroup(_N_F_ESSAO.floatValue == 0.0f);
                        materialEditor.ShaderProperty(_SSAOColor, new GUIContent(_SSAOColor.displayName, TOTIPS[146]));
                        EditorGUI.EndDisabledGroup();


                        //Self Shadow

                        #region Self Shadow

                        if (_N_F_SS.floatValue == 1)
                        {

                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            Rect r_selfshadow = EditorGUILayout.BeginVertical("Button");
                            GUILayout.Label("Self Shadow", EditorStyles.boldLabel);
                            EditorGUILayout.EndVertical();

                            if (_N_F_SS.floatValue == 1)
                            {

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_SelfShadowThreshold, new GUIContent(_SelfShadowThreshold.displayName, TOTIPS[65]));

                                materialEditor.ShaderProperty(_VertexColorGreenControlSelfShadowThreshold, new GUIContent(_VertexColorGreenControlSelfShadowThreshold.displayName, TOTIPS[66]));

                                materialEditor.ShaderProperty(_SelfShadowHardness, new GUIContent(_SelfShadowHardness.displayName, TOTIPS[67]));

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_SelfShadowRealTimeShadowColor, new GUIContent(_SelfShadowRealTimeShadowColor.displayName, TOTIPS[68]));
                                materialEditor.ShaderProperty(_SelfShadowRealTimeShadowColorPower, new GUIContent(_SelfShadowRealTimeShadowColorPower.displayName, TOTIPS[69]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_SelfShadowAffectedByLightShadowStrength, new GUIContent(_SelfShadowAffectedByLightShadowStrength.displayName, TOTIPS[70]));

                            }

                            GUILayout.Space(10);

                        }
                        #endregion

                        //Smooth Object Normal

                        #region Smooth Object Normal

                        if (_N_F_SON.floatValue == 1)
                        {

                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            Rect r_smoothobjectnormal = EditorGUILayout.BeginVertical("Button");
                            GUILayout.Label("Smooth Object Normal", EditorStyles.boldLabel);
                            EditorGUILayout.EndVertical();

                            if (_N_F_SON.floatValue == 1)
                            {

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_SmoothObjectNormal, new GUIContent(_SmoothObjectNormal.displayName, TOTIPS[71]));

                                materialEditor.ShaderProperty(_VertexColorRedControlSmoothObjectNormal, new GUIContent(_VertexColorRedControlSmoothObjectNormal.displayName, TOTIPS[72]));

                                //materialEditor.ShaderProperty(_XYZPosition, new GUIContent(_XYZPosition.displayName, TOTIPS[73]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_ShowNormal, new GUIContent(_ShowNormal.displayName, TOTIPS[75]));

                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                                EditorGUI.BeginDisabledGroup(true);
                                EditorGUILayout.TextArea("Add the 'Smooth Object Normal - Helper' component to your object for this to work.\n\nTo Add:\nClick your object then click 'Add Component'\nthen 'RealToon>Tool>Smooth Object Normal - Helper.", EditorStyles.label, GUILayout.ExpandWidth(true));
                                EditorGUI.EndDisabledGroup();

                            }

                            GUILayout.Space(10);

                        }
                        #endregion

                        //Shadow Color Texture

                        #region Shadow Color Texture

                        if (_N_F_SCT.floatValue == 1)
                        {
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            Rect r_shadowcolortexture = EditorGUILayout.BeginVertical("Button");
                            GUILayout.Label("Shadow Color Texture", EditorStyles.boldLabel);
                            EditorGUILayout.EndVertical();

                            if (_N_F_SCT.floatValue == 1)
                            {

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_ShadowColorTexture, new GUIContent(_ShadowColorTexture.displayName, TOTIPS[76]));
                                materialEditor.ShaderProperty(_ShadowColorTexturePower, new GUIContent(_ShadowColorTexturePower.displayName, TOTIPS[77]));
                            }

                            GUILayout.Space(10);

                        }

                        #endregion

                        //ShadowT

                        #region ShadowT

                        if (_N_F_ST.floatValue == 1)
                        {
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            Rect r_shadowt = EditorGUILayout.BeginVertical("Button");
                            GUILayout.Label("ShadowT", EditorStyles.boldLabel);
                            EditorGUILayout.EndVertical();

                            if (_N_F_ST.floatValue == 1)
                            {
                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_ShadowTIntensity, new GUIContent(_ShadowTIntensity.displayName, TOTIPS[78]));
                                materialEditor.ShaderProperty(_ShadowT, new GUIContent(_ShadowT.displayName, TOTIPS[79]));
                                materialEditor.ShaderProperty(_ShadowTLightThreshold, new GUIContent(_ShadowTLightThreshold.displayName, TOTIPS[80]));

                                if (_N_F_STSDFM.floatValue == 0)
                                {
                                    materialEditor.ShaderProperty(_ShadowTShadowThreshold, new GUIContent(_ShadowTShadowThreshold.displayName, TOTIPS[81]));
                                }

                                materialEditor.ShaderProperty(_ShadowTHardness, new GUIContent(_ShadowTHardness.displayName, TOTIPS[82]));

                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_ShadowTColor, new GUIContent(_ShadowTColor.displayName, TOTIPS[129]));
                                materialEditor.ShaderProperty(_ShadowTColorPower, new GUIContent(_ShadowTColorPower.displayName, TOTIPS[130]));

                                if (_N_F_STSDFM.floatValue == 0)
                                {
                                    GUILayout.Space(10);
                                    materialEditor.ShaderProperty(_STIL, new GUIContent(_STIL.displayName, TOTIPS[131]));

                                    GUILayout.Space(10);
                                    materialEditor.ShaderProperty(_N_F_STIS, new GUIContent(_N_F_STIS.displayName, TOTIPS[83]));
                                    materialEditor.ShaderProperty(_N_F_STIAL, new GUIContent(_N_F_STIAL.displayName, TOTIPS[84]));

                                    EditorGUI.BeginDisabledGroup(_N_F_STIAL.floatValue == 0 && _N_F_STIS.floatValue == 0);
                                    materialEditor.ShaderProperty(_ShowInAmbientLightShadowIntensity, new GUIContent(_ShowInAmbientLightShadowIntensity.displayName, TOTIPS[85]));
                                    EditorGUI.EndDisabledGroup();

                                    GUILayout.Space(10);
                                    materialEditor.ShaderProperty(_ShowInAmbientLightShadowThreshold, new GUIContent(_ShowInAmbientLightShadowThreshold.displayName, TOTIPS[86]));

                                    GUILayout.Space(10);
                                    materialEditor.ShaderProperty(_LightFalloffAffectShadowT, new GUIContent(_LightFalloffAffectShadowT.displayName, TOTIPS[87]));
                                }

                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                                materialEditor.ShaderProperty(_N_F_STSDFM, new GUIContent(_N_F_STSDFM.displayName, TOTIPS[162]));

                                if (_N_F_STSDFM.floatValue == 1)
                                {
                                    GUILayout.Space(10);

                                    EditorGUI.BeginDisabledGroup(true);
                                    EditorGUILayout.TextArea("Add the 'ShadowT SDF Mode - Helper' component to your object for this to work.\n\nTo Add:\nClick your object then click 'Add Component'\nthen 'RealToon>Tool>ShadowT SDF Mode - Helper.", EditorStyles.label, GUILayout.ExpandWidth(true));
                                    EditorGUI.EndDisabledGroup();
                                }

                            }

                            GUILayout.Space(10);

                        }

                        #endregion

                        //Shadow PTexture

                        #region PTexture

                        if (_N_F_PT.floatValue == 1)
                        {
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            Rect r_ptexture = EditorGUILayout.BeginVertical("Button");
                            GUILayout.Label("PTexture", EditorStyles.boldLabel);
                            EditorGUILayout.EndVertical();

                            GUILayout.Space(10);

                            if (_N_F_PT.floatValue == 1)
                            {
                                materialEditor.ShaderProperty(_PTexture, new GUIContent(_PTexture.displayName, TOTIPS[88]));
                                materialEditor.ShaderProperty(_PTexturePower, new GUIContent(_PTexturePower.displayName, TOTIPS[89]));

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_PTCol, new GUIContent(_PTCol.displayName, TOTIPS[122]));
                            }

                            GUILayout.Space(10);

                        }

                        #endregion

                    }

                    EditorGUILayout.EndVertical();

                }

                #endregion

                //Lighting

                #region Lighting

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                Rect r_lighting = EditorGUILayout.BeginVertical("Button");
                ShowLighting = EditorGUILayout.Foldout(ShowLighting, "(Lighting)", true, EditorStyles.foldout);

                if (ShowLighting)
                {

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_RELG, new GUIContent(_RELG.displayName, TOTIPS[90]));
                    EditorGUI.BeginDisabledGroup(_RELG.floatValue == 0);
                    materialEditor.ShaderProperty(_EnvironmentalLightingIntensity, new GUIContent(_EnvironmentalLightingIntensity.displayName, TOTIPS[91]));

                    GUILayout.Space(10);

                    materialEditor.ShaderProperty(_GIFlatShade, new GUIContent(_GIFlatShade.displayName, TOTIPS[92]));
                    materialEditor.ShaderProperty(_GIShadeThreshold, new GUIContent(_GIShadeThreshold.displayName, TOTIPS[93]));
                    EditorGUI.EndDisabledGroup();

                    if (_N_F_OFLMB.floatValue == 0)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_LightAffectShadow, new GUIContent(_LightAffectShadow.displayName, TOTIPS[94]));
                        EditorGUI.BeginDisabledGroup(_LightAffectShadow.floatValue == 0);
                        materialEditor.ShaderProperty(_LightIntensity, new GUIContent(_LightIntensity.displayName, TOTIPS[132]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_UseTLB, new GUIContent(_UseTLB.displayName, TOTIPS[134]));
                        materialEditor.ShaderProperty(_N_F_EAL, new GUIContent(_N_F_EAL.displayName, TOTIPS[133]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_DirectionalLightIntensity, new GUIContent(_DirectionalLightIntensity.displayName, TOTIPS[95]));
                        EditorGUI.BeginDisabledGroup(_N_F_EAL.floatValue == 0);
                        materialEditor.ShaderProperty(_PointSpotlightIntensity, new GUIContent(_PointSpotlightIntensity.displayName, TOTIPS[96]));

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_LightFalloffSoftness, new GUIContent(_LightFalloffSoftness.displayName, TOTIPS[97]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);
                        materialEditor.ShaderProperty(_N_F_LLI, new GUIContent(_N_F_LLI.displayName, TOTIPS[171]));
                        EditorGUI.BeginDisabledGroup(_N_F_LLI.floatValue == 0);
                        materialEditor.ShaderProperty(_LLI_Min, new GUIContent(_LLI_Min.displayName, TOTIPS[172]));
                        materialEditor.ShaderProperty(_LLI_Max, new GUIContent(_LLI_Max.displayName, TOTIPS[173]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);

                        //Custom Light Direction

                        #region Custom Light Direction

                        if (_N_F_CLD.floatValue == 1)
                        {

                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            EditorGUI.BeginDisabledGroup(_N_F_CLD.floatValue == 0);

                            Rect r_customlightdirection = EditorGUILayout.BeginVertical("Button");
                            GUILayout.Label("Custom Light Direction", EditorStyles.boldLabel);
                            EditorGUILayout.EndVertical();

                            if (_N_F_CLD.floatValue == 1)
                            {

                                GUILayout.Space(10);

                                materialEditor.ShaderProperty(_CustomLightDirectionIntensity, new GUIContent(_CustomLightDirectionIntensity.displayName, TOTIPS[98]));
                                materialEditor.ShaderProperty(_CustomLightDirection, new GUIContent(_CustomLightDirection.displayName, TOTIPS[99]));
                                materialEditor.ShaderProperty(_CustomLightDirectionFollowObjectRotation, new GUIContent(_CustomLightDirectionFollowObjectRotation.displayName, TOTIPS[100]));

                            }

                            EditorGUI.EndDisabledGroup();

                            GUILayout.Space(10);

                        }

                        #endregion

                    }

                    GUILayout.Space(10);
                }

                EditorGUILayout.EndVertical();

                #endregion

                //Reflection

                #region Reflection

                if (_N_F_R.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_reflection = EditorGUILayout.BeginVertical("Button");
                    ShowReflection = EditorGUILayout.Foldout(ShowReflection, "(Reflection)", true, EditorStyles.foldout);

                    if (ShowReflection)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_ReflectionIntensity, new GUIContent(_ReflectionIntensity.displayName, TOTIPS[101]));
                        materialEditor.ShaderProperty(_Smoothness, new GUIContent(_Smoothness.displayName, TOTIPS[102]));
                        materialEditor.ShaderProperty(_RefMetallic, new GUIContent(_RefMetallic.displayName, TOTIPS[103]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_MaskReflection, new GUIContent(_MaskReflection.displayName, TOTIPS[104]));

                        GUILayout.Space(10);

                        //FReflection

                        #region FReflection

                        if (_N_F_FR.floatValue == 1)
                        {

                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                            EditorGUI.BeginDisabledGroup(_N_F_FR.floatValue == 0);

                            Rect r_freflection = EditorGUILayout.BeginVertical("Button");
                            GUILayout.Label("FReflection", EditorStyles.boldLabel);
                            EditorGUILayout.EndVertical();

                            materialEditor.ShaderProperty(_FReflection, new GUIContent(_FReflection.displayName, TOTIPS[105]));

                            EditorGUI.EndDisabledGroup();

                            GUILayout.Space(10);
                        }

                    }

                    #endregion

                    EditorGUILayout.EndVertical();
                }

                #endregion

                // Rim Light

                #region Rim Light

                if (_N_F_RL.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_rimlight = EditorGUILayout.BeginVertical("Button");
                    ShowRimLight = EditorGUILayout.Foldout(ShowRimLight, "(Rim Light)", true, EditorStyles.foldout);

                    if (ShowRimLight)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_RimLigInt, new GUIContent(_RimLigInt.displayName, TOTIPS[125]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_RimLightUnfill, new GUIContent(_RimLightUnfill.displayName, TOTIPS[106]));
                        materialEditor.ShaderProperty(_RimLightSoftness, new GUIContent(_RimLightSoftness.displayName, TOTIPS[107]));
                        materialEditor.ShaderProperty(_RimLigPosi, new GUIContent(_RimLigPosi.displayName, TOTIPS[180]));

                        GUILayout.Space(10);

                        EditorGUI.BeginDisabledGroup(_RimLightInLight.floatValue == 1);
                            materialEditor.ShaderProperty(_LightAffectRimLightColor, new GUIContent(_LightAffectRimLightColor.displayName, TOTIPS[108]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_RimLightColor, new GUIContent(_RimLightColor.displayName, TOTIPS[109]));
                        materialEditor.ShaderProperty(_RimLightColorPower, new GUIContent(_RimLightColorPower.displayName, TOTIPS[110]));

                        EditorGUI.BeginDisabledGroup(_N_F_RLIS.floatValue == 1);
                            if (_N_F_OFLMB.floatValue == 0)
                            {
                                GUILayout.Space(10);
                                materialEditor.ShaderProperty(_RimLightInLight, new GUIContent(_RimLightInLight.displayName, TOTIPS[111]));
                            }
                        EditorGUI.EndDisabledGroup();

                        materialEditor.ShaderProperty(_N_F_RLIS, new GUIContent(_N_F_RLIS.displayName, TOTIPS[179]));
                        if (_N_F_RLIS.floatValue == 1.0)
                        {
                            targetMat.SetFloat("_RimLightInLight", 0.0f);
                        }

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();

                }

                #endregion

                //Near Fade Dithering

                #region Near Fade Dithering

                if (_N_F_NFD.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_nerfaddithe = EditorGUILayout.BeginVertical("Button");
                    NearFadeDithering = EditorGUILayout.Foldout(NearFadeDithering, "(Near Fade Dithering)", true, EditorStyles.foldout);

                    if (NearFadeDithering)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_MinFadDistance, new GUIContent(_MinFadDistance.displayName, TOTIPS[152]));
                        materialEditor.ShaderProperty(_MaxFadDistance, new GUIContent(_MaxFadDistance.displayName, TOTIPS[153]));

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();

                }

                #endregion

                //Triplanar

                #region Triplanar

                if (_N_F_TP.floatValue == 1)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_tripla = EditorGUILayout.BeginVertical("Button");
                    Triplanar = EditorGUILayout.Foldout(Triplanar, "(Triplanar)", true, EditorStyles.foldout);

                    if (Triplanar)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_TriPlaTile, new GUIContent(_TriPlaTile.displayName, TOTIPS[155]));
                        materialEditor.ShaderProperty(_TriPlaBlend, new GUIContent(_TriPlaBlend.displayName, TOTIPS[156]));

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();

                }

                #endregion

                //Perspective Adjustment

                #region Perspective Adjustment

                if (_N_F_PA.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_perspecadju = EditorGUILayout.BeginVertical("Button");
                    ShowPerspecAdju = EditorGUILayout.Foldout(ShowPerspecAdju, "(Perspective Adjustment)", true, EditorStyles.foldout);

                    if (ShowPerspecAdju)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_PresAdju, new GUIContent(_PresAdju.displayName, TOTIPS[157]));
                        materialEditor.ShaderProperty(_ClipAdju, new GUIContent(_ClipAdju.displayName, TOTIPS[158]));

                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                        materialEditor.ShaderProperty(_PASize, new GUIContent(_PASize.displayName, TOTIPS[159]));
                        materialEditor.ShaderProperty(_PASmooTrans, new GUIContent(_PASmooTrans.displayName, TOTIPS[160]));
                        materialEditor.ShaderProperty(_PADist, new GUIContent(_PADist.displayName, TOTIPS[161]));

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();

                }

                #endregion

                //Smear Effect

                #region Smear Effect

                if (_N_F_SE.floatValue == 1)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_perspecadju = EditorGUILayout.BeginVertical("Button");
                    ShowSmeEff = EditorGUILayout.Foldout(ShowSmeEff, "(Smear Effect)", true, EditorStyles.foldout);

                    if (ShowSmeEff)
                    {

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextArea("Add the 'Smear Effect [Helper]' component to your object for this to work.\nAdjustable options are on the 'Smear Effect [Helper]' component.\n\nTo Add:\nClick your object then click 'Add Component'\nthen 'RealToon>Tool>Smear Effect [Helper].", EditorStyles.label, GUILayout.ExpandWidth(true));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();

                }

                #endregion

                //Tessellation (In Progress)

                #region Tessellation

                if (tess_supp == true)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_tessellation = EditorGUILayout.BeginVertical("Button");
                    ShowTessellation = EditorGUILayout.Foldout(ShowTessellation, "(Tessellation)", true, EditorStyles.foldout);

                    if (ShowTessellation)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_TessellationSmoothness, new GUIContent(_TessellationSmoothness.displayName, TOTIPS[187]));

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_TessellationTransition, new GUIContent(_TessellationTransition.displayName, TOTIPS[188]));
                        materialEditor.ShaderProperty(_TessellationNear, new GUIContent(_TessellationNear.displayName, TOTIPS[189]));
                        materialEditor.ShaderProperty(_TessellationFar, new GUIContent(_TessellationFar.displayName, TOTIPS[190]));

                        GUILayout.Space(10);

                        EditorGUI.BeginDisabledGroup(_N_F_NM.floatValue == 0);
                        materialEditor.ShaderProperty(_NorMapAsDis, new GUIContent(_NorMapAsDis.displayName, TOTIPS[191]));
                        EditorGUI.EndDisabledGroup();

                        GUILayout.Space(10);
                    }

                    EditorGUILayout.EndVertical();

                }

                #endregion

                //See Through

                #region See Through

                if (add_st == false)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    Rect r_seethrough = EditorGUILayout.BeginVertical("Button");
                    ShowSeeThrough = EditorGUILayout.Foldout(ShowSeeThrough, "(See Through)", true, EditorStyles.foldout);

                    if (ShowSeeThrough)
                    {

                        GUILayout.Space(10);

                        materialEditor.ShaderProperty(_RefVal, new GUIContent(_RefVal.displayName, TOTIPS[112]));
                        materialEditor.ShaderProperty(_Oper, new GUIContent(_Oper.displayName, TOTIPS[113]));
                        materialEditor.ShaderProperty(_Compa, new GUIContent(_Compa.displayName, TOTIPS[114]));

                        GUILayout.Space(10);

                    }

                    EditorGUILayout.EndVertical();

                    GUILayout.Space(20);

                }

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                #endregion

                GUILayout.Space(20);

                //Disable/Enable Features

                #region Disable/Enable Features

                Rect r_disableenablefeature = EditorGUILayout.BeginVertical("Button");
                ShowDisableEnable = EditorGUILayout.Foldout(ShowDisableEnable, "(Disable/Enable Features)", true, EditorStyles.foldout);

                if (ShowDisableEnable)
                {

                    GUILayout.Space(10);

                    Rect r_mc = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_MC, new GUIContent(_N_F_MC.displayName, TOTIPSEDF[0]));
                    EditorGUILayout.EndVertical();

                    Rect r_nm = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_NM, new GUIContent(_N_F_NM.displayName, TOTIPSEDF[1]));
                    EditorGUILayout.EndVertical();

                    if (remoout == true)
                    {
                        Rect r_ou = EditorGUILayout.BeginVertical("HelpBox");

                        EditorGUI.BeginChangeCheck();

                        materialEditor.ShaderProperty(_N_F_O, new GUIContent(_N_F_O.displayName, TOTIPSEDF[2]));

                        if (EditorGUI.EndChangeCheck())
                        {
                            int f_deo_int = (int)_N_F_O.floatValue;
                            foreach (Material m in materialEditor.targets)
                            {
                                switch (f_deo_int)
                                {
                                    case 0:
                                        m.SetShaderPassEnabled("SRPDefaultUnlit", false);
                                        break;
                                    case 1:
                                        m.SetShaderPassEnabled("SRPDefaultUnlit", true);
                                        break;
                                    default:
                                        break;
                                }
                            }

                        }

                        EditorGUILayout.EndVertical();
                    }

                    EditorGUI.BeginDisabledGroup(_TRANSMODE.floatValue == 0);

                    Rect r_co = EditorGUILayout.BeginVertical("HelpBox");

                    EditorGUI.BeginChangeCheck();

                    materialEditor.ShaderProperty(_N_F_CO, new GUIContent(_N_F_CO.displayName, TOTIPSEDF[3]));

                    if (EditorGUI.EndChangeCheck())
                    {
                        int f_co_int = (int)_N_F_CO.floatValue;
                        foreach (Material m in materialEditor.targets)
                        {
                            switch (f_co_int)
                            {
                                case 0:

                                    m.renderQueue = 3000;
                                    MatRenQue = m.renderQueue;
                                    m.SetOverrideTag("RenderType", "Transparent");

                                    m.DisableKeyword("N_F_ATC_ON");
                                    m.SetFloat("_AlpToCov", 0.0f);

                                    break;

                                case 1:

                                    m.renderQueue = 2450;
                                    MatRenQue = m.renderQueue;
                                    m.SetOverrideTag("RenderType", "TransparentCutout");
                                    break;

                                default:
                                    break;
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();

                    EditorGUI.EndDisabledGroup();

                    Rect r_ca = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_CA, new GUIContent(_N_F_CA.displayName, TOTIPSEDF[4]));
                    EditorGUILayout.EndVertical();


                    EditorGUI.BeginChangeCheck();

                    Rect r_sl = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_SL, new GUIContent(_N_F_SL.displayName, TOTIPSEDF[5]));
                    EditorGUILayout.EndVertical();

                    if (EditorGUI.EndChangeCheck())
                    {
                        int f_sl_int = (int)_N_F_SL.floatValue;
                        foreach (Material m in materialEditor.targets)
                        {
                            switch (f_sl_int)
                            {
                                case 0:
                                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                                    break;
                                case 1:
                                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                                    break;
                                default:
                                    break;
                            }
                        }

                    }


                    if (_N_F_OFLMB.floatValue == 0)
                    {


                        Rect r_o = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_GLO, new GUIContent(_N_F_GLO.displayName, TOTIPSEDF[6]));
                        EditorGUILayout.EndVertical();

                        Rect r_glot = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_GLOT, new GUIContent(_N_F_GLOT.displayName, TOTIPSEDF[7]));
                        EditorGUILayout.EndVertical();

                    }


                    if (_N_F_OFLMB.floatValue == 0)
                    {
                        Rect r_ss = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_SS, new GUIContent(_N_F_SS.displayName, TOTIPSEDF[8]));
                        EditorGUILayout.EndVertical();
                    }


                    if (_N_F_OFLMB.floatValue == 0)
                    {
                        Rect r_son = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_SON, new GUIContent(_N_F_SON.displayName, TOTIPSEDF[9]));
                        EditorGUILayout.EndVertical();
                    }


                    if (_N_F_OFLMB.floatValue == 0)
                    {
                        Rect r_sct = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_SCT, new GUIContent(_N_F_SCT.displayName, TOTIPSEDF[10]));
                        EditorGUILayout.EndVertical();
                    }


                    if (_N_F_OFLMB.floatValue == 0)
                    {
                        Rect r_st = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_ST, new GUIContent(_N_F_ST.displayName, TOTIPSEDF[11]));
                        EditorGUILayout.EndVertical();
                    }


                    if (_N_F_OFLMB.floatValue == 0)
                    {
                        Rect r_pt = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_PT, new GUIContent(_N_F_PT.displayName, TOTIPSEDF[12]));
                        EditorGUILayout.EndVertical();
                    }



                    if (_N_F_OFLMB.floatValue == 0)
                    {
                        Rect r_cld = EditorGUILayout.BeginVertical("HelpBox");
                        materialEditor.ShaderProperty(_N_F_CLD, new GUIContent(_N_F_CLD.displayName, TOTIPSEDF[13]));
                        EditorGUILayout.EndVertical();
                    }



                    Rect r_r = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_R, new GUIContent(_N_F_R.displayName, TOTIPSEDF[14]));
                    EditorGUILayout.EndVertical();

                    Rect r_fr = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_FR, new GUIContent(_N_F_FR.displayName, TOTIPSEDF[15]));
                    EditorGUILayout.EndVertical();

                    Rect r_rl = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_RL, new GUIContent(_N_F_RL.displayName, TOTIPSEDF[16]));
                    EditorGUILayout.EndVertical();

                    Rect r_nfd = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_NFD, new GUIContent(_N_F_NFD.displayName, TOTIPSEDF[17]));
                    EditorGUILayout.EndVertical();

                    Rect r_tp = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_TP, new GUIContent(_N_F_TP.displayName, TOTIPSEDF[18]));
                    EditorGUILayout.EndVertical();

                    Rect r_pa = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_PA, new GUIContent(_N_F_PA.displayName, TOTIPSEDF[19]));
                    EditorGUILayout.EndVertical();

                    Rect r_se = EditorGUILayout.BeginVertical("HelpBox");
                    materialEditor.ShaderProperty(_N_F_SE, new GUIContent(_N_F_SE.displayName, TOTIPSEDF[20]));
                    EditorGUILayout.EndVertical();

                    GUILayout.Space(10);

                }

                EditorGUILayout.EndVertical();

                #endregion

                //Settings

                #region Settings

                GUILayout.Space(5);

                Rect r_showsettings = EditorGUILayout.BeginVertical("Button");
                ShowSettings = EditorGUILayout.Foldout(ShowSettings, "(Settings)", true, EditorStyles.foldout);

                if (ShowSettings)
                {

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (_N_F_OFLMB.floatValue == 0)
                    {
                        materialEditor.ShaderProperty(_N_F_HDLS, new GUIContent(_N_F_HDLS.displayName, TOTIPS[117]));
                        materialEditor.ShaderProperty(_N_F_HPSS, new GUIContent(_N_F_HPSS.displayName, TOTIPS[118]));

                        EditorGUI.BeginChangeCheck();

                        materialEditor.ShaderProperty(_N_F_DCS, new GUIContent(_N_F_DCS.displayName, TOTIPS[119]));

                        if (EditorGUI.EndChangeCheck())
                        {
                            int f_hcs_int = (int)_N_F_DCS.floatValue;
                            foreach (Material m in materialEditor.targets)
                            {
                                switch (f_hcs_int)
                                {
                                    case 0:
                                        m.SetShaderPassEnabled("ShadowCaster", true);
                                        break;
                                    case 1:
                                        m.SetShaderPassEnabled("ShadowCaster", false);
                                        break;
                                    default:
                                        break;
                                }
                            }

                        }

                        materialEditor.ShaderProperty(_N_F_NLASOBF, new GUIContent(_N_F_NLASOBF.displayName, TOTIPS[115]));

                    }

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    materialEditor.ShaderProperty(_ZWrite, new GUIContent(_ZWrite.displayName, TOTIPS[120]));

                    GUILayout.Space(4);

                    materialEditor.ShaderProperty(_ZTest, new GUIContent(_ZTest.displayName, TOTIPS[182]));

                    GUILayout.Space(5);

                    EditorGUI.BeginChangeCheck();
                    materialEditor.RenderQueueField();
                    if (EditorGUI.EndChangeCheck())
                    {
                        MatRenQue = targetMat.renderQueue;
                        targetMat.SetFloat("_RQSO", 0);
                    }

                    EditorGUI.BeginChangeCheck();
                    materialEditor.ShaderProperty(_RQSO, new GUIContent("Render Order", TOTIPS[181]));
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (targetMat.renderQueue != 0)
                        {
                            targetMat.renderQueue = MatRenQue + (int)_RQSO.floatValue;
                        }

                        if ((int)_RQSO.floatValue == 0)
                        {
                            targetMat.renderQueue = MatRenQue;
                        }
                    }

                    GUILayout.Space(4);

                    materialEditor.ShaderProperty(_ObjePosiZCS, new GUIContent(_ObjePosiZCS.displayName, TOTIPS[178]));

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    materialEditor.EnableInstancingField();

                    EditorGUI.BeginDisabledGroup(tess_supp == true);
                        materialEditor.ShaderProperty(_N_F_DDMD, new GUIContent(_N_F_DDMD.displayName, TOTIPS[151]));
                    EditorGUI.EndDisabledGroup();

                    materialEditor.ShaderProperty(_N_F_RDC, new GUIContent(_N_F_RDC.displayName, TOTIPS[147]));
                    materialEditor.ShaderProperty(_N_F_OFLMB, new GUIContent(_N_F_OFLMB.displayName, TOTIPS[141]));
                    aruskw = EditorGUILayout.Toggle(new GUIContent("Automatic Remove Unused Shader Keywords (Global)", TOTIPS[121]), aruskw);

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (GUILayout.Button(new GUIContent(twofourfive_target_string, TOTIPS[116]), "Button"))
                    {
                        if (tess_supp == false)
                        {
                            TWOFORFIVE();
                        }
                        else
                        {
                            TWOFORFIVE();
                            TESS_SUPP();
                        }
                    }

                    GUILayout.Space(5);

                    EditorGUI.BeginDisabledGroup(tess_supp == true);
                        if (twofourfive_target == true)
                        {
                            if (GUILayout.Button(new GUIContent(dots_lbs_cd_string, TOTIPS[143]), "Button"))
                            {
                                DOTSLBSCD();
                            }
                        }
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (twofourfive_target == true)
                    {
                        if (GUILayout.Button(new GUIContent(tess_supp_string, TOTIPS[186]), "Button"))
                        {
                            TESS_SUPP();
                        }
                    }

                    GUILayout.Space(5);

                    if (GUILayout.Button(new GUIContent(add_st_string, TOTIPS[163]), "Button"))
                    {
                        ADD_ST();
                    }

                    GUILayout.Space(10);

                }

                EditorGUILayout.EndVertical();

                #endregion

                GUILayout.Space(20);
            }

            #region Automatic Remove UorOSKW
            if (aruskw == true)
            {
                foreach (Material m1 in materialEditor.targets)
                {
                    for (int x = 0; x < m1.shaderKeywords.Length; x++)
                    {
                        if (m1.shaderKeywords[x] != String.Empty)
                        {
                            for (int y = 0; y < Enum.GetValues(typeof(SFKW)).Length; y++)
                            {
                                if (m1.shaderKeywords[x] == Enum.GetValues(typeof(SFKW)).GetValue(y).ToString())
                                {
                                    del_skw = false;
                                    break;
                                }
                                else
                                {
                                    del_skw = true;
                                }
                            }

                            if (del_skw == true)
                            {
                                m1.DisableKeyword(m1.shaderKeywords[x]);
                                del_skw = false;
                            }

                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            #endregion

            //Footbar
            #region Footbar

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            Rect r_footbar = EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("[" + remooutstat + " (On Shader)]", TOTIPS[135]), "Toolbar"))
            {
                REMO_OUTL();
            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("[Refresh Settings]", TOTIPS[62]), "Toolbar"))
            {
                foreach (Material m in materialEditor.targets)
                {
                    CheckingPropKeyWord(m);
                }

                Check_RE_OL();

                Debug.Log("You clicked [Refresh Settings]: RealToon on the material has been refresh and re-apply the settings properly.");

            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("[Video Tutorials]", TOTIPS[136]), "Toolbar"))
            {
                Application.OpenURL("www.youtube.com/playlist?list=PL0M1m9smMVPJ4qEkJnZObqJE5mU9uz6SY");
            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("[RealToon (User Guide).pdf]", TOTIPS[137]), "Toolbar"))
            {
                Application.OpenURL(Application.dataPath + "/RealToon/RealToon (User Guide).pdf");
            }

            GUILayout.Space(5);

            if (GUILayout.Button(new GUIContent("[" + ShowUIString + "(Global)]", TOTIPS[138]), "Toolbar"))
            {
                if (ShowUI == false)
                {
                    ShowUI = true;
                    ShowUIString = "Hide UI";
                }
                else
                {
                    ShowUI = false;
                    ShowUIString = "Show UI";
                }
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            #endregion

        }

        //
        #region Checking

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader.name != "Universal Render Pipeline/RealToon/Version 5/Default/Default")
            {

                if (oldShader.name == "HDRP/RealToon/Version 5/Default")
                {
                    material.SetFloat("_MaiColPo", material.GetFloat("_MaiColPo") + 0.65f);
                }

            }

            CheckingPropKeyWord(material);
        }

        #region CheckingPropKeyWord

        void CheckingPropKeyWord(Material material)
        {

            if (material.IsKeywordEnabled("N_F_TRANS_ON") || material.GetFloat("_TRANSMODE") == 1.0f)
            {

                if (material.IsKeywordEnabled("N_F_CO_ON") || material.GetFloat("_N_F_CO") == 1.0f)
                {

                    material.renderQueue = 2450;
                    material.SetOverrideTag("RenderType", "TransparentCutout");

                }
                else if (material.IsKeywordEnabled("N_F_TRANS_ON") || material.GetFloat("_TRANSMODE") == 1.0f)
                {
                    material.renderQueue = 3000;

                    material.EnableKeyword("N_F_TRANS_ON");
                    material.SetFloat("_TRANSMODE", 1.0f);
                    material.SetOverrideTag("RenderType", "Transparent");
                }

                shader_type = "Transparency";
            }
            else if (!material.IsKeywordEnabled("N_F_TRANS_ON") || material.GetFloat("_TRANSMODE") == 0.0f)
            {
                material.DisableKeyword("N_F_TRANS_ON");
                material.SetFloat("_TRANSMODE", 0.0f);

                shader_type = "Default";
            }

            if ((material.IsKeywordEnabled("N_F_TRANSAFFSHA_ON") || material.GetFloat("_TransAffSha") == 1.0f))
            {
                material.EnableKeyword("N_F_TRANSAFFSHA_ON");
                material.SetFloat("_TransAffSha", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_TRANSAFFSHA_ON") || material.GetFloat("_TransAffSha") == 0.0f))
            {
                material.DisableKeyword("N_F_TRANSAFFSHA_ON");
                material.SetFloat("_TransAffSha", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_ATC_ON") || material.GetFloat("_AlpToCov") == 1.0f))
            {
                material.EnableKeyword("N_F_ATC_ON");
                material.SetFloat("_AlpToCov", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_ATC_ON") || material.GetFloat("_AlpToCov") == 0.0f))
            {
                material.DisableKeyword("N_F_ATC_ON");
                material.SetFloat("_AlpToCov", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_OFLMB_ON") || material.GetFloat("_N_F_OFLMB") == 1.0f))
            {
                material.EnableKeyword("N_F_OFLMB_ON");
                material.SetFloat("_N_F_OFLMB", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_OFLMB_ON") || material.GetFloat("_N_F_OFLMB") == 0.0f))
            {
                material.DisableKeyword("N_F_OFLMB_ON");
                material.SetFloat("_N_F_OFLMB", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_SIMTRANS_ON") || material.GetFloat("_SimTrans") == 1.0f))
            {
                material.EnableKeyword("N_F_SIMTRANS_ON");
                material.SetFloat("_SimTrans", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_SIMTRANS_ON") || material.GetFloat("_SimTrans") == 0.0f))
            {
                material.DisableKeyword("N_F_SIMTRANS_ON");
                material.SetFloat("_SimTrans", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_DDMD_ON") || material.GetFloat("_N_F_DDMD") == 1.0f))
            {
                material.EnableKeyword("N_F_DDMD_ON");
                material.SetFloat("_N_F_DDMD", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_DDMD_ON") || material.GetFloat("_N_F_DDMD") == 0.0f))
            {
                material.DisableKeyword("N_F_DDMD_ON");
                material.SetFloat("_N_F_DDMD", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_RLIS_ON") || material.GetFloat("_N_F_RLIS") == 1.0f))
            {
                material.EnableKeyword("N_F_RLIS_ON");
                material.SetFloat("_N_F_RLIS", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_RLIS_ON") || material.GetFloat("_N_F_RLIS") == 0.0f))
            {
                material.DisableKeyword("N_F_RLIS_ON");
                material.SetFloat("_N_F_RLIS", 0.0f);
            }

            //======================================================================================================

            if ((material.IsKeywordEnabled("N_F_DNO_ON") || material.GetFloat("_DynamicNoisyOutline") == 1.0f))
            {
                material.EnableKeyword("N_F_DNO_ON");
                material.SetFloat("_DynamicNoisyOutline", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_DNO_ON") || material.GetFloat("_DynamicNoisyOutline") == 0.0f))
            {
                material.DisableKeyword("N_F_DNO_ON");
                material.SetFloat("_DynamicNoisyOutline", 0.0f);
            }

            //======================================================================================================

            if ((material.IsKeywordEnabled("N_F_COEDGL_ON") || material.GetFloat("_N_F_COEDGL") == 1.0f))
            {
                material.EnableKeyword("N_F_COEDGL_ON");
                material.SetFloat("_N_F_COEDGL", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_COEDGL_ON") || material.GetFloat("_N_F_COEDGL") == 0.0f))
            {
                material.DisableKeyword("N_F_COEDGL_ON");
                material.SetFloat("_N_F_COEDGL", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_MC_ON") || material.GetFloat("_N_F_MC") == 1.0f))
            {
                material.EnableKeyword("N_F_MC_ON");
                material.SetFloat("_N_F_MC", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_MC_ON") || material.GetFloat("_N_F_MC") == 0.0f))
            {
                material.DisableKeyword("N_F_MC_ON");
                material.SetFloat("_N_F_MC", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_NM_ON") || material.GetFloat("_N_F_NM") == 1.0f))
            {
                material.EnableKeyword("N_F_NM_ON");
                material.SetFloat("_N_F_NM", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_NM_ON") || material.GetFloat("_N_F_NM") == 0.0f))
            {
                material.DisableKeyword("N_F_NM_ON");
                material.SetFloat("_N_F_NM", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_CO_ON") || material.GetFloat("_N_F_CO") == 1.0f))
            {
                material.EnableKeyword("N_F_CO_ON");
                material.SetFloat("_N_F_CO", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_CO_ON") || material.GetFloat("_N_F_CO") == 0.0f))
            {
                material.DisableKeyword("N_F_CO_ON");
                material.SetFloat("_N_F_CO", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_SCO_ON") || material.GetFloat("_N_F_SCO") == 1.0f))
            {
                material.EnableKeyword("N_F_SCO_ON");
                material.SetFloat("_N_F_SCO", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_SCO_ON") || material.GetFloat("_N_F_SCO") == 0.0f))
            {
                material.DisableKeyword("N_F_SCO_ON");
                material.SetFloat("_N_F_SCO", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_O_ON") || material.GetFloat("_N_F_O") == 1.0f))
            {
                material.EnableKeyword("N_F_O_ON");
                material.SetShaderPassEnabled("SRPDefaultUnlit", true);
                material.SetFloat("_N_F_O", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_O_ON") || material.GetFloat("_N_F_O") == 0.0f))
            {
                material.DisableKeyword("N_F_O_ON");
                material.SetShaderPassEnabled("SRPDefaultUnlit", false);
                material.SetFloat("_N_F_O", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_CA_ON") || material.GetFloat("_N_F_CA") == 1.0f))
            {
                material.EnableKeyword("N_F_CA_ON");
                material.SetFloat("_N_F_CA", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_CA_ON") || material.GetFloat("_N_F_CA") == 0.0f))
            {
                material.DisableKeyword("N_F_CA_ON");
                material.SetFloat("_N_F_CA", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_SL_ON") || material.GetFloat("_N_F_SL") == 1.0f))
            {
                material.EnableKeyword("N_F_SL_ON");
                material.SetFloat("_N_F_SL", 1.0f);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }
            else if ((!material.IsKeywordEnabled("N_F_SL_ON") || material.GetFloat("_N_F_SL") == 0.0f))
            {
                material.DisableKeyword("N_F_SL_ON");
                material.SetFloat("_N_F_SL", 0.0f);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }

            if ((material.IsKeywordEnabled("N_F_SLMM_ON") || material.GetFloat("_N_F_SLMM") == 1.0f))
            {
                material.EnableKeyword("N_F_SLMM_ON");
                material.SetFloat("_N_F_SLMM", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_SLMM_ON") || material.GetFloat("_N_F_SLMM") == 0.0f))
            {
                material.DisableKeyword("N_F_SLMM_ON");
                material.SetFloat("_N_F_SLMM", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_GLO_ON") || material.GetFloat("_N_F_GLO") == 1.0f))
            {
                material.EnableKeyword("N_F_GLO_ON");
                material.SetFloat("_N_F_GLO", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_GLO_ON") || material.GetFloat("_N_F_GLO") == 0.0f))
            {
                material.DisableKeyword("N_F_GLO_ON");
                material.SetFloat("_N_F_GLO", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_GLOT_ON") || material.GetFloat("_N_F_GLOT") == 1.0f))
            {
                material.EnableKeyword("N_F_GLOT_ON");
                material.SetFloat("_N_F_GLOT", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_GLOT_ON") || material.GetFloat("_N_F_GLOT") == 0.0f))
            {
                material.DisableKeyword("N_F_GLOT_ON");
                material.SetFloat("_N_F_GLOT", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_SS_ON") || material.GetFloat("_N_F_SS") == 1.0f))
            {
                material.EnableKeyword("N_F_SS_ON");
                material.SetFloat("_N_F_SS", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_SS_ON") || material.GetFloat("_N_F_SS") == 0.0f))
            {
                material.DisableKeyword("N_F_SS_ON");
                material.SetFloat("_N_F_SS", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_SON_ON") || material.GetFloat("_N_F_SON") == 1.0f))
            {
                material.EnableKeyword("N_F_SON_ON");
                material.SetFloat("_N_F_SON", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_SON_ON") || material.GetFloat("_N_F_SON") == 0.0f))
            {
                material.DisableKeyword("N_F_SON_ON");
                material.SetFloat("_N_F_SON", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_SCT_ON") || material.GetFloat("_N_F_SCT") == 1.0f))
            {
                material.EnableKeyword("N_F_SCT_ON");
                material.SetFloat("_N_F_SCT", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_SCT_ON") || material.GetFloat("_N_F_SCT") == 0.0f))
            {
                material.DisableKeyword("N_F_SCT_ON");
                material.SetFloat("_N_F_SCT", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_ST_ON") || material.GetFloat("_N_F_ST") == 1.0f))
            {
                material.EnableKeyword("N_F_ST_ON");
                material.SetFloat("_N_F_ST", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_ST_ON") || material.GetFloat("_N_F_ST") == 0.0f))
            {
                material.DisableKeyword("N_F_ST_ON");
                material.SetFloat("_N_F_ST", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_PT_ON") || material.GetFloat("_N_F_PT") == 1.0f))
            {
                material.EnableKeyword("N_F_PT_ON");
                material.SetFloat("_N_F_PT", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_PT_ON") || material.GetFloat("_N_F_PT") == 0.0f))
            {
                material.DisableKeyword("N_F_PT_ON");
                material.SetFloat("_N_F_PT", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_CLD_ON") || material.GetFloat("_N_F_CLD") == 1.0f))
            {
                material.EnableKeyword("N_F_CLD_ON");
                material.SetFloat("_N_F_CLD", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_CLD_ON") || material.GetFloat("_N_F_CLD") == 0.0f))
            {
                material.DisableKeyword("N_F_CLD_ON");
                material.SetFloat("_N_F_CLD", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_R_ON") || material.GetFloat("_N_F_R") == 1.0f))
            {
                material.EnableKeyword("N_F_R_ON");
                material.SetFloat("_N_F_R", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_R_ON") || material.GetFloat("_N_F_R") == 0.0f))
            {
                material.DisableKeyword("N_F_R_ON");
                material.SetFloat("_N_F_R", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_FR_ON") || material.GetFloat("_N_F_FR") == 1.0f))
            {
                material.EnableKeyword("N_F_FR_ON");
                material.SetFloat("_N_F_FR", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_FR_ON") || material.GetFloat("_N_F_FR") == 0.0f))
            {
                material.DisableKeyword("N_F_FR_ON");
                material.SetFloat("_N_F_FR", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_RL_ON") || material.GetFloat("_N_F_RL") == 1.0f))
            {
                material.EnableKeyword("N_F_RL_ON");
                material.SetFloat("_N_F_RL", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_RL_ON") || material.GetFloat("_N_F_RL") == 0.0f))
            {
                material.DisableKeyword("N_F_RL_ON");
                material.SetFloat("_N_F_RL", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_NFD_ON") || material.GetFloat("_N_F_NFD") == 1.0f))
            {
                material.EnableKeyword("N_F_NFD_ON");
                material.SetFloat("_N_F_NFD", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_NFD_ON") || material.GetFloat("_N_F_NFD") == 0.0f))
            {
                material.DisableKeyword("N_F_NFD_ON");
                material.SetFloat("_N_F_NFD", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_TP_ON") || material.GetFloat("_N_F_TP") == 1.0f))
            {
                material.EnableKeyword("N_F_TP_ON");
                material.SetFloat("_N_F_TP", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_TP_ON") || material.GetFloat("_N_F_TP") == 0.0f))
            {
                material.DisableKeyword("N_F_TP_ON");
                material.SetFloat("_N_F_TP", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_STSDFM_ON") || material.GetFloat("_N_F_STSDFM") == 1.0f))
            {
                material.EnableKeyword("N_F_STSDFM_ON");
                material.SetFloat("_N_F_STSDFM", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_STSDFM_ON") || material.GetFloat("_N_F_STSDFM") == 0.0f))
            {
                material.DisableKeyword("N_F_STSDFM_ON");
                material.SetFloat("_N_F_STSDFM", 0.0f);
            }

            //======================================================================================================

            if ((material.IsKeywordEnabled("N_F_ANIS_ON") || material.GetFloat("_N_F_ANIS") == 1.0f))
            {
                material.EnableKeyword("N_F_ANIS_ON");
                material.SetFloat("_N_F_ANIS", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_ANIS_ON") || material.GetFloat("_N_F_ANIS") == 0.0f))
            {
                material.DisableKeyword("N_F_ANIS_ON");
                material.SetFloat("_N_F_ANIS", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_ESSAO_ON") || material.GetFloat("_N_F_ESSAO") == 1.0f))
            {
                material.EnableKeyword("N_F_ESSAO_ON");
                material.SetFloat("_N_F_ESSAO", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_ESSAO_ON") || material.GetFloat("_N_F_ESSAO") == 0.0f))
            {
                material.DisableKeyword("N_F_ESSAO_ON");
                material.SetFloat("_N_F_ESSAO", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_RELGI_ON") || material.GetFloat("_RELG") == 1.0f))
            {
                material.EnableKeyword("N_F_RELGI_ON");
                material.SetFloat("_RELG", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_RELGI_ON") || material.GetFloat("_RELG") == 0.0f))
            {
                material.DisableKeyword("N_F_RELGI_ON");
                material.SetFloat("_RELG", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_USETLB_ON") || material.GetFloat("_UseTLB") == 1.0f))
            {
                material.EnableKeyword("N_F_USETLB_ON");
                material.SetFloat("_UseTLB", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_USETLB_ON") || material.GetFloat("_UseTLB") == 0.0f))
            {
                material.DisableKeyword("N_F_USETLB_ON");
                material.SetFloat("_UseTLB", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_EAL_ON") || material.GetFloat("_N_F_EAL") == 1.0f))
            {
                material.EnableKeyword("N_F_EAL_ON");
                material.SetFloat("_N_F_EAL", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_EAL_ON") || material.GetFloat("_N_F_EAL") == 0.0f))
            {
                material.DisableKeyword("N_F_EAL_ON");
                material.SetFloat("_N_F_EAL", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_LLI_ON") || material.GetFloat("_N_F_LLI") == 1.0f))
            {
                material.EnableKeyword("N_F_LLI_ON");
                material.SetFloat("_N_F_LLI", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_LLI_ON") || material.GetFloat("_N_F_LLI") == 0.0f))
            {
                material.DisableKeyword("N_F_LLI_ON");
                material.SetFloat("_N_F_LLI", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_HDLS_ON") || material.GetFloat("_N_F_HDLS") == 1.0f))
            {
                material.EnableKeyword("N_F_HDLS_ON");
                material.SetFloat("_N_F_HDLS", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_HDLS_ON") || material.GetFloat("_N_F_HDLS") == 0.0f))
            {
                material.DisableKeyword("N_F_HDLS_ON");
                material.SetFloat("_N_F_HDLS", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_HPSS_ON") || material.GetFloat("_N_F_HPSS") == 1.0f))
            {
                material.EnableKeyword("N_F_HPSS_ON");
                material.SetFloat("_N_F_HPSS", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_HPSS_ON") || material.GetFloat("_N_F_HPSS") == 0.0f))
            {
                material.DisableKeyword("N_F_HPSS_ON");
                material.SetFloat("_N_F_HPSS", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_DCS_ON") || material.GetFloat("_N_F_DCS") == 1.0f))
            {
                material.EnableKeyword("N_F_DCS_ON");
                material.SetShaderPassEnabled("ShadowCaster", false);
                material.SetFloat("_N_F_DCS", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_DCS_ON") || material.GetFloat("_N_F_DCS") == 0.0f))
            {
                material.DisableKeyword("N_F_DCS_ON");
                material.SetShaderPassEnabled("ShadowCaster", true);
                material.SetFloat("_N_F_DCS", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_RDC_ON") || material.GetFloat("_N_F_RDC") == 1.0f))
            {
                material.EnableKeyword("N_F_RDC_ON");
                material.SetFloat("_N_F_RDC", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_RDC_ON") || material.GetFloat("_N_F_RDC") == 0.0f))
            {
                material.DisableKeyword("N_F_RDC_ON");
                material.SetFloat("_N_F_RDC", 0.0f);
            }

            if ((material.IsKeywordEnabled("N_F_NLASOBF_ON") || material.GetFloat("_N_F_NLASOBF") == 1.0f))
            {
                material.EnableKeyword("_N_F_NLASOBF");
                material.SetFloat("_N_F_NLASOBF", 1.0f);
            }
            else if ((!material.IsKeywordEnabled("N_F_NLASOBF_ON") || material.GetFloat("_N_F_NLASOBF") == 0.0f))
            {
                material.DisableKeyword("N_F_NLASOBF_ON");
                material.SetFloat("_N_F_NLASOBF", 0.0f);
            }

            #endregion

        }

        #endregion

        #region ChanLi
        static void ChanLi(string searchTXT, string TXTChange, string fileName)
        {

            if (System.IO.File.Exists(fileName))
            {
                string[] arrLine = System.IO.File.ReadAllLines(fileName);

                for (int i = 0; i < arrLine.Length; ++i)
                {
                    if (arrLine[i] == searchTXT)
                    {
                        arrLine[i] = TXTChange;
                        System.IO.File.WriteAllLines(fileName, arrLine);
                        break;
                    }
                }

            }
            else
            {
                Debug.Log("Can't enable do 'Use Screen Space Outline' or 'Use Traditional Outline' because '" + fileName + "' Does not exist or file not found.");
            }

        }
        #endregion

        #region ReaLi
        static bool ReaLi(string searchTXT, string fileName)
        {

            if (System.IO.File.Exists(fileName))
            {
                string[] arrLine = System.IO.File.ReadAllLines(fileName);

                for (int i = 0; i < arrLine.Length; ++i)
                {
                    if (arrLine[i] == searchTXT)
                    {
                        return true;
                    }
                }

            }
            else
            {
                Debug.Log("Can't read a line because '" + fileName + "' Does not exist or file not found.");
            }

            return false;

        }

        #endregion

        #region Check_RE_OL
        void Check_RE_OL()
        {
            if (ReaLi("//OL_RE", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader"))
            {
                remoout = true;
                REMO_OUTL();
            }
            else
            {
                remoout = false;
                REMO_OUTL();
            }
        }
        #endregion

        #region REMO_OUTL
        void REMO_OUTL()
        {
            if (remoout == true)
            {
                ChanLi("Tags{" + (char)34 + "LightMode" + (char)34 + "=" + (char)34 + "SRPDefaultUnlit" + (char)34 + "}", "Tags{" + (char)34 + "LightMode" + (char)34 + "=" + (char)34 + "remove" + (char)34 + "}", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Cull [_DoubleSidedOutline]//OL_RCUL", "//Cull [_DoubleSidedOutline]//OL_RCUL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#ifdef UNITY_COLORSPACE_GAMMA//SSOL", "//#ifdef UNITY_COLORSPACE_GAMMA//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("_OutlineColor=float4(LinearToGamma22(_OutlineColor.rgb),_OutlineColor.a);//SSOL", "//_OutlineColor=float4(LinearToGamma22(_OutlineColor.rgb),_OutlineColor.a);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#endif//SSOL", "//#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#if N_F_O_ON//SSOL", "//#if N_F_O_ON//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("float3 SSOLi=(float3)EdgDet(sceneUVs.xy);//SSOL", "//float3 SSOLi=(float3)EdgDet(sceneUVs.xy);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#if N_F_O_MOTTSO_ON//SSOL", "//#if N_F_O_MOTTSO_ON//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("float3 Init_FO=((RTD_CA*RTD_SON_CHE_1))*lerp((float3)1.0,_OutlineColor.rgb,SSOLi);//SSOL", "//float3 Init_FO=((RTD_CA*RTD_SON_CHE_1))*lerp((float3)1.0,_OutlineColor.rgb,SSOLi);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#else//SSOL", "//#else//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("float3 Init_FO=lerp((RTD_CA*RTD_SON_CHE_1),_OutlineColor.rgb,SSOLi);//SSOL", "//float3 Init_FO=lerp((RTD_CA*RTD_SON_CHE_1),_OutlineColor.rgb,SSOLi);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#endif//SSOL", "//#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#else//SSOL", "//#else//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#endif//SSOL", "//#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//OL_NRE", "//OL_RE", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//SSOL_U", "//SSOL_NU", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                //============================
                //============================

                ChanLi("static bool remoout = true;", "static bool remoout = false;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string remooutstat = " + (char)34 + "Remove Outline" + (char)34 + ";", "static string remooutstat = " + (char)34 + "Add Outline" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.Log("Outline feature removed on RealToon URP shader.");
            }
            else if (remoout == false)
            {
                ChanLi("Tags{" + (char)34 + "LightMode" + (char)34 + "=" + (char)34 + "remove" + (char)34 + "}", "Tags{" + (char)34 + "LightMode" + (char)34 + "=" + (char)34 + "SRPDefaultUnlit" + (char)34 + "}", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Cull [_DoubleSidedOutline]//OL_RCUL", "Cull [_DoubleSidedOutline]//OL_RCUL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//OL_RE", "//OL_NRE", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                Check_SSOL_TOL();

                //============================
                //============================

                ChanLi("static bool remoout = false;", "static bool remoout = true;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string remooutstat = " + (char)34 + "Add Outline" + (char)34 + ";", "static string remooutstat = " + (char)34 + "Remove Outline" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.Log("Outline feature added on RealToon URP shader.");
            }
        }
        #endregion

        #region Check_SSOL_TOL
        void Check_SSOL_TOL()
        {
            if (ReaLi("//SSOL_U", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader"))
            {
                UseSSOL = true;
                USSOL_OR_TOL();
            }
            else
            {
                UseSSOL = false;
                USSOL_OR_TOL();
            }
        }
        #endregion

        #region USSOL_OR_TOL
        void USSOL_OR_TOL()
        {
            if (UseSSOL == true)
            {
                ChanLi("Tags{" + (char)34 + "LightMode" + (char)34 + "=" + (char)34 + "SRPDefaultUnlit" + (char)34 + "}", "Tags{" + (char)34 + "LightMode" + (char)34 + "=" + (char)34 + "remove" + (char)34 + "}", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Cull [_DoubleSidedOutline]//OL_RCUL", "//Cull [_DoubleSidedOutline]//OL_RCUL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#ifdef UNITY_COLORSPACE_GAMMA//SSOL", "#ifdef UNITY_COLORSPACE_GAMMA//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//_OutlineColor=float4(LinearToGamma22(_OutlineColor.rgb),_OutlineColor.a);//SSOL", "_OutlineColor=float4(LinearToGamma22(_OutlineColor.rgb),_OutlineColor.a);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//#endif//SSOL", "#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#if N_F_O_ON//SSOL", "#if N_F_O_ON//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//float3 SSOLi=(float3)EdgDet(sceneUVs.xy);//SSOL", "float3 SSOLi=(float3)EdgDet(sceneUVs.xy);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//#if N_F_O_MOTTSO_ON//SSOL", "#if N_F_O_MOTTSO_ON//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//float3 Init_FO=((RTD_CA*RTD_SON_CHE_1))*lerp((float3)1.0,_OutlineColor.rgb,SSOLi);//SSOL", "float3 Init_FO=((RTD_CA*RTD_SON_CHE_1))*lerp((float3)1.0,_OutlineColor.rgb,SSOLi);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//#else//SSOL", "#else//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//float3 Init_FO=lerp((RTD_CA*RTD_SON_CHE_1),_OutlineColor.rgb,SSOLi);//SSOL", "float3 Init_FO=lerp((RTD_CA*RTD_SON_CHE_1),_OutlineColor.rgb,SSOLi);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//#endif//SSOL", "#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//#else//SSOL", "#else//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#endif//SSOL", "#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//SSOL_NU", "//SSOL_U", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                //============================
                //============================

                ChanLi("static bool UseSSOL = true;", "static bool UseSSOL = false;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string OLType = " + (char)34 + "Traditional" + (char)34 + ";", "static string OLType = " + (char)34 + "Screen Space" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string UseSSOLStat = " + (char)34 + "Use Screen Space Outline" + (char)34 + ";", "static string UseSSOLStat = " + (char)34 + "Use Traditional Outline" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.Log("Screen Space Outline is now use.");
            }
            else if (UseSSOL == false)
            {
                ChanLi("Tags{" + (char)34 + "LightMode" + (char)34 + "=" + (char)34 + "remove" + (char)34 + "}", "Tags{" + (char)34 + "LightMode" + (char)34 + "=" + (char)34 + "SRPDefaultUnlit" + (char)34 + "}", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Cull [_DoubleSidedOutline]//OL_RCUL", "Cull [_DoubleSidedOutline]//OL_RCUL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#ifdef UNITY_COLORSPACE_GAMMA//SSOL", "//#ifdef UNITY_COLORSPACE_GAMMA//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("_OutlineColor=float4(LinearToGamma22(_OutlineColor.rgb),_OutlineColor.a);//SSOL", "//_OutlineColor=float4(LinearToGamma22(_OutlineColor.rgb),_OutlineColor.a);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#endif//SSOL", "//#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#if N_F_O_ON//SSOL", "//#if N_F_O_ON//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("float3 SSOLi=(float3)EdgDet(sceneUVs.xy);//SSOL", "//float3 SSOLi=(float3)EdgDet(sceneUVs.xy);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#if N_F_O_MOTTSO_ON//SSOL", "//#if N_F_O_MOTTSO_ON//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("float3 Init_FO=((RTD_CA*RTD_SON_CHE_1))*lerp((float3)1.0,_OutlineColor.rgb,SSOLi);//SSOL", "//float3 Init_FO=((RTD_CA*RTD_SON_CHE_1))*lerp((float3)1.0,_OutlineColor.rgb,SSOLi);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#else//SSOL", "//#else//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("float3 Init_FO=lerp((RTD_CA*RTD_SON_CHE_1),_OutlineColor.rgb,SSOLi);//SSOL", "//float3 Init_FO=lerp((RTD_CA*RTD_SON_CHE_1),_OutlineColor.rgb,SSOLi);//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#endif//SSOL", "//#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#else//SSOL", "//#else//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#endif//SSOL", "//#endif//SSOL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//SSOL_U", "//SSOL_NU", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                //============================
                //============================

                ChanLi("static bool UseSSOL = false;", "static bool UseSSOL = true;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string OLType = " + (char)34 + "Screen Space" + (char)34 + ";", "static string OLType = " + (char)34 + "Traditional" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string UseSSOLStat = " + (char)34 + "Use Traditional Outline" + (char)34 + ";", "static string UseSSOLStat = " + (char)34 + "Use Screen Space Outline" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.Log("Traditional Outline is now use.");
            }
        }
        #endregion

        #region TWOFORFIVE
        void TWOFORFIVE()
        {
            if (twofourfive_target == false)
            {
                ChanLi("static bool twofourfive_target = false;", "static bool twofourfive_target = true;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string twofourfive_target_string = " + (char)34 + "Change shader compilation target to 4.5" + (char)34 + ";", "static string twofourfive_target_string = " + (char)34 + "Change shader compilation target to 2.0" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                ChanLi("#pragma target 2.0 //targetol", "#pragma target 4.5 //targetol", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 2.0 //targetfl", "#pragma target 4.5 //targetfl", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 2.0 //targetsc", "#pragma target 4.5 //targetsc", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 2.0 //targetgb", "#pragma target 4.5 //targetgb", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 2.0 //targetdo", "#pragma target 4.5 //targetdo", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 2.0 //targetdn", "#pragma target 4.5 //targetdn", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 2.0 //targetm", "#pragma target 4.5 //targetm", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.LogWarning("RealToon shader compilation target has been changed to 4.5, added support for DOTS and Tessellation.");
            }
            else if (twofourfive_target == true)
            {
                ChanLi("static bool twofourfive_target = true;", "static bool twofourfive_target = false;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string twofourfive_target_string = " + (char)34 + "Change shader compilation target to 2.0" + (char)34 + ";", "static string twofourfive_target_string = " + (char)34 + "Change shader compilation target to 4.5" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                ChanLi("#pragma target 4.5 //targetol", "#pragma target 2.0 //targetol", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 4.5 //targetfl", "#pragma target 2.0 //targetfl", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 4.5 //targetsc", "#pragma target 2.0 //targetsc", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 4.5 //targetgb", "#pragma target 2.0 //targetgb", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 4.5 //targetdo", "#pragma target 2.0 //targetdo", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 4.5 //targetdn", "#pragma target 2.0 //targetdn", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("#pragma target 4.5 //targetm", "#pragma target 2.0 //targetm", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.LogWarning("RealToon shader compilation target has been changed to 2.0.");
            }
        }
        #endregion

        #region  DOTSLBSCD

        void DOTSLBSCD()
        {
            if (dots_lbs_cd == false)
            {

            ChanLi("static bool dots_lbs_cd = false;", "static bool dots_lbs_cd = true;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
            ChanLi("static string dots_lbs_cd_string = " + (char)34 + "DOTS|HR - Use Compute Deformation" + (char)34 + ";", "static string dots_lbs_cd_string = " + (char)34 + "DOTS|HR - Use Linear Blend Skinning" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");


            ChanLi("float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_OL", "//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("uint4 indices : BLENDINDICES;//DOTS_LiBleSki_OL", "//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//uint vertexID : SV_VertexID;//DOTS_CompDef_OL", "uint vertexID : SV_VertexID;//DOTS_CompDef_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_OL", "DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_OL", "//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


            ChanLi("float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_FL", "//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("uint4 indices : BLENDINDICES;//DOTS_LiBleSki_FL", "//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//uint vertexID : SV_VertexID;//DOTS_CompDef_FL", "uint vertexID : SV_VertexID;//DOTS_CompDef_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_FL", "DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_FL", "//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


            ChanLi("float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_GB", "//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("uint4 indices : BLENDINDICES;//DOTS_LiBleSki_GB", "//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//uint vertexID : SV_VertexID;//DOTS_CompDef_GB", "uint vertexID : SV_VertexID;//DOTS_CompDef_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_GB", "DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_GB", "//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


            ChanLi("float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_SC", "//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("uint4 indices : BLENDINDICES;//DOTS_LiBleSki_SC", "//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//uint vertexID : SV_VertexID;//DOTS_CompDef_SC", "uint vertexID : SV_VertexID;//DOTS_CompDef_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_SC", "DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_SC", "//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


            ChanLi("float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_DO", "//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("uint4 indices : BLENDINDICES;//DOTS_LiBleSki_DO", "//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//uint vertexID : SV_VertexID;//DOTS_CompDef_DO", "uint vertexID : SV_VertexID;//DOTS_CompDef_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_DO", "DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("DOTS_LiBleSki(input.indices, input.weights, input.position.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_DO", "//DOTS_LiBleSki(input.indices, input.weights, input.position.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


            ChanLi("float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_DN", "//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("uint4 indices : BLENDINDICES;//DOTS_LiBleSki_DN", "//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//uint vertexID : SV_VertexID;//DOTS_CompDef_DN", "uint vertexID : SV_VertexID;//DOTS_CompDef_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_DN", "DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            ChanLi("DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normal.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_DN", "//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normal.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


            AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
            AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
            Debug.LogWarning("DOTS|HR - Compute Deformation is now use, This will enable you to use BlendShapes and other deformation.");

            }
            else if (dots_lbs_cd == true)
            {
                ChanLi("static bool dots_lbs_cd = true;", "static bool dots_lbs_cd = false;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string dots_lbs_cd_string = " + (char)34 + "DOTS|HR - Use Linear Blend Skinning" + (char)34 + ";", "static string dots_lbs_cd_string = " + (char)34 + "DOTS|HR - Use Compute Deformation" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");


                ChanLi("//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_OL", "float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_OL", "uint4 indices : BLENDINDICES;//DOTS_LiBleSki_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("uint vertexID : SV_VertexID;//DOTS_CompDef_OL", "//uint vertexID : SV_VertexID;//DOTS_CompDef_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_OL", "//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_OL", "DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_OL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


                ChanLi("//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_FL", "float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_FL", "uint4 indices : BLENDINDICES;//DOTS_LiBleSki_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("uint vertexID : SV_VertexID;//DOTS_CompDef_FL", "//uint vertexID : SV_VertexID;//DOTS_CompDef_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_FL", "//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_FL", "DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_FL", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


                ChanLi("//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_GB", "float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_GB", "uint4 indices : BLENDINDICES;//DOTS_LiBleSki_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("uint vertexID : SV_VertexID;//DOTS_CompDef_GB", "//uint vertexID : SV_VertexID;//DOTS_CompDef_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_GB", "//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_GB", "DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_GB", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


                ChanLi("//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_SC", "float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_SC", "uint4 indices : BLENDINDICES;//DOTS_LiBleSki_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("uint vertexID : SV_VertexID;//DOTS_CompDef_SC", "//uint vertexID : SV_VertexID;//DOTS_CompDef_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_SC", "//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_SC", "DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_SC", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


                ChanLi("//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_DO", "float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_DO", "uint4 indices : BLENDINDICES;//DOTS_LiBleSki_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("uint vertexID : SV_VertexID;//DOTS_CompDef_DO", "//uint vertexID : SV_VertexID;//DOTS_CompDef_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_DO", "//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//DOTS_LiBleSki(input.indices, input.weights, input.position.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_DO", "DOTS_LiBleSki(input.indices, input.weights, input.position.xyz, input.normalOS.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_DO", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


                ChanLi("//float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_DN", "float4 weights : BLENDWEIGHTS;//DOTS_LiBleSki_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//uint4 indices : BLENDINDICES;//DOTS_LiBleSki_DN", "uint4 indices : BLENDINDICES;//DOTS_LiBleSki_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("uint vertexID : SV_VertexID;//DOTS_CompDef_DN", "//uint vertexID : SV_VertexID;//DOTS_CompDef_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_DN", "//DOTS_CompDef(input.vertexID, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_CompDef_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normal.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_DN", "DOTS_LiBleSki(input.indices, input.weights, input.positionOS.xyz, input.normal.xyz, input.tangentOS.xyz, (float3)_LBS_CD_Position, _LBS_CD_Normal, (float3)_LBS_CD_Tangent);//DOTS_LiBleSki_DN", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");


                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.LogWarning("DOTS|HR - Linear Blending Skinning is now use.");
            }
        }

        #endregion

        #region ADD_ST
        void ADD_ST()
        {
            if (add_st == true)
            {
                ChanLi("static bool add_st = true;", "static bool add_st = false;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string add_st_string = " + (char)34 + "Add 'See Through' feature" + (char)34 + ";", "static string add_st_string = " + (char)34 + "Remove 'See Through' feature" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                ChanLi("/*//O_ST", "//O_ST/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//F_ST", "//F_ST/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//G_ST", "//G_ST/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Pass [_OutStenPass]//O_PI", "//Pass [_OutStenPass]//O_PI", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.LogWarning("'See Through feature' has been added.");
            }
            else if (add_st == false)
            {
                ChanLi("static bool add_st = false;", "static bool add_st = true;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string add_st_string = " + (char)34 + "Remove 'See Through' feature" + (char)34 + ";", "static string add_st_string = " + (char)34 + "Add 'See Through' feature" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("//Pass [_OutStenPass]//O_PI", "Pass [_OutStenPass]//O_PI", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//O_ST/*", "/*//O_ST", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//F_ST/*", "/*//F_ST", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//G_ST/*", "/*//G_ST", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.LogWarning("'See Through feature' has been remove.");
            }
        }
        #endregion

        #region TESS_SUPP
        void TESS_SUPP()
        {
            if (tess_supp == false)
            {
                ChanLi("static bool tess_supp = false;", "static bool tess_supp = true;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string tess_supp_string = " + (char)34 + "Enable Tessellation" + (char)34 + ";", "static string tess_supp_string = " + (char)34 + "Disable Tessellation" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                ChanLi("//#define N_F_TESS_ON//FPT", "#define N_F_TESS_ON//FPT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_FP_1", "//Tess_FP_1/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings LitPassVertex(Attributes input)//FPV", "Varyings PostProcessVertex(Attributes input)//FPV", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_FP_2", "//Tess_FP_2/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#define N_F_TESS_ON//SCT", "#define N_F_TESS_ON//SCT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_SCP_1", "//Tess_SCP_1/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings ShadowPassVertex(Attributes input)//SCP", "Varyings PostProcessVertex(Attributes input)//SCP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_SCP_2", "//Tess_SCP_2/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#define N_F_TESS_ON//GBT", "#define N_F_TESS_ON//GBT//", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_GBP_1", "//Tess_GBP_1/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings LitPassVertex(Attributes input)//GBP", "Varyings PostProcessVertex(Attributes input)//GBP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_GBP_2", "//Tess_GBP_2/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#define N_F_TESS_ON//DOPT", "#define N_F_TESS_ON//DOPT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_DOP_1", "//Tess_DOP_1/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings DepthOnlyVertex(Attributes input)//DOP", "Varyings PostProcessVertex(Attributes input)//DOP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_DOP_2", "//Tess_DOP_2/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#define N_F_TESS_ON//DNT", "#define N_F_TESS_ON//DNT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_DNP_1", "//Tess_DNP_1/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings DepthNormalsVertex(Attributes input)//DNP", "Varyings PostProcessVertex(Attributes input)//DNP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_DNP_2", "//Tess_DNP_2/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#define N_F_TESS_ON//OT", "#define N_F_TESS_ON//OT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_OP_1", "//Tess_OP_1/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings LitPassVertex(Attributes input)//OP", "Varyings PostProcessVertex(Attributes input)//OP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("/*//Tess_OP_2", "//Tess_OP_2/*", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#if N_F_NM_ON//NMKW", "//#if N_F_NM_ON//NMKW", "Assets/RealToon/RealToon Shaders/RealToon Core/URP/RT_URP_PROP.hlsl");
                ChanLi("#endif//NMKW_END", "//#endif//NMKW_END", "Assets/RealToon/RealToon Shaders/RealToon Core/URP/RT_URP_PROP.hlsl");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.LogWarning("'Tessellation' has been enabled.");
            }
            else if (tess_supp == true)
            {
                ChanLi("static bool tess_supp = true;", "static bool tess_supp = false;", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                ChanLi("static string tess_supp_string = " + (char)34 + "Disable Tessellation" + (char)34 + ";", "static string tess_supp_string = " + (char)34 + "Enable Tessellation" + (char)34 + ";", "Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");

                ChanLi("#define N_F_TESS_ON//FPT", "//#define N_F_TESS_ON//FPT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_FP_1/*", "/*//Tess_FP_1", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings PostProcessVertex(Attributes input)//FPV", "Varyings LitPassVertex(Attributes input)//FPV", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_FP_2/*", "/*//Tess_FP_2", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#define N_F_TESS_ON//SCT", "//#define N_F_TESS_ON//SCT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_SCP_1/*", "/*//Tess_SCP_1", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings PostProcessVertex(Attributes input)//SCP", "Varyings ShadowPassVertex(Attributes input)//SCP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_SCP_2/*", "/*//Tess_SCP_2", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#define N_F_TESS_ON//GBT", "//#define N_F_TESS_ON//GBT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_GBP_1/*", "/*//Tess_GBP_1", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings PostProcessVertex(Attributes input)//GBP", "Varyings LitPassVertex(Attributes input)//GBP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_GBP_2/*", "/*//Tess_GBP_2", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#define N_F_TESS_ON//DOPT", "//#define N_F_TESS_ON//DOPT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_DOP_1/*", "/*//Tess_DOP_1", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings PostProcessVertex(Attributes input)//DOP", "Varyings DepthOnlyVertex(Attributes input)//DOP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_DOP_2/*", "/*//Tess_DOP_2", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#define N_F_TESS_ON//DNT", "//#define N_F_TESS_ON//DNT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_DNP_1/*", "/*//Tess_DNP_1", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings PostProcessVertex(Attributes input)//DNP", "Varyings DepthNormalsVertex(Attributes input)//DNP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_DNP_2/*", "/*//Tess_DNP_2", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("#define N_F_TESS_ON//OT", "//#define N_F_TESS_ON//OT", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_OP_1/*", "/*//Tess_OP_1", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("Varyings PostProcessVertex(Attributes input)//OP", "Varyings LitPassVertex(Attributes input)//OP", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                ChanLi("//Tess_OP_2/*", "/*//Tess_OP_2", "Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");

                ChanLi("//#if N_F_NM_ON//NMKW", "#if N_F_NM_ON//NMKW", "Assets/RealToon/RealToon Shaders/RealToon Core/URP/RT_URP_PROP.hlsl");
                ChanLi("//#endif//NMKW_END", "#endif//NMKW_END", "Assets/RealToon/RealToon Shaders/RealToon Core/URP/RT_URP_PROP.hlsl");

                AssetDatabase.ImportAsset("Assets/RealToon/RealToon Shaders/Version 5/URP/Default/D_Default_URP.shader");
                AssetDatabase.ImportAsset("Assets/RealToon/Editor/RealToonShaderGUI_URP_SRP.cs");
                Debug.LogWarning("'Tessellation' has been disabled.");
            }
        }
        #endregion

    }

}

#endif
