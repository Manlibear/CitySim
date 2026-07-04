using System.IO;
using System.Text.Json;
using CitySim.Data;
using CitySim.Helpers.Converters;
using Godot;

namespace CitySim.Scripts;

public static class SaveGame
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new Vector2IJsonConverter() },
    };

    public static string DefaultSavePath => ProjectSettings.GlobalizePath("user://savegame.json");

    public static void Save(SaveGameData data, string absolutePath)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(absolutePath, json);
    }

    public static SaveGameData Load(string absolutePath)
    {
        var json = File.ReadAllText(absolutePath);
        return JsonSerializer.Deserialize<SaveGameData>(json, _jsonOptions)
            ?? throw new InvalidDataException($"Failed to deserialize save file at {absolutePath}");
    }
}
