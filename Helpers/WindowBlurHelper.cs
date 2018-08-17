using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace GBCLV2.Helpers
{
    static class Win10BlurHelper
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData pData);

        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        public static void EnableBlur(Window _window)
        {
            var WindowPtr = new WindowInteropHelper(_window).Handle;

            var accent = new AccentPolicy()
            {
                AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND,
                AccentFlags = 0x20 | 0x40 | 0x80 | 0x100,
                //GradientColor = 0x000000FF,
                //AnimationId = 
            };
            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData()
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };
            SetWindowCompositionAttribute(WindowPtr, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
    }

    static class Win7BlurHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND pBlurBehind);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public uint dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int Left;     // width of left border that retains its size  
            public int Right;    // width of right border that retains its size  
            public int Top;      // height of top border that retains its size  
            public int Bottom;   // height of bottom border that retains its size  
        };

        public static void EnableAeroGlass(Window _window)
        {
            var windowPtr = new WindowInteropHelper(_window).Handle;

            int val = 1;
            DwmSetWindowAttribute(windowPtr, 3, ref val, sizeof(int));

            var blur = new DWM_BLURBEHIND
            {
                dwFlags = 0x00000001,
                fEnable = true
            };

            var margin = new MARGINS
            {
                Left = 0,
                Right = 0,
                Top = 0,
                Bottom = 0,
            };

            DwmEnableBlurBehindWindow(windowPtr, ref blur);
            DwmExtendFrameIntoClientArea(windowPtr, ref margin);
        }
    }
}
