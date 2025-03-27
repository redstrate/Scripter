using System;
using Dalamud.Hooking;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using Scripter.Windows;
using FFXIVClientStructs.FFXIV.Common.Lua;

namespace Scripter;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider Hooking { get; private set; } = null!;

    public readonly WindowSystem WindowSystem = new("Scripter");
    private MainWindow MainWindow { get; init; }

    // FIXME: what a *terrible* signature
    [Signature("48 89 5c 24 10 48 89 6c 24 18 48 89 74 24 20 57 48 81 ec 30 01 00 00 48 8b 05 0a 2a 18 02 48 33 c4 48 89 84 24 20 01 00 00 48 8b f9 e8 cf ea 21 01", DetourName = nameof(LuaPrint))]
    private readonly Hook<LuaPrintDelegate>? luaPrintHook = null!;

    private unsafe delegate void LuaPrintDelegate(lua_State* L);

    public Plugin()
    {
        Hooking.InitializeFromAttributes(this);
        luaPrintHook?.Enable();

        MainWindow = new MainWindow(this);
        MainWindow.IsOpen = true;
        WindowSystem.AddWindow(MainWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;
    }

    public void Dispose()
    {
        luaPrintHook?.Dispose();

        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();
    }

    private void DrawUI() => WindowSystem.Draw();

    private unsafe void LuaPrint(lua_State* L)
    {
        var message = L->lua_tostring(1).ToString();

        Log.Information($"lua: {message}");
        luaPrintHook!.Original(L);
    }
}
