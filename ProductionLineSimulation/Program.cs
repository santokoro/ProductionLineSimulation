using System;
using System.Collections.Generic;

public class ProductionLineSimulation
{
    private readonly Random random = new Random();


    private const double MeanArrivalTime = 0.4;
    private const double MeanServiceTime1 = 1.25;
    private const double MeanServiceTime2 = 0.5;
    private const int Buffer1Capacity = 4;
    private const int Buffer2Capacity = 2;


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

    public void Run(double simulationTime)
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

        PrintStatistics(simulationTime);
    }

    private void Initialize()
    {
        nextArrivalTime = Exponential(MeanArrivalTime);
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

    
        if (buffer1.Count < Buffer1Capacity && !isStation1Blocked)
        {
            buffer1.Enqueue(item);

      
            if (station1Item == null && buffer1.Count > 0)
            {
                station1Item = buffer1.Dequeue();
                station1CompletionTime = currentTime + Exponential(MeanServiceTime1);
            }
        }

  
        nextArrivalTime = currentTime + Exponential(MeanArrivalTime);
    }

    private void HandleStation1Completion()
    {
        
        station1Item.Station1ExitTime = currentTime;

        if (buffer2.Count < Buffer2Capacity)
        {
            buffer2.Enqueue(station1Item);

 
            if (station2Item == null && buffer2.Count > 0)
            {
                station2Item = buffer2.Dequeue();
                station2CompletionTime = currentTime + Exponential(MeanServiceTime2);
            }

           
            isStation1Blocked = false;

            if (buffer1.Count > 0)
            {
                station1Item = buffer1.Dequeue();
                station1CompletionTime = currentTime + Exponential(MeanServiceTime1);
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
            station2CompletionTime = currentTime + Exponential(MeanServiceTime2);
        }
        else
        {
            station2CompletionTime = double.MaxValue;
        }

        if (isStation1Blocked && buffer2.Count < Buffer2Capacity)
        {
            isStation1Blocked = false;

            if (station1Item != null)
            {
                station1CompletionTime = currentTime + Exponential(MeanServiceTime1);
            }
            else if (buffer1.Count > 0)
            {
                station1Item = buffer1.Dequeue();
                station1CompletionTime = currentTime + Exponential(MeanServiceTime1);
            }
        }
    }

    private double Exponential(double mean)
    {
        return -mean * Math.Log(random.NextDouble());
    }

    private void PrintStatistics(double simulationTime)
    {
        Console.WriteLine("=== Результаты моделирования ===");
        Console.WriteLine($"Общее время моделирования: {simulationTime:F2}");
        Console.WriteLine($"Всего поступило изделий: {totalItemsArrived}");
        Console.WriteLine($"Всего обработано изделий: {totalItemsProcessed}");
        Console.WriteLine($"Среднее время обработки: {(totalProcessingTime / totalItemsProcessed):F2}");
        Console.WriteLine($"Загрузка станции 1: {(station1BusyTime / simulationTime * 100):F2}%");
        Console.WriteLine($"Загрузка станции 2: {(station2BusyTime / simulationTime * 100):F2}%");
    }

    private class Item
    {
        public double ArrivalTime { get; set; }
        public double Station1ExitTime { get; set; }
        public double Station2ExitTime { get; set; }
    }
}


class Program
{
    static void Main()
    {
        var simulation = new ProductionLineSimulation();
        simulation.Run(1000); // Запуск моделирования на 1000 единиц времени
    }
}