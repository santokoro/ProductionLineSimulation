using System;
using System.Collections.Generic;
using System.Linq;

public class ProductionLineSimulation
{
    private readonly Random random = new Random();

    
    private readonly double meanArrivalTime;
    private readonly double meanServiceTime1;
    private readonly double meanServiceTime2;
    private readonly int buffer1Capacity;
    private readonly int buffer2Capacity;


    private Queue<Item> buffer1 = new Queue<Item>();
    private Queue<Item> buffer2 = new Queue<Item>();
    private Item station1Item;
    private Item station2Item;
    private bool isStation1Blocked = false;

   
    private int totalItemsArrived = 0;
    private int totalItemsProcessed = 0;
    private double totalProcessingTime = 0;
    private double station1BusyTime = 0;
    private double station2BusyTime = 0;
    private double currentTime = 0;

    
    private double nextArrivalTime;
    private double station1CompletionTime = double.MaxValue;
    private double station2CompletionTime = double.MaxValue;

    public ProductionLineSimulation(double arrivalTime = 0.4, double serviceTime1 = 1.25,
                                  double serviceTime2 = 0.5, int buf1Capacity = 4, int buf2Capacity = 2)
    {
        meanArrivalTime = arrivalTime;
        meanServiceTime1 = serviceTime1;
        meanServiceTime2 = serviceTime2;
        buffer1Capacity = buf1Capacity;
        buffer2Capacity = buf2Capacity;
    }

    public SimulationResult Run(double simulationTime)
    {
        Initialize();

        while (currentTime < simulationTime)
        {
            double nextEventTime = Math.Min(nextArrivalTime,
                                          Math.Min(station1CompletionTime, station2CompletionTime));

            
            UpdateStationBusyTimes(nextEventTime);

            currentTime = nextEventTime;
            if (currentTime >= simulationTime) break;

            if (currentTime == nextArrivalTime)
            {
                HandleArrival();
            }
            else if (currentTime == station1CompletionTime)
            {
                HandleStation1Completion();
            }
            else if (currentTime == station2CompletionTime)
            {
                HandleStation2Completion();
            }
        }

        return GetResults(simulationTime);
    }

    private void Initialize()
    {
        buffer1.Clear();
        buffer2.Clear();
        station1Item = null;
        station2Item = null;
        isStation1Blocked = false;

        totalItemsArrived = 0;
        totalItemsProcessed = 0;
        totalProcessingTime = 0;
        station1BusyTime = 0;
        station2BusyTime = 0;
        currentTime = 0;

        nextArrivalTime = Exponential(meanArrivalTime);
        station1CompletionTime = double.MaxValue;
        station2CompletionTime = double.MaxValue;
    }

    private void UpdateStationBusyTimes(double time)
    {
        double delta = time - currentTime;

        if (station1Item != null)
            station1BusyTime += delta;

        if (station2Item != null)
            station2BusyTime += delta;
    }

    private void HandleArrival()
    {
        totalItemsArrived++;
        var item = new Item { ArrivalTime = currentTime };

        if (buffer1.Count < buffer1Capacity && !isStation1Blocked)
        {
            buffer1.Enqueue(item);

            if (station1Item == null && buffer1.Count > 0)
            {
                station1Item = buffer1.Dequeue();
                station1CompletionTime = currentTime + Exponential(meanServiceTime1);
            }
        }

        nextArrivalTime = currentTime + Exponential(meanArrivalTime);
    }

    private void HandleStation1Completion()
    {
        station1Item.Station1ExitTime = currentTime;

        if (buffer2.Count < buffer2Capacity)
        {
            buffer2.Enqueue(station1Item);

            if (station2Item == null && buffer2.Count > 0)
            {
                station2Item = buffer2.Dequeue();
                station2CompletionTime = currentTime + Exponential(meanServiceTime2);
            }

            isStation1Blocked = false;

            if (buffer1.Count > 0)
            {
                station1Item = buffer1.Dequeue();
                station1CompletionTime = currentTime + Exponential(meanServiceTime1);
            }
            else
            {
                station1Item = null;
                station1CompletionTime = double.MaxValue;
            }
        }
        else
        {
            isStation1Blocked = true;
            station1CompletionTime = double.MaxValue;
        }
    }

