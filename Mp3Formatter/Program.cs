using FFMpegCore;
using FFMpegCore.Extensions.Downloader;

namespace Mp3Formatter;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Парсинг аргументов командной строки
        string inputDir = null;
        string outputDir = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i":
                case "--input":
                {
                    if (i + 1 < args.Length)
                        inputDir = Path.GetFullPath(args[++i]);
                    break;
                }
                case "-o":
                case "--output":
                {
                    if (i + 1 < args.Length)
                        outputDir = Path.GetFullPath(args[++i]);
                    break;
                }
            }
        }

        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        inputDir ??= currentDir; // по умолчанию — текущая папка
        outputDir ??= Path.Combine(currentDir, "renamed_mp3"); // по умолчанию — подпапка

        // Валидация входной директории
        if (!Directory.Exists(inputDir))
        {
            Console.WriteLine($"Ошибка: входная директория не существует: {inputDir}");
            return;
        }

        Console.WriteLine($"Загрузка FFmpeg (если ещё не загружен)...");
        await FFMpegDownloader.DownloadBinaries(options: new FFOptions { BinaryFolder = currentDir });

        // Убедимся, что outputDir существует
        Directory.CreateDirectory(outputDir);

        var mp3Files = Directory.GetFiles(inputDir, "*.mp3")
            .OrderBy(Path.GetFileName)
            .ToArray();

        if (mp3Files.Length == 0)
        {
            Console.WriteLine($"Нет MP3-файлов в директории: {inputDir}");
            return;
        }

        const int totalDigits = 4;

        for (var i = 0; i < mp3Files.Length; i++)
        {
            var sourcePath = mp3Files[i];
            var originalFileName = Path.GetFileName(sourcePath);
            var newPrefix = (i + 1).ToString().PadLeft(totalDigits, '0');
            var tempPath = Path.Combine(outputDir, $"{newPrefix}_temp.mp3");
            var finalPath = Path.Combine(outputDir, $"{newPrefix}.mp3");

            try
            {
                // 1. Нормализация громкости
                FFMpegArguments
                    .FromFileInput(sourcePath)
                    .OutputToFile(
                        tempPath,
                        overwrite: true,
                        options => options
                            .WithCustomArgument("-af loudnorm=I=-16:TP=-1.5:LRA=11")
                            .WithAudioCodec(FFMpegCore.Enums.AudioCodec.LibMp3Lame)
                    )
                    .ProcessSynchronously();

                // 2. Запись оригинального имени в метаданные
                Mp3MetadataReader.WriteOriginalFilename(tempPath, Path.GetFileNameWithoutExtension(originalFileName));
                File.Move(tempPath, finalPath, overwrite: true);

                Console.WriteLine($"✓ {originalFileName} → {newPrefix}.mp3");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Ошибка при обработке {originalFileName}: {ex.Message}");
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        Console.WriteLine($"\nГотово! Файлы сохранены в: {outputDir}");
    }
}