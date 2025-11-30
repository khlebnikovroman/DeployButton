using TagLib;
using TagLib.Id3v2;
using File = System.IO.File;

namespace Mp3Formatter;

/// <summary>
/// Сервис для чтения и записи пользовательских метаданных в MP3-файлы.
/// </summary>
public static class Mp3MetadataReader
{
    private const string OriginalFilenameKey = "ORIGINAL_FILENAME";

    /// <summary>
    /// Записывает оригинальное имя файла в ID3-тег TXXX.
    /// </summary>
    public static void WriteOriginalFilename(string mp3FilePath, string originalName)
    {
        if (!File.Exists(mp3FilePath))
            throw new FileNotFoundException($"Файл не найден: {mp3FilePath}");

        using var file = TagLib.File.Create(mp3FilePath);
        var tag = file.GetTag(TagTypes.Id3v2, true) as TagLib.Id3v2.Tag;

        if (tag == null)
        {
            return;
        }

        // Удаляем существующие TXXX с таким описанием
        var existingFrames = tag.GetFrames("TXXX")
            .OfType<UserTextInformationFrame>()
            .Where(f => string.Equals(f.Description, OriginalFilenameKey, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var frame in existingFrames)
            tag.RemoveFrame(frame);

        // Добавляем новое значение
        var newFrame = new UserTextInformationFrame(OriginalFilenameKey);
        newFrame.Text = [originalName];
        tag.AddFrame(newFrame);

        file.Save();
    }

    /// <summary>
    /// Читает оригинальное имя файла из ID3-тега TXXX.
    /// </summary>
    /// <returns>Оригинальное имя или null, если не найдено.</returns>
    public static string? GetOriginalFilename(string mp3FilePath)
    {
        if (!File.Exists(mp3FilePath))
            throw new FileNotFoundException($"Файл не найден: {mp3FilePath}");

        using var file = TagLib.File.Create(mp3FilePath);
        var tag = file.GetTag(TagTypes.Id3v2) as TagLib.Id3v2.Tag;

        var frame = tag?.GetFrames("TXXX")
            .OfType<UserTextInformationFrame>()
            .FirstOrDefault(f => string.Equals(f.Description, OriginalFilenameKey, StringComparison.OrdinalIgnoreCase));

        return frame?.Text?.FirstOrDefault();
    }
}