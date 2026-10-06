using Microsoft.AspNetCore.Http;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudentManagement.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Định dạng thời gian kèm mili giây (fff)
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;

            // Chức năng 1: Ghi log request nhận vào ra Console
            Console.WriteLine($"[{time}] Method: {method} - Path: {path}");

            // Chức năng 3: Chặn truy cập ID không hợp lệ (<= 0 như /Students/Details/0, /Students/Details/-1,...)
            // Bắt cả path dạng /Students/Details/0 hoặc /Students/Delete/-1
            if (path.StartsWith("/Students/Details/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/Students/Edit/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/Students/Delete/", StringComparison.OrdinalIgnoreCase))
            {
                var segments = path.Split('/');
                if (segments.Length >= 4 && int.TryParse(segments[3], out int id))
                {
                    if (id <= 0)
                    {
                        context.Response.StatusCode = 400; // Bad Request
                        context.Response.ContentType = "text/plain; charset=utf-8";
                        await context.Response.WriteAsync("Student id không hợp lệ");

                        Console.WriteLine($"[BLOCKED] Status Code: {context.Response.StatusCode} - Path: {path}");
                        return; // Ngắt luồng (short-circuit), không gọi _next
                    }
                }
            }

            // Chuyển request sang middleware / controller tiếp theo
            await _next(context);

            // Chức năng 2: Ghi log status code sau khi Controller / Endpoint xử lý xong
            Console.WriteLine($"[COMPLETED] Path: {path} - Status Code: {context.Response.StatusCode}");
        }
    }
}