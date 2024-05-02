using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ShipsPort.Model.Harbours;
using ShipsPort.Model.Ships;

namespace ShipsPort;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        float t = 1.645f; // a = 0.05 p = 0.95
        float eps = 0.15f;

        int n = 100;
        int nStep = 1000;
        int prevN = 50;
        float error = 1;

        Console.WriteLine("ТОЧНОСТЬ = " + eps);
        do
        {
            float d = 0;


            Console.WriteLine("текущее N = " + n);
            List<float> ps = [];
            List<float> ms = [];
            for (int i = 0; i < 10; i++)
            {
                HarbourStatistics stats = new();


                HarbourOptions options = new()
                {
                    PiersAmount = 3,
                    // StormMax = 200,
                    // StormMin = 100,   
                    // StormEx = 500,
                    FourthShipAmount = 3,
                    PrintSteps = false
                };
                Harbour harbour = new Harbour(new ShipFactory(), options);
                stats = await harbour.Open(n);
                ps.Add((float)stats.ShipsLoaded / stats.ShipsArrived);
                ms.Add((float)stats.AverageLoadingTime);
            }


            float mError = ps.Max() - ps.Min();
            error =ms.Max() - ms.Min();
            Console.WriteLine("\t Вероятность обработки " + mError + " <= " + eps + " = " + (mError <= eps));
            Console.WriteLine("\t Время загрузки " + error + " <= " + eps + " = " + (error <= eps));
            if (error < eps)
                break;
            //
            // Console.WriteLine($"Кораблей прибыло: " + stats.ShipsArrived);
            // Console.WriteLine($"Кораблей обработано: " + stats.ShipsLoaded);
            // Console.WriteLine($"Кораблей в очереди: " + stats.QueueSize);
            // Console.WriteLine($"\t Вероятность обслуживания кораблей: " + (float)stats.ShipsLoaded / stats.ShipsArrived);
            // Console.WriteLine($"\n Количество штормов: " + stats.StormsAmount);
            // Console.WriteLine($"\n Количество часов, в которые корабли не могли выплыть и порта: " + stats.ShipsInStormTime);
            //
            // Console.WriteLine($"\nСредний интервал между штормами: " + stats.AverageStormsInterval);
            // Console.WriteLine($"Среднее время кораблей в шторме: " + stats.ShipsInStormTime);
            // Console.WriteLine($"Средний интервал загрузки: " + stats.AverageLoadingTime);
            // Console.WriteLine($"Среднее время ожидания: " + (float)stats.WaitingTimes.Sum() / stats.ShipsLoaded);


           // float p = values.Average();
           // Console.WriteLine("\t\t p = " + p);
           // float x = p * (1 - p) * t * t / eps / eps;
            //Console.WriteLine("\t\t N* = " + x);
                //prevN = (int)MathF.Ceiling(x);

            // float m = (float)stats.AverageLoadingTime;
            // foreach (float s in stats.LoadingTimes)
            //     d += MathF.Pow(s - m, 2) / stats.LoadingTimes.Count;
            // Console.WriteLine("\t\t m = " + m + " D = " + d);
            // float x = d * t * t / eps / eps;
            // Console.WriteLine("\t\t N* = " + x);
            // prevN = (int)MathF.Ceiling(x);
            n += nStep;
        } while (error > eps);

        // FunctionSeries series = new FunctionSeries();
        //
        // foreach (var pair in stats.LoadingProbability)
        //     series.Points.Add(new(pair.Key, pair.Value));
        //
        // FunctionSeries stormSeries = new FunctionSeries();
        //
        // foreach (var pair in stats.StormsTimes)
        // {
        //     stormSeries.Points.Add(new(pair.Key, 0));
        //     stormSeries.Points.Add(new(pair.Key, 0.1f));
        //     stormSeries.Points.Add(new(pair.Value, 0.1f));
        //     stormSeries.Points.Add(new(pair.Value, 0));
        // }
        //
        // stormSeries.Color = OxyColors.Red;
        // PlotModel model = new();
        // model.Series.Add(series);
        // model.Series.Add(stormSeries);
        // model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Время моделирования t, час" });
        // model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Вероятность обслуживания кораблей p" });
        //
        // Oxxxy.Model = model;
    }
}