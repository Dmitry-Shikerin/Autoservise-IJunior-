using System;
using System.Collections.Generic;

namespace Автосервис_IJunior_
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DetailsFactory detailsFactory = new DetailsFactory();
            DetailWarehouse detailWarehouse = new DetailWarehouse(detailsFactory.Create());
            CarFactory carFactory = new CarFactory(detailsFactory);
            CarService carService = new CarService(detailWarehouse);

            Queue<Driver> driversQueue = new Queue<Driver>();

            int queueCount = 7;
            int driverNumber = 0;

            for (int i = 0; i < queueCount; i++)
            {
                driversQueue.Enqueue(new Driver(carFactory.Create()));
            }

            for (int i = 0; i < queueCount; i++)
            {
                driverNumber++;
                Console.WriteLine($"Клиент {driverNumber}");

                carService.Serve(driversQueue.Dequeue());

                Console.WriteLine();
                Console.ReadKey();
            }
        }
    }

    class Driver
    {
        private int _money = 30000;

        public Driver(Car car)
        {
            Car = car;
        }

        public Car Car { get; }

        public bool CanPay(int price)
        {
            return _money >= price;
        }

        public void Pay(int price)
        {
            _money -= price;
        }
    }

    class Car
    {
        private List<Detail> _details = new List<Detail>();

        public Car(List<Detail> details)
        {
            _details = details;
        }
        public List<Detail> Details => new List<Detail>(_details);

        public void ShowInfo()
        {
            for (int i = 0; i < _details.Count; i++)
            {
                _details[i].ShowInfo();
            }
        }

        public bool TryReplace(Detail detail, Detail brokenDetail)
        {
            if (detail.Name != brokenDetail.Name)
                return false;

            if (_details.Remove(brokenDetail) == false)
                return false;

            _details.Add(detail);
            return true;
        }
    }

    class Detail
    {
        public Detail(string name, int price)
        {
            Name = name;
            Price = price;
        }
        public string Name { get; private set; }
        public int Price { get; private set; }
        public bool IsWorking { get; private set; } = true;

        public void ShowInfo()
        {
            Console.WriteLine($"{Name} {Price} {IsWorking}");
        }

        public void Brake()
        {
            IsWorking = false;
        }
    }

    class DetailsFactory
    {
        public List<Detail> Create()
        {
            return new List<Detail>()
            {
                new Detail("Тормозная колодка", 1500),
                new Detail("Рулевая рейка", 3500),
                new Detail("Замена масла", 700),
                new Detail("Топлевный насос", 2600),
                new Detail("Ремень ГРМ", 600),
                new Detail("Масляный фильтр", 500),
                new Detail("Трансмиссия", 6000),
                new Detail("Колейнвал", 3400)
            };
        }
    }

    class CarFactory
    {
        private readonly DetailsFactory _detailsFactory;

        public CarFactory(DetailsFactory detailsFactory)
        {
            _detailsFactory = detailsFactory;
        }

        public Car Create()
        {
            List<Detail> details = _detailsFactory.Create();

            int minValueBrokenDetail = 1;
            int maxValueBrokenDetail = 3;
            int valueBrokenDetail = Utils.GetRandomValue(minValueBrokenDetail, maxValueBrokenDetail);

            for (int i = 0; i <= valueBrokenDetail; i++)
            {
                int indexBrokenDetail = Utils.GetRandomValue(details.Count);
                details[indexBrokenDetail].Brake();
            }

            return new Car(details);
        }
    }

    class DetailWarehouse
    {
        private Dictionary<string, List<Detail>> _details;

        public DetailWarehouse(List<Detail> details)
        {
            SortDetails(details);
        }

        public bool TryGetDetail(string name, out Detail detail)
        {
            if (_details.TryGetValue(name, out List<Detail> details) == false)
            {
                detail = null;

                return false;
            }

            if (details.Count == 0)
            {
                detail = null;

                return false;
            }

            detail = details[0];
            details.Remove(detail);

            return true;
        }

        private void SortDetails(List<Detail> details)
        {
            _details = new Dictionary<string, List<Detail>>();

            foreach (Detail detail in details)
            {
                if (_details.ContainsKey(detail.Name) == false)
                {
                    _details.Add(detail.Name, new List<Detail>());
                }

                _details[detail.Name].Add(detail);
            }
        }
    }

    class CarService
    {
        private int _moneyBalance = 0;
        private int _fine = 500;
        private readonly DetailWarehouse _detailWarehouse;

        public CarService(DetailWarehouse detailWarehouse)
        {
            _detailWarehouse = detailWarehouse;
        }

        public void Serve(Driver driver)
        {
            Car car = driver.Car;

            car.ShowInfo();
            List<Detail> brokenDetails = GetBrokenDetails(car);
            int sumAllRepairs = CalculateSumAllRepairs(brokenDetails);
            RepairCar(brokenDetails, car);
            if (driver.CanPay(sumAllRepairs))
            {
                _moneyBalance += sumAllRepairs;
                driver.Pay(sumAllRepairs);
            }
            else
            {
                Console.WriteLine("У водителя недостаточно денег");
            }

            Console.WriteLine($"Вы обслужили очередного клиента\nВаш баланс {_moneyBalance}");
        }

        private int EvaluateRepairs(Detail detail)
        {
            int repairSum = detail.Price * 2;

            return repairSum;
        }

        private int CalculateSumAllRepairs(List<Detail> brokenDetails)
        {
            int sumAllRepairs = 0;

            foreach (Detail detail in brokenDetails)
            {
                sumAllRepairs += EvaluateRepairs(detail);
            }

            return sumAllRepairs;
        }

        private void RepairCar(List<Detail> brokenDetails, Car car)
        {
            foreach (Detail brokenDetail in brokenDetails)
            {
                string brokenDetailName = GetRandomDetailName(car, brokenDetail);

                if (_detailWarehouse.TryGetDetail(brokenDetailName, out Detail detail) == false)
                {
                    _moneyBalance -= _fine;

                    Console.WriteLine($"На складе не оказалось нужной детали: {brokenDetailName}");
                    Console.WriteLine("Вы заплатили штраф");

                    return;
                }

                if (car.TryReplace(detail, brokenDetail) == false)
                {
                    Console.WriteLine("Вы заменили не ту деталь и заплатили штраф");
                    Console.WriteLine("Вы заплатили штраф");
                    _moneyBalance -= _fine;
                }
            }
        }

        private string GetRandomDetailName(Car car, Detail detail)
        {
            int mistakeChance = 50;
            int maxChance = 100;

            int chance = Utils.GetRandomValue(maxChance);

            if (chance < mistakeChance)
            {
                int detailIndex = Utils.GetRandomValue(car.Details.Count);

                return car.Details[detailIndex].Name;
            }

            return detail.Name;
        }

        private List<Detail> GetBrokenDetails(Car car)
        {
            List<Detail> details = new List<Detail>();

            foreach (Detail detail in car.Details)
            {
                if (detail.IsWorking == false)
                    details.Add(detail);
            }

            return details;
        }
    }

    public static class Utils
    {
        private static Random _random = new Random();

        public static int GetRandomValue(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        public static int GetRandomValue(int value)
        {
            return _random.Next(value);
        }
    }
}
