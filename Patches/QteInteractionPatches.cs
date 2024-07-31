using System.Reflection;
using SPT.Reflection.Patching;
using EFT.Hideout;
using EFT.InputSystem;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace stckytwl.CircleClickingGame.Patches
{
    public class QteEnableCursorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutAreaQTEOverlay), nameof(HideoutAreaQTEOverlay.ShouldLockCursor));
        }

        [PatchPrefix]
        // ReSharper disable once InconsistentNaming
        private static bool Prefix(ref ECursorResult __result)
        {
            __result = ECursorResult.ShowCursor;
            return false;
        }
    }

    public class QteClickablePatch : ModulePatch
    {
        private static FieldInfo _staticCircleImageField;
        private static FieldInfo _staticCircleOuterBorderField;

        protected override MethodBase GetTargetMethod()
        {
            _staticCircleOuterBorderField = AccessTools.Field(typeof(ShrinkingCircleQTE), "_staticCircleOuterBorder");
            _staticCircleImageField = AccessTools.Field(typeof(ShrinkingCircleQTE), "_staticCircleImage");
            return AccessTools.Method(typeof(ShrinkingCircleQTE), nameof(ShrinkingCircleQTE.method_1));
        }

        [PatchPostfix]
        // ReSharper disable all InconsistentNaming
        private static void PatchPostFix(ShrinkingCircleQTE __instance, ref bool __result)
        {
            var staticCircleOuterBorder = (Transform)_staticCircleOuterBorderField.GetValue(__instance);
            var staticCircleImage = (Image)_staticCircleImageField.GetValue(__instance);

            var StaticCirclePosition = staticCircleOuterBorder.position;
            var StaticCircleLocalScale = staticCircleOuterBorder.localScale;
            var StaticCircleImageSpriteRect = staticCircleImage.sprite.rect;

            var distToMouse = Vector2.Distance(Input.mousePosition, StaticCirclePosition);
            var inCircle = distToMouse < (StaticCircleImageSpriteRect.width * StaticCircleLocalScale.x);

            // Alternative key press don't work.
            __result = __result && inCircle;
        }
    }
}