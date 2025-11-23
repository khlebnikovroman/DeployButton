// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
//
// var builder = Host.CreateApplicationBuilder(args);
//
// // Определяем, запущено ли как служба
// bool isService = !Environment.UserInteractive;
//
// // Настройка службы Windows (если нужно)
// if (isService)
// {
//     builder.Services.AddWindowsService(options =>
//     {
//         options.ServiceName = "DeployButton Service";
//     });
// }
//
// // Добавляем ASP.NET Core
// builder.Services.AddRazorPages(); // не обязательно, но можно
// builder.Services.AddControllers();
//
// var app = builder.Build();
//
// // SPA: раздаём Angular как статические файлы
// string webRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
// if (!Directory.Exists(webRoot))
// {
//     Directory.CreateDirectory(webRoot);
// }
//
// app.UseDefaultFiles();
// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRoot),
//     RequestPath = ""
// });
//
// // API
// app.MapControllers();
//
// // Fallback для SPA (чтобы маршруты Angular работали при обновлении страницы)
// app.MapFallbackToFile("/index.html");
//
// // Запуск
// if (isService)
// {
//     app.Run(); // будет работать как служба + веб-сервер
// }
// else
// {
//     // Для отладки: запускаем и веб, и логику службы
//     var deployService = new DeployButtonService(app.Services);
//     deployService.Start();
//
//     await app.RunAsync(); // остаёмся жить в веб-режиме
// }