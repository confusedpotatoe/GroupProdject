using BrickBreaker.UI.Game.Infrastructure;
using System.Runtime.InteropServices;

public sealed class Win32Keyboard : IKeyboard
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static bool IsDown(int key) => (GetAsyncKeyState(key) & 0x8000) != 0;

    private const int VK_LEFT = 0x25;
    private const int VK_RIGHT = 0x27;
    private const int VK_ESCAPE = 0x1B;

    public bool IsLeftPressed() => IsDown(VK_LEFT);
    public bool IsRightPressed() => IsDown(VK_RIGHT);
    public bool IsEscapePressed() => IsDown(VK_ESCAPE);
    public bool IsSpacePressed() => IsDown((int)ConsoleKey.Spacebar);
    public bool IsUpPressed() => IsDown((int)ConsoleKey.UpArrow);
}