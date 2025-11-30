using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Extensions.Downloader;
using File = System.IO.File;

namespace Mp3Formatter;

public class Program
{
    public static async Task Main(string[] args)
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        
        Console.WriteLine("Загрузка FFmpeg (если ещё не загружен)...");
        await FFMpegDownloader.DownloadBinaries(options: new FFOptions(){BinaryFolder = currentDir});

        var outputDir = Path.Combine(currentDir, "renamed_mp3");
        Directory.CreateDirectory(outputDir);

        var mp3Files = Directory.GetFiles(currentDir, "*.mp3")
            .OrderBy(Path.GetFileName)
            .ToArray();

        if (mp3Files.Length == 0)
        {
            Console.WriteLine("Нет MP3-файлов в текущей директории.");
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