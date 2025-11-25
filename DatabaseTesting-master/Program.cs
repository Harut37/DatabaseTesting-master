using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace DatabaseTesting
{
    class Program
    {
      
        private static string ConnectionString = @"Server=HARGUS\SQLEXPRESS;Database=Cloth;Trusted_Connection=true;";

        static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static async Task MainAsync()
        {
            Console.WriteLine("🗄️ Тестирование производительности базы данных");
            Console.WriteLine("📊 Тестируем операции с таблицей [Изделие$]");
            Console.WriteLine($"🔗 Подключение к: {ConnectionString}");

            try
            {
                // Тест вставки 100 записей
                Console.WriteLine("\n1. ТЕСТ ВСТАВКИ 100 ЗАПИСЕЙ:");
                var insertTime = await MeasureInsertPerformance();
                Console.WriteLine($"✅ Время вставки 100 записей: {insertTime} мс");

                // Тест выборки данных
                Console.WriteLine("\n2. ТЕСТ ВЫБОРКИ ДАННЫХ:");
                var selectTime = await MeasureSelectPerformance();
                Console.WriteLine($"✅ Время выборки всех записей: {selectTime} мс");

                // Тест удаления тестовых данных
                Console.WriteLine("\n3. ТЕСТ УДАЛЕНИЯ ДАННЫХ:");
                var deleteTime = await MeasureDeletePerformance();
                Console.WriteLine($"✅ Время удаления тестовых записей: {deleteTime} мс");

                // Сводная статистика
                Console.WriteLine("\n📊 СВОДНАЯ СТАТИСТИКА:");
                Console.WriteLine($"Общее время всех операций: {insertTime + selectTime + deleteTime} мс");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                Console.WriteLine("💡 Проверьте:");
                Console.WriteLine("   1. Запущен ли SQL Server Express");
                Console.WriteLine("   2. Существует ли база данных 'Cloth'");
                Console.WriteLine("   3. Существует ли таблица '[Изделие$]'");
            }
        }

        static async Task<long> MeasureInsertPerformance()
        {
            var stopwatch = Stopwatch.StartNew();

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                // Создаем 100 тестовых записей
                var items = Enumerable.Range(1, 100).Select(i => new
                {
                    Артикул = $"TEST_ART{i}",
                    Наименование = $"Тестовый товар {i}",
                    Ширина = 10 + i,
                    Длина = 20 + i,
                    Изображение = "test_image.jpg",
                    Комментарий = "Тестовая запись для нагрузочного тестирования"
                });

                string query = @"INSERT INTO [Изделие$] 
                               (Артикул, Наименование, Ширина, Длина, Изображение, Комментарий) 
                               VALUES (@Артикул, @Наименование, @Ширина, @Длина, @Изображение, @Комментарий)";

                await connection.ExecuteAsync(query, items);
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        static async Task<long> MeasureSelectPerformance()
        {
            var stopwatch = Stopwatch.StartNew();

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT * FROM [Изделие$] WHERE Артикул LIKE 'TEST_%'";
                var results = await connection.QueryAsync<dynamic>(query);

                Console.WriteLine($"📋 Найдено тестовых записей: {results.Count()}");

                // Выводим первые 5 записей для проверки
                Console.WriteLine("Примеры записей:");
                foreach (var item in results.Take(5))
                {
                    Console.WriteLine($"   Артикул: {item.Артикул}, Наименование: {item.Наименование}, Ширина: {item.Ширина}, Длина: {item.Длина}");
                }
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        static async Task<long> MeasureDeletePerformance()
        {
            var stopwatch = Stopwatch.StartNew();

            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                string query = "DELETE FROM [Изделие$] WHERE Артикул LIKE 'TEST_%'";
                var affectedRows = await connection.ExecuteAsync(query);

                Console.WriteLine($"🗑️ Удалено тестовых записей: {affectedRows}");
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
