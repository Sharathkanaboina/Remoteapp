using System.Runtime.InteropServices;
using System.Text.Json;

namespace RemoteAdminClientUI
{
    public static class InputHandler
    {
        [DllImport("user32.dll")] static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")] static extern void mouse_event(int flags, int dx, int dy, int buttons, int extra);

        public static void Handle(JsonElement ev)
        {
            string type = ev.GetProperty("type").GetString()!;

            switch (type)
            {
                case "mouse_move":
                    int x = ev.GetProperty("x").GetInt32();
                    int y = ev.GetProperty("y").GetInt32();
                    SetCursorPos(x, y);
                    break;

                case "mouse_click":
                    mouse_event(0x02, 0, 0, 0, 0);
                    mouse_event(0x04, 0, 0, 0, 0);
                    break;

                case "send_keys":
                    // (Use InputSimulator if needed)
                    break;
            }
        }
    }
}
