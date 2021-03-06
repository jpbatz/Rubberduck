using System;
using System.Runtime.InteropServices;
using System.Text;
using Rubberduck.VBEditor.WindowsApi;

namespace Rubberduck.Common.WinAPI
{
    public enum KeyModifier : uint
    {
        ALT = 0x1,
        CONTROL = 0x2,
        SHIFT = 0x4,
        WIN = 0x8
    }

    /// <summary>
    /// Exposes User32.dll API.
    /// </summary>
    public static class User32
    {
        /// <summary>
        /// Defines a system-wide hot key.
        /// </summary>
        /// <param name="hWnd">A handle to the window that will receive WM_HOTKEY messages generated by the hot key. 
        /// If this parameter is NULL, WM_HOTKEY messages are posted to the message queue of the calling thread and must be processed in the message loop.</param>
        /// <param name="id">The identifier of the hot key. 
        /// If the hWnd parameter is NULL, then the hot key is associated with the current thread rather than with a particular window. 
        /// If a hot key already exists with the same hWnd and id parameters</param>
        /// <param name="fsModifiers">The keys that must be pressed in combination with the key specified by the uVirtKey parameter in order to generate the WM_HOTKEY message. 
        /// The fsModifiers parameter can be a combination of the following values.</param>
        /// <param name="vk">The virtual-key code of the hot key</param>
        /// <returns>If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, IntPtr id, uint fsModifiers, uint vk);


        /// <summary>
        /// Frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hWnd">A handle to the window associated with the hot key to be freed. This parameter should be NULL if the hot key is not associated with a window.</param>
        /// <param name="id">The identifier of the hot key to be freed.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, IntPtr id);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);

        public delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        [DllImport("user32.dll")]
        public static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);
    }
}