    private void HandleStation2Completion()
    {
        station2Item.Station2ExitTime = currentTime;
        totalItemsProcessed++;
        totalProcessingTime += (currentTime - station2Item.ArrivalTime);

        station2Item = null;

        if (buffer2.Count > 0)
        {
            station2Item = buffer2.Dequeue();
            station2CompletionTime = currentTime + Exponential(meanServiceTime2);
        }
        else
        {
            station2CompletionTime = double.MaxValue;
        }

        if (isStation1Blocked && buffer2.Count < buffer2Capacity)
        {
            isStation1Blocked = false;

            if (station1Item != null)
            {
                station1CompletionTime = currentTime + Exponential(meanServiceTime1);
            }
            else if (buffer1.Count > 0)
            {
                station1Item = buffer1.Dequeue();
                station1CompletionTime = currentTime + Exponential(meanServiceTime1);
            }
        }
    }

    private double Exponential(double mean)
    {
        return -mean * Math.Log(random.NextDouble());
    }

    private SimulationResult GetResults(double simulationTime)
    {
        return new SimulationResult
        {
            TotalSimulationTime = simulationTime,
            TotalItemsArrived = totalItemsArrived,
            TotalItemsProcessed = totalItemsProcessed,
            AverageProcessingTime = totalItemsProcessed > 0 ? totalProcessingTime / totalItemsProcessed : 0,
            Station1Utilization = station1BusyTime / simulationTime,
            Station2Utilization = station2BusyTime / simulationTime,
            Buffer1AvgSize = CalculateAverageBufferSize(buffer1, simulationTime),
            Buffer2AvgSize = CalculateAverageBufferSize(buffer2, simulationTime),   
            Buffer1Capacity = buffer1Capacity,
            Buffer2Capacity = buffer2Capacity
        };
    }

    private double CalculateAverageBufferSize(Queue<Item> buffer, double totalTime)
    {
        
        
        return buffer.Count;
    }
}

public class Item
{
    public double ArrivalTime { get; set; }
    public double Station1ExitTime { get; set; }
    public double Station2ExitTime { get; set; }
}

public class SimulationResult
{
    public double TotalSimulationTime { get; set; }
    public int TotalItemsArrived { get; set; }
    public int TotalItemsProcessed { get; set; }
    public double AverageProcessingTime { get; set; }
    public double Station1Utilization { get; set; }
    public double Station2Utilization { get; set; }
    public double Buffer1AvgSize { get; set; }
    public double Buffer2AvgSize { get; set; }
    public int Buffer1Capacity { get; set; }
    public int Buffer2Capacity { get; set; }

    public void Print()
    {
        Console.WriteLine("=== Результаты моделирования ===");
        Console.WriteLine($"Конфигурация: Буфер1={Buffer1Capacity}, Буфер2={Buffer2Capacity}");
        Console.WriteLine($"Общее время моделирования: {TotalSimulationTime:F2}");
        Console.WriteLine($"Всего поступило изделий: {TotalItemsArrived}");
        Console.WriteLine($"Всего обработано изделий: {TotalItemsProcessed}");
        Console.WriteLine($"Среднее время обработки: {AverageProcessingTime:F2}");
        Console.WriteLine($"Загрузка станции 1: {Station1Utilization * 100:F2}%");
        Console.WriteLine($"Загрузка станции 2: {Station2Utilization * 100:F2}%");
        Console.WriteLine($"Средний размер буфера 1: {Buffer1AvgSize:F2}");
        Console.WriteLine($"Средний размер буфера 2: {Buffer2AvgSize:F2}");
        Console.WriteLine();
    }
}

class Program
{
    static void Main()
    {
        
        var bufferSizes = new[] { 2, 3, 4, 5 };
        var results = new List<SimulationResult>();

        foreach (var size in bufferSizes)
        {
            var sim = new ProductionLineSimulation(buf2Capacity: size);
            var result = sim.Run(1000);
            results.Add(result);
            result.Print();
        }

        
        Console.WriteLine("Сводная таблица результатов:");
        Console.WriteLine("| Размер буфера 2 | Обработано изделий | Загрузка станции 1 | Загрузка станции 2 | Среднее время обработки |");
        Console.WriteLine("|-----------------|--------------------|--------------------|--------------------|-------------------------|");

        foreach (var r in results.OrderBy(x => x.Buffer2Capacity))
        {
            Console.WriteLine($"| {r.Buffer2Capacity,15} | {r.TotalItemsProcessed,18} | {r.Station1Utilization * 100,18:F2}% | {r.Station2Utilization * 100,18:F2}% | {r.AverageProcessingTime,23:F2} |");
        }
    }
}