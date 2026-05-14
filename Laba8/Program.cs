using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TriangleApp
{
    public class TriangleCalculator
    {
        private const int FIELD_SIZE = 100;
        private const double EPSILON = 1e-9;
        private static readonly string logFile = "triangle_app.log";

        public class Result
        {
            public string TriangleType { get; set; }
            public List<Tuple<int, int>> Coordinates { get; set; }
        }

        private void Log(string level, string message)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {level} - {message}";

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(logMessage);

            try
            {
                File.AppendAllText(logFile, logMessage + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }

        public Result ProcessRequest(string sideA, string sideB, string sideC)
        {
            var result = new Result();
            DateTime requestTime = DateTime.Now;

            if (!TryParseSides(sideA, sideB, sideC, out double a, out double b, out double c))
            {
                result.TriangleType = "";
                result.Coordinates = new List<Tuple<int, int>>
                {
                    Tuple.Create(-2, -2),
                    Tuple.Create(-2, -2),
                    Tuple.Create(-2, -2)
                };

                Log("ERROR", $"Неуспешный запрос. Ошибка: Нечисловые данные");
                return result;
            }

            result.TriangleType = GetTriangleType(a, b, c);

            result.Coordinates = CalculateCoordinates(a, b, c);

            if (result.TriangleType == "равносторонний" ||
                result.TriangleType == "равнобедренный" ||
                result.TriangleType == "разносторонний")
            {
                Log("INFO", $"Успешный запрос: {requestTime:yyyy-MM-dd HH:mm:ss} | Параметры: ({a:F2}, {b:F2}, {c:F2}) | Тип: {result.TriangleType} | Координаты: ({result.Coordinates[0].Item1},{result.Coordinates[0].Item2}), ({result.Coordinates[1].Item1},{result.Coordinates[1].Item2}), ({result.Coordinates[2].Item1},{result.Coordinates[2].Item2})");
            }
            else
            {
                Log("ERROR", $"Неуспешный запрос: {requestTime:yyyy-MM-dd HH:mm:ss} | Параметры: ({a:F2}, {b:F2}, {c:F2}) | Результат: {result.TriangleType}");
            }

            return result;
        }

        private bool TryParseSides(string s1, string s2, string s3, out double a, out double b, out double c)
        {
            a = b = c = 0;

            try
            {
                s1 = s1?.Trim().Replace(',', '.');
                s2 = s2?.Trim().Replace(',', '.');
                s3 = s3?.Trim().Replace(',', '.');

                if (!double.TryParse(s1, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out a))
                    return false;

                if (!double.TryParse(s2, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out b))
                    return false;

                if (!double.TryParse(s3, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out c))
                    return false;

                if (a <= 0 || b <= 0 || c <= 0 ||
                    double.IsInfinity(a) || double.IsInfinity(b) || double.IsInfinity(c) ||
                    double.IsNaN(a) || double.IsNaN(b) || double.IsNaN(c))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsTriangle(double a, double b, double c)
        {
            return (a + b > c + EPSILON) &&
                   (a + c > b + EPSILON) &&
                   (b + c > a + EPSILON);
        }

        private string GetTriangleType(double a, double b, double c)
        {
            if (!IsTriangle(a, b, c))
                return "не треугольник";

            if (Math.Abs(a - b) < EPSILON && Math.Abs(b - c) < EPSILON)
                return "равносторонний";
            else if (Math.Abs(a - b) < EPSILON || Math.Abs(b - c) < EPSILON || Math.Abs(a - c) < EPSILON)
                return "равнобедренный";
            else
                return "разносторонний";
        }

        private List<Tuple<int, int>> CalculateCoordinates(double a, double b, double c)
        {
            var coordinates = new List<Tuple<int, int>>();

            if (!IsTriangle(a, b, c))
            {
                coordinates.Add(Tuple.Create(-1, -1));
                coordinates.Add(Tuple.Create(-1, -1));
                coordinates.Add(Tuple.Create(-1, -1));
                return coordinates;
            }

            double xA = 0, yA = 0;

            double xB = c, yB = 0;

            double xC = (b * b + c * c - a * a) / (2 * c);
            double yC = Math.Sqrt(Math.Max(0, b * b - xC * xC));

            double[] allX = { xA, xB, xC };
            double[] allY = { yA, yB, yC };

            double minX = allX.Min();
            double maxX = allX.Max();
            double minY = allY.Min();
            double maxY = allY.Max();

            double padding = FIELD_SIZE * 0.1;
            double availableWidth = FIELD_SIZE - 2 * padding;
            double availableHeight = FIELD_SIZE - 2 * padding;

            double width = maxX - minX;
            double height = maxY - minY;

            double scale = 1.0;
            if (width > EPSILON && height > EPSILON)
            {
                scale = Math.Min(availableWidth / width, availableHeight / height);
            }

            int TransformX(double x) => (int)Math.Round(padding + (x - minX) * scale);
            int TransformY(double y) => (int)Math.Round(FIELD_SIZE - padding - (y - minY) * scale);

            coordinates.Add(Tuple.Create(TransformX(xA), TransformY(yA)));
            coordinates.Add(Tuple.Create(TransformX(xB), TransformY(yB)));
            coordinates.Add(Tuple.Create(TransformX(xC), TransformY(yC)));

            return coordinates;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.WriteLine("===========================================");
            Console.WriteLine("        КАЛЬКУЛЯТОР ТРЕУГОЛЬНИКА          ");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            var calculator = new TriangleCalculator();

            while (true)
            {
                Console.WriteLine("Введите длины трех сторон (каждую на отдельной строке):");
                Console.WriteLine("(или введите 'exit' для выхода)");

                string sideA = Console.ReadLine();
                if (sideA?.ToLower() == "exit") break;

                string sideB = Console.ReadLine();
                if (sideB?.ToLower() == "exit") break;

                string sideC = Console.ReadLine();
                if (sideC?.ToLower() == "exit") break;

                Console.WriteLine();
                Console.WriteLine("Обработка запроса...");
                Console.WriteLine();

                var result = calculator.ProcessRequest(sideA, sideB, sideC);

                Console.WriteLine("РЕЗУЛЬТАТ:");
                Console.WriteLine($"Тип треугольника: {result.TriangleType}");
                Console.WriteLine("Координаты вершин:");

                string[] vertexNames = { "Вершина A", "Вершина B", "Вершина C" };
                for (int i = 0; i < result.Coordinates.Count; i++)
                {
                    Console.WriteLine($"  {vertexNames[i]}: ({result.Coordinates[i].Item1}, {result.Coordinates[i].Item2})");
                }

                Console.WriteLine();
                Console.WriteLine("-------------------------------------------");
                Console.WriteLine();
            }

            Console.WriteLine("Программа завершена. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}